﻿namespace ItemCommander.EntityService.Models
{
    /// <summary>
    /// Copy single contract
    /// </summary>
    public class CopySingle : BaseRequest
    {
        /// <summary>
        /// Gets or sets the target path.
        /// </summary>
        /// <value>
        /// The target path.
        /// </value>
        public string TargetPath { get; set; }

        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        public string Item { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [copy sub items].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [copy sub items]; otherwise, <c>false</c>.
        /// </value>
        public bool CopySubItems { get; set; }
    }
}