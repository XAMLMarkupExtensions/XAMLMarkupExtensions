namespace XAMLMarkupExtensions.PerformanceTests.TestEntities
{
    #region Uses
    using Base;
    #endregion

    /// <summary>
    /// Test extension which implements NestedMarkupExtension.
    /// </summary>
    public class TestExtension : NestedMarkupExtension
    {
        /// <inheritdoc />
        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            return string.Empty;
        }

        /// <inheritdoc />
        protected override bool UpdateOnEndpoint(TargetInfo endpoint)
        {
            return true;
        }
    }
}
