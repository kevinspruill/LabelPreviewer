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
        public string FunctionType { get; set; } = "ExecuteScriptFunction";

        private static VBScriptInterpreter _interpreter;

        // Lazy-initialize the interpreter
        protected static VBScriptInterpreter Interpreter
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
        /// Executes the function with the provided variables
        /// </summary>
        public virtual string Execute(Dictionary<string, Variable> variables, Dictionary<string, string> idToNameMap)
        {
            try
            {
                // Prepare variable values with their IDs (not friendly names)
                var variableValues = new Dictionary<string, string>();

                // Add all variables by their ID
                foreach (var variable in variables)
                {
                    variableValues[variable.Key] = variable.Value.SampleValue ?? string.Empty;
                }

                // First try to use ScriptWithReferences if available, then fall back to Script
                string scriptToExecute = !string.IsNullOrEmpty(ScriptWithReferences)
                    ? ScriptWithReferences : Script;

                if (string.IsNullOrEmpty(scriptToExecute))
                    return SampleValue ?? string.Empty;

                // Execute with the script and variable ID replacements
                object result = Interpreter.ExecuteBase64Script(scriptToExecute, variableValues);

                return result?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                return SampleValue ?? $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Creates the appropriate type of function based on the function type
        /// </summary>
        public static Function CreateFunction(string type)
        {
            switch (type)
            {
                case "ConcatenateFunction":
                    return new ConcatenateFunction();
                case "DateAddFunction":
                    // You could implement a DateAddFunction class later
                    return new Function { FunctionType = "DateAddFunction" };
                case "ExecuteScriptFunction":
                default:
                    return new Function();
            }
        }
    }
}