#region Copyright information
// <copyright file="DynBindingExtension.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://xamlmarkupextensions.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace XAMLMarkupExtensions.Binding
{
    #region Uses
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;
    using XAMLMarkupExtensions.Base;
    #endregion

    /// <summary>
    /// A dynamic binding extension.
    /// </summary>
    [MarkupExtensionReturnType(typeof(BindingExpression))]
    [ContentProperty("Path")]
    public class DynBindingExtension : NestedMarkupExtension, INotifyPropertyChanged
    {
        #region PropertyChanged Logic
        /// <summary>
        /// Informiert über sich ändernde Eigenschaften.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify that a property has changed
        /// </summary>
        /// <param name="property">
        /// The property that changed
        /// </param>
        internal void OnNotifyPropertyChanged(string property)
        {
            UpdateNewValue();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion

        #region Variables and Properties
        private object source = null;
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public object Source
        {
            get { return source; }
            set
            {
                source = value;
                OnNotifyPropertyChanged(nameof(Source));
            }
        }

        private string path = null;
        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                OnNotifyPropertyChanged(nameof(Path));
            }
        }

        private IValueConverter converter = null;
        /// <summary>
        /// Gets or sets the converter.
        /// </summary>
        public IValueConverter Converter
        {
            get { return converter; }
            set
            {
                converter = value;
                OnNotifyPropertyChanged(nameof(Converter));
            }
        }

        private object converterParameter = null;
        /// <summary>
        /// Gets or sets the converter parameter.
        /// </summary>
        public object ConverterParameter
        {
            get { return converterParameter; }
            set
            {
                converterParameter = value;
                OnNotifyPropertyChanged(nameof(ConverterParameter));                
            }
        }
        #endregion

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public DynBindingExtension()
        {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="path">The binding path.</param>
        public DynBindingExtension(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// This function returns the properly prepared output of the markup extension.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <param name="endPoint">Information about the endpoint.</param>
        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            //if (!info.IsDependencyObject)
            //    throw new NotSupportedException("A Binding can only be applied to a DependencyObject!");

            if (endPoint == null)
                return null;

            DependencyObject obj = (DependencyObject)endPoint.TargetObject;
            Binding binding = new Binding();
            object src = source;

            if ((src == null) || ((src is string) && (".".CompareTo(src) == 0)))
            {
                if (obj is FrameworkElement)
                    src = ((FrameworkElement)obj).DataContext;
                else if (obj is FrameworkContentElement)
                    src = ((FrameworkContentElement)obj).DataContext;

                if (src == null)
                    return null;
                //throw new ArgumentNullException("Neither a source was specified, nor it could be retrieved from the DataContext!");
            }

            if (String.IsNullOrEmpty(path) || String.IsNullOrEmpty(path.Trim()))
                return null;
            //throw new ArgumentNullException("No path was specified!");

            binding.Source = src;
            binding.Path = new PropertyPath(path);
            binding.Converter = converter;
            binding.ConverterParameter = converterParameter;

            object ret = null;

            if (info.IsDependencyObject)
                ret = binding.ProvideValue(new SimpleProvideValueServiceProvider(endPoint.TargetObject, endPoint.TargetProperty, endPoint.TargetPropertyType, endPoint.TargetPropertyIndex));
            else
            {
                BindingProxy proxy = new BindingProxy();
                proxy.Source = binding.ProvideValue(new SimpleProvideValueServiceProvider(proxy, BindingProxy.SourceProperty, null, -1));
                proxy.Target = info;
                ret = proxy.TargetValue;
            }

            return ret;
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
    }
}
