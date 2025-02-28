using System;
using System.Collections.Generic;
using System.Text;

namespace LabelPreviewer
{
    public class ConcatenateFunction : Function
    {
        /// <summary>
        /// The character sequence to use as a separator between concatenated values
        /// </summary>
        public string Separator { get; set; } = "\n";

        /// <summary>
        /// Whether to ignore empty values when concatenating
        /// </summary>
        public bool IgnoreEmptyValues { get; set; } = false;

        /// <summary>
        /// List of data source IDs that should be concatenated
        /// </summary>
        public List<string> DataSourceIds { get; set; } = new List<string>();

        /// <summary>
        /// Executes the concatenation function with the provided variables
        /// </summary>
        public override string Execute(Dictionary<string, Variable> variables, Dictionary<string, string> idToNameMap)
        {
            // If no data sources, return empty string or sample value
            if (DataSourceIds == null || DataSourceIds.Count == 0)
                return SampleValue ?? string.Empty;

            List<string> values = new List<string>();

            // Get values for each data source
            foreach (string dataSourceId in DataSourceIds)
            {
                if (variables.TryGetValue(dataSourceId, out Variable variable))
                {
                    string value = variable.SampleValue ?? string.Empty;

                    // Skip empty values if configured to do so
                    if (IgnoreEmptyValues && string.IsNullOrEmpty(value))
                        continue;

                    values.Add(value);
                }
            }

            // Join all values with the separator
            return string.Join(Separator, values);
        }

        /// <summary>
        /// Decodes a Base64 encoded separator string
        /// </summary>
        public static string DecodeSeparator(string base64Separator)
        {
            if (string.IsNullOrEmpty(base64Separator))
                return "\n"; // Default separator

            try
            {
                byte[] bytes = Convert.FromBase64String(base64Separator);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception)
            {
                // If decoding fails, return the original string
                return base64Separator;
            }
        }
    }
}