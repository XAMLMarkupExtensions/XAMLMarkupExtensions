namespace XAMLMarkupExtensions.Base
{
    #region Uses
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Markup;
    using System.Windows;
    using System.Windows.Data;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Collections;
    #endregion

    /// <summary>
    /// The event arguments of the EndpointReached event.
    /// </summary>
    public class EndpointReachedEventArgs : EventArgs
    {
        /// <summary>
        /// The endpoint.
        /// </summary>
        public TargetInfo Endpoint { get; private set; }

        /// <summary>
        /// Get or set the value that will be stored to the endpoint.
        /// </summary>
        public object EndpointValue { get; set; }

        /// <summary>
        /// Creates a new <see cref="EndpointReachedEventArgs"/> object.
        /// </summary>
        /// <param name="endPoint">The endpoint.</param>
        public EndpointReachedEventArgs(TargetInfo endPoint)
        {
            this.Endpoint = endPoint;
            this.EndpointValue = null;
        }
    }

    /// <summary>
    /// This class walks up the tree of markup extensions to support nesting.
    /// Based on <see cref="https://github.com/SeriousM/WPFLocalizationExtension"/>
    /// </summary>
#if SILVERLIGHT
#else
    [MarkupExtensionReturnType(typeof(object))]
#endif
    public abstract class NestedMarkupExtension : MarkupExtension, INestedMarkupExtension, IDisposable
    {
        /// <summary>
        /// Holds the collection of assigned dependency objects as WeakReferences
        /// Instead of a single reference, a list is used, if this extension is applied to multiple instances.
        /// 
        /// The values are lists of tuples, containing the target property and property type.
        /// </summary>
        private readonly Dictionary<WeakReference, Dictionary<Tuple<object, int>, Type>> targetObjects = new Dictionary<WeakReference, Dictionary<Tuple<object, int>, Type>>();

        /// <summary>
        /// Get the target objects and properties.
        /// </summary>
        /// <returns>A list of target objects.</returns>
        private List<TargetInfo> GetTargetObjectsAndProperties()
        {
            List<TargetInfo> list = new List<TargetInfo>();
            
            // Select all targets that are still alive.
            foreach (var target in targetObjects)
            {
                if (!target.Key.IsAlive)
                    continue;

                list.AddRange(from kvp in target.Value
                              select new TargetInfo(target.Key.Target, kvp.Key.Item1, kvp.Value, kvp.Key.Item2));
            }

            return list;
        }

        /// <summary>
        /// Get the paths to all target properties through the nesting hierarchy.
        /// </summary>
        /// <returns>A list of paths to the properties.</returns>
        public List<TargetPath> GetTargetPropertyPaths()
        {
            var list = new List<TargetPath>();
            var objList = GetTargetObjectsAndProperties();

            foreach (var info in objList)
            {
                if (info.IsDependencyObject)
                {
                    TargetPath path = new TargetPath(info);
                    list.Add(path);
                }
                else
                {
                    foreach (var path in ((INestedMarkupExtension)info.TargetObject).GetTargetPropertyPaths())
                    {
                        // Push the ITargetMarkupExtension
                        path.AddStep(info);
                        // Add the tuple to the list
                        list.Add(path);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// An action that is called when the first target is bound.
        /// </summary>
        protected Action OnFirstTarget;

        /// <summary>
        /// This function must be implemented by all child classes.
        /// It shall return the properly prepared output of the markup extension.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <param name="endPoint">Information about the endpoint.</param>
        public abstract object FormatOutput(TargetInfo endPoint, TargetInfo info);

        /// <summary>
        /// Check, if the given target is connected to this markup extension.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <returns>True, if a connection exits.</returns>
        public bool IsConnected(TargetInfo info)
        {
            WeakReference wr = (from kvp in targetObjects
                                where kvp.Key.Target == info.TargetObject
                                select kvp.Key).FirstOrDefault();

            if (wr == null)
                return false;

            Tuple<object, int> tuple = new Tuple<object, int>(info.TargetProperty, info.TargetPropertyIndex);

            return targetObjects[wr].ContainsKey(tuple);
        }

        /// <summary>
        /// The ProvideValue method of the <see cref="MarkupExtension"/> base class.
        /// </summary>
        /// <param name="serviceProvider">A service provider</param>
        /// <returns>The value of the extension, or this if something gone wrong (needed for Templates).</returns>
        public sealed override object ProvideValue(IServiceProvider serviceProvider)
        {
            // if the service provider is null, return this
            if (serviceProvider == null)
                return this;

            // try to cast the passed serviceProvider to a IProvideValueTarget
            IProvideValueTarget service = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            // if the cast fails, return this
            if (service == null)
                return this;

            // declare a target object and property
            object targetObject = service.TargetObject;
            object targetProperty = service.TargetProperty;
            int targetPropertyIndex = -1;
            Type targetPropertyType = null;
            //IList targetList = null;

            // First, check if the service provider is of type SimpleProvideValueServiceProvider
            //      -> If yes, get the target property type and index.
            // check if the service.TargetProperty is a DependencyProperty or a PropertyInfo and set the type info
            if (serviceProvider is SimpleProvideValueServiceProvider)
            {
                targetPropertyType = ((SimpleProvideValueServiceProvider)serviceProvider).TargetPropertyType;
                targetPropertyIndex = ((SimpleProvideValueServiceProvider)serviceProvider).TargetPropertyIndex;
            }
            else
            {
                if (targetProperty is DependencyProperty)
                {
                    DependencyProperty dp = (DependencyProperty)targetProperty;

                    #region Get the property type
#if SILVERLIGHT
                    // Dirty reflection hack - get the property type (property not included in the SL DependencyProperty class) from the internal declared field.
                    targetPropertyType = typeof(DependencyProperty).GetField("_propertyType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dp) as Type;
#else
                    targetPropertyType = dp.PropertyType;
#endif 
                    #endregion
                }
                else if (targetProperty is PropertyInfo)
                {
                    PropertyInfo pi = (PropertyInfo)targetProperty;
                    targetPropertyType = pi.PropertyType;

                    // Kick out indexers.
                    if (pi.GetIndexParameters().Count() > 0)
                        throw new InvalidOperationException("Indexers are not supported!");
                }
                else
                    return this;

                #region Lists - not used
                //// Check, if a List is the target and get it.
                //if (typeof(IList).IsAssignableFrom(targetPropertyType))
                //{
                //    // Try to read out the own return type.
                //    var att = (MarkupExtensionReturnTypeAttribute)this.GetType().GetCustomAttributes(typeof(MarkupExtensionReturnTypeAttribute), true).FirstOrDefault();

                //    if ((att == null) || !typeof(IList).IsAssignableFrom(att.ReturnType))
                //    {
                //        // We've got the case, that this extension does not return a list, although being assigned to one.
                //        // We now have to set the appropriate index value and we have to return the list itself with our own member added to the end.
                //        if (targetLists.ContainsKey(targetObject))
                //            targetList = targetLists[targetObject];
                //        else
                //        {
                //            targetList = (IList)GetPropertyValue(new TargetInfo(targetObject, targetProperty, targetPropertyType, -1));
                        
                //            // The list does not yet exist? Create it!
                //            if (targetList == null)
                //                targetList = (IList)Activator.CreateInstance(targetPropertyType);

                //            targetLists.Add(targetObject, targetList);
                //        }

                //        // Store the count value to our index.
                //        targetPropertyIndex = targetList.Count;

                //        // If our own return type is unknown or an object, add null to the list.
                //        // If we have value type, add a default instance.
                //        if ((att == null) || !att.ReturnType.IsValueType)
                //            targetList.Add(null);
                //        else
                //            targetList.Add(Activator.CreateInstance(att.ReturnType));

                //        System.IO.StreamWriter writer = new System.IO.StreamWriter(@"C:\Temp\Test.txt", true);
                //        writer.WriteLine("***************************************");
                //        writer.WriteLine(targetList.Count);
                //        writer.WriteLine(targetLists.Count);
                //        writer.WriteLine((this as WPFMarkupExtensions.Binding.DynBindingExtension).Path);
                //        writer.WriteLine();
                //        writer.Close();
                //    }
                //}
                #endregion
            }

            // if the service.TargetObject is System.Windows.SharedDp (= not a DependencyObject), we return "this".
            // the SharedDp will call this instance later again.
            if (!(targetObject is DependencyObject) && !(targetProperty is PropertyInfo))
                return this;
            
            // check supported types and throw exceptions on errors
            if (!(targetObject is DependencyObject || targetObject is INestedMarkupExtension))
                throw new InvalidOperationException("Only dependency objects and objects implementing ITargetMarkupExtension are supported!\r\n" + targetObject + "\r\n" + targetProperty); ;
            
            if (targetObject is Binding)
                throw new InvalidOperationException("Binding is not supported!"); ;
            
            // search for the target in the target object list
            WeakReference wr = (from kvp in targetObjects
                                where kvp.Key.Target == targetObject
                                select kvp.Key).FirstOrDefault();

            if (wr == null)
            {
                // if it's the first object, call the appropriate action
                if (targetObjects.Count == 0)
                {
                    EndpointReachedEvent.AddListener(this);
                    if (OnFirstTarget != null)
                        OnFirstTarget();
                }

                // add the target as an dependency object as weakreference to the dependency object list
                wr = new WeakReference(targetObject);
                targetObjects.Add(wr, new Dictionary<Tuple<object, int>, Type>());

                // add this extension to the ObjectDependencyManager to ensure the lifetime along with the targetobject
                ObjectDependencyManager.AddObjectDependency(new WeakReference(service.TargetObject), this);
            }

            // finally, add the target prop and info to the list of this weakreference
            Tuple<object, int> tuple = new Tuple<object, int>(targetProperty, targetPropertyIndex);
            if (!targetObjects[wr].ContainsKey(tuple))
                targetObjects[wr].Add(tuple, targetPropertyType);

            // create the target info
            TargetInfo info = new TargetInfo(targetObject, targetProperty, targetPropertyType, targetPropertyIndex);

            // return the result of FormatOutput
            object result = null;

            if (info.IsDependencyObject)
            {
                var args = new EndpointReachedEventArgs(info);
                EndpointReachedEvent.Invoke(this, args);
                result = args.EndpointValue;
            }
            else
                result = FormatOutput(null, info);

            #region Lists - not used
            //// check again the list issue from above...
            //if ((targetList != null) && (targetPropertyIndex >= 0) && (targetList.Count > targetPropertyIndex))
            //{
            //    // Substitute the value in the list using the supplied target index.
            //    targetList[targetPropertyIndex] = result;

            //    // Now, set the list as the result.
            //    result = targetList;
            //} 
            #endregion

            // check type
            if ((result != null) && targetPropertyType.IsAssignableFrom(result.GetType()))
                return result;
            
            // finally, if nothing was there, return null or default
            if (targetPropertyType.IsValueType)
                return Activator.CreateInstance(targetPropertyType);
            else
                return null;
        }

        /// <summary>
        /// Set the new value for all targets.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        protected void UpdateNewValue()
        {
            UpdateNewValue(null);
        }

        /// <summary>
        /// Trigger the update of the target(s).
        /// </summary>
        /// <param name="targetPath">A specific path to follow or null for all targets.</param>
        /// <returns>The output of the path at the endpoint.</returns>
        public object UpdateNewValue(TargetPath targetPath)
        {
            if (targetPath == null)
            {
                // No path supplied - send it to all targets.
                foreach (var path in GetTargetPropertyPaths())
                {
                    // Call yourself and supply the path to follow.
                    UpdateNewValue(path);
                }
            }
            else
            {
                // Get the info of the next step.
                TargetInfo info = targetPath.GetNextStep();

                // Get the own formatted output.
                object output = FormatOutput(targetPath.EndPoint, info);

                // Set the property of the target to the new value.
                SetPropertyValue(output, info, false);

                // Have we reached a DependencyObject?
                // If not, call the UpdateNewValue function of the next ITargetMarkupExtension
                if (info.IsDependencyObject)
                    return output;
                else
                    return ((INestedMarkupExtension)info.TargetObject).UpdateNewValue(targetPath);
            }

            return null;
        }

        /// <summary>
        /// Sets the value of a property of type PropertyInfo or DependencyProperty.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="info">The target information.</param>
        /// <param name="forceNull">Determines, whether null values should be written.</param>
        public static void SetPropertyValue(object value, TargetInfo info, bool forceNull)
        {
            if ((value == null) && !forceNull)
                return;

            // Anyway, a value type cannot receive null values...
            if (info.TargetPropertyType.IsValueType && (value == null))
                value = Activator.CreateInstance(info.TargetPropertyType);

            // Set the value.
            if (info.TargetProperty is DependencyProperty)
                ((DependencyObject)info.TargetObject).SetValue((DependencyProperty)info.TargetProperty, value);
            else
            {
                PropertyInfo pi = (PropertyInfo)info.TargetProperty;

                if (typeof(IList).IsAssignableFrom(info.TargetPropertyType) && (value != null) && !info.TargetPropertyType.IsAssignableFrom(value.GetType()))
                {
                    // A list, a list - get it and set the value directly via its index.
                    if (info.TargetPropertyIndex >= 0)
                    {
                        IList list = (IList)pi.GetValue(info.TargetObject, null);
                        if (list.Count > info.TargetPropertyIndex)
                            list[info.TargetPropertyIndex] = value;
                    }
                    return;
                }

                pi.SetValue(info.TargetObject, value, null);
            }
        }

        /// <summary>
        /// Gets the value of a property of type PropertyInfo or DependencyProperty.
        /// </summary>
        /// <param name="info">The target information.</param>
        /// <returns>The value.</returns>
        public static object GetPropertyValue(TargetInfo info)
        {
            if (info.TargetProperty is DependencyProperty)
                return ((DependencyObject)info.TargetObject).GetValue((DependencyProperty)info.TargetProperty);
            else if (info.TargetProperty is PropertyInfo)
            {
                PropertyInfo pi = (PropertyInfo)info.TargetProperty;

                if (info.TargetPropertyIndex >= 0)
                {
                    if (typeof(IList).IsAssignableFrom(info.TargetPropertyType))
                    {
                        IList list = (IList)pi.GetValue(info.TargetObject, null);
                        if (list.Count > info.TargetPropertyIndex)
                            return list[info.TargetPropertyIndex];
                    }
                }

                return ((PropertyInfo)info.TargetProperty).GetValue(info.TargetObject, null);
            }

            return null;
        }

        /// <summary>
        /// Safely get the value of a property that might be set by a further MarkupExtension.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="value">The value supplied by the set accessor of the property.</param>
        /// <param name="property">The property information.</param>
        /// <param name="index">The index of the indexed property, if applicable.</param>
        /// <returns>The value or default.</returns>
        protected T GetValue<T>(object value, PropertyInfo property, int index)
        {
            // Simple case: value is of same type
            if (value is T)
                return (T)value;
            
            // No property supplied
            if (property == null)
                return default(T);
            
            // Is value of type MarkupExtension?
            if (value is MarkupExtension)
            {
                object result = ((MarkupExtension)value).ProvideValue(new SimpleProvideValueServiceProvider(this, property, property.PropertyType, index));
                if (result != null)
                    return (T)result;
                else
                    return default(T);
            }

            // Default return path.
            return default(T);
        }

        /// <summary>
        /// This method must return true, if an update shall be executed when the given endpoint is reached.
        /// This method is called each time an endpoint is reached.
        /// </summary>
        /// <param name="endpoint">Information on the specific endpoint.</param>
        /// <returns>True, if an update of the path to this endpoint shall be performed.</returns>
        protected abstract bool UpdateOnEndpoint(TargetInfo endpoint);

        /// <summary>
        /// Get the path to a specific endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint info.</param>
        /// <returns>The path to the endpoint.</returns>
        protected TargetPath GetPathToEndpoint(TargetInfo endpoint)
        {
            return (from p in GetTargetPropertyPaths() where p.EndPoint.Equals(endpoint) select p).FirstOrDefault();
        }

        /// <summary>
        /// Checks the existance of the given object in the target endpoint list.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>True, if the extension nesting tree reaches the given object.</returns>
        protected bool IsEndpointObject(object obj)
        {
            return (from p in GetTargetPropertyPaths() where p.EndPoint.TargetObject == obj select p).Count() > 0;
        }

        /// <summary>
        /// An event handler that is called from the static <see cref="EndpointReachedEvent"/> class.
        /// </summary>
        /// <param name="sender">The markup extension that reached an enpoint.</param>
        /// <param name="args">The event args containing the endpoint information.</param>
        private void OnEndpointReached(NestedMarkupExtension sender, EndpointReachedEventArgs args)
        {
            var path = GetPathToEndpoint(args.Endpoint);

            if (path == null)
                return;

            if ((this != sender) && !UpdateOnEndpoint(path.EndPoint))
                return;

            args.EndpointValue = UpdateNewValue(path);
        }

        /// <summary>
        /// Implements the IDisposable.Dispose function.
        /// </summary>
        public void Dispose()
        {
            EndpointReachedEvent.RemoveListener(this);
            targetObjects.Clear();
        }

        #region EndpointReachedEvent
        /// <summary>
        /// A static proxy class that handles endpoint reached events for a list of weak references of TargetMarkupExtensions.
        /// This circumvents the usage of a WeakEventManager while providing a static instance that is capable of firing the event.
        /// </summary>
        internal static class EndpointReachedEvent
        {
            /// <summary>
            /// The list of listeners
            /// </summary>
            private static List<WeakReference> listeners = new List<WeakReference>();

            /// <summary>
            /// Fire the event.
            /// </summary>
            /// <param name="sender">The markup extension that reached an enpoint.</param>
            /// <param name="args">The event args containing the endpoint information.</param>
            internal static void Invoke(NestedMarkupExtension sender, EndpointReachedEventArgs args)
            {
                List<WeakReference> purgeList = new List<WeakReference>();

                for (int i = 0; i < listeners.Count; i++)
                {
                    WeakReference wr = listeners[i];
                 
                    if (wr.IsAlive)
                        ((NestedMarkupExtension)wr.Target).OnEndpointReached(sender, args);
                    else
                        purgeList.Add(wr);
                }

                foreach (WeakReference wr in purgeList)
                    listeners.Remove(wr);

                purgeList.Clear();
            }

            /// <summary>
            /// Adds a listener to the inner list of listeners.
            /// </summary>
            /// <param name="listener">The listener to add.</param>
            internal static void AddListener(NestedMarkupExtension listener)
            {
                if (listener == null)
                    return;

                listeners.Add(new WeakReference(listener));
            }

            /// <summary>
            /// Removes a listener from the inner list of listeners.
            /// </summary>
            /// <param name="listener">The listener to remove.</param>
            internal static void RemoveListener(NestedMarkupExtension listener)
            {
                if (listener == null)
                    return;

                List<WeakReference> purgeList = new List<WeakReference>();

                foreach (WeakReference wr in listeners)
                {
                    if (!wr.IsAlive)
                        purgeList.Add(wr);
                    else if ((NestedMarkupExtension)wr.Target == listener)
                        purgeList.Add(wr);
                }
            }

            /// <summary>
            /// Remove a list of weak references from the list.
            /// </summary>
            /// <param name="purgeList">The list of references to remove.</param>
            private static void Purge(List<WeakReference> purgeList)
            {
                foreach (WeakReference wr in purgeList)
                    listeners.Remove(wr);

                purgeList.Clear();
            }
        }
        #endregion
    }
}
