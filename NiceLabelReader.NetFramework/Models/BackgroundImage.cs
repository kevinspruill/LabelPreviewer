using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace LabelPreviewer
{
    public class BackgroundImage
    {
        public string ImagePath { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsPrintable { get; set; } = false;

        /// <summary>
        /// Loads background image information from XML
        /// </summary>
        public static BackgroundImage LoadFromXml(XmlNode backgroundNode, double labelWidth, double labelHeight)
        {
            if (backgroundNode == null)
                return null;

            var background = new BackgroundImage
            {
                Width = labelWidth,
                Height = labelHeight
            };

            // Get image path
            XmlNode graphicFileNameNode = backgroundNode.SelectSingleNode("GraphicFileName");
            if (graphicFileNameNode != null)
            {
                background.ImagePath = graphicFileNameNode.InnerText;
            }

            // Check if printable
            XmlNode printableNode = backgroundNode.SelectSingleNode("Printable");
            if (printableNode != null && !string.IsNullOrEmpty(printableNode.InnerText))
            {
                background.IsPrintable = bool.Parse(printableNode.InnerText);
            }

            return background;
        }

        /// <summary>
        /// Renders the background image to the specified canvas
        /// </summary>
        public void Render(Canvas canvas, bool showDebugInfo)
        {
            if (string.IsNullOrEmpty(ImagePath))
            {
                // No background image specified, set a white background
                canvas.Background = Brushes.White;
                return;
            }

            try
            {
                ImageBrush imageBrush = new ImageBrush();

                // Try to load the actual image first
                if (File.Exists(ImagePath))
                {
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(ImagePath);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    imageBrush.ImageSource = bmp;
                }
                else
                {
                    // If file doesn't exist, create a placeholder image
                    DrawingVisual drawingVisual = new DrawingVisual();
                    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                    {
                        Rect rect = new Rect(0, 0, Width, Height);
                        drawingContext.DrawRectangle(Brushes.White, null, rect);

                        // Add text to indicate it's a placeholder
                        FormattedText text = new FormattedText(
                            Path.GetFileName(ImagePath) + "\n(Placeholder Background)",
                            System.Globalization.CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Arial"),
                            14,
                            Brushes.Gray,
                            VisualTreeHelper.GetDpi(drawingVisual).PixelsPerDip);

                        drawingContext.DrawText(text, new Point(Width / 2 - text.Width / 2, Height / 2 - text.Height / 2));
                    }

                    RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                        (int)(Width * 2), // Double resolution
                        (int)(Height * 2), // Double resolution
                        192, // Higher DPI
                        192, // Higher DPI
                        PixelFormats.Pbgra32);

                    renderBitmap.Render(drawingVisual);
                    imageBrush.ImageSource = renderBitmap;
                }

                imageBrush.Stretch = Stretch.None;
                imageBrush.AlignmentX = AlignmentX.Left;
                imageBrush.AlignmentY = AlignmentY.Top;
                imageBrush.ViewportUnits = BrushMappingMode.Absolute;
                imageBrush.Viewport = new Rect(0, 0, Width, Height);
                canvas.Background = imageBrush;

                // Add debug info if needed
                if (showDebugInfo)
                {
                    TextBlock debugInfo = new TextBlock
                    {
                        Text = $"Background: {Path.GetFileName(ImagePath)}{(IsPrintable ? "" : " [Not Printable]")}",
                        FontSize = 8,
                        Foreground = Brushes.Gray,
                        Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)),
                        Margin = new Thickness(5)
                    };

                    Canvas.SetLeft(debugInfo, 5);
                    Canvas.SetTop(debugInfo, 5);
                    Canvas.SetZIndex(debugInfo, 1000); // Ensure it's on top
                    canvas.Children.Add(debugInfo);
                }
            }
            catch (Exception ex)
            {
                // If loading the image fails, just set a white background
                canvas.Background = Brushes.White;

                // Display error message
                TextBlock errorText = new TextBlock
                {
                    Text = $"Error loading background image: {ex.Message}",
                    FontSize = 10,
                    Foreground = Brushes.Red,
                    Background = Brushes.White,
                    Margin = new Thickness(5)
                };

                Canvas.SetLeft(errorText, 5);
                Canvas.SetTop(errorText, 5);
                Canvas.SetZIndex(errorText, 1000);
                canvas.Children.Add(errorText);
            }
        }
    }
}