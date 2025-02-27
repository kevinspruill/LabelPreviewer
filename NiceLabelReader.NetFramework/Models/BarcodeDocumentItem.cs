namespace LabelPreviewer
{
    public class BarcodeDocumentItem : DocumentItem
    {
        public string BarcodeType { get; set; } = "UPC_A";
        public bool HasCheckDigit { get; set; } = true;
        public bool DisplayCheckDigit { get; set; } = false;
        public double ModuleWidth { get; set; } = 1;
        public double ModuleHeight { get; set; } = 50;
        public int Margin { get; set; } = 4;
    }
}