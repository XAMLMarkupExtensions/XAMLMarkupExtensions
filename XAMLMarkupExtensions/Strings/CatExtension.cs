#region Copyright information
// <copyright file="CatExtension.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://xamlmarkupextensions.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace XAMLMarkupExtensions.Strings
{
    #region Uses
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows.Markup;
    using XAMLMarkupExtensions.Base;
    #endregion

#if SILVERLIGHT
#else
    [MarkupExtensionReturnType(typeof(String))]
#endif
    [ContentProperty("Items"), DefaultProperty("Items")]
    public class CatExtension : NestedMarkupExtension
    {
        private List<object> items = new List<object>();
        public List<object> Items
        {
            get { return items; }
        }

        private string format = "";
        public string Format
        {
            get { return format; }
            set
            {
                if (format != value)
                {
                    format = value;
                    UpdateNewValue();
                }
            }
        }
        
        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            string s = Format;

            if (s == null)
                return null;

            for (int i = 0; i < items.Count; i++)
            {
                string tag = "{" + i + "}";
                
                if (s.Contains(tag))
                {
                    object t = items[i];

                    if (t is INestedMarkupExtension)
                    {
                        PropertyInfo pi = this.GetType().GetProperty("Items");
                        TargetInfo ti = new TargetInfo(this, pi, pi.PropertyType, i);
                        INestedMarkupExtension nme = (INestedMarkupExtension)t;

                        if (nme.IsConnected(ti))
                            t = nme.FormatOutput(endPoint, ti);
                        else
                            t = GetValue<string>(t, pi, i, endPoint);
                    }

                    if (t != null)
                        s = s.Replace(tag, t.ToString());
                }
            }

            return s;
        }

        protected override bool UpdateOnEndpoint(TargetInfo endpoint)
        {
            return false;
        }
    }
}
