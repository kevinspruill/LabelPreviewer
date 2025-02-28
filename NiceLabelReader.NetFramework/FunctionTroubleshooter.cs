using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LabelPreviewer
{
    /// <summary>
    /// Utility class for troubleshooting function issues
    /// </summary>
    public class FunctionTroubleshooter
    {
        /// <summary>
        /// Shows a detailed analysis of why a function reference might not be found
        /// </summary>
        public static void TroubleshootFunctionReference(Window owner, LabelModel model, string reference)
        {
            if (model == null)
            {
                MessageBox.Show("No label model loaded. Please open a label file first.",
                    "No Model", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create window for analysis
            var window = new Window
            {
                Title = $"Function Reference Analysis: {reference}",
                Width = 700,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = owner
            };

            var scrollViewer = new ScrollViewer();
            window.Content = scrollViewer;

            var panel = new StackPanel { Margin = new Thickness(10) };
            scrollViewer.Content = panel;

            // Add title
            panel.Children.Add(new TextBlock
            {
                Text = $"Analysis of Function Reference: {reference}",
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 10)
            });

            // Check all possible resolutions
            AddAnalysisSection(panel, "Direct Function ID Lookup", 
                $"Checking if '{reference}' exists directly as a function ID...",
                model.Functions.ContainsKey(reference));

            if (model.Functions.ContainsKey(reference))
            {
                var function = model.Functions[reference];
                AddDetail(panel, "Found function:", $"{function.Name} ({function.FunctionType})");
            }

            AddAnalysisSection(panel, "Function Name Lookup",
                $"Checking if '{reference}' exists as a function name...",
                model.FunctionNameToIdMap.ContainsKey(reference));

            if (model.FunctionNameToIdMap.ContainsKey(reference))
            {
                string id = model.FunctionNameToIdMap[reference];
                AddDetail(panel, "Maps to ID:", id);
                
                if (model.Functions.ContainsKey(id))
                {
                    var function = model.Functions[id];
                    AddDetail(panel, "Found function:", $"{function.Name} ({function.FunctionType})");
                }
                else
                {
                    AddDetail(panel, "Warning:", $"ID {id} not found in Functions dictionary!");
                }
            }

            // Check if it's a variable
            AddAnalysisSection(panel, "Variable ID Lookup",
                $"Checking if '{reference}' exists as a variable ID...",
                model.Variables.ContainsKey(reference));

            if (model.Variables.ContainsKey(reference))
            {
                var variable = model.Variables[reference];
                AddDetail(panel, "Found variable:", $"{variable.Name} = {variable.SampleValue ?? "(empty)"}");
            }

            AddAnalysisSection(panel, "Variable Name Lookup",
                $"Checking if '{reference}' exists as a variable name...",
                model.VariableNameToIdMap.ContainsKey(reference));

            if (model.VariableNameToIdMap.ContainsKey(reference))
            {
                string id = model.VariableNameToIdMap[reference];
                AddDetail(panel, "Maps to ID:", id);
                
                if (model.Variables.ContainsKey(id))
                {
                    var variable = model.Variables[id];
                    AddDetail(panel, "Found variable:", $"{variable.Name} = {variable.SampleValue ?? "(empty)"}");
                }
                else
                {
                    AddDetail(panel, "Warning:", $"ID {id} not found in Variables dictionary!");
                }
            }

            // String similarity search
            AddAnalysisSection(panel, "Similar Function Names", 
                "Searching for similar function names...", true);

            var similarFunctionNames = FindSimilarStrings(reference, 
                new List<string>(model.FunctionNameToIdMap.Keys), 3);
            
            if (similarFunctionNames.Count > 0)
            {
                foreach (var name in similarFunctionNames)
                {
                    string id = model.FunctionNameToIdMap[name];
                    AddDetail(panel, $"Similar name: {name}", $"ID: {id}");
                }
            }
            else
            {
                AddDetail(panel, "Result:", "No similar function names found");
            }

            // Check string similarity with function IDs
            AddAnalysisSection(panel, "Similar Function IDs", 
                "Searching for similar function IDs...", true);

            var similarFunctionIds = FindSimilarStrings(reference, 
                new List<string>(model.Functions.Keys), 3);
            
            if (similarFunctionIds.Count > 0)
            {
                foreach (var id in similarFunctionIds)
                {
                    var function = model.Functions[id];
                    AddDetail(panel, $"Similar ID: {id}", $"Name: {function.Name}");
                }
            }
            else
            {
                AddDetail(panel, "Result:", "No similar function IDs found");
            }

            // Format XML Functions section
            AddAnalysisSection(panel, "Format XML Functions",
                "Checking Format XML for function references...", true);

            var formatXmlFunctions = ExtractFormatXmlFunctionReferences(model.FormatXmlData);
            bool formatXmlHasReference = formatXmlFunctions.Contains(reference);
            
            AddDetail(panel, "Reference found in Format XML:", formatXmlHasReference.ToString());
            
            if (formatXmlFunctions.Count > 0)
            {
                AddDetail(panel, "All Format XML function references:", 
                    string.Join(", ", formatXmlFunctions));
            }
            else
            {
                AddDetail(panel, "Result:", "No function references found in Format XML");
            }

            // Show reference chains - which functions reference this one?
            AddAnalysisSection(panel, "Reference Chain Analysis",
                "Checking which functions reference this...", true);

            var referencingFunctions = FindReferencingFunctions(model, reference);
            if (referencingFunctions.Count > 0)
            {
                foreach (var func in referencingFunctions)
                {
                    AddDetail(panel, $"Referenced by: {func.Name}", $"ID: {func.Id}");
                }
            }
            else
            {
                AddDetail(panel, "Result:", "Not referenced by any functions");
            }

            // Add function list section
            AddAnalysisSection(panel, "All Functions in Model",
                "Listing all functions in the model...", true);

            foreach (var function in model.Functions.Values)
            {
                AddDetail(panel, function.Name, $"ID: {function.Id}, Type: {function.FunctionType}");
            }

            // Add possible solution buttons
            AddAnalysisSection(panel, "Potential Solutions", "", true);

            // Create special solutions for the specific GUID you mentioned
            if (reference == "246def0c-4bd4-4a59-885f-901b15ae3eee" || 
                reference == "DescriptionFields")
            {
                // Create a button to manually create the function
                var createButton = new Button
                {
                    Content = "Create DescriptionFields Concatenate Function",
                    Padding = new Thickness(10, 5, 10, 5),
                    Margin = new Thickness(0, 5, 0, 5)
                };
                
                createButton.Click += (s, e) => 
                {
                    CreateDescriptionFieldsFunction(model);
                    MessageBox.Show("Function created successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    window.Close();
                };
                
                panel.Children.Add(createButton);
            }

            // Add close button
            var closeButton = new Button
            {
                Content = "Close",
                Padding = new Thickness(10, 5, 10, 5),
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };
            closeButton.Click += (s, e) => window.Close();
            panel.Children.Add(closeButton);

            // Show the window
            window.ShowDialog();
        }

        /// <summary>
        /// Creates the DescriptionFields concatenate function
        /// </summary>
        private static void CreateDescriptionFieldsFunction(LabelModel model)
        {
            // Create a concatenate function with ID "246def0c-4bd4-4a59-885f-901b15ae3eee"
            var concatFunction = new ConcatenateFunction
            {
                Id = "246def0c-4bd4-4a59-885f-901b15ae3eee",
                Name = "DescriptionFields",
                FunctionType = "ConcatenateFunction",
                Separator = "\r\n", // Decoded from Base64 "DQo="
                IgnoreEmptyValues = false
            };

            // Add the data sources (from your example)
            // Description1 and Description2
            concatFunction.DataSourceIds.Add("9b19b3d6-fd8e-4250-b84e-64fa7bd7a049");
            concatFunction.DataSourceIds.Add("d8409940-a513-4e07-8cda-b92361625140");

            // Add the function to the model
            model.Functions[concatFunction.Id] = concatFunction;
            
            // Update the function name mappings
            if (!model.FunctionIdToNameMap.ContainsKey(concatFunction.Id))
            {
                model.FunctionIdToNameMap[concatFunction.Id] = concatFunction.Name;
            }
            if (!model.FunctionNameToIdMap.ContainsKey(concatFunction.Name))
            {
                model.FunctionNameToIdMap[concatFunction.Name] = concatFunction.Id;
            }
        }

        /// <summary>
        /// Finds functions that reference the given function ID or name
        /// </summary>
        private static List<Function> FindReferencingFunctions(LabelModel model, string reference)
        {
            var result = new List<Function>();

            foreach (var function in model.Functions.Values)
            {
                // Check input sources for direct references
                if (function.InputDataSourceIds.Contains(reference))
                {
                    result.Add(function);
                    continue;
                }
                
                // For concatenate functions, also check data sources
                if (function is ConcatenateFunction concatFunction)
                {
                    if (concatFunction.DataSourceIds.Contains(reference))
                    {
                        result.Add(function);
                        continue;
                    }
                }
                
                // Check script for references
                if (!string.IsNullOrEmpty(function.Script) || 
                    !string.IsNullOrEmpty(function.ScriptWithReferences))
                {
                    string script = !string.IsNullOrEmpty(function.ScriptWithReferences) 
                        ? function.ScriptWithReferences 
                        : function.Script;
                        
                    // Decode and check
                    try
                    {
                        var interpreter = new VBScriptInterpreter();
                        string decodedScript = interpreter.DecodeBase64Script(script);
                        
                        if (decodedScript.Contains(reference))
                        {
                            result.Add(function);
                        }
                    }
                    catch { /* Ignore decoding errors */ }
                }
            }

            return result;
        }

        /// <summary>
        /// Extracts function references from Format XML
        /// </summary>
        private static List<string> ExtractFormatXmlFunctionReferences(string formatXml)
        {
            var result = new List<string>();
            
            if (string.IsNullOrEmpty(formatXml))
                return result;
                
            try
            {
                // Simple regex approach to find DataSourceReference/Id tags
                var regex = new System.Text.RegularExpressions.Regex(
                    @"<DataSourceReference[^>]*>\s*<Id>([^<]+)</Id>", 
                    System.Text.RegularExpressions.RegexOptions.Compiled);
                    
                var matches = regex.Matches(formatXml);
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        result.Add(match.Groups[1].Value);
                    }
                }
            }
            catch { /* Ignore parsing errors */ }
            
            return result;
        }

        /// <summary>
        /// Finds similar strings using Levenshtein distance
        /// </summary>
        private static List<string> FindSimilarStrings(string target, List<string> candidates, int maxDistance)
        {
            var result = new List<Tuple<string, int>>();
            
            foreach (var candidate in candidates)
            {
                int distance = LevenshteinDistance(target, candidate);
                if (distance <= maxDistance)
                {
                    result.Add(new Tuple<string, int>(candidate, distance));
                }
            }
            
            // Sort by distance (closest matches first)
            result.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            
            // Extract just the strings
            return result.ConvertAll(t => t.Item1);
        }

        /// <summary>
        /// Calculates Levenshtein distance between two strings
        /// </summary>
        private static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
                return m;
            if (m == 0)
                return n;

            for (int i = 0; i <= n; i++)
                d[i, 0] = i;
            for (int j = 0; j <= m; j++)
                d[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        #region UI Helper Methods

        /// <summary>
        /// Adds a section header with result to the analysis panel
        /// </summary>
        private static void AddAnalysisSection(StackPanel panel, string title, string description, bool result)
        {
            // Add section with colored result indicator
            var headerPanel = new DockPanel
            {
                Margin = new Thickness(0, 15, 0, 5)
            };

            var resultIndicator = new Border
            {
                Width = 16,
                Height = 16,
                CornerRadius = new CornerRadius(8),
                Background = result ? Brushes.Green : Brushes.Red,
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            DockPanel.SetDock(resultIndicator, Dock.Left);
            headerPanel.Children.Add(resultIndicator);

            var headerText = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };
            headerPanel.Children.Add(headerText);

            panel.Children.Add(headerPanel);

            // Add description if provided
            if (!string.IsNullOrEmpty(description))
            {
                panel.Children.Add(new TextBlock
                {
                    Text = description,
                    Margin = new Thickness(5, 0, 0, 5),
                    TextWrapping = TextWrapping.Wrap
                });
            }
        }

        /// <summary>
        /// Adds a detail row to the analysis panel
        /// </summary>
        private static void AddDetail(StackPanel panel, string label, string value)
        {
            var grid = new Grid { Margin = new Thickness(20, 2, 0, 2) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            grid.Children.Add(new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.SemiBold
            });

            var valueText = new TextBlock
            {
                Text = value,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(valueText, 1);
            grid.Children.Add(valueText);

            panel.Children.Add(grid);
        }

        #endregion
    }
}