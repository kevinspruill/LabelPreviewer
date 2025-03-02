using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using ZXing.Common;
using ZXing;
using ZXing.Rendering;
using System.Linq;


namespace LabelPreviewer
{
    public class LabelModel
    {
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<string, Function> Functions { get; set; } = new Dictionary<string, Function>();
        public List<DocumentItem> DocumentItems { get; set; } = new List<DocumentItem>();
        public Dictionary<string, string> VariableNameToIdMap { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> VariableIdToNameMap { get; set; } = new Dictionary<string, string>();
        private Dictionary<string, string> functionResultCache = new Dictionary<string, string>();
        private HashSet<string> functionsInProgress = new HashSet<string>();
        public Dictionary<string, string> FunctionIdToNameMap { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, string> FunctionNameToIdMap { get; private set; } = new Dictionary<string, string>();
        public string VariablesXmlData { get; private set; }
        public string FormatXmlData { get; private set; }

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
                                    // Save the XML data for debugging
                                    this.VariablesXmlData = variablesXMLData;
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
                                    // Save the XML data for debugging
                                    this.FormatXmlData = formatXMLData;
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
                            variable.SampleValue = "Description One";
                            break;
                        case "Description2":
                            variable.SampleValue = "Description Two";
                            break;
                        case "Description4":
                            variable.SampleValue = "Description Four";
                            break;
                        case "Description5":
                            variable.SampleValue = "Description Five";
                            break;
                        case "Description6":
                            variable.SampleValue = "Description Six";
                            break;
                        case "Description7":
                            variable.SampleValue = "Description Seven";
                            break;
                        case "Description8":
                            variable.SampleValue = "Description Eight";
                            break;
                        case "Description9":
                            variable.SampleValue = "Description Nine";
                            break;
                        case "Description10":
                            variable.SampleValue = "Description Ten";
                            break;
                        case "Description12":
                            variable.SampleValue = "Description Twelve";
                            break;
                        case "Description14":
                            variable.SampleValue = "Description Fourteen";
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
                                variable.SampleValue = "0";
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

            // First, load all variables
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

                    // Create name-to-ID and ID-to-name mappings
                    VariableNameToIdMap[variable.Name] = variable.Id;
                    VariableIdToNameMap[variable.Id] = variable.Name;
                }
            }

