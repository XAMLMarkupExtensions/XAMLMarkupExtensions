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
    /// Based on: <see href="http://www.codeproject.com/Articles/71348/Binding-on-a-Property-which-is-not-a-DependencyPro"/> and
    /// </summary>
    public class BindingProxy : FrameworkElement
    {
        #region Source DP
        /// <summary>
        /// The source dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(BindingProxy),
            new FrameworkPropertyMetadata()
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });

        /// <summary>
        /// Gets or sets the binding source.
        /// </summary>
        public Object Source
        {
            get { return this.GetValueSync<object>(BindingProxy.SourceProperty); }
            set { this.SetValueSync(BindingProxy.SourceProperty, value); }
        }
        #endregion

        #region Target
        private TargetInfo target = null;
        /// <summary>
        /// Gets or sets the target.
        /// </summary>
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
        /// <summary>
        /// Gets called, when a property changed.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == BindingProxy.SourceProperty.Name)
            {
                if (!object.ReferenceEquals(this.Source, target) && (target != null))
                    NestedMarkupExtension.SetPropertyValue(this.Source, target, false);
            }
        }
        #endregion

        /// <summary>
        /// Gets the target value.
        /// </summary>
        public object TargetValue
        {
            get { return NestedMarkupExtension.GetPropertyValue(target); }
        }
    }
}
