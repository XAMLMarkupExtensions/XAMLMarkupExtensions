namespace XAMLMarkupExtensions.Base
{
    #region Uses
    using System;
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// Interface for object which store at <see cref="ObjectDependencyManager" />.
    /// </summary>
    public interface IObjectDependency
    {
        /// <summary>
        /// Notify that some of references are dead.
        /// </summary>
        void OnReferencesRemoved(IEnumerable<WeakReference> deadReferences);

        /// <summary>
        /// Notify that all of related references are dead.
        /// </summary>
        void OnAllReferencesRemoved();
    }
}
