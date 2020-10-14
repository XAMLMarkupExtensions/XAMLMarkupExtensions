#region Copyright information
// <copyright file="TargetObjectList.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     https://github.com/XAMLMarkupExtensions/XAMLMarkupExtensions/blob/master/LICENSE
// </copyright>
// <author>Vsevolod Pilipenko, Konrad Mattheis</author>
#endregion

namespace XAMLMarkupExtensions.Base
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Linq; 
    #endregion

    /// <summary>
    /// Defines a collection of assigned dependency objects.
    /// Instead of a single reference, a list is used, if this extension is applied to multiple instances.
    /// </summary>
    internal class TargetObjectsList
    {
        /// <summary>
        /// Holds the collection of assigned dependency objects as WeakReferences and its property metadata.
        /// </summary>
        private readonly Dictionary<WeakReference, TargetObjectValue> targetObjects = new Dictionary<WeakReference, TargetObjectValue>();

        /// <summary>
        /// Holds hash codes of each target in <see cref="targetObjects" />.
        /// Allows fast find key in <see cref="targetObjects" /> by object.
        /// <see cref="List{WeakReference}" /> is necessary, because different objects can has same hashcode.
        /// </summary>
        private readonly Dictionary<int, List<WeakReference>> hashCodeTargetObjects = new Dictionary<int, List<WeakReference>>();

        /// <summary>
        /// Holds the collection of assigned dependency objects which implements <see cref="INestedMarkupExtension" />.
        /// This collection is subset of <see cref="targetObjects" />.
        /// </summary>
        private readonly HashSet<WeakReference> nestedTargetObjects = new HashSet<WeakReference>();

        /// <summary>
        /// Holds the collection of weak references, which target already collected by GC.
        /// This references should be removed from other collections.
        /// </summary>
        private readonly List<WeakReference> deadTargets = new List<WeakReference>();

        /// <summary>
        /// Gets the count of assigned dependency objects.
        /// </summary>
        public int Count => targetObjects.Count;

        /// <summary>
        /// Add new target to internal list.
        /// </summary>
        /// <param name="targetObject">the new target.</param>
        /// <returns>Weak reference of target object.</returns>
        public WeakReference AddTargetObject(object targetObject)
        {
            var wr = new WeakReference(targetObject);
            var targetObjectHashCode = targetObject.GetHashCode();
            targetObjects.Add(wr, new TargetObjectValue(targetObjectHashCode));

            if (!hashCodeTargetObjects.ContainsKey(targetObjectHashCode))
                hashCodeTargetObjects.Add(targetObjectHashCode, new List<WeakReference>());
            hashCodeTargetObjects[targetObjectHashCode].Add(wr);

            if (targetObject is INestedMarkupExtension)
                nestedTargetObjects.Add(wr);

            return wr;
        }

        /// <summary>
        /// Add property info of target object.
        /// </summary>
        /// <param name="targetObjectWeakReference">The weak reference of target object.</param>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="targetPropertyType">The type of target property.</param>
        /// <param name="targetPropertyIndex">The index of target property.</param>
        public void AddTargetObjectProperty(WeakReference targetObjectWeakReference, object targetProperty, Type targetPropertyType, int targetPropertyIndex)
        {
            var targetPropertyInfo = new TargetPropertyInfo(targetProperty, targetPropertyType, targetPropertyIndex);
            var targetObjectProperties = targetObjects[targetObjectWeakReference].TargetProperties;

            if (!targetObjectProperties.Contains(targetPropertyInfo))
                targetObjectProperties.Add(targetPropertyInfo);
        }

        /// <summary>
        /// Check if target object and its property contains in list.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <returns><see langwrod="true" /> if it is contains.</returns>
        public bool IsConnected(TargetInfo info)
        {
            WeakReference wr = TryFindKey(info.TargetObject);
            if (wr == null)
                return false;

            var targetPropertyInfo = new TargetPropertyInfo(info.TargetProperty, info.TargetPropertyType, info.TargetPropertyIndex);
            return targetObjects[wr].TargetProperties.Contains(targetPropertyInfo);
        }

        /// <summary>
        /// Remove target object's property info.
        /// </summary>
        /// <param name="info">The information about the target.</param>
        /// <returns>
        /// <see langwrod="true" /> if info was removed.
        /// <see langwrod="false" /> if info didn't contains at list.
        /// </returns>
        public bool RemoveTargetInfo(TargetInfo info)
        {
            WeakReference wr = TryFindKey(info.TargetObject);
            if (wr == null)
                return false;

            var targetProperties = targetObjects[wr].TargetProperties;
            var targetPropertyInfo = new TargetPropertyInfo(info.TargetProperty, info.TargetPropertyType, info.TargetPropertyIndex);
            bool isRemoved = targetProperties.Remove(targetPropertyInfo);
            if (isRemoved && targetProperties.Count == 0)
                RemoveTargetObject(wr);

            return isRemoved;
        }

        /// <summary>
        /// Try find weak reference key of target object in list.
        /// </summary>
        /// <param name="targetObject">The searching target object.</param>
        /// <returns>
        /// <see cref="WeakReference" /> of target object if it contains in list.
        /// <see langword="null" /> otherwise.
        /// </returns>
        public WeakReference TryFindKey(object targetObject)
        {
            var hashCode = targetObject.GetHashCode();
            if (!hashCodeTargetObjects.TryGetValue(hashCode, out var weakReferences))
                return null;

            return weakReferences.FirstOrDefault(wr => wr.Target == targetObject);
        }

        /// <summary>
        /// Get information of all target objects.
        /// </summary>
        public IEnumerable<TargetInfo> GetTargetInfos()
        {
            foreach (var target in targetObjects)
            {
                var targetReference = target.Key.Target;
                if (targetReference == null)
                {
                    deadTargets.Add(target.Key);
                    continue;
                }

                foreach (var data in target.Value.TargetProperties)
                {
                    yield return new TargetInfo(targetReference, data.TargetProperty, data.TargetPropertyType, data.TargetPropertyIndex);
                }
            }
        }

        /// <summary>
        /// Get information of all targets objects which implements <see cref="INestedMarkupExtension" />.
        /// </summary>
        public IEnumerable<TargetInfo> GetNestedTargetInfos()
        {
            foreach (var nestedTargetObject in nestedTargetObjects)
            {
                var targetReference = nestedTargetObject.Target;
                if (targetReference == null)
                {
                    deadTargets.Add(nestedTargetObject);
                    continue;
                }

                foreach (var data in targetObjects[nestedTargetObject].TargetProperties)
                {
                    yield return new TargetInfo(targetReference, data.TargetProperty, data.TargetPropertyType, data.TargetPropertyIndex);
                }
            }
        }

        /// <summary>
        /// Clear all references to dead targets.
        /// </summary>
        public void ClearDeadReferences()
        {
            if (!deadTargets.Any())
                return;

            foreach (var deadWeakReference in deadTargets)
                RemoveTargetObject(deadWeakReference);

            deadTargets.Clear();
        }

        /// <summary>
        /// Clear specified references.
        /// </summary>
        public void ClearReferences(IEnumerable<WeakReference> references)
        {
            deadTargets.AddRange(references);
            ClearDeadReferences();
        }

        /// <summary>
        /// Clear all targets.
        /// </summary>
        public void Clear()
        {
            targetObjects.Clear();
            hashCodeTargetObjects.Clear();
            nestedTargetObjects.Clear();
            deadTargets.Clear();
        }

        /// <summary>
        /// Remove target object from internal list.
        /// </summary>
        /// <param name="targetObjectWeakReference">Weak reference key of target object.</param>
        private void RemoveTargetObject(WeakReference targetObjectWeakReference)
        {
            if (!targetObjects.TryGetValue(targetObjectWeakReference, out var targetValue))
                return;

            hashCodeTargetObjects[targetValue.TargetObjectHashCode].Remove(targetObjectWeakReference);
            if (!hashCodeTargetObjects[targetValue.TargetObjectHashCode].Any())
                hashCodeTargetObjects.Remove(targetValue.TargetObjectHashCode);

            targetObjects.Remove(targetObjectWeakReference);
            nestedTargetObjects.Remove(targetObjectWeakReference);
        }

        /// <summary>
        /// Information about target object.
        /// </summary>
        private class TargetObjectValue
        {
            /// <summary>
            /// Create new <see cref="TargetObjectValue" /> instance.
            /// </summary>
            /// <param name="targetObjectHashCode">The hashcode of target object.</param>
            public TargetObjectValue(int targetObjectHashCode)
            {
                TargetObjectHashCode = targetObjectHashCode;
                TargetProperties = new HashSet<TargetPropertyInfo>();
            }

            /// <summary>
            /// Hashcode of target object.
            /// </summary>
            public int TargetObjectHashCode { get; }

            /// <summary>
            /// Information about properties of target object.
            /// </summary>
            public HashSet<TargetPropertyInfo> TargetProperties { get; }
        }
    }
}
