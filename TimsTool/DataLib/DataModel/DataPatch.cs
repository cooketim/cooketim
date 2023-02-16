using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DataLib
{
    [Serializable]
    public class DataPatch
    {
        /// <summary>
        /// The patch date for which the extract is required
        /// </summary>
        public DateTime? PatchDate { get; set; }

        /// <summary>
        /// The date of the previous export for live
        /// </summary>
        public DateTime? LastExportDate { get; set; }

        /// <summary>
        /// The date of the previous export for test
        /// </summary>
        public DateTime? TestExportDate { get; set; }

        /// <summary>
        /// A flag to indicate that a full extract, ignoring the last export date is required
        /// </summary>
        public bool IsFullExport { get; set; }

        /// <summary>
        /// A string to indicate that data support for a toggled feature is required
        /// </summary>
        public List<string> FeatureToggles { get; set; }

        /// <summary>
        /// A flag to indicate that a test data patch is required
        /// </summary>
        public bool IsTestExport { get; set; }

        /// <summary>
        /// A flag to indicate that a prod data patch is required
        /// </summary>
        public bool IsProdExport { get; set; }

        /// <summary>
        /// A flag to indicate that a welsh only export is required
        /// </summary>
        public bool IsWelshExport { get; set; }
    }
}