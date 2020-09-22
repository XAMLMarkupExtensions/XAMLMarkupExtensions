namespace XAMLMarkupExtensions.PerformanceTests.Tests
{
    #region Uses
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using Base;
    using TestEntities;
    #endregion

    [MemoryDiagnoser]
    public class ProvideValueTests
    {
        private List<TestExtension> _extensions;
        private List<TestTargetObject> _targets;

        /// <summary>
        /// Test <see cref="NestedMarkupExtension.ProvideValue" /> method.
        /// </summary>
        /// <param name="extensionsCount">Count of created extensions.</param>
        /// <param name="extensionTargetsCount">Count of targets for each extension.</param>
        [Benchmark]
        [Arguments(3000, 1)]
        [Arguments(50, 100)]
        public void ProvideValue(int extensionsCount, int extensionTargetsCount)
        {
            Setup(extensionsCount, extensionTargetsCount);

            for (int extensionIndex = 0; extensionIndex < extensionsCount; extensionIndex++)
            {
                var extension = new TestExtension();
                _extensions.Add(extension);

                for (int targetIndex = 0; targetIndex < extensionTargetsCount; targetIndex++)
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
        public void Setup(int extensionsCount, int extensionTargetsCount)
        {
            _extensions = new List<TestExtension>(extensionsCount);
            _targets = new List<TestTargetObject>(extensionsCount * extensionTargetsCount);
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
