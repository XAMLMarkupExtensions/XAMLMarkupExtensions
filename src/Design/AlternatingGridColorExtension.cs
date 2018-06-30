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

        /// <summary>
        /// This function returns the properly prepared output of the markup extension.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <param name="endPoint">Information about the endpoint.</param>
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

        /// <summary>
        /// This method must return true, if an update shall be executed when the given endpoint is reached.
        /// This method is called each time an endpoint is reached.
        /// </summary>
        /// <param name="endpoint">Information on the specific endpoint.</param>
        /// <returns>True, if an update of the path to this endpoint shall be performed.</returns>
        protected override bool UpdateOnEndpoint(TargetInfo endpoint)
        {
            return true;
        }

        /// <summary>
        /// This property must return true, if the markup extension wants to update at all if an endpoint is reached.
        /// </summary>
        /// <returns>True, if the markup extension wants to update at all if an endpoint is reached.</returns>
        protected override bool WillUpdateOnEndpoint
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public AlternatingGridColorExtension()
        {
        }
    }
}
