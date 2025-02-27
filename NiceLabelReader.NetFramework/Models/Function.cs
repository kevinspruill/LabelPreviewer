using System;
using System.Collections.Generic;


namespace LabelPreviewer
{
    public class Function
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SampleValue { get; set; }
        public string Script { get; set; }             // Base64-encoded script
        public string ScriptWithReferences { get; set; } // Base64-encoded script with variable references
        public List<string> InputDataSourceIds { get; set; } = new List<string>();

        private static VBScriptInterpreter _interpreter;

        // Lazy-initialize the interpreter
        private static VBScriptInterpreter Interpreter
        {
            get
            {
                if (_interpreter == null)
                {
                    _interpreter = new VBScriptInterpreter();
                }
                return _interpreter;
            }
        }

        /// <summary>
        /// Executes the function's script with the provided variables
        /// </summary>
        public string Execute(Dictionary<string, Variable> variables, Dictionary<string, string> idToNameMap)
        {
            try
            {
                // Prepare variable values with friendly names
                var variableValues = new Dictionary<string, string>();

                // Add all variables by their friendly names
                foreach (var variable in variables)
                {
                    if (idToNameMap.TryGetValue(variable.Key, out string friendlyName))
                    {
                        variableValues[friendlyName] = variable.Value.SampleValue ?? string.Empty;
                    }
                }

                // Get and decode the script
                string scriptToExecute = !string.IsNullOrEmpty(Script)
                    ? Script : Script;

                if (string.IsNullOrEmpty(scriptToExecute))
                    return SampleValue ?? string.Empty;

                // Execute with the decoded script and friendly variable names
                object result = Interpreter.ExecuteDecodedScript(scriptToExecute, variableValues);

                return result?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                return SampleValue ?? $"Error: {ex.Message}";
            }
        }
    }
}