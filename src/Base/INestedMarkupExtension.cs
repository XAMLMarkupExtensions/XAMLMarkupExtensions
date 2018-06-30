#region Copyright information
// <copyright file="INestedMarkupExtension.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://xamlmarkupextensions.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace XAMLMarkupExtensions.Base
{
    #region Uses
    using System;
    using System.Collections.Generic;
    using System.Windows;
    #endregion

    #region Helper classes
    /// <summary>
    /// This class stores information about a markup extension target.
    /// </summary>
    public class TargetInfo
    {
        /// <summary>
        /// Gets the target object.
        /// </summary>
        public object TargetObject { get; private set; }

        /// <summary>
        /// Gets the target property.
        /// </summary>
        public object TargetProperty { get; private set; }

        /// <summary>
        /// Gets the target property type.
        /// </summary>
        public Type TargetPropertyType { get; private set; }

        /// <summary>
        /// Gets the target property index.
        /// </summary>
        public int TargetPropertyIndex { get; private set; }

        /// <summary>
        /// True, if the target object is a DependencyObject.
        /// </summary>
        public bool IsDependencyObject { get { return TargetObject is DependencyObject; } }

        /// <summary>
        /// True, if the target object is an endpoint (not another nested markup extension).
        /// </summary>
        public bool IsEndpoint { get { return !(TargetObject is INestedMarkupExtension); } }

        /// <summary>
        /// Determines, whether both objects are equal.
        /// </summary>
        /// <param name="obj">An object of type TargetInfo.</param>
        /// <returns>True, if both are equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj is TargetInfo)
            {
                var ti = (TargetInfo)obj;

                if (ti.TargetObject != this.TargetObject)
                    return false;
                if (ti.TargetProperty != this.TargetProperty)
                    return false;
                if (ti.TargetPropertyIndex != this.TargetPropertyIndex)
                    return false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function.
        /// </summary>
        /// <returns>The hash value.</returns>
        public override int GetHashCode()
        {
            // As this class is similar to a Tuple<T1, T2, T3> (the property type is redundant),
            // we take this as a template for the hash generation.
            return Tuple.Create<object, object, int>(this.TargetObject, this.TargetProperty, this.TargetPropertyIndex).GetHashCode();
        }

        /// <summary>
        /// Creates a new TargetInfo instance.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="targetPropertyType">The target property type.</param>
        /// <param name="targetPropertyIndex">The target property index.</param>
        public TargetInfo(object targetObject, object targetProperty, Type targetPropertyType, int targetPropertyIndex)
        {
            this.TargetObject = targetObject;
            this.TargetProperty = targetProperty;
            this.TargetPropertyType = targetPropertyType;
            this.TargetPropertyIndex = targetPropertyIndex;
        }
    }

    /// <summary>
    /// This class helps tracking the path to a specific endpoint.
    /// </summary>
    public class TargetPath
    {
        /// <summary>
        /// The path to the endpoint.
        /// </summary>
        private Stack<TargetInfo> Path { get; set; }
        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        public TargetInfo EndPoint { get; private set; }

        /// <summary>
        /// Add another step to the path.
        /// </summary>
        /// <param name="info">The TargetInfo object of the step.</param>
        public void AddStep(TargetInfo info) { Path.Push(info); }

        /// <summary>
        /// Get the next step and remove it from the path.
        /// </summary>
        /// <returns>The next steps TargetInfo.</returns>
        public TargetInfo GetNextStep() { return (Path.Count > 0) ? Path.Pop() : EndPoint; }

        /// <summary>
        /// Get the next step.
        /// </summary>
        /// <returns>The next steps TargetInfo.</returns>
        public TargetInfo ShowNextStep() { return (Path.Count > 0) ? Path.Peek() : EndPoint; }

        /// <summary>
        /// Creates a new TargetPath instance.
        /// </summary>
        /// <param name="endPoint">The endpoints TargetInfo of the path.</param>
        public TargetPath(TargetInfo endPoint)
        {
            if (!endPoint.IsEndpoint)
                throw new ArgumentException("A path endpoint cannot be another INestedMarkupExtension.");

            EndPoint = endPoint;
            Path = new Stack<TargetInfo>();
        }
    }
    #endregion

    /// <summary>
    /// Markup extensions that implement this interface shall be able to return their target objects.
    /// They should also implement a SetNewValue function that properly set the value to all their targets with their own modification of the value.
    /// </summary>
    public interface INestedMarkupExtension
    {
        /// <summary>
        /// Get the paths to all target properties through the nesting hierarchy.
        /// </summary>
        /// <returns>A list of combinations of property types and the corresponsing stacks that resemble the paths to the properties.</returns>
        List<TargetPath> GetTargetPropertyPaths();

        /// <summary>
        /// Trigger the update of the target(s).
        /// </summary>
        /// <param name="targetPath">A specific path to follow or null for all targets.</param>
        /// <returns>The output of the path at the endpoint.</returns>
        object UpdateNewValue(TargetPath targetPath);

        /// <summary>
        /// Format the output of the markup extension.
        /// </summary>
        /// <param name="endpoint">Information about the endpoint.</param>
        /// <param name="info">Information about the target.</param>
        /// <returns>The output of this extension for the given endpoint and target.</returns>
        object FormatOutput(TargetInfo endpoint, TargetInfo info);

        /// <summary>
        /// Check, if the given target is connected to this markup extension.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <returns>True, if a connection exits.</returns>
        bool IsConnected(TargetInfo info);
    }
}
