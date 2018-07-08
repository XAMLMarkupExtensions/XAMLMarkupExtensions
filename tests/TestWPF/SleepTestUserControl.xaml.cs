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
    /// Interaction logic for SleepTestUserControl.xaml
    /// </summary>
    public partial class SleepTestUserControl : UserControl
    {
        private bool loaded;
        private Stopwatch stopWatch;

        public SleepTestUserControl()
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
