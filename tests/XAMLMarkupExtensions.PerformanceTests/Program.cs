namespace XAMLMarkupExtensions.PerformanceTests
{
    #region Uses
    using BenchmarkDotNet.Running;
    using Tests;
    #endregion

    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ProvideValueTests>();
            //var summary = BenchmarkRunner.Run<CleanupTest>();
        }
    }
}
