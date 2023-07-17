﻿using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ClientDTO;
using Database.Repositories;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Database.DTOs.BrandDTO;
using System.Linq;
using System;
using Database.DTOs.CmpBrndDTO;
using System.Text.RegularExpressions;

namespace CampaignEditor
{
    public partial class CmpInfo : Window
    {
        private CampaignDTO _campaign = null;
        private BrandDTO _brand = null;
        private ClientDTO _client = null;

        private CampaignController _campaignController;
        private BrandController _brandController;
        private CmpBrndController _cmpBrndController;

        private ObservableCollection<BrandDTO> _brands = new ObservableCollection<BrandDTO>();
        public ObservableCollection<BrandDTO> Brands
        {
            get { return _brands; }
            set {_brands = value; }
        }

        public bool infoModified = false;
        private bool isModified = false;
        public bool shouldClose = false;
        public CampaignDTO Campaign {
            get { return _campaign; }
            set { _campaign = value; }
        }

        public BrandDTO Brand
        {
            get { return _brand; }
            set { _brand = value; }
        }

        private bool lbSelected = false;

        public CmpInfo(ICampaignRepository campaignRepository,
                       IBrandRepository brandRepository, ICmpBrndRepository cmpBrndRepository)
        {
            this.DataContext = this;
            _campaignController = new CampaignController(campaignRepository);
            _brandController = new BrandController(brandRepository);
            _cmpBrndController = new CmpBrndController(cmpBrndRepository);

            InitializeComponent();
        }

        public async Task Initialize(ClientDTO client, CampaignDTO campaign = null)
        {
            _campaign = campaign;
            _client = client;

            if (campaign != null)
            {
                cbActive.IsChecked = campaign.active;
                tbName.Text = campaign.cmpname.Trim();
                tbClientname.Text = client.clname.Trim();

                dpStartDate.SelectedDate = TimeFormat.YMDStringToDateTime(campaign.cmpsdate);
                dpEndDate.SelectedDate = TimeFormat.YMDStringToDateTime(campaign.cmpedate);

                FillTBTextBoxes();
                await FillTBBrand();
                await FillLBBrand();
            }
            isModified = false;
        }

        private async Task FillTBBrand()
        {
            try
            {
                var cmpbrnd = await _cmpBrndController.GetCmpBrndByCmpId(_campaign.cmpid);
                var brand = await _brandController.GetBrandById(cmpbrnd.brbrand);
                Brand = brand;
                tbBrand.Text = brand.brand.Trim();
            }
            catch
            {
                tbBrand.Text = "";
            }
        }

        private async Task FillLBBrand()
        {
            var brands = (await _brandController.GetAllBrands()).OrderBy(b => b.brand);
            Brands = new ObservableCollection<BrandDTO>(brands);
        }

        private void FillTBTextBoxes()
        {
            tbTbStartHours.Text = _campaign.cmpstime[0].ToString() + _campaign.cmpstime[1].ToString();
            tbTbStartMinutes.Text = _campaign.cmpstime[3].ToString() + _campaign.cmpstime[4].ToString();

            tbTbEndHours.Text = _campaign.cmpetime[0].ToString() + _campaign.cmpetime[1].ToString();
            tbTbEndMinutes.Text = _campaign.cmpetime[3].ToString() + _campaign.cmpetime[4].ToString();
        }

