using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace LabelPreviewer
{
    public class XmlViewerWindow : Window
    {
        private TextBox xmlTextBox;
        private TabControl tabControl;

        public XmlViewerWindow(string variablesXml, string formatXml)
        {
            this.Title = "Label XML Data Viewer";
            this.Width = 800;
            this.Height = 600;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Create the main layout
            Grid mainGrid = new Grid();
            this.Content = mainGrid;

            // Create a tab control to separate variables and format XML
            tabControl = new TabControl();
            mainGrid.Children.Add(tabControl);

            // Tab for Variables XML
            TabItem variablesTab = new TabItem { Header = "Variables XML" };
            tabControl.Items.Add(variablesTab);

            // Text box for Variables XML with scrolling
            ScrollViewer variablesScrollViewer = new ScrollViewer();
            variablesTab.Content = variablesScrollViewer;

            TextBox variablesTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 12
            };
            variablesScrollViewer.Content = variablesTextBox;

            // Format and display Variables XML
            if (!string.IsNullOrEmpty(variablesXml))
            {
                variablesTextBox.Text = FormatXml(variablesXml);
            }
            else
            {
                variablesTextBox.Text = "No Variables XML data available.";
            }

            // Tab for Format XML
            TabItem formatTab = new TabItem { Header = "Format XML" };
            tabControl.Items.Add(formatTab);

            // Text box for Format XML with scrolling
            ScrollViewer formatScrollViewer = new ScrollViewer();
            formatTab.Content = formatScrollViewer;

            TextBox formatTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 12
            };
            formatScrollViewer.Content = formatTextBox;

            // Format and display Format XML
            if (!string.IsNullOrEmpty(formatXml))
            {
                formatTextBox.Text = FormatXml(formatXml);
            }
            else
            {
                formatTextBox.Text = "No Format XML data available.";
            }

            // Add close button at the bottom
            Button closeButton = new Button
            {
                Content = "Close",
                Width = 80,
                Height = 25,
                Margin = new Thickness(0, 10, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            closeButton.Click += (s, e) => this.Close();

            // Add button to the grid
            Grid.SetRow(closeButton, 1);
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.Children.Add(closeButton);
        }

        /// <summary>
        /// Format XML string to be more readable
        /// </summary>
        private string FormatXml(string xml)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                System.Xml.Formatting format = System.Xml.Formatting.Indented;
                using (var stringWriter = new System.IO.StringWriter())
                {
                    using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true, IndentChars = "  " }))
                    {
                        doc.Save(xmlWriter);
                    }
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"Error formatting XML: {ex.Message}\n\nOriginal XML:\n{xml}";
            }
        }
    }
}