            // Then load all functions including concatenate functions
            XmlNodeList functionNodes = doc.SelectNodes("//EuroPlus.NiceLabel/Functions/Item");
            if (functionNodes != null)
            {
                foreach (XmlNode node in functionNodes)
                {
                    string functionType = node.Attributes?["Type"]?.Value ?? "ExecuteScriptFunction";

                    // Create the appropriate function type
                    Function function = Function.CreateFunction(functionType);

                    // Set common properties
                    function.Id = GetNodeValue(node, "Id");
                    function.Name = GetNodeValue(node, "Name");
                    function.SampleValue = GetSampleValue(node);
                    function.FunctionType = functionType;

                    // Store in function maps for lookup
                    if (!string.IsNullOrEmpty(function.Id) && !string.IsNullOrEmpty(function.Name))
                    {
                        FunctionIdToNameMap[function.Id] = function.Name;
                        FunctionNameToIdMap[function.Name] = function.Id;
                    }

                    // Handle specific function types
                    if (functionType == "ConcatenateFunction")
                    {
                        if (function is ConcatenateFunction concatFunction)
                        {
                            // Get separator (may be Base64 encoded)
                            string base64Separator = GetNodeValue(node, "Separator");
                            concatFunction.Separator = ConcatenateFunction.DecodeSeparator(base64Separator);

                            // Get ignore empty values flag
                            string ignoreEmptyValuesStr = GetNodeValue(node, "IgnoreEmptyValues");
                            if (!string.IsNullOrEmpty(ignoreEmptyValuesStr))
                            {
                                concatFunction.IgnoreEmptyValues = bool.Parse(ignoreEmptyValuesStr);
                            }

                            // Get data sources to concatenate
                            XmlNodeList dataValueNodes = node.SelectNodes("DataValues/Item/DataSourceReference");
                            if (dataValueNodes != null)
                            {
                                foreach (XmlNode dataValueNode in dataValueNodes)
                                {
                                    string dataSourceId = GetNodeValue(dataValueNode, "Id");
                                    if (!string.IsNullOrEmpty(dataSourceId))
                                    {
                                        concatFunction.DataSourceIds.Add(dataSourceId);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // For script-based functions
                        function.Script = GetNodeValue(node, "Script");
                        function.ScriptWithReferences = GetNodeValue(node, "ScriptWithReferences");

                        // Extract input data source references
                        XmlNodeList inputDataNodes = node.SelectNodes("InputDataSourceReferences/Item");
                        if (inputDataNodes != null)
                        {
                            foreach (XmlNode inputNode in inputDataNodes)
                            {
                                string refId = GetNodeValue(inputNode, "Id");
                                if (!string.IsNullOrEmpty(refId))
                                {
                                    function.InputDataSourceIds.Add(refId);
                                }
                            }
                        }
                    }

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
                            GraphicDocumentItem graphicItem = new GraphicDocumentItem();
                            item = graphicItem;

                            // Get the GraphicFileName if available
                            XmlNode graphicFileNameNode = node.SelectSingleNode("GraphicFileName");
                            if (graphicFileNameNode != null)
                            {
                                graphicItem.ImagePath = graphicFileNameNode.InnerText;
                            }
                            else
                            {
                                // If no file name is specified, it uses a DataSourceReference
                                XmlNode dataSourceRefNode = node.SelectSingleNode("DataSourceReference");
                                if (dataSourceRefNode != null)
                                {
                                    string dataSourceId = GetNodeValue(dataSourceRefNode, "Id");
                                    if (!string.IsNullOrEmpty(dataSourceId))
                                    {
                                        graphicItem.DataSourceId = dataSourceId;
                                    }
                                }
                            }

                            // Get ResizeMode if available
                            XmlNode resizeModeNode = node.SelectSingleNode("ResizeMode");
                            if (resizeModeNode != null && int.TryParse(resizeModeNode.InnerText, out int resizeMode))
                            {
                                graphicItem.ResizeMode = resizeMode;
                            }

                            // Get ForceColor if available
                            XmlNode forceColorNode = node.SelectSingleNode("ForceColor");
                            if (forceColorNode != null && bool.TryParse(forceColorNode.InnerText, out bool forceColor))
                            {
                                graphicItem.ForceColor = forceColor;
                            }
                            break;
                        case "BarcodeDocumentItem":
                            item = new BarcodeDocumentItem();

                            if (item is BarcodeDocumentItem barcodeItem)
                            {
                                // Get barcode type from XML if available
                                XmlNode barcodeDataNode = node.SelectSingleNode("BarcodeData");
                                if (barcodeDataNode != null)
                                {
                                    string barcodeType = barcodeDataNode.Attributes?["Type"]?.Value;
                                    if (!string.IsNullOrEmpty(barcodeType))
                                    {
                                        // Convert from "UpcABarcodeData" to "UPC_A" format
                                        if (barcodeType.EndsWith("BarcodeData"))
                                        {
                                            string formatName = barcodeType.Substring(0, barcodeType.Length - 11);
                                            // Convert camelCase to UPPER_SNAKE_CASE
                                            barcodeItem.BarcodeType = System.Text.RegularExpressions.Regex.Replace(formatName, "([a-z])([A-Z])", "$1_$2").ToUpper();
                                        }
                                    }

                                    // Get other barcode properties
                                    XmlNode hasCheckDigitNode = barcodeDataNode.SelectSingleNode("HasCheckDigit");
                                    if (hasCheckDigitNode != null && !string.IsNullOrEmpty(hasCheckDigitNode.InnerText))
                                    {
                                        barcodeItem.HasCheckDigit = bool.Parse(hasCheckDigitNode.InnerText);
                                    }

                                    XmlNode displayCheckDigitNode = barcodeDataNode.SelectSingleNode("DisplayCheckDigit");
                                    if (displayCheckDigitNode != null && !string.IsNullOrEmpty(displayCheckDigitNode.InnerText))
                                    {
                                        barcodeItem.DisplayCheckDigit = bool.Parse(displayCheckDigitNode.InnerText);
                                    }

                                    XmlNode moduleHeightNode = barcodeDataNode.SelectSingleNode("ModuleHeight");
                                    if (moduleHeightNode != null && !string.IsNullOrEmpty(moduleHeightNode.InnerText))
                                    {
                                        barcodeItem.ModuleHeight = double.Parse(moduleHeightNode.InnerText) * 0.00377953;
                                    }

                                    XmlNode baseBarWidthNode = barcodeDataNode.SelectSingleNode("BaseBarWidth");
                                    if (baseBarWidthNode != null && !string.IsNullOrEmpty(baseBarWidthNode.InnerText))
                                    {
                                        barcodeItem.ModuleWidth = double.Parse(baseBarWidthNode.InnerText) * 0.00377953;
                                    }
                                }
                            }

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
                    // Inside the LoadFormatFromXml method, in the part that handles TextDocumentItem:

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
                            XmlNode alignmentNode = node.SelectSingleNode("TextBoxAlignment");
                            if (alignmentNode != null)
                            {
                                string alignmentValue = alignmentNode.InnerText;
                                switch (alignmentValue)
                                {
                                    case "1": // Left
                                        textItem.TextAlignment = TextAlignment.Left;
                                        break;
                                    case "2": // Center
                                        textItem.TextAlignment = TextAlignment.Center;
                                        break;
                                    case "3": // Right
                                        textItem.TextAlignment = TextAlignment.Right;
                                        break;
                                    case "4": // Justify
                                        textItem.TextAlignment = TextAlignment.Justify;
                                        break;
                                }
                            }

                            // For TextBoxItem, handle text wrapping and best fit
                            if (textItem is TextBoxItem textBox)
                            {
                                // Handle text wrapping
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

                                // Handle best fit properties
                                XmlNode bestFitNode = node.SelectSingleNode("BestFit");
                                if (bestFitNode != null && !string.IsNullOrEmpty(bestFitNode.InnerText))
                                {
                                    textBox.BestFit = bestFitNode.InnerText == "1";
                                }

                                XmlNode minFontSizeNode = node.SelectSingleNode("BestFitMinimumFontSize");
                                if (minFontSizeNode != null && !string.IsNullOrEmpty(minFontSizeNode.InnerText))
                                {
                                    double.TryParse(minFontSizeNode.InnerText, out double minSize);
                                    textBox.BestFitMinimumFontSize = minSize;
                                }

                                XmlNode maxFontSizeNode = node.SelectSingleNode("BestFitMaximumFontSize");
                                if (maxFontSizeNode != null && !string.IsNullOrEmpty(maxFontSizeNode.InnerText))
                                {
                                    double.TryParse(maxFontSizeNode.InnerText, out double maxSize);
                                    textBox.BestFitMaximumFontSize = maxSize;
                                }

                                // Additional scaling factors if needed
                                XmlNode minScalingNode = node.SelectSingleNode("BestFitMinimumFontScaling");
                                if (minScalingNode != null && !string.IsNullOrEmpty(minScalingNode.InnerText))
                                {
                                    double.TryParse(minScalingNode.InnerText, out double minScaling);
                                    textBox.BestFitMinimumFontScaling = minScaling;
                                }

                                XmlNode maxScalingNode = node.SelectSingleNode("BestFitMaximumFontScaling");
                                if (maxScalingNode != null && !string.IsNullOrEmpty(maxScalingNode.InnerText))
                                {
                                    double.TryParse(maxScalingNode.InnerText, out double maxScaling);
                                    textBox.BestFitMaximumFontScaling = maxScaling;
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

            VerifyDocumentItemReferences();

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
            // Clear function result cache before rendering
            ClearFunctionCache();

            // Add debug logging for textboxes with function datasources
            System.Diagnostics.Debug.WriteLine("----------- RENDER STARTED -----------");
            foreach (var item in DocumentItems)
            {
                if (!string.IsNullOrEmpty(item.DataSourceId))
                {
                    string content = ResolveContent(item);
                    System.Diagnostics.Debug.WriteLine($"Item: {item.Name} (ID: {item.DataSourceId})");
                    System.Diagnostics.Debug.WriteLine($"  Resolved content: '{content}'");

                    // Special handling for the DescriptionFields function
                    if (item.DataSourceId == "246def0c-4bd4-4a59-885f-901b15ae3eee" ||
                        item.DataSourceId == "DescriptionFields")
                    {
                        System.Diagnostics.Debug.WriteLine($"  Special DescriptionFields function detected!");

                        // Directly resolve by function ID
                        if (Functions.TryGetValue("246def0c-4bd4-4a59-885f-901b15ae3eee", out Function func))
                        {
                            string directResult = ExecuteFunctionWithDependencies(func.Id);
                            System.Diagnostics.Debug.WriteLine($"  Direct function result: '{directResult}'");
                        }
                    }
                }
            }

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

        // Update the RenderTextObject method in LabelModel.cs
        private void RenderTextObject(Canvas canvas, TextObjectItem item)
        {
            string content = ResolveContent(item);

            item.Width = GetTextWidth(content, new FontFamily(item.FontName), item.FontSize);


            TextBlock textBlock = new TextBlock
            {
                Text = FormatTextContent(content),
                Width = item.Width,
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
                    Width = item.Width > 0 ? item.Width : 100, // Default width for visualization
                    Height = item.FontSize,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Red,
                    StrokeThickness = 1
                };

                Canvas.SetLeft(debugRect, position.X);
                Canvas.SetTop(debugRect, position.Y);

                // Calculate the actual anchor point coordinates based on the anchoring type
                Point anchorPoint = CalculateAnchorPoint(position, item.Width, item.FontSize, item.AnchoringPoint);

                Rectangle debugAnchor = new Rectangle
                {
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Red
                };

                // Position the anchor point indicator at the actual anchor point
                Canvas.SetLeft(debugAnchor, anchorPoint.X - 2.5); // Center the 5x5 indicator
                Canvas.SetTop(debugAnchor, anchorPoint.Y - 2.5);  // Center the 5x5 indicator

                TextBlock anchorText = new TextBlock
                {
                    Text = item.AnchoringPoint.ToString(),
                    Foreground = Brushes.Red,
                    FontSize = 8
                };

                Canvas.SetLeft(anchorText, anchorPoint.X);
                Canvas.SetTop(anchorText, anchorPoint.Y - 15);

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

        // Update the RenderTextBox method in LabelModel.cs
        private void RenderTextBox(Canvas canvas, TextBoxItem item)
        {
            // Get content with special handling for DescriptionFields function
            string content;

            if (item.DataSourceId == "246def0c-4bd4-4a59-885f-901b15ae3eee" ||
                item.DataSourceId == "DescriptionFields")
            {
                // Special handling for DescriptionFields function
                System.Diagnostics.Debug.WriteLine($"Special handling for DescriptionFields in TextBox: {item.Name}");

                // Try to find function directly by ID
                if (Functions.TryGetValue("246def0c-4bd4-4a59-885f-901b15ae3eee", out Function func))
                {
                    content = ExecuteFunctionWithDependencies(func.Id);
                    System.Diagnostics.Debug.WriteLine($"Direct result: '{content}'");
                }
                else if (FunctionNameToIdMap.TryGetValue("DescriptionFields", out string functionId))
                {
                    // Try by name lookup
                    content = ExecuteFunctionWithDependencies(functionId);
                    System.Diagnostics.Debug.WriteLine($"Name lookup result: '{content}'");
                }
                else
                {
                    // Fall back to normal content resolution
                    content = ResolveContent(item);
                    System.Diagnostics.Debug.WriteLine($"Standard resolve result: '{content}'");
                }
            }
            else
            {
                // Normal content resolution
                content = ResolveContent(item);
            }

            // Process content to replace VBScript notations
            content = FormatTextContent(content);

            // Create the text block with initial properties
            TextBlock textBlock = new TextBlock
            {
                Text = content,
                FontFamily = new FontFamily(item.FontName),
                FontSize = item.FontSize,
                Foreground = new SolidColorBrush(item.FontColor),
                TextAlignment = item.TextAlignment,
                TextWrapping = item.TextWrapping
            };

            // Set initial width and height
            if (item.Width > 0)
            {
                textBlock.Width = item.Width;
            }

            if (item.Height > 0)
            {
                textBlock.Height = item.Height;
            }

            // Apply best fit text sizing if enabled
            if (item.BestFit && item.Width > 0 && item.Height > 0)
            {
                try
                {
                    // Start with the maximum font size
                    double currentFontSize = item.BestFitMaximumFontSize;
                    textBlock.FontSize = currentFontSize;

                    // Create a temporary text block to measure text
                    TextBlock measuringBlock = new TextBlock
                    {
                        Text = content,
                        FontFamily = textBlock.FontFamily,
                        TextWrapping = textBlock.TextWrapping,
                        Width = item.Width
                    };

                    // Binary search for best fit font size
                    double minSize = item.BestFitMinimumFontSize;
                    double maxSize = item.BestFitMaximumFontSize;

                    int iterations = 0;  // Safeguard to prevent infinite loops
                    const int MaxIterations = 20;

                    while (maxSize - minSize > 0.5 && iterations < MaxIterations) // 0.5pt precision
                    {
                        iterations++;
                        currentFontSize = (minSize + maxSize) / 2;
                        measuringBlock.FontSize = currentFontSize;

                        // Measure the text at current font size
                        measuringBlock.Measure(new Size(item.Width, double.PositiveInfinity));

                        if (measuringBlock.DesiredSize.Height <= item.Height)
                        {
                            // Text fits, try larger
                            minSize = currentFontSize;
                        }
                        else
                        {
                            // Text too large, try smaller
                            maxSize = currentFontSize;
                        }
                    }

                    // Set the final best fit size
                    textBlock.FontSize = minSize; // Use the largest size that fits

                    // Debug info
                    if (ShowDebugInfo)
                    {
                        System.Diagnostics.Debug.WriteLine($"Best fit for {item.Name}: Original size {item.FontSize}, Best fit size {minSize}");
                    }
                }
                catch (Exception ex)
                {
                    // If best fit fails, use the original font size
                    System.Diagnostics.Debug.WriteLine($"Best fit failed: {ex.Message}");
                }
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

                // Calculate the actual anchor point based on the anchoring type
                Point anchorPoint = CalculateAnchorPoint(
                    position,
                    item.Width > 0 ? item.Width : 100,
                    item.Height > 0 ? item.Height : item.FontSize * 2,
                    item.AnchoringPoint);

                Ellipse debugAnchor = new Ellipse
                {
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Blue
                };

                // Position the anchor point indicator at the actual anchor point
                Canvas.SetLeft(debugAnchor, anchorPoint.X - 2.5); // Center the 5x5 indicator
                Canvas.SetTop(debugAnchor, anchorPoint.Y - 2.5);  // Center the 5x5 indicator

                canvas.Children.Add(debugRect);
                canvas.Children.Add(debugAnchor);
            }

            if (ShowDebugInfo &&
                (item.DataSourceId == "246def0c-4bd4-4a59-885f-901b15ae3eee" ||
                 item.DataSourceId == "DescriptionFields"))
            {
                // Visual indicator that this is using DescriptionFields
                Rectangle marker = new Rectangle
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Purple,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                Canvas.SetLeft(marker, position.X - 15);
                Canvas.SetTop(marker, position.Y);
                canvas.Children.Add(marker);

                // Add an annotation
                TextBlock annotation = new TextBlock
                {
                    Text = "DescriptionFields",
                    FontSize = 8,
                    Foreground = Brushes.Purple,
                    Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255))
                };

                Canvas.SetLeft(annotation, position.X - 15);
                Canvas.SetTop(annotation, position.Y + 15);
                canvas.Children.Add(annotation);

                // Add best fit info if enabled
                if (item.BestFit)
                {
                    TextBlock bestFitInfo = new TextBlock
                    {
                        Text = $"BestFit: {textBlock.FontSize:0.0}pt",
                        FontSize = 8,
                        Foreground = Brushes.DarkBlue,
                        Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255))
                    };

                    Canvas.SetLeft(bestFitInfo, position.X - 15);
                    Canvas.SetTop(bestFitInfo, position.Y + 30);
                    canvas.Children.Add(bestFitInfo);
                }
            }

            // Set z-order if available
            if (item.ZOrder != 0)
            {
                Canvas.SetZIndex(textBlock, item.ZOrder);
            }

            canvas.Children.Add(textBlock);
        }

        /// <summary>
        /// Ensures text content has proper newline formatting for WPF TextBlock
        /// </summary>
        private string FormatTextContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            // Ensure Windows newlines are converted properly for TextBlock
            // TextBlock uses Environment.NewLine which is \r\n on Windows
            return content.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
        }

        /// <summary>
        /// Verifies and fixes document item references to functions
        /// </summary>
        public void VerifyDocumentItemReferences()
        {
            System.Diagnostics.Debug.WriteLine("Verifying document item references...");

            // Target function ID
            string targetFunctionId = "246def0c-4bd4-4a59-885f-901b15ae3eee";
            string targetFunctionName = "DescriptionFields";

            // Check if function exists
            bool functionExists = Functions.ContainsKey(targetFunctionId);
            System.Diagnostics.Debug.WriteLine($"Function ID '{targetFunctionId}' exists: {functionExists}");

            if (!functionExists)
            {
                System.Diagnostics.Debug.WriteLine("Function doesn't exist - checking by name...");

                // Try to find by name
                if (FunctionNameToIdMap.TryGetValue(targetFunctionName, out string foundId))
                {
                    System.Diagnostics.Debug.WriteLine($"Found via name: {foundId}");
                    targetFunctionId = foundId;
                    functionExists = true;
                }
            }

            if (!functionExists)
            {
                System.Diagnostics.Debug.WriteLine("Function doesn't exist - creating it...");

                // Create the function if it doesn't exist
                var concatFunction = new ConcatenateFunction
                {
                    Id = targetFunctionId,
                    Name = targetFunctionName,
                    FunctionType = "ConcatenateFunction",
                    Separator = "\r\n", // Decoded from Base64 "DQo="
                    IgnoreEmptyValues = false
                };

                // Add the data sources
                concatFunction.DataSourceIds.Add("9b19b3d6-fd8e-4250-b84e-64fa7bd7a049"); // Description1
                concatFunction.DataSourceIds.Add("d8409940-a513-4e07-8cda-b92361625140"); // Description2

                // Add the function to the model
                Functions[targetFunctionId] = concatFunction;

                // Update the mappings
                FunctionIdToNameMap[targetFunctionId] = targetFunctionName;
                FunctionNameToIdMap[targetFunctionName] = targetFunctionId;

                System.Diagnostics.Debug.WriteLine("Function created successfully!");
            }

            // Check document items
            System.Diagnostics.Debug.WriteLine("Checking document items that reference this function...");
            int count = 0;

            foreach (var item in DocumentItems)
            {
                if (item.DataSourceId == targetFunctionId || item.DataSourceId == targetFunctionName)
                {
                    count++;
                    System.Diagnostics.Debug.WriteLine($"Item {count}: {item.GetType().Name} '{item.Name}' (ID: {item.DataSourceId})");

                    // Ensure the item references the correct function ID
                    if (item.DataSourceId != targetFunctionId)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Fixing reference from '{item.DataSourceId}' to '{targetFunctionId}'");
                        item.DataSourceId = targetFunctionId;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Found {count} items referencing this function");

            if (count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No items reference the function - checking for items that might be missing reference...");

                // Look for text items that might be intended to show the concatenated fields
                foreach (var item in DocumentItems.OfType<TextDocumentItem>())
                {
                    if (item.Content != null &&
                        (item.Content.Contains("Description1") || item.Content.Contains("Description2")))
                    {
                        System.Diagnostics.Debug.WriteLine($"Possible candidate: {item.GetType().Name} '{item.Name}' with content: '{item.Content}'");
                    }
                }
            }
        }

        // Add this helper method to calculate the actual anchor point coordinates
        private Point CalculateAnchorPoint(Point adjustedPosition, double width, double height, AnchoringPoint anchorType)
        {
            double x = adjustedPosition.X;
            double y = adjustedPosition.Y;

            switch (anchorType)
            {
                case AnchoringPoint.LeftTop:
                    // No adjustment needed, anchor is at the top-left
                    break;
                case AnchoringPoint.CenterTop:
                    x += width / 2;
                    break;
                case AnchoringPoint.RightTop:
                    x += width;
                    break;
                case AnchoringPoint.LeftMiddle:
                    y += height / 2;
                    break;
                case AnchoringPoint.CenterMiddle:
                    x += width / 2;
                    y += height / 2;
                    break;
                case AnchoringPoint.RightMiddle:
                    x += width;
                    y += height / 2;
                    break;
                case AnchoringPoint.LeftBottom:
                    y += height;
                    break;
                case AnchoringPoint.CenterBottom:
                    x += width / 2;
                    y += height;
                    break;
                case AnchoringPoint.RightBottom:
                    x += width;
                    y += height;
                    break;
            }

            return new Point(x, y);
        }
        public static double GetTextWidth(string text, FontFamily fontFamily, double fontSize)
        {
            // Create a FormattedText object to measure the text
            FormattedText formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                fontSize,
                Brushes.Black,
                VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);

            // Return the width
            return formattedText.Width;
        }

        // Add a property to control debug visualization
        public bool ShowDebugInfo { get; set; } = true;

        private void RenderGraphicItem(Canvas canvas, GraphicDocumentItem item)
        {
            // Set default size
            if (item.Width == 0) { item.Width = 50; }
            if (item.Height == 0) { item.Height = 50; }

            // Get position adjusted for anchoring point
            Point position = item.GetAdjustedPosition();

            // Try to get image path from the data source if available
            string imagePath = null;

            if (!string.IsNullOrEmpty(item.ImagePath))
            {
                imagePath = item.ImagePath;
            }
            else if (!string.IsNullOrEmpty(item.DataSourceId))
            {
                // Try to resolve from a variable or function
                if (Variables.TryGetValue(item.DataSourceId, out Variable variable))
                {
                    imagePath = variable.SampleValue;
                }
                else if (Functions.TryGetValue(item.DataSourceId, out Function function))
                {
                    imagePath = ResolveContent(item); //cp_35
                }
            }

            // Try to load and display the image if a path is available
            if (!string.IsNullOrEmpty(imagePath))
            {
                if (!System.IO.Path.IsPathRooted(imagePath))
                {
                    // Prepend the default images folder path
                    imagePath = System.IO.Path.Combine(@"C:\Program Files\MM_Label\Labels\Images", imagePath);
                }

                if (File.Exists(imagePath))
                {
                    try
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(imagePath);
                        bitmap.EndInit();
                        bitmap.Freeze(); // Optimize performance

                        // Convert white to transparent if needed
                        BitmapSource transparentBitmap = MakeWhiteTransparent(bitmap);

                        Image image = new Image
                        {
                            Source = transparentBitmap,
                            Width = item.Width,
                            Height = item.Height,
                            Stretch = Stretch.Uniform
                        };

                        // Apply stretching based on resize mode
                        switch (item.ResizeMode)
                        {
                            case 0: // None
                                image.Stretch = Stretch.None;
                                break;
                            case 1: // Fill
                                image.Stretch = Stretch.Fill;
                                break;
                            case 2: // Uniform
                                image.Stretch = Stretch.Uniform;
                                break;
                            case 3: // Uniform to Fill
                                image.Stretch = Stretch.UniformToFill;
                                break;
                        }

                        Canvas.SetLeft(image, position.X);
                        Canvas.SetTop(image, position.Y);

                        // Set z-order if available
                        if (item.ZOrder != 0)
                        {
                            Canvas.SetZIndex(image, item.ZOrder);
                        }

                        canvas.Children.Add(image);

                        // If debug mode is on, add a border around the image
                        if (ShowDebugInfo)
                        {
                            Rectangle debugRect = new Rectangle
                            {
                                Width = item.Width,
                                Height = item.Height,
                                Stroke = Brushes.Green,
                                StrokeThickness = 1,
                                Fill = Brushes.Transparent
                            };

                            Canvas.SetLeft(debugRect, position.X);
                            Canvas.SetTop(debugRect, position.Y);
                            Canvas.SetZIndex(debugRect, item.ZOrder + 1);
                            canvas.Children.Add(debugRect);

                            TextBlock debugText = new TextBlock
                            {
                                Text = item.Name,
                                FontSize = 8,
                                Foreground = Brushes.Green,
                                Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255))
                            };

                            Canvas.SetLeft(debugText, position.X + 2);
                            Canvas.SetTop(debugText, position.Y + 2);
                            Canvas.SetZIndex(debugText, item.ZOrder + 2);
                            canvas.Children.Add(debugText);
                        }

                        return;
                    }
                    catch (Exception ex)
                    {
                        // If image loading fails, fall back to the placeholder
                        Console.WriteLine($"Error loading image: {ex.Message}");
                    }
                }

                // If image loading failed or no path, create a placeholder
                Rectangle rect = new Rectangle
                {
                    Width = item.Width,
                    Height = item.Height,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Fill = Brushes.LightGray
                };

                Canvas.SetLeft(rect, position.X);
                Canvas.SetTop(rect, position.Y);

                // Set z-order if available
                if (item.ZOrder != 0)
                {
                    Canvas.SetZIndex(rect, item.ZOrder);
                }

                canvas.Children.Add(rect);

                // Add text showing the image name or data source
                string displayText = item.Name;
                if (!string.IsNullOrEmpty(imagePath))
                {
                    displayText += $"\n{System.IO.Path.GetFileName(imagePath)}";
                }
                else if (!string.IsNullOrEmpty(item.DataSourceId))
                {
                    displayText += $"\nDataSource: {item.DataSourceId}";
                }

                TextBlock textBlock = new TextBlock
                {
                    Text = displayText,
                    FontSize = 8,
                    TextWrapping = TextWrapping.Wrap,
                    Width = item.Width - 10,
                    Height = item.Height - 10,
                    Foreground = Brushes.Black,
                    Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255))
                };

                Canvas.SetLeft(textBlock, position.X + 5);
                Canvas.SetTop(textBlock, position.Y + 5);
                Canvas.SetZIndex(textBlock, item.ZOrder + 1);
                canvas.Children.Add(textBlock);
            }
        }

