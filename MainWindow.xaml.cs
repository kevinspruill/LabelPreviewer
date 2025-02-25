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
        private string formatXmlPath;
        private LabelModel labelModel;

        public MainWindow()
        {
            InitializeComponent();

            // Enable high DPI rendering
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

            labelModel = new LabelModel();

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

        private void btnOpenFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*",
                Title = "Select Variable Definition File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                variablesXmlPath = openFileDialog.FileName;

                openFileDialog.Title = "Select Format Definition File";
                if (openFileDialog.ShowDialog() == true)
                {
                    formatXmlPath = openFileDialog.FileName;
                    btnRenderPreview.IsEnabled = true;

                    MessageBox.Show("Files loaded successfully. Click 'Render Preview' to display the label.",
                        "Files Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void btnRenderPreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                labelModel.LoadFromXml(variablesXmlPath, formatXmlPath);
                labelModel.Render(previewCanvas);

                // Update canvas size to match the label dimensions
                previewCanvas.Width = labelModel.Width;
                previewCanvas.Height = labelModel.Height;

                // Show information about the label
                this.Title = $"NiceLabel Previewer - {Path.GetFileName(formatXmlPath)} ({labelModel.Width:F0}x{labelModel.Height:F0})";

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
}