namespace TestWPF
{
    using XAMLMarkupExtensions.Base;
    using System.Windows.Media;

    public class AddAlphaExtension : NestedMarkupExtension
    {
        public byte Alpha { get; set; }
        public SolidColorBrush Brush { get; set; }

        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
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

        protected override bool WillUpdateOnEndpoint
        {
            get
            {
                return false;
            }
        }

        public AddAlphaExtension()
        {
        }
    }
}
