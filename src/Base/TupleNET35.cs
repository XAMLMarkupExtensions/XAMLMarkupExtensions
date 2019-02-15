#region Copyright information
// <copyright file="NestedMarkupExtension.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://xamlmarkupextensions.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace XAMLMarkupExtensions.Base
{
#if NET35
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    #endregion

    /// <summary>
    /// A simple Tuple class for .NET3.5
    /// <para>From an answer in http://stackoverflow.com/questions/1171812/multi-key-dictionary-in-c</para>
    /// </summary>
    /// <typeparam name="T1">The first item type.</typeparam>
    /// <typeparam name="T2">The second item type.</typeparam>
    public struct Tuple<T1, T2>
    {
        /// <summary>
        /// Gets the first item.
        /// </summary>
        public readonly T1 Item1;

        /// <summary>
        /// Gets the second item.
        /// </summary>
        public readonly T2 Item2;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        public Tuple(T1 item1, T2 item2) { Item1 = item1; Item2 = item2; }
    }

    /// <summary>
    /// A simple Tuple class for .NET3.5
    /// <para>From an answer in http://stackoverflow.com/questions/1171812/multi-key-dictionary-in-c</para>
    /// </summary>
    /// <typeparam name="T1">The first item type.</typeparam>
    /// <typeparam name="T2">The second item type.</typeparam>
    /// <typeparam name="T3">The third item type.</typeparam>
    public struct Tuple<T1, T2, T3>
    {
        /// <summary>
        /// Gets the first item.
        /// </summary>
        public readonly T1 Item1;

        /// <summary>
        /// Gets the second item.
        /// </summary>
        public readonly T2 Item2;

        /// <summary>
        /// Gets the third item.
        /// </summary>
        public readonly T3 Item3;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <param name="item3">The third item.</param>
        public Tuple(T1 item1, T2 item2, T3 item3) { Item1 = item1; Item2 = item2; Item3 = item3; }
    }

    /// <summary>
    /// A static tuple factory class.
    /// </summary>
    public static class Tuple
    {
        /// <summary>
        /// Creates a new tuple.
        /// </summary>
        /// <typeparam name="T1">The first item type.</typeparam>
        /// <typeparam name="T2">The second item type.</typeparam>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <returns>A new tuple.</returns>
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }

        /// <summary>
        /// Creates a new tuple.
        /// </summary>
        /// <typeparam name="T1">The first item type.</typeparam>
        /// <typeparam name="T2">The second item type.</typeparam>
        /// <typeparam name="T3">The third item type.</typeparam>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <param name="item3">The third item.</param>
        /// <returns>A new tuple.</returns>
        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new Tuple<T1, T2, T3>(item1, item2, item3);
        }
    }
#endif
}
