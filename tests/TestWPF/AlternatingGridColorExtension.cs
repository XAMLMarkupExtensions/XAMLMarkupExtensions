namespace TestWPF
{    
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System;
    using System.Reflection; 

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
                targetPropertyType = ((DependencyProperty)service.TargetProperty).PropertyType;
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