        private void TextBoxes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).MaxLength == ((TextBox)sender).Text.Length)
            {
                // move focus to the next
                var ue = e.OriginalSource as FrameworkElement;
                e.Handled = true;
                ue.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
            isModified = true;
        }

        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            isModified = true;
        }

        private void dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            isModified = true;
        }

        private void cb_Checked(object sender, RoutedEventArgs e)
        {
            isModified = true;
        }

        #region Save and Cancel buttons
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            infoModified = isModified;
            if (infoModified)
            {
                if (await CheckCampaign())
                {
                    try
                    {
                        await UpdateCampaign();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unabe to update campaign", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }

                }
                else
                {
                    return;
                }

            }

            this.Hide();
        }

        private async Task UpdateCampaign()
        {
            int cmpid = Campaign.cmpid;
            int cmprev = 0;
            int cmpown = 1; // Don't know what this is
            string cmpname = tbName.Text.Trim();
            int clid = _client.clid;
            string cmpsdate = TimeFormat.DPToYMDString(dpStartDate);
            string cmpedate = TimeFormat.DPToYMDString(dpEndDate);
            string cmpstime = tbTbStartHours.Text.PadLeft(2, '0') + ":" + tbTbStartMinutes.Text.PadLeft(2, '0') + ":00";
            string cmpetime = tbTbEndHours.Text.PadLeft(2, '0') + ":" + tbTbEndMinutes.Text.PadLeft(2, '0') + ":59";
            int cmpstatus = 0;
            string sostring = "1;999;F;01234;012345";
            int activity = 0;
            int cmpaddedon = _campaign.cmpaddedon;
            int cmpaddedat = _campaign.cmpaddedat;
            bool active = (bool)cbActive.IsChecked;
            bool forcec = _campaign.forcec;

            Campaign = new CampaignDTO(cmpid, cmprev, cmpown, cmpname, clid, cmpsdate, cmpedate,
                cmpstime, cmpetime, cmpstatus, sostring, activity, cmpaddedon, cmpaddedat,
                active, forcec);

        }

        private async Task UpdateCmpBrnd()
        {
            if (Brand != null)
            {
                CmpBrndDTO cmpBrnd = new CmpBrndDTO(_campaign.cmpid, Brand.brbrand);
                try
                {
                    await _cmpBrndController.GetCmpBrndByCmpId(_campaign.cmpid);
                    await _cmpBrndController.UpdateBrand(cmpBrnd);
                }
                catch
                {
                    await _cmpBrndController.CreateCmpBrnd(cmpBrnd);
                }
            }
        }

        private async Task<bool> CheckCampaign()
        {
            if (tbName.Text.Trim() == "")
            {
                MessageBox.Show("Enter campaign name", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else if (dpStartDate.SelectedDate > dpEndDate.SelectedDate)
            {
                MessageBox.Show("Start date must be prior the end date", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            // Check this only if there is selected brand
            else if (Brand == null && tbBrand.Text.Trim().Length != 0)
            {
                MessageBox.Show("Invalid brand", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else
            {
                return true;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        #endregion

        public async Task UpdateDatabase(CampaignDTO campaign)
        {
            await _campaignController.UpdateCampaign(new UpdateCampaignDTO(campaign.cmpid,
                campaign.cmprev, campaign.cmpown, campaign.cmpname, campaign.clid, 
                campaign.cmpsdate, campaign.cmpedate, campaign.cmpstime, campaign.cmpetime,
                campaign.cmpstatus, campaign.sostring, campaign.activity, campaign.cmpaddedon,
                campaign.cmpaddedat, campaign.active, campaign.forcec));
            await UpdateCmpBrnd();
        }

        private void tbBrand_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lbSelected)
            {
                lbSelected = false;
                return;
            }

            lbBrand.Items.Clear();

            if (tbBrand.Text.Trim() != "")
            {
                Regex regex = new Regex(tbBrand.Text.ToString(), RegexOptions.IgnoreCase);

                for (int i = 0; i < Brands.Count(); i++)
                {
                    string brandname = Brands[i].brand;

                    if (regex.IsMatch(brandname))
                    {
                        lbBrand.Items.Add(Brands[i]);
                    }
                }
            }

        }

        private void lbBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Brand = null;

            var brand = lbBrand.SelectedItem as BrandDTO;
            if (brand != null)
            {
                lbSelected = true;
                tbBrand.Text = brand.brand;
                Brand = brand;
            }
            isModified = true;
        }

        // Overriding OnClosing because click on x button should only hide window
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!shouldClose)
            {
                e.Cancel = true;
                Hide();
            }

        }

    }
}
