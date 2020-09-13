using XAMLMarkupExtensions.Base;

namespace XAMLMarkupExtensions.PerformanceTests.Tests
{
    #region Uses
    using System;
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using TestEntities;
    #endregion

    [MemoryDiagnoser]
    public class CleanupTest
    {
        private Dictionary<TestExtension, List<TestTargetObject>> _extensions;

        /// <summary>
        /// Count of created extensions.
        /// </summary>
        [Params(100)]
        public int ExtensionsCount;

        /// <summary>
        /// Count of targets for each extension.
        /// </summary>
        [Params(100)]
        public int ExtensionTargetsCount;

        [Benchmark]
        public void ReferencesCleanup()
        {
            // Remove half of dependencies.
            foreach (var targetObjects in _extensions.Values)
            {
                var middle = targetObjects.Count / 2;
                targetObjects.RemoveRange(middle, targetObjects.Count - middle);
            }

            // Force GC, so part of weak references will be dead.
            GC.Collect(2, GCCollectionMode.Forced);

            // Create new extension and provide value. Inside dead weak references will be removed.
            var extension = new TestExtension();
            var target = new TestTargetObject();
            var serviceProvider = new TargetProvider(target, TestTargetObject.ValueProperty);
            extension.ProvideValue(serviceProvider);
        }

        /// <summary>
        /// Fill internal list of listeners.
        /// </summary>
        [IterationSetup]
        public void Setup()
        {
            _extensions = new Dictionary<TestExtension, List<TestTargetObject>>();
            for (int extensionIndex = 0; extensionIndex < ExtensionsCount; extensionIndex++)
            {
                var extension = new TestExtension();
                _extensions.Add(extension, new List<TestTargetObject>());

                for (int targetIndex = 0; targetIndex < ExtensionTargetsCount; targetIndex++)
                {
                    var target = new TestTargetObject();
                    var serviceProvider = new TargetProvider(target, TestTargetObject.ValueProperty);
                    extension.ProvideValue(serviceProvider);

                    _extensions[extension].Add(target);
                }
            }
        }

        /// <summary>
        /// Remove all extensions from internal list.
        /// </summary>
        [IterationCleanup]
        public void Cleanup()
        {
            foreach (var (extension, _) in _extensions)
            {
                extension.Dispose();
                
                // TODO Remove after correct dispose realization.
                ObjectDependencyManager.CleanUp(extension);
            }

            _extensions.Clear();
        }
    }
}
