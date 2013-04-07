#region Copyright information
// <copyright file="NullExtension.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://xamlmarkupextensions.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace XAMLMarkupExtensions.Base
{
    public class NullExtension : NestedMarkupExtension
    {
        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            return null;
        }

        protected override bool UpdateOnEndpoint(TargetInfo endpoint)
        {
            return false;
        }
    }
}