        /// <summary>
        /// Utility class for unit conversions
        /// </summary>


        private void RenderBarcodeItem(Canvas canvas, BarcodeDocumentItem item)
        {
            // Set default size
            if (item.Width == 0) { item.Width = 100; }
            if (item.Height == 0) { item.Height = 50; }

            // Get the content for the barcode
            string content = ResolveContent(item);

            // Get position adjusted for anchoring point
            Point position = item.GetAdjustedPosition();

            // Try to render the barcode using ZXing, but fall back to a placeholder if it fails
            try
            {
                // Create the barcode writer
                // Create a barcode writer for direct bitmap output
                var barcodeWriter = new BarcodeWriter()
                {
                    Format = BarcodeFormat.UPC_A, // Default to UPC_A
                    Options = new EncodingOptions
                    {
                        Width = (int)item.Width,
                        Height = (int)item.Height,
                        Margin = 2,
                        GS1Format = false, // Enable GS1 format which includes proper descender bars
                        PureBarcode = false // Ensure we get the full barcode with descenders
                    },
                    Renderer = new BitmapRenderer
                    {
                        TextFont = new System.Drawing.Font("calibri", 8),
                        Background = System.Drawing.Color.Transparent,
                        Foreground = System.Drawing.Color.Black
                    }
                };

                // Generate the barcode as a System.Drawing.Bitmap
                System.Drawing.Bitmap drawingBitmap = barcodeWriter.Write(content);

                // Convert System.Drawing.Bitmap to BitmapSource for WPF
                BitmapSource barcodeBitmap;
                using (var memory = new System.IO.MemoryStream())
                {
                    drawingBitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); // Important for cross-thread usage

                    barcodeBitmap = MakeWhiteTransparent(bitmapImage);
                }

                // Clean up the drawing bitmap
                drawingBitmap.Dispose();

                // Create an Image control to display the barcode
                Image image = new Image
                {
                    Source = barcodeBitmap,
                    Width = item.Width,
                    Height = item.Height,
                    Stretch = Stretch.Fill
                };

                // Position the image
                Canvas.SetLeft(image, position.X);
                Canvas.SetTop(image, position.Y);

                // Set z-order if available
                if (item.ZOrder != 0)
                {
                    Canvas.SetZIndex(image, item.ZOrder);
                }

                canvas.Children.Add(image);

                // Add debug info if needed
                if (ShowDebugInfo)
                {
                    Rectangle debugRect = new Rectangle
                    {
                        Width = item.Width,
                        Height = item.Height,
                        Stroke = Brushes.Purple,
                        StrokeThickness = 1,
                        Fill = Brushes.Transparent
                    };

                    Canvas.SetLeft(debugRect, position.X);
                    Canvas.SetTop(debugRect, position.Y);
                    Canvas.SetZIndex(debugRect, item.ZOrder + 1);
                    canvas.Children.Add(debugRect);

                    TextBlock debugText = new TextBlock
                    {
                        Text = $"{item.Name}: {content}",
                        FontSize = 8,
                        Foreground = Brushes.Purple,
                        Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255))
                    };

