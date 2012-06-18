namespace TestSL
{
    using XAMLMarkupExtensions.Base;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System;
    using System.Reflection;

    public class AlternatingGridColorExtension : NestedMarkupExtension
    {
        public Color ColorEven { get; set; }
        public Color ColorOdd { get; set; }

        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            // Check the correct type.
            if (!typeof(Brush).IsAssignableFrom(info.TargetPropertyType))
                return null;

            // Create the default (even) brush.
            SolidColorBrush brush = new SolidColorBrush(ColorEven);

            // Check if we got an endpoint and if it is also a DependencyObject.
            if (endPoint != null && endPoint.IsDependencyObject)
            {
                var depObject = (DependencyObject)endPoint.TargetObject;
                var row = (int)depObject.GetValue(Grid.RowProperty);

                // Check the odd row case and alter the brush.
                if (row % 2 != 0)
                    brush.Color = ColorOdd;
            }

            return brush;
        }

        protected override bool UpdateOnEndpoint(TargetInfo endpoint)
        {
            return true;
        }

        public AlternatingGridColorExtension()
        {
        }
    }

    public class AlternatingGridColorSimpleExtension : MarkupExtension
    {
        public Color ColorEven { get; set; }
        public Color ColorOdd { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Prüfung von serviceProvider
            if (serviceProvider == null)
                return this;

            // Dienst vom Typ IProvideValueTarget anfordern
            IProvideValueTarget service = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            // Prüfung des Dienstes
            if (service == null)
                return this;

            // Den Typ der Zieleigenschaft ermitteln
            Type targetPropertyType = null;
            
            if (service.TargetProperty is DependencyProperty)
                // Dirty reflection hack - get the property type (property not included in the SL DependencyProperty class) from the internal declared field.
                targetPropertyType = typeof(DependencyProperty).GetField("_propertyType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue((DependencyProperty)service.TargetProperty) as Type;
            else if (service.TargetProperty is PropertyInfo)
                targetPropertyType = ((PropertyInfo)service.TargetProperty).PropertyType;
            else
                return this;

            // Prüfung des Typs
            if (!typeof(Brush).IsAssignableFrom(targetPropertyType))
                return null;

            // Einen Standard-Brush erzeugen
            SolidColorBrush brush = new SolidColorBrush(ColorEven);

            // Prüfung auf ein DependencyObject als Ziel -> Grid.Row auslesen
            if (service.TargetObject is DependencyObject)
            {
                var depObject = (DependencyObject)service.TargetObject;
                var row = (int)depObject.GetValue(Grid.RowProperty);

                // Im Fall einer ungeraden Zahl eine andere Farbe wählen
                if (row % 2 != 0)
                    brush.Color = ColorOdd;
            }

            return brush;
        }

        public AlternatingGridColorSimpleExtension()
        {
        }
    }
}
