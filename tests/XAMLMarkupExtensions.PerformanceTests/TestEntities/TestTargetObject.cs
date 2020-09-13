namespace XAMLMarkupExtensions.PerformanceTests.TestEntities
{
    #region Uses
    using System.Windows;
    #endregion

    /// <summary>
    /// Simple dependency object.
    /// </summary>
    public class TestTargetObject : DependencyObject
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(string), typeof(TestTargetObject), new PropertyMetadata(default(string)));

        public string Value
        {
            get => (string) GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
    }
}
