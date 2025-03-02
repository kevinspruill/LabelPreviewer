using System.Windows;

namespace LabelPreviewer
{
    // A text box has explicit width/height constraints and can wrap text
    public class TextBoxItem : TextDocumentItem
    {
        public TextWrapping TextWrapping { get; set; } = TextWrapping.Wrap;

        // Best fit properties
        public bool BestFit { get; set; } = false;
        public double BestFitMinimumFontSize { get; set; } = 4.0;
        public double BestFitMaximumFontSize { get; set; } = 72.0;

        // Font scaling - optional for advanced scaling
        public double BestFitMinimumFontScaling { get; set; } = 20; // Percentage, 20 = 20%
        public double BestFitMaximumFontScaling { get; set; } = 200; // Percentage, 200 = 200%
    }
}