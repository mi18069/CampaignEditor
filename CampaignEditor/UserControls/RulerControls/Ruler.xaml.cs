using Database.Entities;
using System.Windows.Controls;
using System.Windows.Media;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Design for Ruler in RulerTimeline
    /// </summary>
    public partial class Ruler : UserControl
    {
        public Ruler()
        {
            InitializeComponent();
        }

        // For passed "hh:mm" values create intervals
        public void Initialize(int height)
        {
            double spTopMargin = spRuler.Margin.Top;
            // From 02:00 to 25:59 in minutes
            for (int i=2*60; i<25*60+59; i++)
            {
                RulerLine rl = new RulerLine(i, height);
                spRuler.Children.Add(rl);

                if (i % 15 == 0 || i % 30 == 0 || i % 60 == 0)
                {
                    string time = TimeFormat.MinToRepresentative(i);
                    TextBlock timeTextBlock = new TextBlock();
                    timeTextBlock.Text = time;
                    timeTextBlock.FontSize = 10;
                    timeTextBlock.Foreground = Brushes.Black; // Set the text color

                    // Add the TextBlock to the Canvas and set its position
                    double rightMargin = rl.LineWidth + 5;
                    double offset = (i - 2*60)*height + spTopMargin - 5; // Minutes between current and 02:00

                    Canvas.SetRight(timeTextBlock, rightMargin); // Set the X-coordinate
                    Canvas.SetTop(timeTextBlock, offset); // Set the Y-coordinate

                    // Add the TextBlock to the Canvas
                    cTimes.Children.Add(timeTextBlock);
                }
            } 
        }
    }
}
