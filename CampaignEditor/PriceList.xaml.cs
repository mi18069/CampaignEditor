﻿using CampaignEditor.Controllers;
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
        private readonly IAbstractFactory<NewTarget> _factoryNewTarget;

        private ClientDTO client;

        // For Plus Icon
        private string appPath = Directory.GetCurrentDirectory();
        private string imgGreenPlusPath = "\\images\\GreenPlus.png";
        public PriceList(IAbstractFactory<Sectable> factorySectable, IAbstractFactory<Seasonality> factorySeasonality,
            IAbstractFactory<NewTarget> factoryNewTarget,
            IChannelRepository channelRepository,
            IPricelistRepository pricelistRepository, IPricelistChannelsRepository pricelistChannelsRepository,
            ISectableRepository sectableRepository, ISeasonalityRepository seasonalityRepository,
            ITargetRepository targetRepository)
        {
            _factorySectable = factorySectable;
            _factorySeasonality = factorySeasonality;
            _factoryNewTarget = factoryNewTarget;

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

        #region Fill Fields

        #region DP functionality
        private void btnAddDP_Click(object sender, RoutedEventArgs e)
        {
            var btnAdd = sender as Button;
            wpDayParts.Children.Remove(btnAdd);

            UpdateDayParts();
        }

        // Selecting whole text when captured
        private void tbSec2_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var tb = sender as TextBox;

            tb.SelectAll();
            tb.Focus();
        }
        #endregion

        private void FillFields()
        {
            FillChannels();
            UpdateDayParts();
            FillComboBoxes();
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

        private async void FillComboBoxes()
        {
            FillCBType();
            await FillCBSectable();
            await FillCBSeasonality();
            await FillCBTarget();
        }

        private void FillCBType()
        {
            cbType.Items.Clear();

            List<string> typeList = new List<string> { "CPP", "Seconds", "Package" };
            foreach (string type in typeList)
            {
                cbType.Items.Add(type);
            }
        }
        private async Task FillCBSectable(int index = 0)
        {
            cbSectable.Items.Clear();
            cbSectable2.Items.Clear();

            List<SectableDTO> sectables = (List<SectableDTO>)await _sectableController.GetAllSectablesByOwnerId(client.clid);
            foreach (var sectable in sectables)
            {
                cbSectable.Items.Add(sectable);
                cbSectable2.Items.Add(sectable);
            }
            cbSectable.SelectedIndex = index;
            cbSectable2.SelectedIndex = index;
        }
        private async Task FillCBSeasonality(int index = 0)
        {
            cbSeasonality.Items.Clear();

            List<SeasonalityDTO> seasonalities = (List<SeasonalityDTO>)await _seasonalityController.GetAllSeasonalitiesByOwnerId(client.clid);
            foreach (var seasonality in seasonalities)
            {
                cbSeasonality.Items.Add(seasonality);
            }

            cbSeasonality.SelectedIndex = index;
        }
        private async Task FillCBTarget(int index = 0)
        {
            cbTarget.Items.Clear();

            List<TargetDTO> targets = (List<TargetDTO>)await _targetController.GetAllClientTargets(client.clid);

            foreach (var target in targets)
            {
                cbTarget.Items.Add(target);
            }

            cbTarget.SelectedIndex = index;
        }

        #endregion

        #region Writing into base
        // for writing data in Database tblpricelist and tblpricelistchn
        private async Task MakeNewPricelist()
        {
            int clid = client.clid;
            string plname = tbName.Text.Trim();
            int pltype = cbType.SelectedIndex; // Place in combobox corresponds to int value
            int chid = 0; // Don't know what this field does
            int sectbid = (cbSectable.SelectedValue as SectableDTO)!.sctid; // By default, first value is selected
            int seasid = (cbSeasonality.SelectedValue as SeasonalityDTO)!.seasid;
            bool plactive = (bool)chbActive.IsChecked;
            float price = float.Parse(tbCP.Text.Trim());
            float minprice = float.Parse(tbMinGRP.Text.Trim());
            bool prgcoef = false;
            int pltarg = (cbTarget.SelectedValue as TargetDTO)!.targid;
            bool use2 = (bool)chbSectable2.IsChecked;
            int sectbid2 = (cbSectable2.SelectedValue as SectableDTO)!.sctid;
            int sectb2st = int.Parse((tbSec2From.Text.Trim()+"00").PadLeft(6, '0'));
            int sectb2en = int.Parse((tbSec2To.Text.Trim()+"59").PadLeft(6,'0'));
            int valfrom = int.Parse(TimeFormat.DPToYMDString(dpValidityFrom));
            int valto = int.Parse(TimeFormat.DPToYMDString(dpValidityTo));
            bool mgtype = (bool)chbGRP.IsChecked;

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
                CheckValidity() && CheckComboBoxes() && CheckDPs())
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

        #region Edit and New Buttons

        #region Edit buttons mechanism
        private void cbSectable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSectable.SelectedIndex <= 0)
                btnEditSectable.Visibility = Visibility.Hidden;
            else
                btnEditSectable.Visibility = Visibility.Visible;
        }

        private void cbSeasonality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSeasonality.SelectedIndex <= 0)
                btnEditSeasonality.Visibility = Visibility.Hidden;
            else
                btnEditSeasonality.Visibility = Visibility.Visible;
        }

        private void cbTarget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTarget.SelectedIndex <= 0)
                btnEditTarget.Visibility = Visibility.Hidden;
            else
                btnEditTarget.Visibility = Visibility.Visible;
        }
        #endregion

        private async void btnNewTarget_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factoryNewTarget.Create();
            await factory.InitializeTree();
            factory.ShowDialog();
            if (factory.success)
                await FillCBTarget();
        }

        private async void btnEditTarget_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factoryNewTarget.Create();
            var target = cbTarget.SelectedItem as TargetDTO;
            int index = cbTarget.SelectedIndex;

            if (target != null)
            {
                var success = await factory.InitializeTargetToEdit(target);
            }

            factory.ShowDialog();

            if (factory.success)
                await FillCBTarget(index);
        }
        private async void btnNewSectable_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factorySectable.Create();
            int index = cbSectable.Items.Count;

            factory.Initialize(client);
            factory.ShowDialog();
            if (factory.success)
                await FillCBSectable(index);

        }
        private async void btnEditSectable_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factorySectable.Create();
            SectableDTO sectable = (cbSectable.SelectedItem as SectableDTO)!;
            int index = cbSectable.SelectedIndex;

            factory.Initialize(client, sectable);
            factory.ShowDialog();
            if (factory.success)
                await FillCBSectable(index);
        }

        private async void btnNewSeasonality_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factorySeasonality.Create();
            int index = cbSeasonality.Items.Count;

            factory.Initialize(client);
            factory.ShowDialog();
            if (factory.success)
                await FillCBSeasonality(index);
        }
        private async void btnEditSeasonality_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factorySeasonality.Create();
            SeasonalityDTO seasonality = (cbSeasonality.SelectedItem as SeasonalityDTO)!;
            int index = cbSeasonality.SelectedIndex;

            factory.Initialize(client, seasonality);
            factory.ShowDialog();
            if (factory.success)
                await FillCBSeasonality(index);
        }
        #endregion

        #region Save and Cancel Buttons
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (await CheckValues())
            {
                await MakeNewPricelist();
                await MakeNewPricelistChannels();
                // Add dayparts here
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

    }
}
