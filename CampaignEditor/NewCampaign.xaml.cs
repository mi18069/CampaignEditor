using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.BrandDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.CmpBrndDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor
{
    public partial class NewCampaign : Window
    {

        private CampaignController _campaignController;
        private ClientController _clientController;
        private BrandController _brandController;
        private CmpBrndController _cmpBrndController;

        private ClientDTO _client;
        BrandDTO[] selectedBrands = new BrandDTO[2];

        private ObservableCollection<BrandDTO> _brands = new ObservableCollection<BrandDTO>();
        public ObservableCollection<BrandDTO> Brands
        {
            get { return _brands; }
            set { _brands = value; }
        }
        // In order to know which textBox to update
        int tbToEditIndex = 0;

        public bool success = false;

        public bool canClientBeDeleted = false;
        public NewCampaign(ICampaignRepository campaignRepository, IClientRepository clientRepository,
                           IBrandRepository brandRepository, ICmpBrndRepository cmpBrndRepository)
        {
            this.DataContext = this;
            InitializeComponent();
            _campaignController = new CampaignController(campaignRepository);
            _clientController = new ClientController(clientRepository);
            _brandController = new BrandController(brandRepository);
            _cmpBrndController = new CmpBrndController(cmpBrndRepository);
        }

        // For binding client to campaign
        public async Task Initialize(string clientname)
        {
            _client = await _clientController.GetClientByName(clientname);
            canClientBeDeleted = (await _campaignController.GetCampaignsByClientId(_client.clid)).Count() == 0;
            FillFields();
            await FillLBBrand();
        }

        private async Task FillLBBrand()
        {
            var brands = (await _brandController.GetAllBrands()).OrderBy(b => b.brand);
            Brands = new ObservableCollection<BrandDTO>(brands);

        }
        private void FillFields()
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
                    int cmpid = await CreateCampaign();
                    foreach (var selectedBrand in selectedBrands)
                    {
                        if (selectedBrand != null)
                        {
                            await CreateCmpBrnd(cmpid, selectedBrand);
                        }                    
                    }
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

        private async Task<int> CreateCampaign()
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
            int activity = 0;
            DatePicker dpNow = new DatePicker();
            var dateTimeNow = DateTime.Now;
            dpNow.SelectedDate = dateTimeNow;
            int cmpaddedon = int.Parse(TimeFormat.DPToYMDString(dpNow));
            int cmpaddedat = int.Parse(TimeFormat.DTToTimeString(dateTimeNow)); 
            bool active = true;
            bool forcec = false;


            var campaign = await _campaignController.CreateCampaign(new CreateCampaignDTO(cmprev, cmpown, cmpname, clid, cmpsdate, cmpedate,
                cmpstime, cmpetime, cmpstatus, sostring, activity, cmpaddedon, cmpaddedat, active, forcec));
            
            return campaign.cmpid;
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
            else if (dpStartDate.SelectedDate > dpEndDate.SelectedDate)
            {
                MessageBox.Show("Start date must be prior the end date", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
}
