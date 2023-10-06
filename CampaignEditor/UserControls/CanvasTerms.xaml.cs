using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Represents canvas with mediaPlan terms
    /// </summary>
    public partial class CanvasTerms : UserControl
    {

        Dictionary<int, Canvas> canvasNumDict = new Dictionary<int, Canvas>();
        int _lineHeight;
        public CanvasTerms()
        {
            InitializeComponent();
        }

        public void Initialize(int lineHeight)
        {
            canvasNumDict.Add(0, canvas1);
            canvasNumDict.Add(1, canvas2);
            _lineHeight = lineHeight;
        }

        public void DrawTermRectangle(int length, int offset, int canvasNum, string name)
        {
            TermRectangle tr = new TermRectangle(_lineHeight * length, Brushes.Beige);

            Canvas cnv = canvasNumDict[canvasNum];

            tr.Width = (double)cnv.ActualWidth;

            tr.lblName.Content = name;
            Canvas.SetLeft(tr, 0);
            Canvas.SetTop(tr, _lineHeight * (offset + 1)); // Set Y-coordinate (vertical offset)
          
            cnv.Children.Add(tr);
        }

        public void ClearCanvas()
        {
            canvas1.Children.Clear();
            canvas2.Children.Clear();
        }

        private void canvas_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            foreach (TermRectangle tr in canvas1.Children)
            {
                tr.Width = (double)canvas1.ActualWidth;
            }

            foreach (TermRectangle tr in canvas2.Children)
            {
                tr.Width = (double)canvas2.ActualWidth;
            }
        }
    }
}
