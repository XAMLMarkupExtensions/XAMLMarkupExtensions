namespace TestWPF
{
    #region Usings
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System;
    using System.Reflection;  
    #endregion

    public class AlternatingGridColorSimpleExtension : MarkupExtension
    {
        #region Properties
        public Color ColorEven { get; set; }
        public Color ColorOdd { get; set; }
        #endregion

        #region Construtor
        public AlternatingGridColorSimpleExtension()
        {
        }
        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // request IProvideValueTarget and check the service
            if (serviceProvider?.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service)
            {
                // get type of targetProperty
                Type targetPropertyType;

                if (service.TargetProperty is DependencyProperty dp)
                    targetPropertyType = dp.PropertyType;
                else if (service.TargetProperty is PropertyInfo pi)
                    targetPropertyType = pi.PropertyType;
                else
                    return this;

                // check the type
                if (!typeof(Brush).IsAssignableFrom(targetPropertyType))
                    return null;

                // generate default brush
                SolidColorBrush brush = new SolidColorBrush(ColorEven);

                // check if DependencyObject as target -> read Grid.Row
                if (service.TargetObject is DependencyObject depObject)
                {
                    var row = (int)depObject.GetValue(Grid.RowProperty);

                    // if odd change color
                    if (row % 2 != 0)
                        brush.Color = ColorOdd;
                }

                return brush;
            }

            return this;
        }
    }
}
