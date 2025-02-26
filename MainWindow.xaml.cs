using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace LabelPreviewer
{
    public partial class MainWindow : Window
    {
        private string variablesXmlPath;
        private string labelFilePath;
        private string formatXmlPath;
        private LabelModel labelModel;

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
            functionsGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Has Script",
                Binding = new System.Windows.Data.Binding("Script"),
                Width = 100,
                Visibility = Visibility.Collapsed
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

                    // Enable/disable test button based on script availability
                    testButton.IsEnabled = !string.IsNullOrEmpty(scriptBox.Text);
                }
                else
                {
                    scriptBox.Text = "";
                    decodedScriptBox.Text = "";
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
                        // Prepare variables for testing
                        var variableValues = new Dictionary<string, string>();

                        // Add all input data source variables
                        foreach (var sourceId in selectedFunction.InputDataSourceIds)
                        {
                            if (labelModel.Variables.TryGetValue(sourceId, out var variable))
                            {
                                variableValues[sourceId] = variable.SampleValue ?? string.Empty;
                            }
                        }

                        // Create tester and execute script
                        var tester = new VBScriptTester();
                        string scriptToUse = !string.IsNullOrEmpty(selectedFunction.ScriptWithReferences)
                            ? selectedFunction.ScriptWithReferences
                            : selectedFunction.Script;

                        if (!string.IsNullOrEmpty(scriptToUse))
                        {
                            tester.TestScript(scriptToUse, variableValues);
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
                this.Title = $"NiceLabel Previewer - {Path.GetFileName(labelFilePath)} ({labelModel.Width:F0}x{labelModel.Height:F0})";

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