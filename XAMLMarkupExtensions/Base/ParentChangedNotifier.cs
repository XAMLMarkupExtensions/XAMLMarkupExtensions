namespace XAMLMarkupExtensions.Base
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Collections.Generic;
    
    /// <summary>
    /// A class that helps listening to changes on the Parent property of FrameworkElement objects.
    /// </summary>
    public class ParentChangedNotifier : DependencyObject
    {
        #region Parent property
        /// <summary>
        /// An attached property that will take over control of change notification.
        /// </summary>
        public static DependencyProperty ParentProperty = DependencyProperty.RegisterAttached("Parent", typeof(FrameworkElement), typeof(ParentChangedNotifier), new PropertyMetadata(ParentChanged));

        /// <summary>
        /// Get method for the attached property.
        /// </summary>
        /// <param name="element">The target FrameworkElement object.</param>
        /// <returns>The target's parent FrameworkElement object.</returns>
        public static FrameworkElement GetParent(FrameworkElement element)
        {
            return (FrameworkElement)element.GetValue(ParentProperty);
        }

        /// <summary>
        /// Set method for the attached property.
        /// </summary>
        /// <param name="element">The target FrameworkElement object.</param>
        /// <param name="value">The target's parent FrameworkElement object.</param>
        public static void SetParent(FrameworkElement element, FrameworkElement value)
        {
            element.SetValue(ParentProperty, value);
        } 
        #endregion

        #region ParentChanged callback
        /// <summary>
        /// The callback for changes of the attached Parent property.
        /// </summary>
        /// <param name="obj">The sender.</param>
        /// <param name="args">The argument.</param>
        private static void ParentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var notifier = obj as FrameworkElement;

            if (notifier != null && OnParentChangedList.ContainsKey(notifier))
                foreach (var OnParentChanged in OnParentChangedList[notifier])
                    OnParentChanged();
        } 
        #endregion

        /// <summary>
        /// A static list of actions that should be performed on parent change events.
        /// - Entries are added by each call of the constructor.
        /// - All elements are called by the parent changed callback with the particular sender as the key.
        /// </summary>
        private static Dictionary<FrameworkElement, List<Action>> OnParentChangedList = new Dictionary<FrameworkElement, List<Action>>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="element">The element whose Parent property should be listened to.</param>
        /// <param name="onParentChanged">The action that will be performed upon change events.</param>
        public ParentChangedNotifier(FrameworkElement element, Action onParentChanged)
        {
            if (onParentChanged != null)
            {
                if (!OnParentChangedList.ContainsKey(element))
                    OnParentChangedList.Add(element, new List<Action>());

                OnParentChangedList[element].Add(onParentChanged);
            }
            
            Binding b = new Binding("Parent");
            b.RelativeSource = new RelativeSource();
            b.RelativeSource.Mode = RelativeSourceMode.FindAncestor;
            b.RelativeSource.AncestorType = typeof(FrameworkElement);

            BindingOperations.SetBinding(element, ParentProperty, b);
        }
    }
}
