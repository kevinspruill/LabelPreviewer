using System.Windows;


namespace LabelPreviewer
{
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
        public AnchoringPoint AnchoringPoint { get; set; } = AnchoringPoint.LeftTop;
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
                case AnchoringPoint.CenterTop:
                    adjustedX -= Width / 2;
                    break;
                case AnchoringPoint.RightTop:
                    adjustedX -= Width;
                    break;
                case AnchoringPoint.LeftMiddle:
                    adjustedY -= Height / 2;
                    break;
                case AnchoringPoint.CenterMiddle:
                    adjustedX -= Width / 2;
                    adjustedY -= Height / 2;
                    break;
                case AnchoringPoint.RightMiddle:
                    adjustedX -= Width;
                    adjustedY -= Height / 2;
                    break;
                case AnchoringPoint.LeftBottom:
                    adjustedY -= Height;
                    break;
                case AnchoringPoint.CenterBottom:
                    adjustedX -= Width / 2;
                    adjustedY -= Height;
                    break;
                case AnchoringPoint.RightBottom:
                    adjustedX -= Width;
                    adjustedY -= Height;
                    break;
                    // Default is TopLeft = no adjustment
            }

            return new Point(adjustedX, adjustedY);
        }

    }
}