#region Copyright information
// <copyright file="AlternatingGridColorExtension.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     https://github.com/XAMLMarkupExtensions/XAMLMarkupExtensions/blob/master/LICENSE
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace XAMLMarkupExtensions.Design
{
    #region Usings
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using XAMLMarkupExtensions.Base;
    #endregion

    /// <summary>
    /// A design extension with alternating grid colors.
    /// </summary>
    public class AlternatingGridColorExtension : NestedMarkupExtension
    {
        /// <summary>
        /// Gets or sets the color for even rows.
        /// </summary>
        public Color ColorEven { get; set; }

        /// <summary>
        /// Gets or sets the color for odd rows.
        /// </summary>
        public Color ColorOdd { get; set; }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override bool UpdateOnEndpoint(TargetInfo endpoint)
        {
            return true;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public AlternatingGridColorExtension()
        {
        }
    }
}
