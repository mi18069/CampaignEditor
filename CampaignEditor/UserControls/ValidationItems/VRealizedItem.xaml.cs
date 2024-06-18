using CampaignEditor.Controllers;
using Database.Entities;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace CampaignEditor.UserControls.ValidationItems
{
    /// <summary>
    /// Interaction logic for VRealizedItem.xaml
    /// </summary>
    public partial class VRealizedItem : UserControl
    {
        private MediaPlanRealized? _mediaPlanRealized;
        public MediaPlanRealizedController _mpRController;
        public VRealizedItem()
        {
            InitializeComponent();
        }

        public void Initialize(MediaPlanRealized mpRealized = null)
        {
            if (mpRealized == null)
            {
                MakeItemEmpty();
                return;
            }

            _mediaPlanRealized = mpRealized;

            tbSpotName.Text = $"{mpRealized.spotname} (F: {mpRealized.durf}, E: {mpRealized.dure})";
            tbProgName.Text = mpRealized.name.Trim();
            lblTime.Content = "Time: " + TimeFormat.TimeStrToRepresentative(mpRealized.stimestr);

            lblAMR1.Content = "Amr%1: " + Math.Round(mpRealized.amrp1, 2).ToString();
            lblAMR2.Content = "Amr%2: " + Math.Round(mpRealized.amrp2, 2).ToString();
            lblAMR3.Content = "Amr%3: " + Math.Round(mpRealized.amrp3, 2).ToString();

            if (mpRealized.Cpp != null)
            {
                lblCpp.Content = "CPP: " + mpRealized.Cpp.ToString();
                lblAMRSale.Content = "Amr sale: " + mpRealized.amrpsale.ToString();
            }

            lblDPCoef.Content = "Dp coef: " + mpRealized.Dpcoef.ToString();
            lblProgCoef.Content = "Prog coef: " + mpRealized.Progcoef.ToString();
            lblSeccoef.Content = "Sec coef: " + mpRealized.Seccoef.ToString();
            lblSeascoef.Content = "Seas coef: " + mpRealized.Seascoef.ToString();

            lblPrice.Content = "Price: " + Math.Round(mpRealized.price.Value, 2).ToString();

            SetBackgroundColor(mpRealized.status);
        }

        private void MakeItemEmpty()
        {
            /*this.leftGrid.Visibility = System.Windows.Visibility.Hidden;
            this.rightGrid.Visibility = System.Windows.Visibility.Hidden;*/
        }

        private void SetBackgroundColor(int? status)
        {
            switch (status)
            {
                case null: this.Background = Brushes.Gray; break;
                case 1: this.Background = Brushes.LightGreen; break;
                case 2: this.Background = Brushes.OrangeRed; break;
                case 5: this.Background = Brushes.Gray; break;
            }
        }

        private async void btnOK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_mediaPlanRealized == null)
                return;

            await _mpRController.SetStatusValue(_mediaPlanRealized.id.Value, 1);
            SetBackgroundColor(1);
        }

        private async void btnNotOK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_mediaPlanRealized == null)
                return;

            await _mpRController.SetStatusValue(_mediaPlanRealized.id.Value, 2);
            SetBackgroundColor(2);

        }

        private async void btnOther_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_mediaPlanRealized == null)
                return;

            await _mpRController.SetStatusValue(_mediaPlanRealized.id.Value, 5);
            SetBackgroundColor(5);

        }
    }
}
