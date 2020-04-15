using System;
using System.Collections.Generic;
using System.Security.Permissions;

namespace XAMLMarkupExtensions.Base
{
    /// <summary>
    /// Class for using <see cref="WeakReference" /> as a key in <see cref="Dictionary{TKey,TValue}" />.
    /// It allows use search by comparing <see cref="WeakReference.Target" /> and keeps HashCode immutable.
    /// </summary>
    [Serializable]
    [SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    internal class WeakReferenceKey : WeakReference
    {
        private readonly int _hashCode;

        /// <summary>
        /// Create new instance of <see cref="WeakReferenceKey" />.
        /// </summary>
        /// <param name="target">Target object for weak reference.</param>
        public WeakReferenceKey(object target) : base(target)
        {
            _hashCode = target.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WeakReferenceKey) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        /// Compare to another object.
        /// </summary>
        /// <param name="other">Other object for comparing.</param>
        /// <returns>
        /// <see langwrod="true" /> if objects are equal, <see langword="false" /> otherwise.
        /// </returns>
        protected bool Equals(WeakReferenceKey other)
        {
            return Target == other.Target;
        }
    }
}