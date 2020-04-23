#region Copyright information
// <copyright file="StaticExtension.cs">
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
    using System.Reflection;
    using System.Windows.Markup;
    #endregion

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                    RaisePropertyChanged(nameof(MemberType));
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
                    RaisePropertyChanged(nameof(Member));
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
                    RaisePropertyChanged(nameof(Result));
                }
            }
        }
        #endregion

        #region Constructors
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
        #endregion

        #region Overrides
        /// <inheritdoc/>
        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            return this.Result;
        }

        /// <inheritdoc/>
        protected override bool UpdateOnEndpoint(TargetInfo endpoint)
        {
            return false;
        }

        /// <inheritdoc/>
        protected override void OnServiceProviderChanged(IServiceProvider serviceProvider)
        {
            if (member == null || member.Trim() == "")
                throw new InvalidOperationException("The member property must be set!");

            var memberName = member;
            var memberTypeEndsAt = memberName.LastIndexOf('.');
            if (memberTypeEndsAt != -1)
            {
                var typeName = memberName.Substring(0, memberTypeEndsAt);

                // Member type included - overwrite the existing one.
                if (serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver service)
                    memberType = service.Resolve(typeName);
                else
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

                memberName = memberName.Substring(memberTypeEndsAt + 1);
            }

            this.Result = GetValueFromMember(memberType, memberName);
        }
        #endregion

        #region GetValueFromMember
        private object GetValueFromMember(Type getMemberType, string getMemberName)
        {
            if (getMemberType == null || getMemberName.Trim() == "")
            {
                return null;
            }

            if (getMemberType.IsEnum)
                return Enum.Parse(getMemberType, getMemberName, true);

            if (getMemberType.GetProperty(getMemberName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) is PropertyInfo pi)
            {
                if (!pi.CanRead)
                    throw new InvalidOperationException("No static get accessor for property " + getMemberName + ".");

                return pi.GetValue(null, null);
            }

            if (getMemberType.GetField(member, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) is FieldInfo fi)
            {
                return fi.GetValue(null);
            }

            throw new InvalidOperationException("No static enum, property or field " + getMemberName + " available in " + getMemberType.FullName);
        } 
        #endregion
    }
}
