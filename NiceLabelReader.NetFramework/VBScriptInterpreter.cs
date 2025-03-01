using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                // Convert value to string and properly escape for VBScript
                string safeValue = value?.ToString() ?? "";

                // Replace special characters with VBScript constants
                safeValue = safeValue.Replace("\r\n", "\" & vbCrLf & \"")
                                     .Replace("\r", "\" & vbCr & \"")
                                     .Replace("\n", "\" & vbLf & \"");

                if (name == "[246def0c-4bd4-4a59-885f-901b15ae3eee]")
                {
                    Debug.WriteLine($"Set variable '{name}' to '{value}'");
                }

                // For empty string or string with only special chars
                if (string.IsNullOrEmpty(safeValue) || safeValue.StartsWith("\" & vb"))
                    scriptControl.AddCode($"Dim {name}\r\n{name} = \"\" {safeValue}\r\n");
                else
                    scriptControl.AddCode($"Dim {name}\r\n{name} = \"{safeValue}\"\r\n");

                Debug.WriteLine($"Set variable '{name}' to '{value}'");
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
                //scriptControl.AddCode("Option Explicit Off");  // Allow undeclared variables
                //scriptControl.AddCode("Const VBCRLF = vbCrLF");

                // Execute the script - the script should set the "Result" variable
                scriptControl.AddCode(script);

                // Get the result
                try
                {
                    var result = scriptControl.Eval("Result");

                    return result;
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

        public object ExecuteOriginalScript(string script, Dictionary<string, string> variableValues)
        {
            Reset();

            // Set all variables with their friendly names
            foreach (var pair in variableValues)
            {
                SetVariable(pair.Key, pair.Value);
            }

            // Execute original script (no replacements)
            return ExecuteScript(script);
        }

        public object ExecuteDecodedScript(string base64Script, Dictionary<string, string> variableValues)
        {
            if (string.IsNullOrEmpty(base64Script))
                return null;

            Reset();

            // Decode the base64 script
            string decodedScript = DecodeBase64Script(base64Script);

            // Set all variables with their friendly names
            foreach (var pair in variableValues)
            {
                SetVariable(pair.Key, pair.Value);
            }

            // Execute the decoded script without text replacements
            return ExecuteScript(decodedScript);
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

            // Replace GUIDs with bracketed variable names
            foreach (var pair in variableValues)
            {
                string bracketVarName = "[" + pair.Key + "]";

                // Replace the GUID in the script with a bracketed version
                script = script.Replace(pair.Key, bracketVarName);

                // Add the variable with the bracketed name
                SetVariable(bracketVarName, pair.Value);
            }

            // Also set original GUIDs as fallback
            //foreach (var pair in variableValues)
            //{
            //    SetVariable(pair.Key, pair.Value);
            //}

            // Add debugging if needed
            System.Diagnostics.Debug.WriteLine($"Executing script:\n{script}");

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