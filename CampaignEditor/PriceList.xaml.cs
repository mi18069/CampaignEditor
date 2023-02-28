using CampaignEditor.Controllers;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ChannelDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.PricelistChannels;
using Database.DTOs.PricelistDTO;
using Database.DTOs.SeasonalityDTO;
using Database.DTOs.SectableDTO;
using Database.DTOs.TargetDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CampaignEditor
{
    public partial class PriceList : Window
    {
        private ChannelController _channelController;
        private PricelistChannelsController _pricelistChannelsController;
        private PricelistController _pricelistController;
        private SectableController _sectableController;
        private SeasonalityController _seasonalityController;
        private TargetController _targetController;


        private readonly IAbstractFactory<Sectable> _factorySectable;
        private readonly IAbstractFactory<Seasonality> _factorySeasonality;
        private ClientDTO client;

        // For Plus Icon
        private string appPath = Directory.GetCurrentDirectory();
        private string imgGreenPlusPath = "\\images\\GreenPlus.png";
        public PriceList(IAbstractFactory<Sectable> factorySectable, IAbstractFactory<Seasonality> factorySeasonality,
            IChannelRepository channelRepository,
            IPricelistRepository pricelistRepository, IPricelistChannelsRepository pricelistChannelsRepository,
            ISectableRepository sectableRepository, ISeasonalityRepository seasonalityRepository,
            ITargetRepository targetRepository)
        {
            _factorySectable = factorySectable;
            _factorySeasonality = factorySeasonality;

            _sectableController = new SectableController(sectableRepository);
            _seasonalityController = new SeasonalityController(seasonalityRepository);
            _targetController = new TargetController(targetRepository);
            _channelController = new ChannelController(channelRepository);
            _pricelistChannelsController = new PricelistChannelsController(pricelistChannelsRepository);
            _pricelistController = new PricelistController(pricelistRepository);

            

            InitializeComponent();

        }

        public void Initialize(ClientDTO client)
        {
            this.client = client;
            FillFields();
        }

        private void FillFields()
        {
            FillChannels();
            UpdateDayParts();
            FillComboBoxes();
        }

        private async void FillComboBoxes()
        {
            List<string> typeList = new List<string> { "CPP", "Seconds", "Package" };
            List<SectableDTO> sectables = (List<SectableDTO>) await _sectableController.GetAllSectablesByOwnerId(client.clid);
            List<SeasonalityDTO> seasonalities = (List<SeasonalityDTO>) await _seasonalityController.GetAllSeasonalitiesByOwnerId(client.clid);
            List<TargetDTO> targets = (List<TargetDTO>)await _targetController.GetAllClientTargets(client.clid);

            foreach (string type in typeList)
            {
                cbType.Items.Add(type);
            }

            foreach (var sectable in sectables)
            {
                cbSectable.Items.Add(sectable);
                cbSectable2.Items.Add(sectable);
            }

            foreach (var seasonality in seasonalities)
            {
                cbSeasonality.Items.Add(seasonality);
            }

            foreach (var target in targets)
            {
                cbTarget.Items.Add(target);
            }
        }

        // Adding Add Button and New TargetDPItem
        private void UpdateDayParts()
        {
            Button btnAddDP = new Button();
            btnAddDP.Click += new RoutedEventHandler(btnAddDP_Click);
            Image imgGreenPlus = new Image();
            imgGreenPlus.Source = new BitmapImage(new Uri(appPath + imgGreenPlusPath));
            btnAddDP.Content = imgGreenPlus;
            btnAddDP.Width = 30;
            btnAddDP.Height = 30;
            btnAddDP.Background = Brushes.White;
            btnAddDP.BorderThickness = new Thickness(0);
            btnAddDP.HorizontalAlignment = HorizontalAlignment.Center;

            TargetDPItem dpItem = new TargetDPItem();
            dpItem.Width = wpDayParts.Width;
            wpDayParts.Children.Add(dpItem);

            wpDayParts.Children.Add(btnAddDP);
        }

        private void btnAddDP_Click(object sender, RoutedEventArgs e)
        {
            var btnAdd = sender as Button;
            wpDayParts.Children.Remove(btnAdd);

            UpdateDayParts();
        }

        private async void FillChannels()
        {
            var channels = await _channelController.GetAllChannels();

            channels = channels.OrderBy(c => c.chname);

            foreach (var channel in channels)
            {
                CheckBox cb = new CheckBox();
                cb.Content = channel.chname;
                cb.Tag = channel;
                wpChannels.Children.Add(cb);
            }
        }

        // Selecting whole text when captured
        private void tbSec2_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var tb = sender as TextBox;
            
            tb.SelectAll();
            tb.Focus();
        }

        private void btnNewSectable_Click(object sender, RoutedEventArgs e)
        {
            _factorySectable.Create().Show();
        }

        private void btnNewSeasonality_Click(object sender, RoutedEventArgs e)
        {
            _factorySeasonality.Create().Show();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (await CheckValues())
            {
                //MakeNewPricelist();
                //MakeNewPricelistChannels();
            }
        }

        #region Writing into base
        // for writing data in Database tblpricelist and tblpricelistchn
        private async Task MakeNewPricelist()
        {
            int clid = client.clid;
            string plname = tbName.Text.Trim();
            int pltype = cbType.SelectedIndex; // Place in combobox corresponds to int value
            int chid = 1;
            int sectbid = (cbSectable.SelectedValue as SectableDTO)!.sctid; // By default, first value is selected
            int seasid = (cbSeasonality.SelectedValue as SeasonalityDTO)!.seasid;
            bool plactive = true;
            float price = 0.0f;
            float minprice = 0.0f;
            bool prgcoef = false;
            int pltarg = (cbTarget.SelectedValue as TargetDTO)!.targid;
            bool use2 = (bool)chbSectable2.IsChecked;
            int sectbid2 = (cbSectable2.SelectedValue as SectableDTO)!.sctid;
            int sectb2st = int.Parse(tbSec2From.Text.Trim());
            int sectb2en = int.Parse(tbSec2To.Text.Trim());
            int valfrom = 0;
            int valto = 0;
            bool mgtype = false;

            await _pricelistController.CreatePricelist(new CreatePricelistDTO
                (clid, plname, pltype, sectbid, seasid, plactive, price, minprice,
                prgcoef, pltarg, use2, sectbid2, sectb2st, sectb2en,
                valfrom, valto, mgtype));
        }

        private async Task MakeNewPricelistChannels()
        {
            List<ChannelDTO> channels = new List<ChannelDTO>();
            foreach (CheckBox channelBox in wpChannels.Children)
            {
                if ((bool)channelBox.IsChecked)
                {
                    channels.Add(await _channelController.GetChannelByName(channelBox.Content.ToString().Trim()));
                }
            }

            PricelistDTO pricelist = await _pricelistController.GetClientPricelistByName(client.clid, tbName.Text.Trim());

            foreach (var channel in channels)
            {
                await _pricelistChannelsController.CreatePricelistChannels(
                    new CreatePricelistChannelsDTO(pricelist.plid, channel.chid));
            }
        }
        #endregion

        #region Check Values
        private async Task<bool> CheckValues()
        {
            
            if (await CheckName() && CheckCP() && CheckMinGRP() &&
                CheckValidity() && CheckDPs() && CheckComboBoxes())
            {
                if ((bool)chbSectable2.IsChecked)
                {
                    return CheckTbSectable2();
                }
                else
                    return true;
            }
            else
                return false;
        }

        // Values in tbSectable2 needs to be 4 chars long integers
        private bool CheckTbSectable2()
        {
            int from;
            int to;

            string fromStr = tbSec2From.Text.Trim();
            string toStr = tbSec2To.Text.Trim();

            if (fromStr.Count() != 4 || toStr.Count() != 4)
            {
                MessageBox.Show("Values in Sectable2 needs to be 4 characters long");
                return false;
            }
            else if (!int.TryParse(fromStr, out from) ||
               !int.TryParse(toStr, out to))
            {
                MessageBox.Show("Invalid values for Sectable2");
                return false;
            }
            else
                return true;
        }
        // Name should be longer than 0 chars and shouldn't 
        // have the same name as another pricelist from the same client
        private async Task<bool> CheckName()
        {
            string name = tbName.Text.Trim();
            if (name.Length <= 0)
            {
                MessageBox.Show("Enter name");
                return false;
            }
            else if ((await _pricelistController.GetClientPricelistByName(client.clid, name)) != null)
            {
                MessageBox.Show("Name already exist");
                return false;
            }
            else
                return true;
        }
        private bool CheckCP()
        {
            string cp = tbCP.Text.Trim();
            double cpDouble = 0;
            if (cp.Length <= 0)
            {
                MessageBox.Show("Enter CP(/I/P/S)");
                return false;
            }else if(!double.TryParse(cp, out cpDouble))
            {
                MessageBox.Show("Value for CP(/I/P/S) is not valid");
                return false;
            }
            else
                return true;
        }
        private bool CheckMinGRP()
        {
            string grp = tbMinGRP.Text.Trim();
            double grpDouble = 0;
            if (grp.Length <= 0)
            {
                MessageBox.Show("Enter MinGRP");
                return false;
            }
            else if (!double.TryParse(grp, out grpDouble))
            {
                MessageBox.Show("Value for MinGRP is not valid");
                return false;
            }
            else
                return true;
        }
        private bool CheckValidity()
        {
            DateTime? fromDate = dpValidityFrom.SelectedDate;
            DateTime? toDate = dpValidityTo.SelectedDate;
            if (!fromDate.HasValue || !toDate.HasValue)
            {
                MessageBox.Show("Select Date Range");
                return false;
            }
            else if (fromDate > toDate)
            {
                MessageBox.Show("Selected Date Range is invalid");
                return false;
            }
            else
                return true;
        }
        // Each TargetDPItem has it's CheckValidity function that returns 
        // error string if something's not ok
        private bool CheckDPs()
        {
            bool success = true;

            for (int i=0; i<wpDayParts.Children.Count -1; i++)
            {
                TargetDPItem item = (TargetDPItem)wpDayParts.Children[i];
                string validity = "";
                if ((validity = item.CheckValidity()) != "")
                {
                    MessageBox.Show(validity);
                    success = false;
                    break;
                }
            }
            return success;
        }
        // Every CheckBox needs to be selected (except cbSectable2)
        private bool CheckComboBoxes()
        {
            if (cbSectable.SelectedIndex == -1)
            {
                MessageBox.Show("Select Sectable");
                return false;
            }
            else if (cbSeasonality.SelectedIndex == -1)
            {
                MessageBox.Show("Select Seasonality");
                return false;
            }
            else if ((bool)chbSectable2.IsChecked && cbSectable2.SelectedIndex == -1)
            {
                MessageBox.Show("Select Sectable2");
                return false;
            }
            else if (cbTarget.SelectedIndex == -1)
            {
                MessageBox.Show("Select Target");
                return false;
            }
            return true;
        }
        #endregion
    }
}
