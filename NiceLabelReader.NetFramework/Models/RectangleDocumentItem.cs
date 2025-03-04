namespace LabelPreviewer
{
    /// <summary>
    /// Represents a rectangle shape in a NiceLabel document
    /// </summary>
    public class RectangleDocumentItem : DocumentItem
    {
        /// <summary>
        /// Corner radius for rounded rectangles (0 for square corners)
        /// </summary>
        public double Radius { get; set; } = 0;

        /// <summary>
        /// Line thickness in NiceLabel units
        /// </summary>
        public double Thickness { get; set; } = 230;

        /// <summary>
        /// Line style (0=Solid, 1=Dash, 2=Dot, 3=DashDot)
        /// </summary>
        public int LineStyle { get; set; } = 0;

        /// <summary>
        /// Fill style (0=None, 1=Solid, 2=Horizontal, 3=Vertical, 4=Diagonal Up, 5=Diagonal Down, 6=Cross, 7=DiagonalCross)
        /// </summary>
        public int FillStyle { get; set; } = 0;

        /// <summary>
        /// Fill color in ARGB format (e.g., FF000000 for black)
        /// </summary>
        public string FillColor { get; set; } = "FF000000";

        /// <summary>
        /// Border color in ARGB format (e.g., FF000000 for black)
        /// </summary>
        public string Color { get; set; } = "FF000000";
    }
}