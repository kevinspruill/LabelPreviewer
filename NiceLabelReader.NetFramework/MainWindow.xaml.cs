using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using NiceLabelReader.NetFramework;
using System.Windows.Shapes;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps;

namespace LabelPreviewer
{
    public partial class MainWindow : Window
    {
        private string variablesXmlPath;
        private string labelFilePath;
        private string formatXmlPath;
        private LabelModel labelModel;
        private VariableEditorWindow variableEditorWindow;

        public MainWindow()
        {
            InitializeComponent();

            // Enable high DPI rendering
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

            labelModel = new LabelModel();
            labelModel.ShowDebugInfo = true; // Default to showing debug info

            // Delay setting up controls to ensure they're properly initialized
            this.Loaded += (s, e) => {
                if (previewViewbox != null && previewScrollViewer != null && cbFitToWindow != null)
                {
                    // Apply initial viewbox configuration
                    previewViewbox.Stretch = Stretch.Uniform;
                    previewScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    previewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
            };
        }
        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void menuScriptTester_Click(object sender, RoutedEventArgs e)
        {
            VBScriptTester.ShowScriptTestingUI(this);
        }

        private void menuViewVariables_Click(object sender, RoutedEventArgs e)
        {
            if (labelModel == null || labelModel.Variables.Count == 0)
            {
                MessageBox.Show("No variables loaded. Please open a label file first.",
                    "No Variables", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Create a window to display variables
            var window = new Window
            {
                Title = "Label Variables",
                Width = 600,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            // Create a data grid to display variables
            var grid = new DataGrid
            {
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Margin = new Thickness(10)
            };

            // Add columns
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "ID",
                Binding = new System.Windows.Data.Binding("Id"),
                Width = 250
            });
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Name",
                Binding = new System.Windows.Data.Binding("Name"),
                Width = 150
            });
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Value",
                Binding = new System.Windows.Data.Binding("SampleValue"),
                Width = 150
            });

            // Set the item source
            grid.ItemsSource = labelModel.Variables.Values;

            // Set the grid as the window content
            window.Content = grid;

