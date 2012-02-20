namespace XAMLMarkupExtensions.Binding
{
    #region Uses
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using XAMLMarkupExtensions.Base;
    using System.Windows.Markup;
    using System.Windows.Data;
    using System.ComponentModel;
    using System.Windows;
    #endregion

#if SILVERLIGHT
#else
    [MarkupExtensionReturnType(typeof(BindingExpression))]
#endif
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

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        #endregion

        #region Variables and Properties
        private object source = null;
        public object Source
        {
            get { return source; }
            set { source = value; OnNotifyPropertyChanged("Source"); }
        }

        private string path = null;
        public string Path
        {
            get { return path; }
            set { path = value; OnNotifyPropertyChanged("Path"); }
        }

        private IValueConverter converter = null;
        public IValueConverter Converter
        {
            get { return converter; }
            set { converter = value; OnNotifyPropertyChanged("Converter"); }
        }

        private object converterParameter = null;
        public object ConverterParameter
        {
            get { return converterParameter; }
            set { converterParameter = value; OnNotifyPropertyChanged("ConverterParameter"); }
        } 
        #endregion

        public DynBindingExtension()
        {
        }

        public DynBindingExtension(string path)
        {
            this.path = path;
        }

        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            //if (!info.IsDependencyObject)
            //    throw new NotSupportedException("A Binding can only be applied to a DependencyObject!");

            if (endPoint == null)
                return null;

            DependencyObject obj = (DependencyObject)endPoint.TargetObject;
            Binding binding = new Binding();
            object src = source;

            if ((src == null) || (".".CompareTo(src) == 0))
            {
                if (obj is FrameworkElement)
                    src = ((FrameworkElement)obj).DataContext;
#if SILVERLIGHT
#else
                else if (obj is FrameworkContentElement)
                    src = ((FrameworkContentElement)obj).DataContext;
#endif

                if (src == null)
                    return null;
                    //throw new ArgumentNullException("Neither a source was specified, nor it could be retrieved from the DataContext!");
            }

            if (String.IsNullOrWhiteSpace(path))
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

        protected override bool UpdateOnEndpoint(TargetInfo endpoint)
        {
            return true;
        }
    }
}
