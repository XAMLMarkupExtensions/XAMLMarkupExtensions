#region Copyright information
// <copyright file="BindingProxy.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://xamlmarkupextensions.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace XAMLMarkupExtensions.Binding
{
    #region Uses
    using System;
    using System.Windows;
    using System.Windows.Data;
    using XAMLMarkupExtensions.Base;
    #endregion

    /// <summary>
    /// A binding proxy class that accepts bindings and forwards them to a normal property.
    /// Based on: <see cref="http://www.codeproject.com/Articles/71348/Binding-on-a-Property-which-is-not-a-DependencyPro"/> and
    /// </summary>
    public class BindingProxy : FrameworkElement
    {
        // TODO: Test and develop SilverLight compatibility.
        
        #region Source DP
        // We don't know what will be the Source/target type so we keep 'object'.
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(BindingProxy),
#if SILVERLIGHT
            null);
#else
            new FrameworkPropertyMetadata()
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });
#endif

        public Object Source
        {
            get { return GetValue(BindingProxy.SourceProperty); }
            set { SetValue(BindingProxy.SourceProperty, value); }
        }
        #endregion

        #region Target
        private TargetInfo target = null;
        public TargetInfo Target
        {
            get { return target; }
            set
            {
                target = value;
                if ((target != null) && (this.Source != null))
                    NestedMarkupExtension.SetPropertyValue(this.Source, target, false);
            }
        }
        #endregion

        #region OnPropertyChanged
#if SILVERLIGHT
#else
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == BindingProxy.SourceProperty.Name)
            {
                if (!object.ReferenceEquals(this.Source, target) && (target != null))
                    NestedMarkupExtension.SetPropertyValue(this.Source, target, false);
            }
        }
#endif
        #endregion

        public object TargetValue
        {
            get { return NestedMarkupExtension.GetPropertyValue(target); }
        }
    }
}
