using System.Windows;


namespace LabelPreviewer
{
    // A text box has explicit width/height constraints and can wrap text
    public class TextBoxItem : TextDocumentItem
    {
        public TextWrapping TextWrapping { get; set; } = TextWrapping.Wrap;
    }
}