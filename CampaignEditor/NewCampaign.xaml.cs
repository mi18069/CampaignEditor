using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.BrandDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.CmpBrndDTO;
using Database.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Database.Entities;
using System.Reflection;
using Database.DTOs.ActivityDTO;

namespace CampaignEditor
{
    public partial class NewCampaign : Window
    {

        private CampaignController _campaignController;
        private ClientController _clientController;
        private BrandController _brandController;
        private CmpBrndController _cmpBrndController;
        private ActivityController _activityController;

        private ClientDTO _client;
        BrandDTO[] selectedBrands = new BrandDTO[2];

        private ObservableCollection<BrandDTO> _brands = new ObservableCollection<BrandDTO>();
        private List<ActivityDTO> _activities = new List<ActivityDTO>();
        public ObservableCollection<BrandDTO> Brands
        {
            get { return _brands; }
            set { _brands = value; }
        }
        // In order to know which textBox to update
        int tbToEditIndex = 0;

        public bool success = false;
        public CampaignDTO _campaign = null;
        public bool canClientBeDeleted = false;
        public NewCampaign(ICampaignRepository campaignRepository, IClientRepository clientRepository,
                           IBrandRepository brandRepository, ICmpBrndRepository cmpBrndRepository,
                           IActivityRepository activityRepository)
        {
            this.DataContext = this;
            InitializeComponent();
            _campaignController = new CampaignController(campaignRepository);
            _clientController = new ClientController(clientRepository);
            _brandController = new BrandController(brandRepository);
            _cmpBrndController = new CmpBrndController(cmpBrndRepository);
            _activityController = new ActivityController(activityRepository);
        }

        // For binding client to campaign
        public async Task Initialize(int clientId, CampaignDTO campaign = null)
        {
            _client = await _clientController.GetClientById(clientId);
            canClientBeDeleted = (await _campaignController.GetCampaignsByClientId(_client.clid)).Count() == 0;      
            FillFields(campaign);
            await FillLBBrand();
            await FillCBAcitivities();
            if (campaign != null)
            {
                await FillTBBrands(campaign);
                FillRbTv(campaign);
                SetActivity(campaign);
            }
        }

        private void FillRbTv(CampaignDTO campaign)
        {
            if (campaign.tv == true)
            {
                rbTv.IsChecked = true;
            }
            else
                rbRadio.IsChecked = true;
        }

        private void SetActivity(CampaignDTO campaign)
        {
            var index = campaign.activity;
            cbActivities.SelectedIndex = index;
        }

        private async Task FillLBBrand()
        {
            var brands = (await _brandController.GetAllBrands()).OrderBy(b => b.brand);
            Brands = new ObservableCollection<BrandDTO>(brands);
        }

        private async Task FillCBAcitivities()
        {
            var activities = (await _activityController.GetAllActivities()).OrderBy(act => act.actid);
            foreach (var activity in activities)
            {
                _activities.Add(activity);
            }
            cbActivities.ItemsSource = _activities;
        }

        private async Task FillTBBrands(CampaignDTO campaign)
        {
            var brands = new List<BrandDTO>();
            var cmpbrnds = await _cmpBrndController.GetCmpBrndsByCmpId(campaign.cmpid);

            if (cmpbrnds.Count() == 0)
                return;

            foreach (var cmpBrnd in cmpbrnds)
            {
                var brand = await _brandController.GetBrandById(cmpBrnd.brbrand);
                brands.Add(brand);
            }

            try
            {

                if (brands[0] == null)
                    tbBrand1.Text = "";
                else
                {
                    //tbBrand1.Text = brands[0].brand.Trim();
                    tbToEditIndex = 0;
                    for (int i=0; i<Brands.Count; i++)
                    {
                        var brand = Brands[i];
                        if (brand.brbrand == brands[0].brbrand)
                        {
                            SetBrandText(brand);
                            break;
                        }
                    }
                }
                if (brands[1] == null)
                    tbBrand2.Text = "";
                else
                {
                    //tbBrand2.Text = brands[1].brand.Trim();
                    tbToEditIndex = 1;
                    for (int i = 0; i < Brands.Count; i++)
                    {
                        var brand = Brands[i];
                        if (brand.brbrand == brands[1].brbrand)
                        {
                            SetBrandText(brand);
                            break;
                        }
                    }
                }

            }
            catch
            {
            }
        }
        private void FillFields(CampaignDTO campaign = null)
        {
            if (campaign == null)
            {
                tbName.Text = "";
                tbClientname.Text = _client.clname.ToString().Trim();
                dpStartDate.SelectedDate = DateTime.Now;
                dpEndDate.SelectedDate = DateTime.Now;
                tbTbStartHours.Text = "02";
                tbTbStartMinutes.Text = "00";
                tbTbEndHours.Text = "25";
                tbTbEndMinutes.Text = "59";
            }
            else
            {
                tbName.Text = campaign.cmpname.Trim() + "_duplicated";
                tbClientname.Text = _client.clname.ToString().Trim();
                dpStartDate.SelectedDate = TimeFormat.YMDStringToDateTime(campaign.cmpsdate);
                dpEndDate.SelectedDate = TimeFormat.YMDStringToDateTime(campaign.cmpedate);
                tbTbStartHours.Text = campaign.cmpstime.Substring(0, 2);
                tbTbStartMinutes.Text = campaign.cmpstime.Substring(3, 2);
                tbTbEndHours.Text = campaign.cmpetime.Substring(0, 2);
                tbTbEndMinutes.Text = campaign.cmpetime.Substring(3, 2);
            }
        }

