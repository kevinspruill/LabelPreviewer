using System;
using System.Collections.Generic;
using System.Text;
using MSScriptControl;

namespace LabelPreviewer
{
    public class VBScriptInterpreter
    {
        private ScriptControl scriptControl;
        private Dictionary<string, object> variables = new Dictionary<string, object>();

        public VBScriptInterpreter()
        {
            try
            {
                // Create an instance of the Microsoft Script Control
                scriptControl = new ScriptControl()
                {
                    Language = "VBScript",
                    AllowUI = false
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to initialize VBScript interpreter. Error: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Decode a base64-encoded script
        /// </summary>
        public string DecodeBase64Script(string base64Script)
        {
            if (string.IsNullOrEmpty(base64Script))
                return string.Empty;

            try
            {
                byte[] bytes = Convert.FromBase64String(base64Script);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to decode base64 script: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Set a variable value for use in scripts
        /// </summary>
        public void SetVariable(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
                return;

            variables[name] = value;

            try
            {
                // Add the variable to the script engine
                scriptControl.AddObject(name, value, true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to set variable '{name}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Execute a VBScript with the currently set variables.
        /// The script must set a variable named 'Result' which will be returned.
        /// </summary>
        public object ExecuteScript(string script)
        {
            if (string.IsNullOrEmpty(script))
                return null;

            try
            {
                // Add VBScript constants
                scriptControl.AddCode("Option Explicit Off");  // Allow undeclared variables
                scriptControl.AddCode("Const VBCRLF = vbCrLF");

                // Execute the script - the script should set the "Result" variable
                scriptControl.AddCode(script);

                // Get the result
                try
                {
                    return scriptControl.Eval("Result");
                }
                catch
                {
                    // If Result variable doesn't exist, return empty string
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Script execution failed: {ex.Message}\nScript: {script}", ex);
            }
        }

        /// <summary>
        /// Execute a base64-encoded VBScript with variable substitution
        /// </summary>
        public object ExecuteBase64Script(string base64Script, Dictionary<string, string> variableValues)
        {
            if (string.IsNullOrEmpty(base64Script))
                return null;

            // Reset variables
            Reset();

            // Decode the script
            string script = DecodeBase64Script(base64Script);

            // Method 1: Direct GUID replacement in the script
            foreach (var pair in variableValues)
            {
                string cleanVarName = "var_" + pair.Key.Replace("-", "_");

                // Replace the GUID in the script with a clean variable name
                script = script.Replace(pair.Key, cleanVarName);

                // Add the variable with the clean name
                SetVariable(cleanVarName, pair.Value);
            }

            // Method 2: Set variables with their original names too (as a fallback)
            foreach (var pair in variableValues)
            {
                SetVariable(pair.Key, pair.Value);
            }

            // Execute the script
            return ExecuteScript(script);
        }

        /// <summary>
        /// Reset the script engine
        /// </summary>
        public void Reset()
        {
            try
            {
                // Reset the script engine
                scriptControl.Reset();
                variables.Clear();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to reset script engine: {ex.Message}", ex);
            }
        }
    }
}