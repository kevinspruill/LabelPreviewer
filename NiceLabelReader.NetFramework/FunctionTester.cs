using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LabelPreviewer
{
    /// <summary>
    /// Utility class for testing NiceLabel functions
    /// </summary>
    public class FunctionTester
    {
        /// <summary>
        /// Tests a specific function with the provided label model
        /// </summary>
        public static void TestFunction(LabelModel model, string functionIdOrName)
        {
            if (model == null)
            {
                MessageBox.Show("No label model loaded. Please open a label file first.",
                    "No Model", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // First dump function info for debugging
            model.DumpFunctionInfo();

            // Try to resolve the function ID if we were given a name
            string functionId = functionIdOrName;
            Function function = null;

            // Try direct lookup by ID
            if (model.Functions.TryGetValue(functionIdOrName, out function))
            {
                functionId = functionIdOrName;
            }
            // Try lookup by name
            else if (model.FunctionNameToIdMap.TryGetValue(functionIdOrName, out string id))
            {
                functionId = id;
                model.Functions.TryGetValue(functionId, out function);
            }

            if (function == null)
            {
                // Function not found - it might be referenced in another format
                MessageBox.Show($"Function '{functionIdOrName}' not found in the loaded model.\n\n" +
                                $"Available functions: {string.Join(", ", model.Functions.Values.Select(f => f.Name))}",
                    "Function Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Clear any cached function results first
                model.ClearFunctionCache();

                // Execute the function with dependency handling
                string result = model.ExecuteFunctionWithDependencies(functionId);

                // Show results in the detailed window
                ShowFunctionDetails(model, function, result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing function: {ex.Message}",
                    "Function Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Tests a specific function by its GUID reference
        /// </summary>
        public static void TestFunctionByReference(LabelModel model, string reference)
        {
            // This is a special method to test functions that are referenced in the format
            // but might not be directly accessible in the model

            MessageBox.Show($"Testing function reference: {reference}\n\n" +
                            "This reference might be a direct GUID or a variable name. " +
                            "The system will try to resolve it using all available mappings.",
                "Test Function Reference", MessageBoxButton.OK, MessageBoxImage.Information);

            // Try all possible ways to resolve the reference
            if (model.Functions.ContainsKey(reference))
            {
                // Direct match by ID
                TestFunction(model, reference);
            }
            else if (model.FunctionNameToIdMap.ContainsKey(reference))
            {
                // Match by name
                TestFunction(model, reference);
            }
            else if (model.VariableIdToNameMap.ContainsKey(reference))
            {
                // It's a variable ID, show its value
                string varName = model.VariableIdToNameMap[reference];
                string varValue = model.Variables[reference].SampleValue ?? "(empty)";

                MessageBox.Show($"The reference '{reference}' is a variable:\n\n" +
                                $"Name: {varName}\n" +
                                $"Value: {varValue}",
                    "Variable Reference", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Not found in any lookup
                MessageBox.Show($"Could not resolve reference: {reference}\n\n" +
                                "This reference was not found in any of the function or variable mappings.",
                    "Reference Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Shows a window with detailed function information, including dependencies
        /// </summary>
        private static void ShowFunctionDetails(LabelModel model, Function function, string result)
        {
            // Create a dialog to show function details
            var dialog = new Window
            {
                Title = $"Function: {function.Name} ({function.FunctionType})",
                Width = 600,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            dialog.Content = grid;

            // Create a tab control for different views
            var tabControl = new TabControl();
            Grid.SetRow(tabControl, 0);
            grid.Children.Add(tabControl);

            // Details tab
            var detailsTab = new TabItem { Header = "Details" };
            tabControl.Items.Add(detailsTab);

            var detailsScroll = new ScrollViewer();
            detailsTab.Content = detailsScroll;

            var detailsPanel = new StackPanel { Margin = new Thickness(10) };
            detailsScroll.Content = detailsPanel;

            // Add function details
            AddDetailHeader(detailsPanel, "Function Information");
            AddDetailRow(detailsPanel, "Name:", function.Name);
            AddDetailRow(detailsPanel, "Type:", function.FunctionType);
            AddDetailRow(detailsPanel, "ID:", function.Id);

            if (function is ConcatenateFunction concatFunction)
            {
                AddDetailHeader(detailsPanel, "Concatenation Settings");
                AddDetailRow(detailsPanel, "Separator:", ReplaceSpecialChars(concatFunction.Separator));
                AddDetailRow(detailsPanel, "Ignore Empty Values:", concatFunction.IgnoreEmptyValues.ToString());

                AddDetailHeader(detailsPanel, "Input Values");
                foreach (var sourceId in concatFunction.DataSourceIds)
                {
                    string name = model.VariableIdToNameMap.TryGetValue(sourceId, out string varName) ?
                        varName : sourceId;

                    string value;
                    if (model.Variables.TryGetValue(sourceId, out Variable variable))
                    {
                        value = variable.SampleValue ?? "(empty)";
                    }
                    else if (model.Functions.TryGetValue(sourceId, out Function sourceFunction))
                    {
                        // Show that this is a function reference
                        value = $"[Function: {sourceFunction.Name}]";
                    }
                    else
                    {
                        value = "(not found)";
                    }

                    AddDetailRow(detailsPanel, $"{name}:", value);
                }
            }
            else
            {
                // For script-based function
                if (!string.IsNullOrEmpty(function.Script))
                {
                    AddDetailHeader(detailsPanel, "Script (Base64)");
                    AddDetailTextBox(detailsPanel, function.Script);

                    // Try to decode the script
                    try
                    {
                        var interpreter = new VBScriptInterpreter();
                        string decodedScript = interpreter.DecodeBase64Script(function.Script);

                        AddDetailHeader(detailsPanel, "Decoded Script");
                        AddDetailTextBox(detailsPanel, decodedScript);
                    }
                    catch { /* Ignore decoding errors */ }
                }

                AddDetailHeader(detailsPanel, "Input Variables");
                foreach (var sourceId in function.InputDataSourceIds)
                {
                    string name = model.VariableIdToNameMap.TryGetValue(sourceId, out string varName) ?
                        varName : sourceId;

                    string value;
                    if (model.Variables.TryGetValue(sourceId, out Variable variable))
                    {
                        value = variable.SampleValue ?? "(empty)";
                    }
                    else if (model.Functions.TryGetValue(sourceId, out Function sourceFunction))
                    {
                        // Show that this is a function reference
                        value = $"[Function: {sourceFunction.Name}]";
                    }
                    else
                    {
                        value = "(not found)";
                    }

                    AddDetailRow(detailsPanel, $"{name}:", value);
                }
            }

            AddDetailHeader(detailsPanel, "Result");
            AddDetailTextBox(detailsPanel, result);

            // Dependencies tab - Visual representation of dependencies
            var dependenciesTab = new TabItem { Header = "Dependencies" };
            tabControl.Items.Add(dependenciesTab);

            var dependenciesScroll = new ScrollViewer();
            dependenciesTab.Content = dependenciesScroll;

            var dependenciesPanel = new StackPanel { Margin = new Thickness(10) };
            dependenciesScroll.Content = dependenciesPanel;

            CreateDependencyTree(dependenciesPanel, model, function);

            // Add close button
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10)
            };
            Grid.SetRow(buttonPanel, 1);
            grid.Children.Add(buttonPanel);

            var closeButton = new Button
            {
                Content = "Close",
                Padding = new Thickness(10, 5, 10, 5)
            };
            closeButton.Click += (s, e) => dialog.Close();
            buttonPanel.Children.Add(closeButton);

            // Show the dialog
            dialog.ShowDialog();
        }

        /// <summary>
        /// Creates a visual tree of function dependencies
        /// </summary>
        private static void CreateDependencyTree(StackPanel panel, LabelModel model, Function function)
        {
            // Start with the root function
            var treeView = new TreeView { Margin = new Thickness(0, 10, 0, 0) };
            panel.Children.Add(treeView);

            var rootNode = CreateFunctionTreeItem(function);
            treeView.Items.Add(rootNode);

            // Recursively add dependencies
            PopulateDependencies(rootNode, model, function);

            // Expand the root node
            rootNode.IsExpanded = true;
        }

        /// <summary>
        /// Recursively populates function dependencies
        /// </summary>
        private static void PopulateDependencies(TreeViewItem parentNode, LabelModel model, Function function)
        {
            // Track processed IDs to avoid circular references
            HashSet<string> processedIds = new HashSet<string>();
            processedIds.Add(function.Id);

            // Get all input dependencies
            List<string> dependencyIds = new List<string>(function.InputDataSourceIds);

            // For concatenate functions, also include their data sources
            if (function is ConcatenateFunction concatFunction)
            {
                dependencyIds.AddRange(concatFunction.DataSourceIds);
            }

            // Process each dependency
            foreach (string dependencyId in dependencyIds)
            {
                TreeViewItem dependencyNode;

                if (model.Variables.TryGetValue(dependencyId, out Variable variable))
                {
                    // Variable dependency
                    dependencyNode = new TreeViewItem
                    {
                        Header = $"Variable: {variable.Name} = {variable.SampleValue ?? "(empty)"}",
                        Foreground = Brushes.Blue
                    };
                }
                else if (model.Functions.TryGetValue(dependencyId, out Function dependencyFunction))
                {
                    // Function dependency
                    dependencyNode = CreateFunctionTreeItem(dependencyFunction);

                    // Recursively add sub-dependencies if not already processed
                    if (!processedIds.Contains(dependencyId))
                    {
                        processedIds.Add(dependencyId);
                        PopulateDependencies(dependencyNode, model, dependencyFunction);
                    }
                    else
                    {
                        // Circular reference
                        var circularNode = new TreeViewItem
                        {
                            Header = "Circular Reference",
                            Foreground = Brushes.Red
                        };
                        dependencyNode.Items.Add(circularNode);
                    }
                }
                else
                {
                    // Missing dependency
                    dependencyNode = new TreeViewItem
                    {
                        Header = $"Missing: {dependencyId}",
                        Foreground = Brushes.Red
                    };
                }

                parentNode.Items.Add(dependencyNode);
            }
        }

        /// <summary>
        /// Creates a TreeViewItem for a function
        /// </summary>
        private static TreeViewItem CreateFunctionTreeItem(Function function)
        {
            string typeIndicator = function is ConcatenateFunction ? "Concatenate" : "Script";

            return new TreeViewItem
            {
                Header = $"Function: {function.Name} ({typeIndicator})",
                Foreground = Brushes.Green
            };
        }

        /// <summary>
        /// Shows a dialog with a list of all functions in the model
        /// </summary>
        public static void ShowFunctionsList(Window owner, LabelModel model)
        {
            if (model == null || model.Functions.Count == 0)
            {
                MessageBox.Show("No functions loaded. Please open a label file first.",
                    "No Functions", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Create a dialog to display functions
            var dialog = new Window
            {
                Title = "Test Functions",
                Width = 500,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = owner
            };

            var mainGrid = new Grid();
            dialog.Content = mainGrid;

            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Add header
            var headerPanel = new StackPanel { Margin = new Thickness(10, 10, 10, 0) };
            Grid.SetRow(headerPanel, 0);
            mainGrid.Children.Add(headerPanel);

            headerPanel.Children.Add(new TextBlock
            {
                Text = "Select a function to test:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            });

            // Add filter textbox
            var filterPanel = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
            headerPanel.Children.Add(filterPanel);

            filterPanel.Children.Add(new TextBlock
            {
                Text = "Filter:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0)
            });

            var filterBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 0)
            };
            DockPanel.SetDock(filterBox, Dock.Right);
            filterPanel.Children.Add(filterBox);

            // Create list view for functions with type indicators
            var listView = new ListView
            {
                Margin = new Thickness(10, 0, 10, 10)
            };

            // Add columns
            var gridView = new GridView();
            listView.View = gridView;

            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Name",
                Width = 200,
                DisplayMemberBinding = new System.Windows.Data.Binding("Name")
            });

            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Type",
                Width = 120,
                DisplayMemberBinding = new System.Windows.Data.Binding("Type")
            });

            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Dependencies",
                Width = 130,
                DisplayMemberBinding = new System.Windows.Data.Binding("Dependencies")
            });

            // Add functions to the list
            var functionsData = new List<FunctionListItem>();
            foreach (var function in model.Functions.Values)
            {
                string type = function is ConcatenateFunction ? "Concatenate" : "Script";

                // Count total dependencies
                int dependencyCount = function.InputDataSourceIds.Count;
                if (function is ConcatenateFunction concatFunction)
                {
                    dependencyCount += concatFunction.DataSourceIds.Count;
                }

                functionsData.Add(new FunctionListItem
                {
                    Id = function.Id,
                    Name = function.Name,
                    Type = type,
                    Dependencies = dependencyCount > 0 ? dependencyCount.ToString() : "None"
                });
            }

            listView.ItemsSource = functionsData;

            // Setup filtering
            filterBox.TextChanged += (s, e) =>
            {
                string filter = filterBox.Text.ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(filter))
                {
                    listView.ItemsSource = functionsData;
                }
                else
                {
                    listView.ItemsSource = functionsData
                        .Where(f => f.Name.ToLowerInvariant().Contains(filter) ||
                                   f.Type.ToLowerInvariant().Contains(filter))
                        .ToList();
                }
            };

            Grid.SetRow(listView, 1);
            mainGrid.Children.Add(listView);

            // Add buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10)
            };

            var testButton = new Button
            {
                Content = "Test Function",
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0, 0, 10, 0),
                IsEnabled = false
            };

            var closeButton = new Button
            {
                Content = "Close",
                Padding = new Thickness(10, 5, 10, 5)
            };

            // Wire up events
            listView.SelectionChanged += (s, e) =>
            {
                testButton.IsEnabled = listView.SelectedItem != null;
            };

            testButton.Click += (s, e) =>
            {
                if (listView.SelectedItem is FunctionListItem item)
                {
                    TestFunction(model, item.Id);
                }
            };

            closeButton.Click += (s, e) => dialog.Close();

            // Add controls to the grid
            buttonPanel.Children.Add(testButton);
            buttonPanel.Children.Add(closeButton);

            Grid.SetRow(buttonPanel, 2);
            mainGrid.Children.Add(buttonPanel);

            // Show dialog
            dialog.ShowDialog();
        }

        #region Helper Methods

        /// <summary>
        /// Adds a section header to the details panel
        /// </summary>
        private static void AddDetailHeader(StackPanel panel, string text)
        {
            panel.Children.Add(new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 5),
                TextDecorations = TextDecorations.Underline
            });
        }

        /// <summary>
        /// Adds a detail row with label and value
        /// </summary>
        private static void AddDetailRow(StackPanel panel, string label, string value)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            grid.Children.Add(new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 2, 10, 2)
            });

            var valueText = new TextBlock
            {
                Text = value,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 2)
            };
            Grid.SetColumn(valueText, 1);
            grid.Children.Add(valueText);

            panel.Children.Add(grid);
        }

        /// <summary>
        /// Adds a text box with content that can be scrolled and copied
        /// </summary>
        private static void AddDetailTextBox(StackPanel panel, string content)
        {
            var textBox = new TextBox
            {
                Text = content,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 200,
                Margin = new Thickness(0, 5, 0, 10),
                Padding = new Thickness(5)
            };

            panel.Children.Add(textBox);
        }

        /// <summary>
        /// Replaces special characters with their escaped representation for display
        /// </summary>
        private static string ReplaceSpecialChars(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "(empty)";

            return input
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

        /// <summary>
        /// Data class for function list items
        /// </summary>
        private class FunctionListItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Dependencies { get; set; }
        }

        #endregion
    }
}