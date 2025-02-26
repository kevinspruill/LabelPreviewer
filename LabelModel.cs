using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;


namespace LabelPreviewer
{
    public class LabelModel
    {
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<string, Function> Functions { get; set; } = new Dictionary<string, Function>();
        public List<DocumentItem> DocumentItems { get; set; } = new List<DocumentItem>();
        public double Width { get; set; }
        public double Height { get; set; }
        public string BackgroundImagePath { get; set; }

        public void LoadFromXml(string labelFilePath)
        {
            // Clear existing items first
            DocumentItems.Clear();
            Variables.Clear();
            Functions.Clear();

            string password = ",^_A5Fus&!?j='Epiq*e";
            string labelFile = System.IO.Path.GetFileName(labelFilePath);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(labelFilePath);
            string fileNameInZip = $"{fileName}.slnx";

            string variablesXMLData = null;
            string formatXMLData = null;

            try
            {
                // Extract all files from the ZIP archive
                using (ZipFile zipFile = new ZipFile(File.OpenRead(labelFilePath)))
                {
                    zipFile.Password = password;

                    // read the contents of the ZIP file, One file is in the root, and has an extension of .slnx, the other is in a subfolder called "Formats", with no extension
                    foreach (ZipEntry entry in zipFile)
                    {
                        if (entry.Name == fileNameInZip)
                        {
                            using (Stream stream = zipFile.GetInputStream(entry))
                            {
                                // Read the contents of the file as a string
                                using (StreamReader reader = new StreamReader(stream))
                                {
                                    variablesXMLData = reader.ReadToEnd();
                                }
                            }
                        }
                        else if (entry.Name.StartsWith("Formats/") && !entry.IsDirectory)
                        {
                            using (Stream stream = zipFile.GetInputStream(entry))
                            {
                                // Read the contents of the file as a string
                                using (StreamReader reader = new StreamReader(stream))
                                {
                                    formatXMLData = reader.ReadToEnd();
                                }
                            }
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Reading Label File: {ex.Message}",
                    "Label Read Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LoadVariablesFromXml(variablesXMLData);
            LoadFormatFromXml(formatXMLData);
            GenerateProvisionalData();
        }

        private void GenerateProvisionalData()
        {
            // Fill in provisional data for empty variables
            foreach (var variable in Variables.Values)
            {
                if (string.IsNullOrEmpty(variable.SampleValue) || variable.SampleValue == "??????")
                {
                    switch (variable.Name)
                    {
                        case "Description1":
                            variable.SampleValue = "Organic Chicken";
                            break;
                        case "Description2":
                            variable.SampleValue = "Free Range";
                            break;
                        case "Description4":
                            variable.SampleValue = "Fresh Daily";
                            break;
                        case "Description5":
                            variable.SampleValue = "Local Farm";
                            break;
                        case "Description6":
                            variable.SampleValue = "No Antibiotics";
                            break;
                        case "Description7":
                            variable.SampleValue = "100% Natural";
                            break;
                        case "Description8":
                            variable.SampleValue = "Premium Quality";
                            break;
                        case "Description9":
                            variable.SampleValue = "USDA Inspected";
                            break;
                        case "Description10":
                            variable.SampleValue = "Family Owned Since 1985";
                            break;
                        case "Description12":
                            variable.SampleValue = "PARA";
                            break;
                        case "Description14":
                            variable.SampleValue = "Store in refrigerator";
                            break;
                        case "Weight":
                            variable.SampleValue = "1.5 lbs";
                            break;
                        case "Price":
                            variable.SampleValue = "$9.99";
                            break;
                        case "BarcodeVal":
                            variable.SampleValue = "123456789012";
                            break;
                        case "SellByDays":
                            variable.SampleValue = "7";
                            break;
                        case "PricePerPound":
                            variable.SampleValue = "$6.99/LB";
                            break;
                        case "Scaleable":
                            variable.SampleValue = "True";
                            break;
                        case "Ingredients":
                            variable.SampleValue = "Chicken, Salt, Herbs, Spices";
                            break;
                        case "NFServingSize":
                            variable.SampleValue = "3 oz (85g)";
                            break;
                        case "NFCalories":
                            variable.SampleValue = "120";
                            break;
                        case "NFCaloriesFat":
                            variable.SampleValue = "30";
                            break;
                        case "NFTotalFat":
                        case "NFTotalFatG":
                            variable.SampleValue = "3g";
                            break;
                        case "NFSatFat":
                        case "NFSatFatG":
                            variable.SampleValue = "1g";
                            break;
                        case "NFTransFat":
                        case "NFTransFatG":
                            variable.SampleValue = "0g";
                            break;
                        case "NFCholesterol":
                        case "NFCholesterolMG":
                            variable.SampleValue = "65mg";
                            break;
                        case "NFSodium":
                        case "NFSodiumMG":
                            variable.SampleValue = "45mg";
                            break;
                        case "NFTotCarbo":
                        case "NFTotCarboG":
                            variable.SampleValue = "0g";
                            break;
                        case "NFDietFiber":
                        case "NFDietFiberG":
                            variable.SampleValue = "0g";
                            break;
                        case "NFSugars":
                        case "NFSugarsG":
                            variable.SampleValue = "0g";
                            break;
                        case "NFProtein":
                        case "NFProteinG":
                            variable.SampleValue = "22g";
                            break;
                        case "NFVitA":
                            variable.SampleValue = "0%";
                            break;
                        case "NFVitC":
                            variable.SampleValue = "0%";
                            break;
                        case "NFCalcium":
                            variable.SampleValue = "0%";
                            break;
                        case "NFIron":
                            variable.SampleValue = "4%";
                            break;
                        case "NFServingPerPack":
                            variable.SampleValue = "4";
                            break;
                        default:
                            if (variable.Name.StartsWith("NF"))
                                variable.SampleValue = "0%";
                            else
                                variable.SampleValue = variable.Name;
                            break;
                    }
                }
            }

            // Update function values based on dependencies
            foreach (var function in Functions.Values)
            {
                if (string.IsNullOrEmpty(function.SampleValue) || function.SampleValue == "??????")
                {
                    function.SampleValue = function.Name;
                }
            }
        }

        private void LoadVariablesFromXml(string variablesXMLData)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(variablesXMLData);

            XmlNodeList variableNodes = doc.SelectNodes("//EuroPlus.NiceLabel/Variables/Item[@Type='Variable']");
            if (variableNodes != null)
            {
                foreach (XmlNode node in variableNodes)
                {
                    Variable variable = new Variable
                    {
                        Id = GetNodeValue(node, "Id"),
                        Name = GetNodeValue(node, "Name"),
                        SampleValue = GetSampleValue(node)
                    };

                    Variables[variable.Id] = variable;
                }
            }

            XmlNodeList functionNodes = doc.SelectNodes("//EuroPlus.NiceLabel/Functions/Item");
            if (functionNodes != null)
            {
                foreach (XmlNode node in functionNodes)
                {
                    Function function = new Function
                    {
                        Id = GetNodeValue(node, "Id"),
                        Name = GetNodeValue(node, "Name"),
                        SampleValue = GetSampleValue(node)
                    };

                    Functions[function.Id] = function;
                }
            }
        }


        private string GetNodeValue(XmlNode parentNode, string childNodeName)
        {
            if (parentNode == null || string.IsNullOrEmpty(childNodeName))
                return null;

            XmlNode childNode = parentNode.SelectSingleNode(childNodeName);
            return childNode?.InnerText;
        }

        private string GetSampleValue(XmlNode node)
        {
            XmlNode sampleValueNode = node.SelectSingleNode("SampleValue/StringValue")
                ?? node.SelectSingleNode("SampleValue/UserValue");

            return sampleValueNode?.InnerText ?? String.Empty;
        }

        // This represents the changes needed in the LabelModel.cs file
        // Method changes for distinguishing between text objects and text boxes

        private void LoadFormatFromXml(string formatXMLData)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(formatXMLData);

            // Get label dimensions
            XmlNode mediaNode = doc.SelectSingleNode("//EuroPlus.NiceLabel/Media");
            if (mediaNode != null)
            {
                XmlNode widthNode = mediaNode.SelectSingleNode("Width");
                XmlNode heightNode = mediaNode.SelectSingleNode("Height");

                if (widthNode != null && heightNode != null)
                {
                    // Convert from microns to WPF units (1/96 inch)
                    double conversionFactor = 0.00377953;

                    Width = (double.Parse(widthNode.InnerText) / 1000) * (96 / 25.4);
                    Height = (double.Parse(heightNode.InnerText) / 1000) * (96 / 25.4);
                }
            }

            // Get background image
            XmlNode backgroundNode = doc.SelectSingleNode("//BackgroundDocumentItem");
            if (backgroundNode != null)
            {
                XmlNode graphicFileNameNode = backgroundNode.SelectSingleNode("GraphicFileName");
                if (graphicFileNameNode != null)
                {
                    BackgroundImagePath = graphicFileNameNode.InnerText;
                }
            }

            // Get document items
            XmlNodeList itemNodes = doc.SelectNodes("//DocumentDesign/Items/Item");
            if (itemNodes != null)
            {
                foreach (XmlNode node in itemNodes)
                {
                    string type = node.Attributes?["Type"]?.Value;

                    DocumentItem item = null;
                    switch (type)
                    {
                        case "TextDocumentItem":
                            // Determine if it's a text object or text box based on geometry and attributes
                            bool isTextBox = DetermineIfTextBox(node);

                            if (isTextBox)
                            {
                                item = new TextBoxItem();
                            }
                            else
                            {
                                item = new TextObjectItem();
                            }
                            break;
                        case "GraphicDocumentItem":
                            item = new GraphicDocumentItem();
                            break;
                        case "BarcodeDocumentItem":
                            item = new BarcodeDocumentItem();
                            break;
                        default:
                            continue;
                    }

                    item.Id = GetNodeValue(node, "Id");
                    item.Name = GetNodeValue(node, "Name");

                    // Load geometry
                    XmlNode geometryNode = node.SelectSingleNode("Geometry");
                    if (geometryNode != null)
                    {
                        string geometryType = geometryNode.Attributes?["Type"]?.Value;
                        double x = 0, y = 0, width = 0, height = 0;
                        int anchoringPoint = 0; // Default to top-left

                        // Try to get anchoring point
                        XmlNode anchorNode = geometryNode.SelectSingleNode("AnchoringPoint");
                        if (anchorNode != null && !string.IsNullOrEmpty(anchorNode.InnerText))
                        {
                            int.TryParse(anchorNode.InnerText, out anchoringPoint);
                        }

                        if (geometryType == "PositionGeometry")
                        {
                            x = double.Parse(GetNodeValue(geometryNode, "X") ?? "0") * 0.00377953;
                            y = double.Parse(GetNodeValue(geometryNode, "Y") ?? "0") * 0.00377953;
                        }
                        else if (geometryType == "RectGeometry")
                        {
                            width = double.Parse(GetNodeValue(geometryNode, "Width") ?? "0") * 0.00377953;
                            height = double.Parse(GetNodeValue(geometryNode, "Height") ?? "0") * 0.00377953;

                            // Get left/top or other position values
                            if (geometryNode.SelectSingleNode("Left") != null)
                            {
                                x = double.Parse(GetNodeValue(geometryNode, "Left") ?? "0") * 0.00377953;
                            }

                            if (geometryNode.SelectSingleNode("Top") != null)
                            {
                                y = double.Parse(GetNodeValue(geometryNode, "Top") ?? "0") * 0.00377953;
                            }
                        }

                        // Store the anchoring point and coordinates
                        item.AnchoringPoint = (AnchoringPoint)anchoringPoint;
                        item.X = x;
                        item.Y = y;
                        item.Width = width;
                        item.Height = height;
                    }

                    // Load content
                    XmlNode contentsNode = node.SelectSingleNode("Contents");
                    if (contentsNode != null)
                    {
                        XmlNode fixedValueNode = contentsNode.SelectSingleNode("FixedValue/StringValue");
                        if (fixedValueNode != null)
                        {
                            item.Content = fixedValueNode.InnerText;

                            // For text objects, approximate width based on content if not specified
                            if (item is TextObjectItem textObject && textObject.Width <= 0)
                            {
                                // Rough estimate: each character is about 0.6 times the font size
                                textObject.Width = item.Content.Length * (textObject.FontSize * 0.6);
                            }
                        }
                    }
                    else
                    {
                        XmlNode fixedContentsNode = node.SelectSingleNode("FixedContents");
                        if (fixedContentsNode != null)
                        {
                            item.Content = fixedContentsNode.InnerText;
                        }
                    }

                    // Load font info for text items
                    if (item is TextDocumentItem textItem)
                    {
                        XmlNode fontNode = node.SelectSingleNode("FontDescriptor");
                        if (fontNode != null)
                        {
                            textItem.FontName = GetNodeValue(fontNode, "Name") ?? "Arial";

                            string heightStr = GetNodeValue(fontNode, "Height");
                            if (!string.IsNullOrEmpty(heightStr))
                            {
                                textItem.FontSize = double.Parse(heightStr);
                            }

                            string colorHex = GetNodeValue(fontNode, "Color") ?? "FF000000";
                            if (colorHex.StartsWith("FF"))
                            {
                                colorHex = "#" + colorHex.Substring(2);
                            }
                            else
                            {
                                colorHex = "#" + colorHex;
                            }

                            try
                            {
                                textItem.FontColor = (Color)ColorConverter.ConvertFromString(colorHex);
                            }
                            catch
                            {
                                textItem.FontColor = Colors.Black; // Default to black if conversion fails
                            }

                            // Check for multiline property
                            XmlNode multilineNode = node.SelectSingleNode("Multiline");
                            if (multilineNode != null)
                            {
                                textItem.IsMultiline = bool.Parse(multilineNode.InnerText);
                            }

                            // Check for text alignment
                            XmlNode alignmentNode = node.SelectSingleNode("Alignment");
                            if (alignmentNode != null)
                            {
                                string alignmentValue = alignmentNode.InnerText;
                                switch (alignmentValue)
                                {
                                    case "0": // Left
                                        textItem.TextAlignment = TextAlignment.Left;
                                        break;
                                    case "1": // Center
                                        textItem.TextAlignment = TextAlignment.Center;
                                        break;
                                    case "2": // Right
                                        textItem.TextAlignment = TextAlignment.Right;
                                        break;
                                    case "3": // Justify
                                        textItem.TextAlignment = TextAlignment.Justify;
                                        break;
                                }
                            }

                            // For TextBoxItem, handle text wrapping
                            if (textItem is TextBoxItem textBox)
                            {
                                XmlNode wrapNode = node.SelectSingleNode("TextWrapping");
                                if (wrapNode != null)
                                {
                                    string wrapValue = wrapNode.InnerText;
                                    switch (wrapValue)
                                    {
                                        case "0": // No Wrap
                                            textBox.TextWrapping = TextWrapping.NoWrap;
                                            break;
                                        case "1": // Wrap
                                            textBox.TextWrapping = TextWrapping.Wrap;
                                            break;
                                        case "2": // Wrap With Overflow
                                            textBox.TextWrapping = TextWrapping.WrapWithOverflow;
                                            break;
                                    }
                                }
                                else
                                {
                                    // Default to Wrap for TextBoxItem
                                    textBox.TextWrapping = TextWrapping.Wrap;
                                }
                            }
                        }
                    }

                    // Check for data source reference
                    XmlNode dataSourceNode = node.SelectSingleNode("DataSourceReference");
                    if (dataSourceNode != null)
                    {
                        string dataSourceId = GetNodeValue(dataSourceNode, "Id");
                        if (!string.IsNullOrEmpty(dataSourceId))
                        {
                            item.DataSourceId = dataSourceId;
                        }
                    }

                    // Get ZOrder if available
                    XmlNode zOrderNode = node.SelectSingleNode("ZOrder");
                    if (zOrderNode != null && !string.IsNullOrEmpty(zOrderNode.InnerText))
                    {
                        int.TryParse(zOrderNode.InnerText, out int zOrder);
                        item.ZOrder = zOrder;
                    }

                    DocumentItems.Add(item);
                }
            }
        }

        // Helper method to determine if a text element is a text box or text object
        private bool DetermineIfTextBox(XmlNode node)
        {
            // Check if geometry is RectGeometry (has explicit width/height)
            XmlNode geometryNode = node.SelectSingleNode("Geometry");
            if (geometryNode != null)
            {
                string geometryType = geometryNode.Attributes?["Type"]?.Value;
                if (geometryType == "RectGeometry")
                {
                    // Has defined width/height - likely a text box
                    return true;
                }
            }

            // Check if multiline flag is present and true
            XmlNode multilineNode = node.SelectSingleNode("Multiline");
            if (multilineNode != null && multilineNode.InnerText.ToLower() == "true")
            {
                return true;
            }

            // Check for wrapping mode
            XmlNode wrapNode = node.SelectSingleNode("TextWrapping");
            if (wrapNode != null && wrapNode.InnerText != "0") // Not "NoWrap"
            {
                return true;
            }

            // Check if there's a defined Max width
            XmlNode maxWidthNode = node.SelectSingleNode("MaxWidth");
            if (maxWidthNode != null && !string.IsNullOrEmpty(maxWidthNode.InnerText))
            {
                double maxWidth;
                if (double.TryParse(maxWidthNode.InnerText, out maxWidth) && maxWidth > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public void Render(Canvas canvas)
        {
            canvas.Children.Clear();
            canvas.Width = Width;
            canvas.Height = Height;

            // Set background image if available
            if (!string.IsNullOrEmpty(BackgroundImagePath))
            {
                try
                {
                    ImageBrush imageBrush = new ImageBrush();

                    // Try to load the actual image first
                    if (File.Exists(BackgroundImagePath))
                    {
                        BitmapImage bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(BackgroundImagePath);
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
                                "NICHOLAS_OVALSTRIP.jpg\n(Placeholder Background)",
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
                            192, // Higher DPI (was 96)
                            192, // Higher DPI (was 96)
                            PixelFormats.Pbgra32);

                        imageBrush.ImageSource = renderBitmap;
                    }

                    imageBrush.Stretch = Stretch.None;
                    imageBrush.AlignmentX = AlignmentX.Left;
                    imageBrush.AlignmentY = AlignmentY.Top;
                    imageBrush.ViewportUnits = BrushMappingMode.Absolute;
                    imageBrush.Viewport = new Rect(0, 0, Width, Height);
                    canvas.Background = imageBrush;
                }
                catch (Exception ex)
                {
                    // If loading the image fails, just set a white background
                    canvas.Background = Brushes.White;
                    MessageBox.Show($"Error loading background image: {ex.Message}",
                        "Image Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                canvas.Background = Brushes.White;
            }

            // Sort items by Z-order for proper rendering
            var sortedItems = DocumentItems.OrderBy(item => item.ZOrder).ToList();

            foreach (var item in sortedItems)
            {
                if (item is TextObjectItem textObjectItem)
                {
                    RenderTextObject(canvas, textObjectItem);
                }
                else if (item is TextBoxItem textBoxItem)
                {
                    RenderTextBox(canvas, textBoxItem);
                }
                else if (item is GraphicDocumentItem graphicItem)
                {
                    RenderGraphicItem(canvas, graphicItem);
                }
                else if (item is BarcodeDocumentItem barcodeItem)
                {
                    RenderBarcodeItem(canvas, barcodeItem);
                }
            }
        }

        private void RenderTextObject(Canvas canvas, TextObjectItem item)
        {
            string content = ResolveContent(item);

            TextBlock textBlock = new TextBlock
            {
                Text = content,
                FontFamily = new FontFamily(item.FontName),
                FontSize = item.FontSize,
                Foreground = new SolidColorBrush(item.FontColor),
                TextAlignment = item.TextAlignment
            };

            // Text objects don't wrap by default
            textBlock.TextWrapping = TextWrapping.NoWrap;

            // Get position adjusted for anchoring point
            Point position = item.GetAdjustedPosition();
            Canvas.SetLeft(textBlock, position.X);
            Canvas.SetTop(textBlock, position.Y);

            // Add debug visualization if needed
            if (ShowDebugInfo)
            {
                Rectangle debugRect = new Rectangle
                {
                    Width = textBlock.Width > 0 ? textBlock.Width : 100, // Default width for visualization
                    Height = item.FontSize,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Red,
                    StrokeThickness = 1
                };

                Canvas.SetLeft(debugRect, position.X);
                Canvas.SetTop(debugRect, position.Y);

                Rectangle debugAnchor = new Rectangle
                {
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Red
                };

                Canvas.SetLeft(debugAnchor, position.X);
                Canvas.SetTop(debugAnchor, position.Y);

                TextBlock anchorText = new TextBlock
                {
                    Text = item.AnchoringPoint.ToString(),
                    Foreground = Brushes.Red,
                    FontSize = 8
                };

                Canvas.SetLeft(anchorText, position.X);
                Canvas.SetTop(anchorText, position.Y - 15);

                canvas.Children.Add(debugRect);
                canvas.Children.Add(debugAnchor);
                canvas.Children.Add(anchorText);
            }

            // Set z-order if available
            if (item.ZOrder != 0)
            {
                Canvas.SetZIndex(textBlock, item.ZOrder);
            }

            canvas.Children.Add(textBlock);
        }

        private void RenderTextBox(Canvas canvas, TextBoxItem item)
        {
            string content = ResolveContent(item);

            TextBlock textBlock = new TextBlock
            {
                Text = content,
                FontFamily = new FontFamily(item.FontName),
                FontSize = item.FontSize,
                Foreground = new SolidColorBrush(item.FontColor),
                TextAlignment = item.TextAlignment,
                TextWrapping = item.TextWrapping
            };

            // TextBox has explicit width and height
            if (item.Width > 0)
            {
                textBlock.Width = item.Width;
            }

            if (item.Height > 0)
            {
                textBlock.Height = item.Height;
            }

            // Get position adjusted for anchoring point
            Point position = item.GetAdjustedPosition();
            Canvas.SetLeft(textBlock, position.X);
            Canvas.SetTop(textBlock, position.Y);

            // Add debug visualization if needed
            if (ShowDebugInfo)
            {
                Rectangle debugRect = new Rectangle
                {
                    Width = item.Width > 0 ? item.Width : 100,
                    Height = item.Height > 0 ? item.Height : item.FontSize * 2,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 1
                };

                Canvas.SetLeft(debugRect, position.X);
                Canvas.SetTop(debugRect, position.Y);

                Rectangle debugAnchor = new Rectangle
                {
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Blue
                };

                Canvas.SetLeft(debugAnchor, position.X);
                Canvas.SetTop(debugAnchor, position.Y);

                TextBlock anchorText = new TextBlock
                {
                    Text = item.AnchoringPoint.ToString(),
                    Foreground = Brushes.Blue,
                    FontSize = 8
                };

                Canvas.SetLeft(anchorText, position.X);
                Canvas.SetTop(anchorText, position.Y - 15);

                canvas.Children.Add(debugRect);
                canvas.Children.Add(debugAnchor);
                canvas.Children.Add(anchorText);
            }

            // Set z-order if available
            if (item.ZOrder != 0)
            {
                Canvas.SetZIndex(textBlock, item.ZOrder);
            }

            canvas.Children.Add(textBlock);
        }

        // Add a property to control debug visualization
        public bool ShowDebugInfo { get; set; } = true;

        private void RenderGraphicItem(Canvas canvas, GraphicDocumentItem item)
        {
            // Set default size
            if (item.Width == 0) { item.Width = 50; }
            if (item.Height == 0) { item.Height = 50; }

            // Placeholder for graphics - just draw a rectangle with name
            Rectangle rect = new Rectangle
            {
                Width = item.Width > 0 ? item.Width : 50,
                Height = item.Height > 0 ? item.Height : 50,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Fill = Brushes.LightGray
            };

            // Get position adjusted for anchoring point
            Point position = item.GetAdjustedPosition();
            Canvas.SetLeft(rect, position.X);
            Canvas.SetTop(rect, position.Y);

            // Set z-order if available
            if (item.ZOrder != 0)
            {
                Canvas.SetZIndex(rect, item.ZOrder);
            }

            canvas.Children.Add(rect);

            TextBlock textBlock = new TextBlock
            {
                Text = item.Name,
                FontSize = 8
            };

            Canvas.SetLeft(textBlock, position.X + 5);
            Canvas.SetTop(textBlock, position.Y + 5);
            Canvas.SetZIndex(textBlock, item.ZOrder + 1);
            canvas.Children.Add(textBlock);
        }

        private void RenderBarcodeItem(Canvas canvas, BarcodeDocumentItem item)
        {
            // Set default size
            if (item.Width == 0) { item.Width = 100; }
            if (item.Height == 0) { item.Height = 50; }


            // Placeholder for barcode - just draw a rectangle with "BARCODE" text
            Rectangle rect = new Rectangle
            {
                Width = item.Width > 0 ? item.Width : 100,
                Height = item.Height > 0 ? item.Height : 50,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Fill = Brushes.White
            };

            // Get position adjusted for anchoring point
            Point position = item.GetAdjustedPosition();
            Canvas.SetLeft(rect, position.X);
            Canvas.SetTop(rect, position.Y);

            // Set z-order if available
            if (item.ZOrder != 0)
            {
                Canvas.SetZIndex(rect, item.ZOrder);
            }

            canvas.Children.Add(rect);

            TextBlock textBlock = new TextBlock
            {
                Text = "BARCODE: " + ResolveContent(item),
                FontSize = 8
            };

            Canvas.SetLeft(textBlock, position.X + 5);
            Canvas.SetTop(textBlock, position.Y + 20);
            Canvas.SetZIndex(textBlock, item.ZOrder + 1);
            canvas.Children.Add(textBlock);
        }

        private string ResolveContent(DocumentItem item)
        {
            if (item == null) return "???";

            if (!string.IsNullOrEmpty(item.DataSourceId))
            {
                if (Variables != null && Variables.TryGetValue(item.DataSourceId, out Variable variable))
                {
                    return variable.SampleValue ?? "???";
                }
                else if (Functions != null && Functions.TryGetValue(item.DataSourceId, out Function function))
                {
                    return function.SampleValue ?? "???";
                }
            }

            return item.Content ?? "???";
        }
    }

    public class Variable
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SampleValue { get; set; }
    }

    public class Function
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SampleValue { get; set; }
    }

    public class DocumentItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Content { get; set; }
        public string DataSourceId { get; set; }
        public AnchoringPoint AnchoringPoint { get; set; } = AnchoringPoint.TopLeft;
        public int ZOrder { get; set; } = 0; // Z-index order

        // NiceLabel anchoring points:
        // 1 = Top-left, 2 = Top-center, 3 = Top-right
        // 4 = Middle-left, 5 = Middle-center, 6 = Middle-right
        // 7 = Bottom-left, 8 = Bottom-center, 9 = Bottom-right
        public virtual Point GetAdjustedPosition()
        {
            double adjustedX = X;
            double adjustedY = Y;

            switch (AnchoringPoint)
            {
                case AnchoringPoint.TopCenter:
                    adjustedX -= Width / 2;
                    break;
                case AnchoringPoint.TopRight:
                    adjustedX -= Width;
                    break;
                case AnchoringPoint.MiddleLeft:
                    adjustedY -= Height / 2;
                    break;
                case AnchoringPoint.MiddleCenter:
                    adjustedX -= Width / 2;
                    adjustedY -= Height / 2;
                    break;
                case AnchoringPoint.MiddleRight:
                    adjustedX -= Width;
                    adjustedY -= Height / 2;
                    break;
                case AnchoringPoint.BottomLeft:
                    adjustedY -= Height;
                    break;
                case AnchoringPoint.BottomCenter:
                    adjustedX -= Width / 2;
                    adjustedY -= Height;
                    break;
                case AnchoringPoint.BottomRight:
                    adjustedX -= Width;
                    adjustedY -= Height;
                    break;
                    // Default is TopLeft = no adjustment
            }

            return new Point(adjustedX, adjustedY);
        }

    }

    // Base class for all text items
    public abstract class TextDocumentItem : DocumentItem
    {
        public string FontName { get; set; } = "Arial";
        public double FontSize { get; set; } = 10;
        public Color FontColor { get; set; } = Colors.Black;
        public bool IsMultiline { get; set; } = false;
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Left;

    }

    // A text object is a single line of text without explicit width/height constraints
    // It expands to fit its content
    public class TextObjectItem : TextDocumentItem
    {
    }

    // A text box has explicit width/height constraints and can wrap text
    public class TextBoxItem : TextDocumentItem
    {
        public TextWrapping TextWrapping { get; set; } = TextWrapping.Wrap;
    }

    public class GraphicDocumentItem : DocumentItem
    {
    }

    public class BarcodeDocumentItem : DocumentItem
    {
    }

    public enum AnchoringPoint
    {
        None = 0,
        TopLeft = 1,
        TopCenter = 2,
        TopRight = 3,
        MiddleLeft = 4,
        MiddleCenter = 5,
        MiddleRight = 6,
        BottomLeft = 7,
        BottomCenter = 8,
        BottomRight = 9
    }
}