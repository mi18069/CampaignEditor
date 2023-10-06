
using System.Windows.Controls;
using System.Windows.Media;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Represents canvas with mediaPlan terms
    /// </summary>
    public partial class CanvasTerms : UserControl
    {
        public CanvasTerms()
        {
            InitializeComponent();
        }

        public void Initialize(int lineHeight)
        {
            TermRectangle tr = new TermRectangle(lineHeight * 10, Brushes.Blue);

            tr.Width = 100;
            Canvas.SetLeft(tr, 0);
            Canvas.SetTop(tr, lineHeight * (15+1)); // Set Y-coordinate (vertical offset)

            canvas1.Children.Add(tr);
        }
    }
}
