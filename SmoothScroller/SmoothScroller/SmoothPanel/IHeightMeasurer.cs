// --------------------------------------------------------------------------
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// --------------------------------------------------------------------------

namespace SmoothScroller
{
    internal interface IHeightMeasurer
    {
        /// <summary>
        /// Gets the estimated height of element.
        /// </summary>
        /// <param name="availableWidth">Available width for element.</param>
        /// <returns>
        /// A <see cref="double"/> that represents an estimated height of element.
        /// </returns>
        double GetEstimatedHeight(double availableWidth);
    }
}