                    Canvas.SetLeft(debugText, position.X + 2);
                    Canvas.SetTop(debugText, position.Y + 2);
                    Canvas.SetZIndex(debugText, item.ZOrder + 2);
                    //canvas.Children.Add(debugText);
                }
            }
            catch (Exception ex)
            {
                // Fallback to placeholder rendering
                Rectangle rect = new Rectangle
                {
                    Width = item.Width > 0 ? item.Width : 100,
                    Height = item.Height > 0 ? item.Height : 50,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Fill = Brushes.White
                };

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
                    Text = $"BARCODE: {content}\nError: {ex.Message}",
                    FontSize = 8,
                    TextWrapping = TextWrapping.Wrap,
                    Width = item.Width - 10,
                    Height = item.Height - 5
                };

                Canvas.SetLeft(textBlock, position.X + 5);
                Canvas.SetTop(textBlock, position.Y + 5);
                Canvas.SetZIndex(textBlock, item.ZOrder + 1);
                canvas.Children.Add(textBlock);
            }
        }

        /// <summary>
        /// Resolves the content for a document item, taking into account function dependencies
        /// </summary>
        private string ResolveContent(DocumentItem item)
        {
            if (item == null) return "???";

            if (!string.IsNullOrEmpty(item.DataSourceId))
            {
                // First check if it's a direct variable reference
                if (Variables.TryGetValue(item.DataSourceId, out Variable variable))
                {
                    return variable.SampleValue ?? "???";
                }

                // Check if it's a direct function reference by ID
                if (Functions.TryGetValue(item.DataSourceId, out Function function))
                {
                    return ExecuteFunctionWithDependencies(function.Id);
                }

                // Check if it's a function reference by name
                if (FunctionNameToIdMap.TryGetValue(item.DataSourceId, out string functionId) &&
                    Functions.TryGetValue(functionId, out function))
                {
                    return ExecuteFunctionWithDependencies(functionId);
                }

                // Debug the missing reference
                System.Diagnostics.Debug.WriteLine($"Could not resolve content for ID: {item.DataSourceId}");
                System.Diagnostics.Debug.WriteLine($"Available Functions: {string.Join(", ", Functions.Keys)}");
                System.Diagnostics.Debug.WriteLine($"Available Function Names: {string.Join(", ", FunctionNameToIdMap.Keys)}");
            }

            return item.Content ?? "???";
        }

        /// <summary>
        /// Executes a function, handling any dependencies on other functions
        /// </summary>
        public string ExecuteFunctionWithDependencies(string functionId)
        {
            // First try to resolve by name if it's not a direct ID
            if (!Functions.ContainsKey(functionId) && FunctionNameToIdMap.TryGetValue(functionId, out string resolvedId))
            {
                functionId = resolvedId;
            }

            // Check if we have a cached result
            if (functionResultCache.TryGetValue(functionId, out string cachedResult))
            {
                return cachedResult;
            }

            // Check for circular dependencies
            if (functionsInProgress.Contains(functionId))
            {
                return $"Circular dependency detected for function {functionId}";
            }

            // Get the function
            if (!Functions.TryGetValue(functionId, out Function function))
            {
                return $"Function {functionId} not found";
            }

            try
            {
                // Mark function as in-progress to detect circular dependencies
                functionsInProgress.Add(functionId);

                // If this function depends on other functions, resolve those first
                Dictionary<string, Variable> resolvedVariables = new Dictionary<string, Variable>();

                // Copy all regular variables
                foreach (var kvp in Variables)
                {
                    resolvedVariables[kvp.Key] = kvp.Value;
                }

                // For each referenced data source, ensure it's resolved
                foreach (string dataSourceId in function.InputDataSourceIds)
                {
                    // If it's a regular variable, it's already in resolvedVariables
                    // If it's a function, we need to execute it and create a temporary variable
                    if (!Variables.ContainsKey(dataSourceId) && Functions.ContainsKey(dataSourceId))
                    {
                        string dependencyResult = ExecuteFunctionWithDependencies(dataSourceId);

                        // Create a temporary variable with the result
                        resolvedVariables[dataSourceId] = new Variable
                        {
                            Id = dataSourceId,
                            Name = Functions[dataSourceId].Name,
                            SampleValue = dependencyResult
                        };
                    }
                }

                // For ConcatenateFunction, also handle its DataSourceIds if they reference functions
                if (function is ConcatenateFunction concatFunction)
                {
                    foreach (string dataSourceId in concatFunction.DataSourceIds)
                    {
                        // If it's a function reference, execute it and create a temporary variable
                        if (!Variables.ContainsKey(dataSourceId) && Functions.ContainsKey(dataSourceId))
                        {
                            string dependencyResult = ExecuteFunctionWithDependencies(dataSourceId);

                            // Create a temporary variable with the result
                            resolvedVariables[dataSourceId] = new Variable
                            {
                                Id = dataSourceId,
                                Name = Functions[dataSourceId].Name,
                                SampleValue = dependencyResult
                            };
                        }
                    }
                }

                // Execute the function with all dependencies resolved
                string result = function.Execute(resolvedVariables, VariableIdToNameMap);

                // Cache the result
                functionResultCache[functionId] = result;

                return result;
            }
            catch (Exception ex)
            {
                return function.SampleValue ?? $"Error: {ex.Message}";
            }
            finally
            {
                // Remove the function from in-progress list
                functionsInProgress.Remove(functionId);
            }
        }

        public void DumpFunctionInfo()
        {
            System.Diagnostics.Debug.WriteLine("=== FUNCTION INFO DUMP ===");

            System.Diagnostics.Debug.WriteLine("Function Count: " + Functions.Count);

            System.Diagnostics.Debug.WriteLine("\nFunction IDs:");
            foreach (var id in Functions.Keys)
            {
                System.Diagnostics.Debug.WriteLine($"  {id}");
            }

            System.Diagnostics.Debug.WriteLine("\nFunction Names:");
            foreach (var func in Functions.Values)
            {
                System.Diagnostics.Debug.WriteLine($"  {func.Name} ({func.Id})");
            }

            System.Diagnostics.Debug.WriteLine("\nFunction ID to Name Map:");
            foreach (var pair in FunctionIdToNameMap)
            {
                System.Diagnostics.Debug.WriteLine($"  {pair.Key} => {pair.Value}");
            }

            System.Diagnostics.Debug.WriteLine("\nFunction Name to ID Map:");
            foreach (var pair in FunctionNameToIdMap)
            {
                System.Diagnostics.Debug.WriteLine($"  {pair.Key} => {pair.Value}");
            }

            System.Diagnostics.Debug.WriteLine("=== END FUNCTION INFO ===");
        }

        /// <summary>
        /// Clears cached function results
        /// </summary>
        public void ClearFunctionCache()
        {
            functionResultCache.Clear();
            functionsInProgress.Clear();
        }

        private BitmapSource MakeWhiteTransparent(BitmapImage bitmap)
        {
            // Convert BitmapImage to a writeable format
            WriteableBitmap writeBmp = new WriteableBitmap(bitmap);

            // Get pixel data
            int width = writeBmp.PixelWidth;
            int height = writeBmp.PixelHeight;
            int stride = width * 4; // 4 bytes per pixel (BGRA)
            byte[] pixels = new byte[height * stride];
            writeBmp.CopyPixels(pixels, stride, 0);

            // Process each pixel
            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte blue = pixels[i];
                byte green = pixels[i + 1];
                byte red = pixels[i + 2];

                // Check if pixel is close to white (within 10% tolerance)
                // 255 * 0.9 = 229.5
                if (red >= 230 && green >= 230 && blue >= 230)
                {
                    pixels[i + 3] = 0; // Set alpha to transparent
                }
            }

            // Create new bitmap with modified pixels
            return BitmapSource.Create(
                width, height,
                96, 96, // DPI
                PixelFormats.Bgra32,
                null,
                pixels,
                stride);
        }
    }

    public static class UnitConverter
    {
        /// <summary>
        /// Converts a measurement in microns (1/1000 mm) to WPF units (1/96 inch)
        /// </summary>
        /// <param name="microns">The value in microns</param>
        /// <returns>The equivalent value in WPF display units (logical pixels at 96 DPI)</returns>
        public static double MicronsToWpfUnits(double microns)
        {
            const double MicronsPerInch = 25400.0;
            const double PixelsPerInch = 96.0;

            return microns * (PixelsPerInch / MicronsPerInch);
        }

        /// <summary>
        /// Converts WPF units (1/96 inch) to microns (1/1000 mm)
        /// </summary>
        /// <param name="wpfUnits">The value in WPF display units (logical pixels at 96 DPI)</param>
        /// <returns>The equivalent value in microns</returns>
        public static double WpfUnitsToMicrons(double wpfUnits)
        {
            const double MicronsPerInch = 25400.0;
            const double PixelsPerInch = 96.0;

            return wpfUnits * (MicronsPerInch / PixelsPerInch);
        }
    }
}