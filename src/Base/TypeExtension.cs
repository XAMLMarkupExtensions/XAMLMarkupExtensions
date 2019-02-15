#region Copyright information
// <copyright file="TypeExtension.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://xamlmarkupextensions.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace XAMLMarkupExtensions.Base
{
    #region Usings
    using System;
    using System.ComponentModel;
    using System.Windows.Markup;
    #endregion

    /// <summary>
    /// A type extension.
    /// <para>Adopted from the work of Henrik Jonsson: http://www.codeproject.com/Articles/305932/Static-and-Type-markup-extensions-for-Silverlight, Licensed under Code Project Open License (CPOL).</para>
    /// </summary>
    [MarkupExtensionReturnType(typeof(Type))]
    [ContentProperty("Type"), DefaultProperty("Type")]
    public class TypeExtension : NestedMarkupExtension, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises a new <see cref="E:INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Properties
        private Type type = null;
        /// <summary>
        /// The resolved type.
        /// </summary>
        public Type Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;
                    RaisePropertyChanged(nameof(Type));
                }
            }
        }

        private string typeName = "";
        /// <summary>
        /// The type name.
        /// </summary>
        public string TypeName
        {
            get { return typeName; }
            set
            {
                if (typeName != value)
                {
                    typeName = value;

                    if (type != null)
                    {
                        try
                        {
                            this.Type = System.Type.GetType(typeName, false);
                        }
                        catch
                        {
                            this.Type = null;
                        }
                    }

                    RaisePropertyChanged(nameof(TypeName));
                }
            }
        }
        #endregion

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public TypeExtension()
            : base()
        {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="type">The name of the type.</param>
        public TypeExtension(string type)
            : this()
        {
            this.TypeName = type;
        }

        /// <summary>
        /// Override this function, if (and only if) additional information is needed from the <see cref="IServiceProvider"/> instance that is passed to <see cref="NestedMarkupExtension.ProvideValue"/>.
        /// </summary>
        /// <param name="serviceProvider">A service provider.</param>
        protected override void OnServiceProviderChanged(IServiceProvider serviceProvider)
        {
            if (typeName == null || typeName.Trim() == "")
                return;

            var service = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;

            if (service != null)
            {
                this.Type = service.Resolve(typeName);
            }
            else
            {
                try
                {
                    this.Type = System.Type.GetType(typeName, false);
                }
                catch
                {
                    this.Type = null;
                }
            }
        }

        /// <summary>
        /// This function returns the properly prepared output of the markup extension.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <param name="endPoint">Information about the endpoint.</param>
        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            return type;
        }

        /// <summary>
        /// This method must return true, if an update shall be executed when the given endpoint is reached.
        /// This method is called each time an endpoint is reached.
        /// </summary>
        /// <param name="endpoint">Information on the specific endpoint.</param>
        /// <returns>True, if an update of the path to this endpoint shall be performed.</returns>
        protected override bool UpdateOnEndpoint(TargetInfo endpoint)
        {
            return false;
        }
    }
}
