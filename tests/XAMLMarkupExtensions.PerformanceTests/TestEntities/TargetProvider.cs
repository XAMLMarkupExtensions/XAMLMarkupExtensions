namespace XAMLMarkupExtensions.PerformanceTests.TestEntities
{
    #region Uses
    using System;
    using System.Windows.Markup;
    #endregion

    /// <summary>
    /// Class can pass as <see cref="IServiceProvider" /> to <see cref="MarkupExtension.ProvideValue" /> method.
    /// </summary>
    public class TargetProvider : IServiceProvider, IProvideValueTarget
    {
        /// <inheritdoc />
        public object TargetObject { get; }

        /// <inheritdoc />
        public object TargetProperty { get; }

        /// <summary>
        /// Create <see cref="TargetProvider" /> instance with specified target and property.
        /// </summary>
        public TargetProvider(object targetObject, object targetProperty)
        {
            TargetObject = targetObject;
            TargetProperty = targetProperty;
        }

        /// <inheritdoc />
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IProvideValueTarget))
                return this;

            return null;
        }
    }
}
