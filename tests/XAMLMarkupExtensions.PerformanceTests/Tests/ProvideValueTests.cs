namespace XAMLMarkupExtensions.PerformanceTests.Tests
{
    #region Uses
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using TestEntities;
    #endregion

    [MemoryDiagnoser]
    public class ProvideValueTests
    {
        private List<TestExtension> _extensions;
        private List<TestTargetObject> _targets;

        /// <summary>
        /// Count of created extensions.
        /// </summary>
        [Params(10000, 100)]
        public int ExtensionsCount;

        /// <summary>
        /// Count of targets for each extension.
        /// </summary>
        public int ExtensionTargetsCount;

        /// <summary>
        /// Count of total targets for all extensions.
        /// </summary>
        [Params(10000)]
        public int TotalExtensionTargetsCount;

        [Benchmark]
        public void ProvideValue()
        {
            for (int extensionIndex = 0; extensionIndex < ExtensionsCount; extensionIndex++)
            {
                var extension = new TestExtension();
                _extensions.Add(extension);

                for (int targetIndex = 0; targetIndex < ExtensionTargetsCount; targetIndex++)
                {
                    var target = new TestTargetObject();
                    var serviceProvider = new TargetProvider(target, TestTargetObject.ValueProperty);
                    extension.ProvideValue(serviceProvider);
                    _targets.Add(target);
                }
            }
        }

        /// <summary>
        /// Initialize lists.
        /// </summary>
        [IterationSetup]
        public void Setup()
        {
            ExtensionTargetsCount = TotalExtensionTargetsCount / ExtensionsCount;

            _extensions = new List<TestExtension>(ExtensionsCount);
            _targets = new List<TestTargetObject>(TotalExtensionTargetsCount);
        }

        /// <summary>
        /// Remove all extensions from internal list.
        /// </summary>
        [IterationCleanup]
        public void Cleanup()
        {
            foreach (var extension in _extensions)
            {
                extension.Dispose();
            }

            _extensions.Clear();
            _targets.Clear();
        }
    }
}
