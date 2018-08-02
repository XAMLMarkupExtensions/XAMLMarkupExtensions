using System;
using System.Diagnostics;

namespace TestWPF
{
    /// <summary>
    /// Interaction logic for TimeTestUserControl.xaml
    /// </summary>
    public partial class TimeTestUserControl
    {
        private bool loaded;
        private Stopwatch stopWatch;

        public TimeTestUserControl()
        {
            stopWatch = Stopwatch.StartNew();
            InitializeComponent();
        }

        private void SleepTestUserControl_OnLayoutUpdated(object sender, EventArgs e)
        {
            if (!loaded && (ActualHeight > 0 || ActualWidth > 0))
            {
                loaded = true;
                stopWatch.Stop();
                TimeTest.Content = $"{stopWatch.ElapsedMilliseconds} ms";
            }
        }
    }
}
