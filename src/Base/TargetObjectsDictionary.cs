using System;
using System.Collections.Generic;
using System.Linq;

namespace XAMLMarkupExtensions.Base
{
    /// <summary>
    /// 
    /// </summary>
    internal class TargetObjectsDictionary : IDisposable
    {
        /// <summary>
        /// Holds the collection of assigned dependency objects as WeakReferences
        /// Instead of a single reference, a list is used, if this extension is applied to multiple instances.
        ///
        /// The values are lists of tuples, containing the target property and property type.
        /// </summary>
        private readonly Dictionary<WeakReference, TargetObjectValue> targetObjects = new Dictionary<WeakReference, TargetObjectValue>();

        private readonly Dictionary<int, List<WeakReference>> hashCodeTargetObjects = new Dictionary<int, List<WeakReference>>();

        private readonly HashSet<WeakReference> nestedTargetObjects = new HashSet<WeakReference>();

        private readonly List<WeakReference> deadTargets = new List<WeakReference>();

        public int Count => targetObjects.Count;

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

        public void AddTargetObjectProperty(WeakReference targetObjectWeakReference, object targetProperty, Type targetPropertyType, int targetPropertyIndex)
        {
            var targetPropertyInfo = new TargetPropertyInfo(targetProperty, targetPropertyType, targetPropertyIndex);
            var targetObjectProperties = targetObjects[targetObjectWeakReference].TargetProperties;

            if (!targetObjectProperties.Contains(targetPropertyInfo))
                targetObjectProperties.Add(targetPropertyInfo);
        }

        public bool IsConnected(TargetInfo info)
        {
            WeakReference wr = TryFindKey(info.TargetObject);
            if (wr == null)
                return false;

            var targetPropertyInfo = new TargetPropertyInfo(info.TargetProperty, info.TargetPropertyType, info.TargetPropertyIndex);
            return targetObjects[wr].TargetProperties.Contains(targetPropertyInfo);
        }

        public WeakReference TryFindKey(object targetObject)
        {
            var hashCode = targetObject.GetHashCode();
            if (!hashCodeTargetObjects.TryGetValue(hashCode, out var weakReferences))
                return null;

            return weakReferences.FirstOrDefault(wr => wr.Target == targetObject);
        }

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

        public void ClearDeadReferences()
        {
            if (!deadTargets.Any())
                return;

            foreach (var deadWeakReference in deadTargets)
            {
                if (!targetObjects.TryGetValue(deadWeakReference, out var targetValue))
                    continue;

                hashCodeTargetObjects.Remove(targetValue.TargetObjectHashCode);
                targetObjects.Remove(deadWeakReference);
                nestedTargetObjects.Remove(deadWeakReference);
            }

            deadTargets.Clear();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            targetObjects.Clear();
            hashCodeTargetObjects.Clear();
            nestedTargetObjects.Clear();
            deadTargets.Clear();
        }

        private class TargetObjectValue
        {
            public TargetObjectValue(int targetObjectHashCode)
            {
                TargetObjectHashCode = targetObjectHashCode;
                TargetProperties = new HashSet<TargetPropertyInfo>();
            }

            public int TargetObjectHashCode { get; }

            public HashSet<TargetPropertyInfo> TargetProperties { get; }
        }
    }
}
