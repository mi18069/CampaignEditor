using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Only one line on the Ruler, it can have 5 different values: ones, divided by 5, 10, 30 and 60
    /// </summary>
    public partial class RulerLine : UserControl
    {

        int _height = 1;
        public double LineWidth = 0;
        public RulerLine(int timeInMins, int height)
        {
            InitializeComponent();
            _height = height;
            this.Height = height;
            Initialize(timeInMins);
        }

        private void Initialize(int i)
        {

            if (i % 5 != 0) // Most occuring case, when i % 5 != 0
            {
                recLine.Height = _height * 0.15;               
                colLabel.Width = new GridLength(0.95, GridUnitType.Star);
                colLine.Width = new GridLength(0.05, GridUnitType.Star);
                LineWidth = this.Width * 0.05;
            }
            else if (i % 10 != 0 && i % 15 != 0) // if it's divideable by 5, but not 10 or 15
            {
                recLine.Height = _height * 0.25;
                colLabel.Width = new GridLength(0.8, GridUnitType.Star);
                colLine.Width = new GridLength(0.2, GridUnitType.Star);
                LineWidth = this.Width * 0.2;

            }
            else if (i % 10 != 0 && i % 15 == 0) // if it's divideable by 5, but not 10 and it is with 15
            {
                recLine.Height = _height * 0.5;
                colLabel.Width = new GridLength(0.6, GridUnitType.Star);
                colLine.Width = new GridLength(0.4, GridUnitType.Star);
                LineWidth = this.Width * 0.4;

            }
            else if (i % 30 != 0) // if it's divideable by 10, but not 30
            {
                recLine.Height = _height * 0.35;
                colLabel.Width = new GridLength(0.7, GridUnitType.Star);
                colLine.Width = new GridLength(0.3, GridUnitType.Star);
                LineWidth = this.Width * 0.3;

            }
            else if (i % 60 != 0) // if it's divideable by 30, but not 60
            {
                recLine.Height = _height * 0.7;
                colLabel.Width = new GridLength(0.4, GridUnitType.Star);
                colLine.Width = new GridLength(0.6, GridUnitType.Star);
                LineWidth = this.Width * 0.6;

            }
            else  // if it's divideable by 60
            {
                recLine.Height = _height;
                colLabel.Width = new GridLength(0.25, GridUnitType.Star);
                colLine.Width = new GridLength(0.75, GridUnitType.Star);
                LineWidth = this.Width * 0.75;

            }

        }
    }
}
