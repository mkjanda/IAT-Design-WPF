using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Models
{
    /// <summary>
    /// Represents the result data of a survey or test, including the administrative time and the result data string.
    /// </summary>
    public class ResultData
    {
        /// <summary>
        /// Gets or sets the administrative time associated with the current operation.
        /// </summary>
        public string AdminTime { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the result data string associated with the current operation.
        /// </summary>
        public string ResultDataString { get; set; } = string.Empty;
    }
}