        private void tbBrand1_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbToEditIndex = 0;
            selectedBrands[tbToEditIndex] = null;

            lbBrand.Items.Clear();

            if (tbBrand1.Text.Trim() != "")
            {
                Regex regex = new Regex(tbBrand1.Text.ToString(), RegexOptions.IgnoreCase);

                for (int i=0; i<Brands.Count(); i++)
                {
                    string brandname = Brands[i].brand;

                    if (regex.IsMatch(brandname))
                    {
                        if (Brands[i] != selectedBrands[1]) // Don't display selected brands
                            lbBrand.Items.Add(Brands[i]);
                    }
                }
            }

        }

        private void tbBrand2_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbToEditIndex = 1;
            selectedBrands[tbToEditIndex] = null;

            lbBrand.Items.Clear();

            if (tbBrand2.Text.Trim() != "")
            {
                Regex regex = new Regex(tbBrand2.Text.ToString(), RegexOptions.IgnoreCase);

                for (int i = 0; i < Brands.Count(); i++)
                {
                    string brandname = Brands[i].brand;

                    if (regex.IsMatch(brandname))
                    {
                        if (Brands[i] != selectedBrands[0]) // Don't display selected brands
                            lbBrand.Items.Add(Brands[i]);
                    }
                }
            }

        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string campaignName = tbName.Text.Trim();
            if (await CheckCampaign())
            {
                try
                {
                    var campaign = await CreateCampaign();
                    foreach (var selectedBrand in selectedBrands)
                    {
                        if (selectedBrand != null)
                        {
                            await CreateCmpBrnd(campaign.cmpid, selectedBrand);
                        }                    
                    }
                    _campaign = campaign;
                    success = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to create new campaign", "Result: ", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        private async Task CreateCmpBrnd(int cmpid, BrandDTO selectedBrand)
        {
            CmpBrndDTO cmpBrnd = new CmpBrndDTO(cmpid, selectedBrand.brbrand);
            await _cmpBrndController.CreateCmpBrnd(cmpBrnd);             
        }

        private async Task<CampaignDTO> CreateCampaign()
        {
            int cmprev = 0;
            int cmpown = MainWindow.user.usrid;
            string cmpname = tbName.Text.Trim();
            int clid = _client.clid;
            string cmpsdate = TimeFormat.DPToYMDString(dpStartDate);
            string cmpedate = TimeFormat.DPToYMDString(dpEndDate);
            string cmpstime = tbTbStartHours.Text.PadLeft(2, '0')+":"+tbTbStartMinutes.Text.PadLeft(2, '0')+":00";
            string cmpetime = tbTbEndHours.Text.PadLeft(2, '0')+":"+tbTbEndMinutes.Text.PadLeft(2, '0')+":59";
            int cmpstatus = 0;
            string sostring = "1;999;F;01234;012345";
            int activity = cbActivities.SelectedIndex;
            DatePicker dpNow = new DatePicker();
            var dateTimeNow = DateTime.Now;
            dpNow.SelectedDate = dateTimeNow;
            int cmpaddedon = int.Parse(TimeFormat.DPToYMDString(dpNow));
            int cmpaddedat = int.Parse(TimeFormat.DTToTimeString(dateTimeNow)); 
            bool active = true;
            bool forcec = false;
            bool tv = rbTv.IsChecked == true ? true : false;

            var campaign = await _campaignController.CreateCampaign(new CreateCampaignDTO(cmprev, cmpown, cmpname, clid, cmpsdate, cmpedate,
                cmpstime, cmpetime, cmpstatus, sostring, activity, cmpaddedon, cmpaddedat, active, forcec, tv));
            
            return campaign;
        }

        private async Task<bool> CheckCampaign()
        {
            if (tbName.Text.Trim() == "")
            {
                MessageBox.Show("Enter campaign name", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            if (await _campaignController.GetCampaignByName(tbName.Text.Trim()) != null)
            {
                MessageBox.Show("Already exists campaign with this name", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else if (!dpStartDate.SelectedDate.HasValue || !dpEndDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Select start and end date of campaign", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else if (dpStartDate.SelectedDate > dpEndDate.SelectedDate)
            {
                MessageBox.Show("Start date must be prior the end date", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else if ((int)(dpEndDate.SelectedDate - dpStartDate.SelectedDate).Value.TotalDays > 365)
            {
                MessageBox.Show("Campaign cannot be longer than a year", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            // Check this only if there is selected brand
            else if (selectedBrands[0] == null && tbBrand1.Text.Trim() != "")
            {
                MessageBox.Show("Invalid Brand 1", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else if (selectedBrands[1] == null && tbBrand2.Text.Trim() != "")
            {
                MessageBox.Show("Invalid Brand 2", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else
            {
                return true;
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void lbBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var brand = lbBrand.SelectedItem as BrandDTO;
            if (brand != null)
            {
                SetBrandText(brand);              
            }
        }

        private void SetBrandText(BrandDTO brand)
        {
            if (tbToEditIndex == 0)
            {
                tbBrand1.Text = brand.brand;
            }
            else if (tbToEditIndex == 1)
            {
                tbBrand2.Text = brand.brand;
            }
            selectedBrands[tbToEditIndex] = brand;

            // reset lbBrand
            lbBrand.Items.Clear();
        }


    }
}
