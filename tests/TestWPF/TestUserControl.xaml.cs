using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestWPF
{
    /// <summary>
    /// Interaction logic for TestUserControl.xaml
    /// </summary>
    public partial class TestUserControl : UserControl
    {
        public TestUserControl()
        {
            InitializeComponent();
        }

        private void CreateNewSleepTestControlButton_OnClick(object sender, RoutedEventArgs e)
        {
            SleepTestStackPanel.Children.Add(new SleepTestUserControl());
        }

        private void RemoveLastSleepTestControlButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (SleepTestStackPanel.Children.Count > 0)
            {
                SleepTestStackPanel.Children.RemoveAt(SleepTestStackPanel.Children.Count - 1);
            }
        }
    }
}
