using System.Windows;

namespace TestWPF
{
    /// <summary>
    /// Interaction logic for TestUserControl.xaml
    /// </summary>
    public partial class TestUserControl
    {
        public TestUserControl()
        {
            InitializeComponent();
        }

        private void CreateNewTimeTestUserControlButton_OnClick(object sender, RoutedEventArgs e)
        {
            TimeTestStackPanel.Children.Insert(0, new TimeTestUserControl());
        }

        private void CreateTenNewTimeTestUserControlsButton_OnClick(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < 10; i++)
            {
                TimeTestStackPanel.Children.Insert(0, new TimeTestUserControl());
            }
        }

        private void RemoveLastAddedTimeTestUserControlButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (TimeTestStackPanel.Children.Count > 0)
            {
                TimeTestStackPanel.Children.RemoveAt(0);
            }
        }

        private void RemoveAllTimeTestUserControlsButton_OnClick(object sender, RoutedEventArgs e)
        {
            TimeTestStackPanel.Children.Clear();
        }

        private void CreateNewMainWindowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
