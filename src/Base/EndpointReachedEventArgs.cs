#region Copyright information
// <copyright file="NestedMarkupExtension.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://xamlmarkupextensions.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace XAMLMarkupExtensions.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The event arguments of the EndpointReached event.
    /// </summary>
    public class EndpointReachedEventArgs : EventArgs
    {
        /// <summary>
        /// The endpoint.
        /// </summary>
        public TargetInfo Endpoint { get; private set; }

        /// <summary>
        /// Get or set the value that will be stored to the endpoint.
        /// </summary>
        public object EndpointValue { get; set; }

        /// <summary>
        /// Get or set a flag indicating that the event was handled.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Creates a new <see cref="EndpointReachedEventArgs"/> object.
        /// </summary>
        /// <param name="endPoint">The endpoint.</param>
        public EndpointReachedEventArgs(TargetInfo endPoint)
        {
            this.Endpoint = endPoint;
            this.EndpointValue = null;
        }
    }
}
