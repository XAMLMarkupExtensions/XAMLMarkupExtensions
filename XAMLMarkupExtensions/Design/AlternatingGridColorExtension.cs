#region Copyright information
// <copyright file="AlternatingGridColorExtension.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://xamlmarkupextensions.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace XAMLMarkupExtensions.Design
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using XAMLMarkupExtensions.Base;

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
}