            // Show the window
            window.ShowDialog();
        }

        private void menuViewFunctions_Click(object sender, RoutedEventArgs e)
        {
            if (labelModel == null || labelModel.Functions.Count == 0)
            {
                MessageBox.Show("No functions loaded. Please open a label file first.",
                    "No Functions", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Create a window to display functions
            var window = new Window
            {
                Title = "Label Functions",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            // Create a grid layout
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Create a data grid for functions list
            var functionsGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Margin = new Thickness(10)
            };

            // Add columns for functions grid
            functionsGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "ID",
                Binding = new System.Windows.Data.Binding("Id"),
                Width = 250
            });
            functionsGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Name",
                Binding = new System.Windows.Data.Binding("Name"),
                Width = 150
            });
            functionsGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Value",
                Binding = new System.Windows.Data.Binding("SampleValue"),
                Width = 150
            });

            // Add functions to the grid
            functionsGrid.ItemsSource = labelModel.Functions.Values;

            // Create a detail view for script
            var scriptPanel = new StackPanel { Margin = new Thickness(10) };

            var scriptLabel = new Label { Content = "Script (Base64 Encoded):" };
            scriptPanel.Children.Add(scriptLabel);

            var scriptBox = new TextBox
            {
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Height = 100
            };
            scriptPanel.Children.Add(scriptBox);

            var decodedScriptLabel = new Label { Content = "Decoded Script:" };
            scriptPanel.Children.Add(decodedScriptLabel);

            var decodedScriptBox = new TextBox
            {
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Height = 120
            };
            scriptPanel.Children.Add(decodedScriptBox);

            var variablesLabel = new Label { Content = "Input Variables:" };
            scriptPanel.Children.Add(variablesLabel);

            var variablesBox = new TextBox
            {
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Height = 80
            };
            scriptPanel.Children.Add(variablesBox);

            // Button to test the script
            var testButton = new Button
            {
                Content = "Test Script",
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            scriptPanel.Children.Add(testButton);

            // Handle selection changed event
            functionsGrid.SelectionChanged += (s, args) =>
            {
                if (functionsGrid.SelectedItem is Function selectedFunction)
                {
                    // Display the script
                    scriptBox.Text = selectedFunction.Script ?? selectedFunction.ScriptWithReferences ?? "";

                    // Try to decode and display the script
                    if (!string.IsNullOrEmpty(scriptBox.Text))
                    {
                        try
                        {
                            var interpreter = new VBScriptInterpreter();
                            decodedScriptBox.Text = interpreter.DecodeBase64Script(scriptBox.Text);
                        }
                        catch (Exception ex)
                        {
                            decodedScriptBox.Text = $"Error decoding script: {ex.Message}";
                        }
                    }
                    else
                    {
                        decodedScriptBox.Text = "No script available.";
                    }

                    // Display the input variables with their friendly names
                    StringBuilder sb = new StringBuilder();
                    foreach (var sourceId in selectedFunction.InputDataSourceIds)
                    {
                        string friendlyName = labelModel.VariableIdToNameMap.TryGetValue(sourceId, out string name)
                            ? name : sourceId;

                        string value = labelModel.Variables.TryGetValue(sourceId, out Variable variable)
                            ? variable.SampleValue ?? "(empty)"
                            : "(not found)";

                        sb.AppendLine($"{friendlyName} = {value}");
                    }
                    variablesBox.Text = sb.ToString();

                    // Enable/disable test button based on script availability
                    testButton.IsEnabled = !string.IsNullOrEmpty(scriptBox.Text);
                }
                else
                {
                    scriptBox.Text = "";
                    decodedScriptBox.Text = "";
                    variablesBox.Text = "";
                    testButton.IsEnabled = false;
                }
            };

            // Handle test button click
            testButton.Click += (s, args) =>
            {
                if (functionsGrid.SelectedItem is Function selectedFunction)
                {
                    try
                    {
                        // Prepare variables using friendly names
                        var variableValues = new Dictionary<string, string>();

                        // Add all variables by friendly name
                        foreach (var pair in labelModel.Variables)
                        {
                            string friendlyName = labelModel.VariableIdToNameMap.TryGetValue(pair.Key, out string name)
                                ? name : pair.Key;
                            variableValues[friendlyName] = pair.Value.SampleValue ?? string.Empty;
                        }

                        // Get script
                        string scriptToUse = !string.IsNullOrEmpty(selectedFunction.Script)
                            ? selectedFunction.Script
                            : selectedFunction.Script;

                        if (!string.IsNullOrEmpty(scriptToUse))
                        {
                            // Create interpreter and decode script
                            var interpreter = new VBScriptInterpreter();
                            string decodedScript = interpreter.DecodeBase64Script(scriptToUse);

                            // Set variables directly
                            foreach (var pair in variableValues)
                            {
                                interpreter.SetVariable(pair.Key, pair.Value);
                            }

                            // Execute decoded script
                            object result = interpreter.ExecuteScript(decodedScript);

                            // Show result
                            MessageBox.Show($"Script Result: {result}", "Script Result");
                        }
                        else
                        {
                            MessageBox.Show("No script available to test.", "No Script", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error testing script: {ex.Message}", "Script Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            };

            // Add controls to the grid
            Grid.SetRow(functionsGrid, 0);
            grid.Children.Add(functionsGrid);

            // Add a splitter
            var splitter = new GridSplitter
            {
                Height = 5,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = Brushes.LightGray
            };
            Grid.SetRow(splitter, 1);
            grid.Children.Add(splitter);

            Grid.SetRow(scriptPanel, 2);
            grid.Children.Add(scriptPanel);

            // Set the grid as the window content
            window.Content = grid;

            // Show the window
            window.ShowDialog();
        }
        private void btnOpenFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "NiceLabel Label File (*.nlbl)|*.nlbl",
                Title = "Select Label File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                labelFilePath = openFileDialog.FileName;
                btnRenderPreview.IsEnabled = true;
                btnRenderPreview_Click(sender, e);
            }
        }

        private void btnViewXml_Click(object sender, RoutedEventArgs e)
        {
            if (labelModel == null || string.IsNullOrEmpty(labelModel.VariablesXmlData))
            {
                MessageBox.Show("No XML data available. Please open a label file first.",
                    "No XML Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Create and show the XML viewer window
            XmlViewerWindow xmlViewer = new XmlViewerWindow(labelModel.VariablesXmlData, labelModel.FormatXmlData)
            {
                Owner = this
            };
            xmlViewer.ShowDialog();
        }

        private void btnEditVariables_Click(object sender, RoutedEventArgs e)
        {
            if (labelModel == null || labelModel.Variables.Count == 0)
            {
                MessageBox.Show("No variables available. Please open a label file first.",
                    "No Variables", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Create the variable editor window if it doesn't exist, or update it
            if (variableEditorWindow == null)
            {
                variableEditorWindow = new VariableEditorWindow(labelModel)
                {
                    Owner = this
                };

                // Subscribe to the VariablesChanged event
                variableEditorWindow.VariablesChanged += (s, changedVariables) =>
                {
                    // Re-render the label with updated variables
                    labelModel.Render(previewCanvas);
                };

                // Handle the window closed event
                variableEditorWindow.Closed += (s, args) => variableEditorWindow = null;

                variableEditorWindow.Show();
            }
            else
            {
                // Update the existing window with the current model
                variableEditorWindow.UpdateModel(labelModel);
                variableEditorWindow.Activate(); // Bring to front
            }
        }
        private void menuTestFunctions_Click(object sender, RoutedEventArgs e)
        {
            if (labelModel == null || string.IsNullOrEmpty(labelFilePath))
            {
                MessageBox.Show("Please load a label file first.",
                    "No Label Loaded", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FunctionTester.ShowFunctionsList(this, labelModel);
        }

        private void TestSpecificFunction()
        {
            if (labelModel == null || string.IsNullOrEmpty(labelFilePath))
            {
                MessageBox.Show("Please load a label file first.",
                    "No Label Loaded", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // The specific function GUID you mentioned
            string functionGuid = "246def0c-4bd4-4a59-885f-901b15ae3eee";

            // Dump the function information first
            labelModel.DumpFunctionInfo();

            // Log to debug output
            System.Diagnostics.Debug.WriteLine($"Testing specific function GUID: {functionGuid}");

            // Check direct match
            bool functionFound = false;
            if (labelModel.Functions.ContainsKey(functionGuid))
            {
                System.Diagnostics.Debug.WriteLine("Function found directly by GUID");
                functionFound = true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Function not found directly by GUID");

                // Look for a function with the name "DescriptionFields"
                string targetName = "DescriptionFields";
                foreach (var func in labelModel.Functions.Values)
                {
                    if (func.Name == targetName)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found function by name '{targetName}' with ID '{func.Id}'");
                        functionGuid = func.Id;
                        functionFound = true;
                        break;
                    }
                }
            }

            if (functionFound)
            {
                // Test the function
                FunctionTester.TestFunction(labelModel, functionGuid);
            }
            else
            {
                // If still not found, try to manually create and test it
                MessageBox.Show(
                    "Specified function GUID not found in model. Do you want to manually create a test function?",
                    "Function Not Found", MessageBoxButton.YesNo, MessageBoxImage.Question);

                // This would go to a routine that manually creates a test function
            }
        }

        // Add a new menu item to your XAML:
        // <MenuItem x:Name="menuTestSpecificFunction" Click="menuTestSpecificFunction_Click" Header="Test Concatenate Function" />

        // Add the click handler:
        private void menuTestSpecificFunction_Click(object sender, RoutedEventArgs e)
        {
            TestSpecificFunction();
        }

        // For quick testing of the concatenation function specifically, you can add this method:
        private void TestConcatenateFunction()
        {
            if (labelModel == null || string.IsNullOrEmpty(labelFilePath))
            {
                MessageBox.Show("Please load a label file first.",
                    "No Label Loaded", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Find the concatenate function with name "DescriptionFields"
            string concatFunctionId = null;
            foreach (var function in labelModel.Functions.Values)
            {
                if (function.Name == "DescriptionFields" && function is ConcatenateFunction)
                {
                    concatFunctionId = function.Id;
                    break;
                }
            }

            if (concatFunctionId != null)
            {
                FunctionTester.TestFunction(labelModel, concatFunctionId);
            }
            else
            {
                MessageBox.Show("Concatenate function 'DescriptionFields' not found in this label.",
                    "Function Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Handles the Print button click event with proper positioning and margins
        /// </summary>
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (labelModel == null || previewCanvas.Children.Count == 0)
            {
                MessageBox.Show("Nothing to print. Please open a label file first.",
                    "No Content", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // Create print dialog
                PrintDialog printDialog = new PrintDialog();

                // Show print dialog - exit if canceled
                if (printDialog.ShowDialog() != true)
                {
                    Mouse.OverrideCursor = null;
                    return;
                }

                // Get the physical dimensions of the label in millimeters
                double labelWidthInMillimeters = labelModel.Width * 25.4 / 96;  // Convert WPF units to mm
                double labelHeightInMillimeters = labelModel.Height * 25.4 / 96; // Convert WPF units to mm

                // Get information about the printable area
                double printableAreaWidth = printDialog.PrintableAreaWidth;
                double printableAreaHeight = printDialog.PrintableAreaHeight;

                System.Diagnostics.Debug.WriteLine($"Printable area: {printableAreaWidth:F2} × {printableAreaHeight:F2}");

                // Create a print-specific canvas with white background
                Canvas printCanvas = new Canvas
                {
                    Width = labelModel.Width,
                    Height = labelModel.Height,
                    Background = Brushes.White
                };

                // Copy all elements from preview canvas to print canvas without debug info
                CopyCanvasElementsForPrinting(previewCanvas, printCanvas);

                // Convert to 1/1000 inch for page media size
                double widthInThousandthsOfInch = labelWidthInMillimeters / 25.4 * 1000;
                double heightInThousandthsOfInch = labelHeightInMillimeters / 25.4 * 1000;

                // Create a visual brush from the print canvas
                VisualBrush canvasBrush = new VisualBrush(printCanvas);

                // Create a visual for printing
                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    // Default margin in device units (1/96 inch)
                    double leftMargin = 96; // 1 inch left margin
                    double topMargin = 48;  // 0.5 inch top margin

                    // Calculate position to center the content within printable area if needed
                    double xPos = (printableAreaWidth - labelModel.Width) / 2;
                    double yPos = (printableAreaHeight - labelModel.Height) / 2;

                    // Ensure we don't position less than our minimum margins
                    xPos = Math.Max(leftMargin, xPos);
                    yPos = Math.Max(topMargin, yPos);

                    // Draw the content at the calculated position
                    drawingContext.DrawRectangle(
                        canvasBrush,
                        null,
                        new Rect(xPos, yPos, labelModel.Width, labelModel.Height));
                }

                // Set the print ticket for physical page size and DPI
                PrintTicket printTicket = printDialog.PrintTicket;
                if (printTicket != null)
                {
                    try
                    {
                        // For custom page size that's smaller than the printer's minimum,
                        // it's often better to use the default page size and position the content
                        // where we want it.

                        // Set high print resolution (300 DPI)
                        printTicket.PageResolution = new PageResolution(300, 300);

                        // Tell the printer not to scale our content
                        printTicket.PageScalingFactor = 100; // 100% scale

                        // Apply the ticket
                        printDialog.PrintTicket = printTicket;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Could not set print ticket: {ex.Message}");
                    }
                }

                // Print the visual
                printDialog.PrintVisual(drawingVisual, $"NiceLabel - {System.IO.Path.GetFileNameWithoutExtension(labelFilePath)}");

                MessageBox.Show($"Print job sent to {printDialog.PrintQueue.Name} at 300 DPI with proper positioning.",
                    "Print", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Creates a clean copy of the canvas elements for printing,
        /// removing any debug visualization elements
        /// </summary>
        private void CopyCanvasElementsForPrinting(Canvas sourceCanvas, Canvas targetCanvas)
        {
            // First, clear debug info setting temporarily to avoid debug visuals in print
            bool originalDebugMode = labelModel.ShowDebugInfo;
            labelModel.ShowDebugInfo = false;

            // Re-render the entire label without debug info
            targetCanvas.Children.Clear();
            labelModel.Render(targetCanvas);

            // Restore original debug mode
            labelModel.ShowDebugInfo = originalDebugMode;
        }

        private void btnRenderPreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                labelModel.LoadFromXml(labelFilePath);
                labelModel.Render(previewCanvas);

                // Update canvas size to match the label dimensions
                previewCanvas.Width = labelModel.Width;
                previewCanvas.Height = labelModel.Height;

                // Show information about the label
                this.Title = $"NiceLabel Previewer - {System.IO.Path.GetFileName(labelFilePath)} ({labelModel.Width:F0}x{labelModel.Height:F0})";

                // Enable debug buttons since we have a label loaded
                btnViewXml.IsEnabled = true;
                btnEditVariables.IsEnabled = true;

                Mouse.OverrideCursor = null;

                // Add checkbox controls for zoom options
                AddZoomControls();
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show($"Error rendering preview: {ex.Message}",
                    "Render Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cbFitToWindow_Checked(object sender, RoutedEventArgs e)
        {
            if (previewViewbox != null)
                previewViewbox.Stretch = Stretch.Uniform;

            if (previewScrollViewer != null)
            {
                previewScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                previewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
        }

        private void cbFitToWindow_Unchecked(object sender, RoutedEventArgs e)
        {
            if (previewViewbox != null)
                previewViewbox.Stretch = Stretch.None;

            if (previewScrollViewer != null)
            {
                previewScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                previewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }

        private void cbShowDebugInfo_Checked(object sender, RoutedEventArgs e)
        {
            if (labelModel != null)
            {
                labelModel.ShowDebugInfo = true;
                if (labelFilePath != null)
                {
                    // Re-render to show debug info
                    btnRenderPreview_Click(sender, e);
                }
            }
        }

        private void cbShowDebugInfo_Unchecked(object sender, RoutedEventArgs e)
        {
            if (labelModel != null)
            {
                labelModel.ShowDebugInfo = false;
                if (labelFilePath != null)
                {
                    // Re-render to hide debug info
                    btnRenderPreview_Click(sender, e);
                }
            }
        }

        private void btnAddTextElement_Click(object sender, RoutedEventArgs e)
        {
            // Check if a label is loaded
            if (labelModel == null || string.IsNullOrEmpty(labelFilePath))
            {
                MessageBox.Show("Please load a label file first.",
                    "No Label Loaded", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Determine text element type
            bool isTextBox = false;
            if (cmbTextElementType.SelectedIndex == 1) // Text Box
            {
                isTextBox = true;
            }

            // Create a text input dialog
            TextInputDialog dialog = new TextInputDialog(isTextBox);
            if (dialog.ShowDialog() == true)
            {
                // Add the new text element to the model
                DocumentItem newItem;

                if (isTextBox)
                {
                    TextBoxItem textBox = new TextBoxItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "New Text Box",
                        Content = dialog.TextContent,
                        X = dialog.X,
                        Y = dialog.Y,
                        Width = dialog.Width,
                        Height = dialog.Height,
                        FontName = dialog.FontName,
                        FontSize = dialog.FontSize,
                        AnchoringPoint = (AnchoringPoint)dialog.AnchorPoint,
                        TextWrapping = TextWrapping.Wrap
                    };

                    newItem = textBox;
                }
                else
                {
                    TextObjectItem textObject = new TextObjectItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "New Text Object",
                        Content = dialog.TextContent,
                        X = dialog.X,
                        Y = dialog.Y,
                        FontName = dialog.FontName,
                        FontSize = dialog.FontSize,
                        AnchoringPoint = (AnchoringPoint)dialog.AnchorPoint
                    };

                    newItem = textObject;
                }

                // Add to model and re-render
                labelModel.DocumentItems.Add(newItem);
                labelModel.Render(previewCanvas);
            }
        }

        private void AddZoomControls()
        {
            // Ensure the fit to window checkbox is checked
            cbFitToWindow.IsChecked = true;
            // Initialize the viewbox stretch mode
            previewViewbox.Stretch = Stretch.Uniform;
            previewScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            previewScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }

        // Add these handlers to your MainWindow.xaml.cs:
        private void menuTroubleshootFunction_Click(object sender, RoutedEventArgs e)
        {
            if (labelModel == null || string.IsNullOrEmpty(labelFilePath))
            {
                MessageBox.Show("Please load a label file first.",
                    "No Label Loaded", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Show an input dialog to get the reference
            InputDialog dialog = new InputDialog("Enter Function Reference", "Enter a function ID or name to troubleshoot:");
            if (dialog.ShowDialog() == true)
            {
                string reference = dialog.ResponseText;
                if (!string.IsNullOrWhiteSpace(reference))
                {
                    FunctionTroubleshooter.TroubleshootFunctionReference(this, labelModel, reference);
                }
            }
        }

        private void menuTestDescriptionFields_Click(object sender, RoutedEventArgs e)
        {
            if (labelModel == null || string.IsNullOrEmpty(labelFilePath))
            {
                MessageBox.Show("Please load a label file first.",
                    "No Label Loaded", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // The DescriptionFields function GUID
            string functionGuid = "246def0c-4bd4-4a59-885f-901b15ae3eee";

            // First try by GUID
            if (labelModel.Functions.ContainsKey(functionGuid))
            {
                FunctionTester.TestFunction(labelModel, functionGuid);
                return;
            }

            // Then try by name
            if (labelModel.FunctionNameToIdMap.ContainsKey("DescriptionFields"))
            {
                FunctionTester.TestFunction(labelModel, "DescriptionFields");
                return;
            }

            // If not found, offer to create it
            if (MessageBox.Show(
                "The DescriptionFields function was not found in the model. Do you want to create it?",
                "Function Not Found", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                // Create the function (implementation from FunctionTroubleshooter)
                var concatFunction = new ConcatenateFunction
                {
                    Id = functionGuid,
                    Name = "DescriptionFields",
                    FunctionType = "ConcatenateFunction",
                    Separator = "\r\n", // Decoded from Base64 "DQo="
                    IgnoreEmptyValues = false
                };

                // Add the data sources (from your example)
                concatFunction.DataSourceIds.Add("9b19b3d6-fd8e-4250-b84e-64fa7bd7a049"); // Description1
                concatFunction.DataSourceIds.Add("d8409940-a513-4e07-8cda-b92361625140"); // Description2

                // Add the function to the model
                labelModel.Functions[concatFunction.Id] = concatFunction;

                // Update the function name mappings
                labelModel.FunctionIdToNameMap[concatFunction.Id] = concatFunction.Name;
                labelModel.FunctionNameToIdMap[concatFunction.Name] = concatFunction.Id;

                // Test the newly created function
                FunctionTester.TestFunction(labelModel, functionGuid);
            }
            else
            {
                // Show troubleshooter for advanced diagnosis
                FunctionTroubleshooter.TroubleshootFunctionReference(this, labelModel, functionGuid);
            }
        }

        private void TestFunctionRendering()
        {
            if (labelModel == null)
            {
                MessageBox.Show("Please load a label file first.",
                    "No Model", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Clear the canvas for testing
            previewCanvas.Children.Clear();

            // First verify/create the function
            labelModel.VerifyDocumentItemReferences();

            // Get the function
            string functionId = "246def0c-4bd4-4a59-885f-901b15ae3eee";
            if (!labelModel.Functions.TryGetValue(functionId, out Function function))
            {
                MessageBox.Show("Could not find the DescriptionFields function.",
                    "Function Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Execute the function
            string result = labelModel.ExecuteFunctionWithDependencies(functionId);

            // Create a test text box to display the result
            var textBox = new System.Windows.Controls.TextBox
            {
                Text = result,
                TextWrapping = TextWrapping.Wrap,
                Width = 300,
                Height = 150,
                Margin = new Thickness(10)
            };

            // Add to a popup window
            var window = new Window
            {
                Title = "Function Result Test",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = $"Function: {function.Name}",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(10, 10, 10, 5)
            });

            panel.Children.Add(new TextBlock
            {
                Text = "Result:",
                Margin = new Thickness(10, 5, 10, 5)
            });

            panel.Children.Add(textBox);

            var button = new Button
            {
                Content = "Close",
                Width = 80,
                Height = 30,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            button.Click += (s, e) => window.Close();
            panel.Children.Add(button);

            window.Content = panel;
            window.ShowDialog();

            // Re-render the label
            labelModel.Render(previewCanvas);
        }

        // Add a menu item:
        // <MenuItem x:Name="menuTestRendering" Click="menuTestRendering_Click" Header="Test Function Rendering" />

        private void menuTestRendering_Click(object sender, RoutedEventArgs e)
        {
            TestFunctionRendering();
        }

    }
    public class InputDialog : Window
    {
        private TextBox txtInput;
        public string ResponseText { get; private set; }

        public InputDialog(string title, string prompt)
        {
            this.Title = title;
            this.Width = 400;
            this.Height = 150;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ResizeMode = ResizeMode.NoResize;

            Grid grid = new Grid();
            grid.Margin = new Thickness(10);
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Prompt
            TextBlock promptBlock = new TextBlock
            {
                Text = prompt,
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(promptBlock, 0);
            grid.Children.Add(promptBlock);

            // Input text box
            txtInput = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(txtInput, 1);
            grid.Children.Add(txtInput);

            // Buttons
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Button okButton = new Button
            {
                Content = "OK",
                IsDefault = true,
                Width = 75,
                Height = 23,
                Margin = new Thickness(0, 0, 10, 0)
            };
            okButton.Click += (s, e) =>
            {
                ResponseText = txtInput.Text;
                this.DialogResult = true;
            };

            Button cancelButton = new Button
            {
                Content = "Cancel",
                IsCancel = true,
                Width = 75,
                Height = 23
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            this.Content = grid;
        }
    }
    // Dialog for entering text element properties
    public class TextInputDialog : Window
    {
        private TextBox txtContent;
        private TextBox txtX;
        private TextBox txtY;
        private TextBox txtWidth;
        private TextBox txtHeight;
        private TextBox txtFontSize;
        private ComboBox cmbFontName;
        private ComboBox cmbAnchorPoint;
        private Button btnOK;
        private Button btnCancel;

        public string TextContent { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public string FontName { get; private set; }
        public double FontSize { get; private set; }
        public int AnchorPoint { get; private set; }

        public TextInputDialog(bool isTextBox)
        {
            this.Title = isTextBox ? "Add Text Box" : "Add Text Object";
            this.Width = 400;
            this.Height = isTextBox ? 500 : 400;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ResizeMode = ResizeMode.NoResize;

            // Create the layout
            Grid grid = new Grid();
            grid.Margin = new Thickness(10);

            // Define rows
            for (int i = 0; i < (isTextBox ? 10 : 8); i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Content input
            AddLabel(grid, "Text Content:", 0, 0);
            txtContent = new TextBox { AcceptsReturn = isTextBox, TextWrapping = isTextBox ? TextWrapping.Wrap : TextWrapping.NoWrap, Height = isTextBox ? 60 : 23 };
            Grid.SetRow(txtContent, 0);
            Grid.SetColumn(txtContent, 1);
            Grid.SetRowSpan(txtContent, isTextBox ? 2 : 1);
            grid.Children.Add(txtContent);

            // Position
            int row = isTextBox ? 2 : 1;
            AddLabel(grid, "X Position:", row, 0);
            txtX = new TextBox();
            Grid.SetRow(txtX, row);
            Grid.SetColumn(txtX, 1);
            grid.Children.Add(txtX);

            row++;
            AddLabel(grid, "Y Position:", row, 0);
            txtY = new TextBox();
            Grid.SetRow(txtY, row);
            Grid.SetColumn(txtY, 1);
            grid.Children.Add(txtY);

            // Size (for text box only)
            if (isTextBox)
            {
                row++;
                AddLabel(grid, "Width:", row, 0);
                txtWidth = new TextBox();
                Grid.SetRow(txtWidth, row);
                Grid.SetColumn(txtWidth, 1);
                grid.Children.Add(txtWidth);

                row++;
                AddLabel(grid, "Height:", row, 0);
                txtHeight = new TextBox();
                Grid.SetRow(txtHeight, row);
                Grid.SetColumn(txtHeight, 1);
                grid.Children.Add(txtHeight);
            }

            // Font settings
            row++;
            AddLabel(grid, "Font:", row, 0);
            cmbFontName = new ComboBox();
            foreach (var font in Fonts.SystemFontFamilies.OrderBy(f => f.Source))
            {
                cmbFontName.Items.Add(font.Source);
            }
            cmbFontName.SelectedItem = "Arial";
            Grid.SetRow(cmbFontName, row);
            Grid.SetColumn(cmbFontName, 1);
            grid.Children.Add(cmbFontName);

            row++;
            AddLabel(grid, "Font Size:", row, 0);
            txtFontSize = new TextBox { Text = "12" };
            Grid.SetRow(txtFontSize, row);
            Grid.SetColumn(txtFontSize, 1);
            grid.Children.Add(txtFontSize);

            // Anchor point
            row++;
            AddLabel(grid, "Anchor Point:", row, 0);
            cmbAnchorPoint = new ComboBox();
            cmbAnchorPoint.Items.Add("Top-Left (1)");
            cmbAnchorPoint.Items.Add("Top-Center (2)");
            cmbAnchorPoint.Items.Add("Top-Right (3)");
            cmbAnchorPoint.Items.Add("Middle-Left (4)");
            cmbAnchorPoint.Items.Add("Middle-Center (5)");
            cmbAnchorPoint.Items.Add("Middle-Right (6)");
            cmbAnchorPoint.Items.Add("Bottom-Left (7)");
            cmbAnchorPoint.Items.Add("Bottom-Center (8)");
            cmbAnchorPoint.Items.Add("Bottom-Right (9)");
            cmbAnchorPoint.SelectedIndex = 0;
            Grid.SetRow(cmbAnchorPoint, row);
            Grid.SetColumn(cmbAnchorPoint, 1);
            grid.Children.Add(cmbAnchorPoint);

            // Buttons
            row++;
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            btnOK = new Button
            {
                Content = "OK",
                Width = 75,
                Height = 23,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button
            {
                Content = "Cancel",
                Width = 75,
                Height = 23,
                IsCancel = true
            };

            buttonPanel.Children.Add(btnOK);
            buttonPanel.Children.Add(btnCancel);

            Grid.SetRow(buttonPanel, row);
            Grid.SetColumn(buttonPanel, 0);
            Grid.SetColumnSpan(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            // Set default values
            txtX.Text = "100";
            txtY.Text = "100";
            if (isTextBox)
            {
                txtWidth.Text = "200";
                txtHeight.Text = "100";
            }

            this.Content = grid;
        }

        private void AddLabel(Grid grid, string text, int row, int column)
        {
            Label label = new Label
            {
                Content = text,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 5, 10, 5)
            };
            Grid.SetRow(label, row);
            Grid.SetColumn(label, column);
            grid.Children.Add(label);
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            // Validate and parse inputs
            if (string.IsNullOrWhiteSpace(txtContent.Text))
            {
                MessageBox.Show("Please enter text content.", "Missing Content", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(txtX.Text, out double x) ||
                !double.TryParse(txtY.Text, out double y) ||
                !double.TryParse(txtFontSize.Text, out double fontSize))
            {
                MessageBox.Show("Please enter valid numeric values for position and font size.",
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // For text box, also validate width and height
            if (txtWidth != null && txtHeight != null)
            {
                if (!double.TryParse(txtWidth.Text, out double width) ||
                    !double.TryParse(txtHeight.Text, out double height))
                {
                    MessageBox.Show("Please enter valid numeric values for width and height.",
                        "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                this.Width = width;
                this.Height = height;
            }

            // Parse anchor point (format is "Description (N)")
            string anchorText = cmbAnchorPoint.SelectedItem.ToString();
            int anchorPoint = int.Parse(anchorText.Substring(anchorText.LastIndexOf('(') + 1, 1));

            // Set properties
            this.TextContent = txtContent.Text;
            this.X = x;
            this.Y = y;
            this.FontName = cmbFontName.SelectedItem.ToString();
            this.FontSize = fontSize;
            this.AnchorPoint = anchorPoint;

            this.DialogResult = true;
            this.Close();
        }
    }
}