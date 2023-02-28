using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace CampaignEditor
{
    public partial class TargetDPItem : UserControl
    {
        public TargetDPItem()
        {
            InitializeComponent();
        }

        // For checking validity of time order
        public string CheckValidity()
        {
            string retString = "";
            retString = CheckFields();
            return retString;
        }

        // Checking text and time 
        private string CheckFields()
        {
            int fromH;
            int fromM;
            int toH;
            int toM;
            double coef;

            if (!int.TryParse(tbFromH.Text, out fromH) ||
                !int.TryParse(tbFromM.Text, out fromM) ||
                !int.TryParse(tbToH.Text, out toH) ||
                !int.TryParse(tbToM.Text, out toM))
                return "Invalid values for Day Parts";
            else if (fromH > toH || (fromH == toH && fromM > toM))
                return "Invalid values for Day Parts";
            else if (!double.TryParse(tbCoef.Text, out coef))
                return "Invalid value for DP Coef";
            else 
                return "";
        }

        // Selecting whole text
        private void tb_GotMouseCapture(object sender, MouseEventArgs e)
        {
            var tb = sender as TextBox;

            if (tb.BorderBrush == Brushes.Red)
                tb.BorderBrush = Brushes.Gray;

            tb.SelectAll();
            tb.Focus();
        }

        // Deleting this instance
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ((Panel)this.Parent).Children.Remove(this);
        }

        // Checking format and range of available values
        private void tbH_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            var content = tb.Text.Trim();
            int myInt = -1;

            if (content.Length > 0)
            {
                myInt = int.TryParse(content, out myInt) ? myInt : -1;
            }
            if (content.Length == 1)
            {
                tb.Text = "0" + content;
            }
            if (myInt < 0) 
            {
                tb.BorderBrush = Brushes.Red;
                tb.Text = "";
            }
        }

        private void tbM_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            var content = tb.Text.Trim();
            int myInt = -1;

            if (content.Length > 0)
            {
                myInt = int.TryParse(content, out myInt) ? myInt : -1;
            }
            if (content.Length == 1)
            {
                tb.Text = "0" + content;
            }
            if (myInt < 0 || myInt > 59)
            {
                tb.BorderBrush = Brushes.Red;
                tb.Text = "";
            }
        }

        private void tbCoef_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            var content = tb.Text.Trim();
            double myDouble = -1;

            if (content.Length > 0)
            {
                myDouble = double.TryParse(content, out myDouble) ? myDouble : -1;
            }
            if (myDouble < 0)
            {
                tb.BorderBrush = Brushes.Red;
                tb.Text = "";
            }
        }
    }
}
