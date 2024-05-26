using System.Windows.Controls;

namespace CampaignEditor.UserControls.Shared
{
    /// <summary>
    /// Interaction logic for TbCoefItem.xaml
    /// </summary>
    public partial class TbCoefItem : UserControl
    {
        private decimal value = 1.0M;
        public bool isModified = false;
        public TbCoefItem()
        {
            InitializeComponent();
            tbCoef.Text = value.ToString("0.0###");
        }

        public void SetValue(decimal newValue)
        {
            value = newValue;
            tbCoef.Text = value.ToString("0.0###");
            isModified = true;
        }

        public decimal? GetValue()
        {
            return value;
        }
        private void InvalidValueEntered()
        {
            SetValue(value);
            border.BorderThickness = new System.Windows.Thickness(2);
        }

        private void tbCoef_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            border.BorderThickness = new System.Windows.Thickness(0);

        }

        private void tbCoef_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            decimal newValue;
            if (decimal.TryParse(tbCoef.Text, out newValue))
            {
                if (newValue != value)
                {
                    if (newValue < 100 && newValue >= 0)
                        SetValue(newValue);
                    else
                        InvalidValueEntered();
                } 
            }
            else
                InvalidValueEntered();
        }
    }
}
