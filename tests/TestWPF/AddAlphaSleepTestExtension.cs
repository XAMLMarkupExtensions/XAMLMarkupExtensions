namespace TestWPF
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Media;
    using XAMLMarkupExtensions.Base;

    public class AddAlphaSleepTestExtension : NestedMarkupExtension
    {
        public AddAlphaSleepTestExtension()
        {
            SleepTime = 100;
        }

        public byte Alpha
        {
            get;
            set;
        }

        public SolidColorBrush Brush
        {
            get;
            set;
        }

        public int SleepTime
        {
            get;
            set;
        }

        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            System.Threading.Thread.Sleep(SleepTime);

            // Check the correct type.
            if (!typeof(Brush).IsAssignableFrom(info.TargetPropertyType))
                return null;

            if (Brush == null)
                return null;

            var color = Brush.Color;
            color.A = Alpha;
            Brush.Color = color;

            return Brush;
        }

        protected override bool UpdateOnEndpoint(TargetInfo endpoint)
        {
            return false;
        }
    }
}
