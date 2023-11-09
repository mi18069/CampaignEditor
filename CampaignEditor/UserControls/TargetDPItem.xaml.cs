using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;


namespace CampaignEditor
{
    public partial class TargetDPItem : UserControl
    {
        public bool modified = false;
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

            if(tbCoef.Text.Trim() == "")
            {
                tbCoef.Text = (1.00).ToString();
            }

            if (tbFromH.Text.Trim() == "" &&
                tbFromM.Text.Trim() == "" &&
                tbToH.Text.Trim() == "" &&
                tbToM.Text.Trim() == "")
                return "empty";
            else if (!int.TryParse(tbFromH.Text.Trim(), out fromH) ||
                !int.TryParse(tbFromM.Text.Trim(), out fromM) ||
                !int.TryParse(tbToH.Text.Trim(), out toH) ||
                !int.TryParse(tbToM.Text.Trim(), out toM))
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

            tb.SelectAll();
            tb.Focus();
        }

        // Deleting this instance
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ((Panel)this.Parent).Children.Remove(this);
        }     

        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            modified = true;
        }

        private void cb_Checked(object sender, RoutedEventArgs e)
        {
            modified = true;
        }

        private void cb_Unchecked(object sender, RoutedEventArgs e)
        {
            modified = true;
        }
    }
}
