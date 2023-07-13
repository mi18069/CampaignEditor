using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
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

namespace CampaignEditor
{
    public partial class NewCampaign : Window
    {

        private CampaignController _campaignController;
        private ClientController _clientController;
        private BrandController _brandController;
        private CmpBrndController _cmpBrndController;

        private ClientDTO _client;
        BrandDTO selectedBrand = null;

        private ObservableCollection<BrandDTO> _brands = new ObservableCollection<BrandDTO>();
        public ObservableCollection<BrandDTO> Brands
        {
            get { return _brands; }
            set { _brands = value; }
        }

        private bool lbSelected = false;

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

                for (int i=0; i<Brands.Count(); i++)
                {
                    string brandname = Brands[i].brand;

                    if (regex.IsMatch(brandname))
                    {
                        lbBrand.Items.Add(Brands[i]);
                    }
                }
            }

        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            lblError.Content = "";
            string campaignName = tbName.Text.Trim();
            if (await CheckCampaign())
            {
                try
                {
                    int cmpid = await CreateCampaign();
                    await CreateCmpBrnd(cmpid);
                    success = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    lblError.Content = "Unable to make new campaign";
                }

            }
        }

        private async Task CreateCmpBrnd(int cmpid)
        {
            if (lbBrand.SelectedItems.Count == 1)
            {
                BrandDTO brand = lbBrand.SelectedItem as BrandDTO;
                if (brand != null)
                {
                    CmpBrndDTO cmpBrnd = new CmpBrndDTO(cmpid, brand.brbrand);
                    await _cmpBrndController.CreateCmpBrnd(cmpBrnd);
                }
            }
        }

        private async Task<int> CreateCampaign()
        {
            int cmprev = 0;
            int cmpown = 1; // Don't know what this is
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
            else if (lbBrand.SelectedItems.Count != 1)
            {
                MessageBox.Show("Select one brand", "Result: ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
            selectedBrand = null;

            var brand = lbBrand.SelectedItem as BrandDTO;
            if (brand != null)
            {
                lbSelected = true;
                tbBrand.Text = brand.brand;
                selectedBrand = brand;
            }
        }
    }
}
