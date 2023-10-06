using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Only one line on the Ruler, it can have 5 different values: ones, divided by 5, 10, 30 and 60
    /// </summary>
    public partial class RulerLine : UserControl
    {
        public RulerLine(int timeInMins, int height)
        {
            InitializeComponent();
            this.Height = height;
            Initialize(timeInMins);
        }

        private void Initialize(int i)
        {
            if (i % 5 != 0) // Most occuring case, when i % 5 != 0
            {
                recLine.Height = 1;               
                colLabel.Width = new GridLength(0.8, GridUnitType.Star);
                colLine.Width = new GridLength(0.2, GridUnitType.Star);
                lblLineValue.Content = "";
            }
            else if (i % 10 != 0) // if it's divideable by 5, but not 10
            {
                recLine.Height = 2;
                colLabel.Width = new GridLength(0.6, GridUnitType.Star);
                colLine.Width = new GridLength(0.4, GridUnitType.Star);
                lblLineValue.Content = TimeFormat.MinToRepresentative(i);
            }
            else if (i % 30 != 0) // if it's divideable by 10, but not 30
            {
                recLine.Height = 3;
                colLabel.Width = new GridLength(0.4, GridUnitType.Star);
                colLine.Width = new GridLength(0.6, GridUnitType.Star);
                lblLineValue.Content = TimeFormat.MinToRepresentative(i);
            }
            else if (i % 60 != 0) // if it's divideable by 30, but not 60
            {
                recLine.Height = 4;
                colLabel.Width = new GridLength(0.3, GridUnitType.Star);
                colLine.Width = new GridLength(0.7, GridUnitType.Star);
                lblLineValue.Content = TimeFormat.MinToRepresentative(i);
            }
            else  // if it's divideable by 60
            {
                recLine.Height = 5;
                colLabel.Width = new GridLength(0.3, GridUnitType.Star);
                colLine.Width = new GridLength(0.7, GridUnitType.Star);
                lblLineValue.Content = TimeFormat.MinToRepresentative(i);
            }
        }
    }
}
