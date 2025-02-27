using System.Windows;
using System.Windows.Media;


namespace LabelPreviewer
{
    // Base class for all text items
    public abstract class TextDocumentItem : DocumentItem
    {
        public string FontName { get; set; } = "Arial";
        public double FontSize { get; set; } = 10;
        public Color FontColor { get; set; } = Colors.Black;
        public bool IsMultiline { get; set; } = false;
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Left;

    }
}