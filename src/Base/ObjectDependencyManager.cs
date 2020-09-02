#region Copyright information
// <copyright file="ObjectDependencyManager.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     https://github.com/XAMLMarkupExtensions/XAMLMarkupExtensions/blob/master/LICENSE
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace XAMLMarkupExtensions.Base
{
    #region Uses
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    #endregion

    /// <summary>
    /// This class ensures, that a specific object lives as long a associated object is alive.
    /// Based on: <see href="https://github.com/SeriousM/WPFLocalizationExtension"/>
    /// </summary>
    public static class ObjectDependencyManager
    {
        /// <summary>
        /// This member holds the list of all <see cref="WeakReference"/>s and their appropriate objects.
        /// </summary>
        private static readonly Dictionary<object, HashSet<WeakReference>> internalList;

        /// <summary>
        /// Initializes static members of the <see cref="ObjectDependencyManager"/> class.
        /// Static Constructor. Creates a new instance of
        /// Dictionary(object, <see cref="WeakReference"/>) and set it to the <see cref="internalList"/>.
        /// </summary>
        static ObjectDependencyManager()
        {
            internalList = new Dictionary<object, HashSet<WeakReference>>();
        }

        /// <summary>
        /// This method adds a new object dependency
        /// </summary>
        /// <param name="weakRefDp">The <see cref="WeakReference"/>, which ensures the live cycle of <paramref name="objToHold"/></param>
        /// <param name="objToHold">The object, which should stay alive as long <paramref name="weakRefDp"/> is alive</param>
        /// <returns>
        /// true, if the binding was successfully, otherwise false
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="objToHold"/> cannot be null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="objToHold"/> cannot be type of <see cref="WeakReference"/>
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The <see cref="WeakReference"/>.Target cannot be the same as <paramref name="objToHold"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool AddObjectDependency(WeakReference weakRefDp, object objToHold)
        {
            // run the clean up to ensure that only objects are watched they are realy still alive
            CleanUp();

            // if the objToHold is null, we cannot handle this afterwards.
            if (objToHold == null)
            {
                throw new ArgumentNullException(nameof(objToHold), "The objToHold cannot be null");
            }

            // if the objToHold is a weakreference, we cannot handle this type afterwards.
            if (objToHold.GetType() == typeof(WeakReference))
            {
                throw new ArgumentException("objToHold cannot be type of WeakReference", nameof(objToHold));
            }

            // if the target of the weakreference is the objToHold, this would be a cycling play.
            if (weakRefDp.Target == objToHold)
            {
                throw new InvalidOperationException("The WeakReference.Target cannot be the same as objToHold");
            }

            // holds the status of registration of the object dependency
            bool itemRegistered = false;

            // check if the objToHold is contained in the internalList.
            if (!internalList.ContainsKey(objToHold))
            {
                // add the objToHold to the internal list.
                HashSet<WeakReference> lst = new HashSet<WeakReference> { weakRefDp };

                internalList.Add(objToHold, lst);

                itemRegistered = true;
            }
            else
            {
                // otherweise, check if the weakRefDp exists and add it if necessary
                HashSet<WeakReference> references = internalList[objToHold];
                if (!references.Contains(weakRefDp))
                {
                    references.Add(weakRefDp);

                    itemRegistered = true;
                }
            }

            // return the status of the registration
            return itemRegistered;
        }

        /// <summary>
        /// This method cleans up all independent (!<see cref="WeakReference"/>.IsAlive) objects.
        /// </summary>
        public static void CleanUp()
        {
            // call the overloaded method
            CleanUp(null);
        }

        /// <summary>
        /// This method cleans up all independent (!<see cref="WeakReference"/>.IsAlive) objects or a single object.
        /// </summary>
        /// <param name="objToRemove">
        /// If defined, the associated object dependency will be removed instead of a full CleanUp
        /// </param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void CleanUp(object objToRemove)
        {
            // if a particular object is passed, remove it.
            if (objToRemove != null)
            {
                // if the key wasnt found, throw an exception.
                if (!internalList.Remove(objToRemove))
                {
                    throw new Exception("Key was not found!");
                }

                // stop here
                return;
            }

            // perform an full clean up

            // this list will hold all keys they has to be removed
            List<object> keysToRemove = new List<object>();

            // This list will hold all references which should be removed.
            // It's one for all keys for reduce memory allocations.
            List<WeakReference> deadReferences = new List<WeakReference>();

            // step through all object dependenies
            foreach (KeyValuePair<object, HashSet<WeakReference>> kvp in internalList)
            {
                foreach (var target in kvp.Value)
                {
                    var targetReference = target.Target;
                    if (targetReference == null)
                        deadReferences.Add(target);
                }

                if (deadReferences.Count > 0)
                {
                    // if the all of weak references is empty, remove the whole entry
                    if (deadReferences.Count == kvp.Value.Count)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                    else
                    {
                        foreach (var deadReference in deadReferences)
                        {
                            kvp.Value.Remove(deadReference);
                        }
                    }
                }

                deadReferences.Clear();
            }

            // remove the key from the internalList
            foreach (var keyToRemove in keysToRemove)
            {
                internalList.Remove(keyToRemove);
            }
        }
    }
}