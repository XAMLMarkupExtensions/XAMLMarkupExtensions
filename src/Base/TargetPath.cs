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
    /// This class helps tracking the path to a specific endpoint.
    /// </summary>
    public class TargetPath
    {
        /// <summary>
        /// The path to the endpoint.
        /// </summary>
        private Stack<TargetInfo> Path { get; set; }
        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        public TargetInfo EndPoint { get; private set; }

        /// <summary>
        /// Add another step to the path.
        /// </summary>
        /// <param name="info">The TargetInfo object of the step.</param>
        public void AddStep(TargetInfo info) { Path.Push(info); }

        /// <summary>
        /// Get the next step and remove it from the path.
        /// </summary>
        /// <returns>The next steps TargetInfo.</returns>
        public TargetInfo GetNextStep() { return (Path.Count > 0) ? Path.Pop() : EndPoint; }

        /// <summary>
        /// Get the next step.
        /// </summary>
        /// <returns>The next steps TargetInfo.</returns>
        public TargetInfo ShowNextStep() { return (Path.Count > 0) ? Path.Peek() : EndPoint; }

        /// <summary>
        /// Creates a new TargetPath instance.
        /// </summary>
        /// <param name="endPoint">The endpoints TargetInfo of the path.</param>
        public TargetPath(TargetInfo endPoint)
        {
            if (!endPoint.IsEndpoint)
                throw new ArgumentException("A path endpoint cannot be another INestedMarkupExtension.");

            EndPoint = endPoint;
            Path = new Stack<TargetInfo>();
        }
    }
}
