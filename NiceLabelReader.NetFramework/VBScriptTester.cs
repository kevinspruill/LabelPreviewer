using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace LabelPreviewer
{
    /// <summary>
    /// Helper class to test VBScript execution
    /// </summary>
    public class VBScriptTester
    {
        private VBScriptInterpreter interpreter;

        public VBScriptTester()
        {
            interpreter = new VBScriptInterpreter();
        }

        /// <summary>
        /// Test execution of a base64-encoded script
        /// </summary>
        public void TestScript(string base64Script, Dictionary<string, string> variables)
        {
            try
            {
                // Decode the script
                string script = interpreter.DecodeBase64Script(base64Script);

                // Show the decoded script in a message box
                MessageBox.Show($"Decoded Script:\n\n{script}", "Decoded Script");

                // Execute the script using the new approach - directly set variables and run decoded script
                interpreter.Reset();

                // Set all variables directly by their names
                foreach (var pair in variables)
                {
                    interpreter.SetVariable(pair.Key, pair.Value);
                }

                // Execute the decoded script directly
                object result = interpreter.ExecuteScript(script);

                // Show the result
                MessageBox.Show($"Script Result: {result}", "Script Result");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Script Execution Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Creates a VBScript testing UI
        /// </summary>
        public static void ShowScriptTestingUI(Window owner)
        {
            // Create a new window for testing
            Window testWindow = new Window
            {
                Title = "VBScript Tester",
                Width = 600,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = owner
            };

            // Create UI controls
            var mainGrid = new System.Windows.Controls.Grid();
            testWindow.Content = mainGrid;

            // Add row definitions
            mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(30) });
            mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(30) });
            mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(50) });

            // Add labels
            var scriptLabel = new System.Windows.Controls.Label { Content = "Base64 Encoded Script:" };
            System.Windows.Controls.Grid.SetRow(scriptLabel, 0);
            mainGrid.Children.Add(scriptLabel);

            var variablesLabel = new System.Windows.Controls.Label { Content = "Variables (Name=Value, one per line):" };
            System.Windows.Controls.Grid.SetRow(variablesLabel, 2);
            mainGrid.Children.Add(variablesLabel);

            // Add text boxes
            var scriptTextBox = new System.Windows.Controls.TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            System.Windows.Controls.Grid.SetRow(scriptTextBox, 1);
            mainGrid.Children.Add(scriptTextBox);

            var variablesTextBox = new System.Windows.Controls.TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            System.Windows.Controls.Grid.SetRow(variablesTextBox, 3);
            mainGrid.Children.Add(variablesTextBox);

            // Add button panel
            var buttonPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10)
            };
            System.Windows.Controls.Grid.SetRow(buttonPanel, 4);
            mainGrid.Children.Add(buttonPanel);

            // Add buttons
            var executeButton = new System.Windows.Controls.Button
            {
                Content = "Execute Script",
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0, 0, 10, 0)
            };
            buttonPanel.Children.Add(executeButton);

            var closeButton = new System.Windows.Controls.Button
            {
                Content = "Close",
                Padding = new Thickness(10, 5, 10, 5)
            };
            buttonPanel.Children.Add(closeButton);

            // Add event handlers
            executeButton.Click += (sender, e) =>
            {
                try
                {
                    // Parse variables
                    var variables = new Dictionary<string, string>();
                    foreach (var line in variablesTextBox.Text.Split('\n'))
                    {
                        string trimmedLine = line.Trim();
                        if (string.IsNullOrEmpty(trimmedLine)) continue;

                        int equalPos = trimmedLine.IndexOf('=');
                        if (equalPos > 0)
                        {
                            string name = trimmedLine.Substring(0, equalPos).Trim();
                            string value = trimmedLine.Substring(equalPos + 1).Trim();
                            variables[name] = value;
                        }
                    }

                    // Create tester and execute script
                    var tester = new VBScriptTester();
                    tester.TestScript(scriptTextBox.Text, variables);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            closeButton.Click += (sender, e) => testWindow.Close();

            // Show the window
            testWindow.ShowDialog();
        }
    }
}