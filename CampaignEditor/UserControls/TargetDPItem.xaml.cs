using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


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

            int days;

            if(tbCoef.Text.Trim() == "")
            {
                tbCoef.Text = (1.00).ToString();
            }

            if (tbFromH.Text.Trim() == "" &&
                tbFromM.Text.Trim() == "" &&
                tbToH.Text.Trim() == "" &&
                tbToM.Text.Trim() == "" &&
                tbDays.Text.Trim() == "")
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
            else if (!int.TryParse(tbDays.Text.Trim(), out days))
                return "Invalid value for DP Days";
            else if (!CheckDaysIntFormat(tbDays.Text.Trim()))
                return "Invalid value for DP Days";
            else
                return "";
        }

        private bool CheckDaysIntFormat(string number)
        {
            int[] numCount = { 0, 1, 1, 1, 1, 1, 1, 1, 0, 0 }; // 0 for 0,8,9 and 1 for 1-7
            foreach (char n in number)
            {
                int num = int.Parse(n.ToString());
                numCount[num]--;
            }

            foreach (int num in numCount)
            {
                if (num < 0)
                    return false;
            }

            return true;
        }

        // Selecting whole text
        private void tb_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
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

        private void tbNum_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true; // Suppress non-numeric input
            }
        }

        private void tbNum1To7_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) || e.Text == "0" || e.Text == "8" || e.Text == "9")
            {
                e.Handled = true; // Suppress non-numeric input
            }

        }

    }
}
