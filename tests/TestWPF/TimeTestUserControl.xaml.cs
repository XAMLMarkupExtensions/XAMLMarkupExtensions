using System;
using System.Diagnostics;
using System.Windows;

namespace TestWPF
{
    /// <summary>
    /// Interaction logic for TimeTestUserControl.xaml
    /// </summary>
    public partial class TimeTestUserControl
    {
        private bool loaded;
        private readonly Stopwatch stopWatch;

        public TimeTestUserControl()
        {
            stopWatch = Stopwatch.StartNew();
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!loaded)
            {
                loaded = true;
                stopWatch.Stop();
                TimeTest.Content = $"{stopWatch.ElapsedMilliseconds} ms";
            }
        }
    }
}