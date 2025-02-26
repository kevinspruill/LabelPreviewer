using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace LabelPreviewer
{
    public class VariableValueItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsModified { get; set; }
    }

    public class VariableEditorWindow : Window
    {
        private DataGrid variableGrid;
        private Button applyButton;
        private Button closeButton;
        private LabelModel labelModel;
        private List<VariableValueItem> variableItems = new List<VariableValueItem>();

        // Event to notify about variable changes
        public event EventHandler<Dictionary<string, string>> VariablesChanged;

        public VariableEditorWindow(LabelModel model)
        {
            this.labelModel = model;
            this.Title = "Variable Editor";
            this.Width = 600;
            this.Height = 500;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Create the main layout
            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            this.Content = mainGrid;

            // Create a data grid for variables
            variableGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                Margin = new Thickness(10),
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                IsReadOnly = false,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            // Add columns
            variableGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Name",
                Binding = new Binding("Name"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                IsReadOnly = true
            });

            variableGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Value",
                Binding = new Binding("Value"),
                Width = new DataGridLength(2, DataGridLengthUnitType.Star)
            });

            // Mark modified rows with a different color
            variableGrid.CellEditEnding += (s, e) =>
            {
                if (e.EditAction == DataGridEditAction.Commit)
                {
                    if (e.Column.DisplayIndex == 1) // Value column
                    {
                        var item = (VariableValueItem)e.Row.Item;
                        item.IsModified = true;
                    }
                }
            };

            variableGrid.LoadingRow += (s, e) =>
            {
                var item = (VariableValueItem)e.Row.DataContext;
                if (item.IsModified)
                {
                    e.Row.Background = new SolidColorBrush(Colors.LightYellow);
                }
            };

            // Add data grid to the layout
            Grid.SetRow(variableGrid, 0);
            mainGrid.Children.Add(variableGrid);

            // Add button panel
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10)
            };
            Grid.SetRow(buttonPanel, 1);
            mainGrid.Children.Add(buttonPanel);

            // Add apply button
            applyButton = new Button
            {
                Content = "Apply Changes",
                Width = 120,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0),
                IsEnabled = false
            };
            applyButton.Click += ApplyChanges_Click;
            buttonPanel.Children.Add(applyButton);

            // Add close button
            closeButton = new Button
            {
                Content = "Close",
                Width = 80,
                Height = 30
            };
            closeButton.Click += (s, e) => this.Close();
            buttonPanel.Children.Add(closeButton);

            // Load variables
            LoadVariables();

            // Watch for changes in the data grid to enable/disable apply button
            variableGrid.CellEditEnding += (s, e) => CheckForChanges();
        }

        private void LoadVariables()
        {
            variableItems.Clear();

            // Add all variables to the list
            foreach (var variable in labelModel.Variables)
            {
                variableItems.Add(new VariableValueItem
                {
                    Id = variable.Key,
                    Name = labelModel.VariableIdToNameMap.ContainsKey(variable.Key)
                        ? labelModel.VariableIdToNameMap[variable.Key]
                        : variable.Key,
                    Value = variable.Value.SampleValue ?? string.Empty,
                    IsModified = false
                });
            }

            // Sort variables by name
            variableItems = variableItems.OrderBy(v => v.Name).ToList();

            // Set the data grid source
            variableGrid.ItemsSource = variableItems;
        }

        private void CheckForChanges()
        {
            bool hasChanges = variableItems.Any(v => v.IsModified);
            applyButton.IsEnabled = hasChanges;
        }

        private void ApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            // Update the model with new values
            Dictionary<string, string> changedVariables = new Dictionary<string, string>();

            foreach (var item in variableItems.Where(v => v.IsModified))
            {
                if (labelModel.Variables.ContainsKey(item.Id))
                {
                    labelModel.Variables[item.Id].SampleValue = item.Value;
                    changedVariables[item.Id] = item.Value;
                }
            }

            // Notify about the changes
            VariablesChanged?.Invoke(this, changedVariables);

            // Reset the modified flag
            foreach (var item in variableItems)
            {
                item.IsModified = false;
            }

            // Refresh the grid
            variableGrid.Items.Refresh();

            // Disable the apply button
            applyButton.IsEnabled = false;
        }

        // Method to keep the window open but update with new model data
        public void UpdateModel(LabelModel model)
        {
            this.labelModel = model;
            LoadVariables();
        }
    }
}