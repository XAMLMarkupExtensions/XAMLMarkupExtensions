#region Copyright information
// <copyright file="SimpleProvideValueServiceProvider.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://xamlmarkupextensions.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace XAMLMarkupExtensions.Base
{
    #region Usings
    using System;
    using System.Windows.Markup;
    #endregion

    /// <summary>
    /// This class implements the interfaces IServiceProvider and IProvideValueTarget for ProvideValue calls on markup extensions.
    /// </summary>
    public class SimpleProvideValueServiceProvider : IServiceProvider, IProvideValueTarget
    {
        /// <summary>
        /// Return the requested service.
        /// </summary>
        /// <param name="service">The type of the service.</param>
        /// <returns>The service (this, if service ist IProvideValueTarget, otherwise null).</returns>
        public object GetService(Type service)
        {
            // This class only implements the IProvideValueTarget service.
            if (service == typeof(IProvideValueTarget))
                return this;
            else
                return null;
        }

        /// <summary>
        /// The target object.
        /// </summary>
        public object TargetObject { get; private set; }

        /// <summary>
        /// The target property.
        /// </summary>
        public object TargetProperty { get; private set; }

        /// <summary>
        /// The target property type.
        /// </summary>
        public Type TargetPropertyType { get; private set; }

        /// <summary>
        /// The target property index.
        /// </summary>
        public int TargetPropertyIndex { get; private set; }

        /// <summary>
        /// An optional endpoint information.
        /// </summary>
        public TargetInfo EndPoint { get; private set; }

        /// <summary>
        /// Create a new instance of a SimpleProvideValueServiceProvider.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="targetPropertyType">The target property type.</param>
        /// <param name="targetPropertyIndex">The target property index.</param>
        public SimpleProvideValueServiceProvider(object targetObject, object targetProperty, Type targetPropertyType, int targetPropertyIndex)
        {
            TargetObject = targetObject;
            TargetProperty = targetProperty;
            TargetPropertyType = targetPropertyType;
            TargetPropertyIndex = targetPropertyIndex;
        }

        /// <summary>
        /// Create a new instance of a SimpleProvideValueServiceProvider.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="targetPropertyType">The target property type.</param>
        /// <param name="targetPropertyIndex">The target property index.</param>
        /// <param name="endPoint">An optional endpoint information.</param>
        public SimpleProvideValueServiceProvider(object targetObject, object targetProperty, Type targetPropertyType, int targetPropertyIndex, TargetInfo endPoint)
        {
            TargetObject = targetObject;
            TargetProperty = targetProperty;
            TargetPropertyType = targetPropertyType;
            TargetPropertyIndex = targetPropertyIndex;
            EndPoint = endPoint;
        }

        /// <summary>
        /// Create a new instance of a SimpleProvideValueServiceProvider.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        public SimpleProvideValueServiceProvider(TargetInfo info)
        {
            TargetObject = info.TargetObject;
            TargetProperty = info.TargetProperty;
            TargetPropertyType = info.TargetPropertyType;
            TargetPropertyIndex = info.TargetPropertyIndex;
        }

        /// <summary>
        /// Create a new instance of a SimpleProvideValueServiceProvider.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <param name="endPoint">An optional endpoint information.</param>
        public SimpleProvideValueServiceProvider(TargetInfo info, TargetInfo endPoint)
        {
            TargetObject = info.TargetObject;
            TargetProperty = info.TargetProperty;
            TargetPropertyType = info.TargetPropertyType;
            TargetPropertyIndex = info.TargetPropertyIndex;
            EndPoint = endPoint;
        }
    }
}
