#region Copyright information
// <copyright file="TypeExtension.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://xamlmarkupextensions.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace XAMLMarkupExtensions.Base
{
    using System;
    using System.ComponentModel;
    using System.Windows.Markup;

    /// <summary>
    /// A type extension.
    /// <para>Adopted from the work of Henrik Jonsson: http://www.codeproject.com/Articles/305932/Static-and-Type-markup-extensions-for-Silverlight, Licensed under Code Project Open License (CPOL).</para>
    /// </summary>
#if SILVERLIGHT
#else
    [MarkupExtensionReturnType(typeof(Type))]
#endif
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
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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
                    RaisePropertyChanged("Type");
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

                    RaisePropertyChanged("TypeName");
                }
            }
        }

        public TypeExtension()
            : base()
        {
        }

        public TypeExtension(string type)
            : this()
        {
            this.TypeName = type;
        }

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

        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            return type;
        }

        protected override bool UpdateOnEndpoint(TargetInfo endpoint)
        {
            return false;
        }
    }
}
