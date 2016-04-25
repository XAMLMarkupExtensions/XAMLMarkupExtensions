#region Copyright information
// <copyright file="StaticExtension.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://xamlmarkupextensions.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace XAMLMarkupExtensions.Base
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows.Markup;

    /// <summary>
    /// A static extension.
    /// <para>Adopted from the work of Henrik Jonsson: http://www.codeproject.com/Articles/305932/Static-and-Type-markup-extensions-for-Silverlight, Licensed under Code Project Open License (CPOL).</para>
    /// </summary>
    [MarkupExtensionReturnType(typeof(Type))]
    [ContentProperty("Member"), DefaultProperty("Member")]
    public class StaticExtension : NestedMarkupExtension, INotifyPropertyChanged
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

        #region Properties
        private Type memberType = null;
        /// <summary>
        /// The member type.
        /// </summary>
        public Type MemberType
        {
            get { return memberType; }
            set
            {
                if (memberType != value)
                {
                    memberType = value;
                    RaisePropertyChanged("MemberType");
                }
            }
        }

        private string member = "";
        /// <summary>
        /// The member.
        /// </summary>
        public string Member
        {
            get { return member; }
            set
            {
                if (member != value)
                {
                    member = value;
                    RaisePropertyChanged("Member");
                }
            }
        }

        private object result = null;
        /// <summary>
        /// The result.
        /// </summary>
        public object Result
        {
            get { return result; }
            set
            {
                if (result != value)
                {
                    result = value;
                    RaisePropertyChanged("Result");
                }
            }
        } 
        #endregion

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public StaticExtension()
            : base()
        {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="member">The member name.</param>
        public StaticExtension(string member)
            : this()
        {
            this.Member = member;
        }

        /// <summary>
        /// Override this function, if (and only if) additional information is needed from the <see cref="IServiceProvider"/> instance that is passed to <see cref="NestedMarkupExtension.ProvideValue"/>.
        /// </summary>
        /// <param name="serviceProvider">A service provider.</param>
        protected override void OnServiceProviderChanged(IServiceProvider serviceProvider)
        {
            if (member == null || member.Trim() == "")
                throw new InvalidOperationException("The member property must be set!");

            var memberCopy = member;
            var memberTypeEndsAt = memberCopy.LastIndexOf('.');

            if (memberTypeEndsAt == -1)
            {
                // No member type included.
                if (memberType == null)
                {
                    this.Result = null;
                    return;
                }
            }
            else
            {
                // Member type included - overwrite the existing one.
                var service = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
                var typeName = memberCopy.Substring(0, memberTypeEndsAt);

                if (service == null)
                {
                    try
                    {
                        memberType = System.Type.GetType(typeName, false);
                    }
                    catch
                    {
                        this.Result = null;
                        return;
                    }
                }
                else
                    memberType = service.Resolve(typeName);

                memberCopy = memberCopy.Substring(memberTypeEndsAt + 1);
            }

            // Again, check all parameters up to now.
            if (memberType == null || memberCopy.Trim() == "")
            {
                this.Result = null;
                return;
            }

            // Try to get a property info.
            var pi = memberType.GetProperty(memberCopy, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            if (pi != null)
            {
                if (!pi.CanRead)
                    throw new InvalidOperationException("No static get accessor for property " + memberCopy + ".");

                this.Result = pi.GetValue(null, null);
            }
            else
            {
                // Now, try to get a field info.
                var fi = memberType.GetField(member, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                if (fi == null)
                    throw new InvalidOperationException("No static property or field " + memberCopy + " available in " + memberType.FullName);

                this.Result = fi.GetValue(null);
            }
        }

        /// <summary>
        /// This function returns the properly prepared output of the markup extension.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <param name="endPoint">Information about the endpoint.</param>
        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            return this.Result;
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
