﻿using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ChannelDTO;
using Database.DTOs.PricelistChannels;
using Database.DTOs.PricelistDTO;
using Database.DTOs.PricesDTO;
using Database.DTOs.SeasonalityDTO;
using Database.DTOs.SectableDTO;
using Database.DTOs.TargetDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CampaignEditor.UserControls;
using Database.Entities;
using System.ComponentModel;

namespace CampaignEditor
{
    public partial class PriceList : Window
    {
        private ChannelController _channelController;
        private PricelistChannelsController _pricelistChannelsController;
        private PricelistController _pricelistController;
        private PricesController _pricesController;
        private SectableController _sectableController;
        private SeasonalityController _seasonalityController;
        private TargetController _targetController;

        private readonly IAbstractFactory<Sectable> _factorySectable;
        private readonly IAbstractFactory<Seasonality> _factorySeasonality;
        private readonly IAbstractFactory<NewTarget> _factoryNewTarget;
        private readonly IAbstractFactory<DuplicatePricelist> _factoryDuplicatePricelist;

        private CampaignDTO _campaign;
        public PricelistDTO _pricelist;

        private bool pricelistModified = false;
        private bool pricelistChannelsModified = false;
        private bool dayPartsModified = false;
        public bool sectableModified = false;
        public bool seasonalityModified = false;

        public bool pricelistChanged = false;
        IEnumerable<ChannelDTO> _allChannels;
        public PriceList(IAbstractFactory<Sectable> factorySectable, IAbstractFactory<Seasonality> factorySeasonality,
            IAbstractFactory<NewTarget> factoryNewTarget, IAbstractFactory<DuplicatePricelist> factoryDuplicatePricelist,
            IChannelRepository channelRepository, IPricesRepository pricesRepository,
            IPricelistRepository pricelistRepository, IPricelistChannelsRepository pricelistChannelsRepository,
            ISectableRepository sectableRepository, ISeasonalityRepository seasonalityRepository,
            ITargetRepository targetRepository)
        {
            _factorySectable = factorySectable;
            _factorySeasonality = factorySeasonality;
            _factoryNewTarget = factoryNewTarget;
            _factoryDuplicatePricelist = factoryDuplicatePricelist;

            _sectableController = new SectableController(sectableRepository);
            _seasonalityController = new SeasonalityController(seasonalityRepository);
            _targetController = new TargetController(targetRepository);
            _channelController = new ChannelController(channelRepository);
            _pricelistChannelsController = new PricelistChannelsController(pricelistChannelsRepository);
            _pricelistController = new PricelistController(pricelistRepository);
            _pricesController = new PricesController(pricesRepository);

            InitializeComponent();

        }
        #region Initialization
        public async Task Initialize(CampaignDTO campaign, PricelistDTO pricelist = null)
        {
            _campaign = campaign;
            _pricelist = pricelist;

            bool isAdmin = MainWindow.user.usrlevel <= 0;
            chbGlobal.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            if (chbGlobal.Visibility == Visibility.Visible && _pricelist != null)
                chbGlobal.IsChecked = _pricelist.clid == 0;

            await FillFields();
            if (_pricelist != null)
            {
                await AssignFields();
            }
            ResetModifiers();
        }

        public void MakeReadonly()
        {
            tbName.IsEnabled = false;
            chbGlobal.IsEnabled = false;
            cbType.IsEnabled = false;
            cbSectable.IsEnabled = false;
            btnEditSectable.IsEnabled = false;
            btnNewSectable.IsEnabled = false;
            cbSectable2.IsEnabled = false;
            chbSectable2.IsEnabled = false;
            tbSec2From.IsEnabled = false;
            tbSec2To.IsEnabled = false;
            cbSeasonality.IsEnabled = false;
            btnNewSeasonality.IsEnabled = false;
            btnEditSeasonality.IsEnabled = false;
            tbCP.IsEnabled = false;
            tbMinGRP.IsEnabled = false;
            chbGRP.IsEnabled = false;
            chbFixed.IsEnabled = false;
            tbFixed.IsEnabled = false;
            cbTarget.IsEnabled = false;
            btnNewTarget.IsEnabled = false;
            btnEditTarget.IsEnabled = false;
            dpValidityFrom.IsEnabled = false;
            dpValidityTo.IsEnabled = false;
            MakeReadonlyDayParts();
            MakeReadonlyChannels();

            btnSave.Visibility = Visibility.Collapsed;
            btnSaveAs.Visibility = Visibility.Collapsed;
            btnCancel.Content = "Close";
        }

        private void MakeReadonlyDayParts()
        {
            for (int i = 0; i < wpDayParts.Children.Count - 1; i++)
            {
                TargetDPItem item = (TargetDPItem)wpDayParts.Children[i];
                item.IsEnabled = false;
            }
            Button button = (Button)wpDayParts.Children[wpDayParts.Children.Count - 1];
            button.Visibility = Visibility.Collapsed;
        }

        private void MakeReadonlyChannels()
        {
            for (int i=0; i<lbChannels.Items.Count - 1; i++)
            {
                ChannelPlItem item = (ChannelPlItem)lbChannels.Items[i];
                item.IsEnabled = false;
            }
            Button button = (Button)lbChannels.Items[lbChannels.Items.Count - 1];
            button.Visibility = Visibility.Collapsed;
        }

        private void ResetModifiers()
        {
            pricelistModified = false;
            pricelistChannelsModified = false;
            dayPartsModified = false;
        }
        #endregion

        #region Fill Fields

        #region DP functionality
        private void btnAddDP_Click(object sender, RoutedEventArgs e)
        {
            /*var btnAdd = sender as Button;
            wpDayParts.Children.Remove(btnAdd);*/

            //var dpItem = new Day
            TargetDPItem dpItem = MakeDPItem();
            wpDayParts.Children.Insert(wpDayParts.Children.Count - 1, dpItem);
            dayPartsModified = true;
            //UpdateDayParts();
        }
        private void btnDeleteDP_Click(object sender, RoutedEventArgs e)
        {
            dayPartsModified = true;
        }

        // Selecting whole text when captured
        private void tbSec2_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var tb = sender as TextBox;

            tb.SelectAll();
            tb.Focus();
        }
        #endregion

        private async Task FillFields()
        {
            //await FillChannels();
            _allChannels = new List<ChannelDTO>(await _channelController.GetAllChannels()).Where(ch => ch.chactive);
            _allChannels = _allChannels.OrderBy(ch => ch.chname);
            var chplItem = new ChannelPlItem();
            chplItem.Initialize(_allChannels);
            chplItem.DeleteClicked += ChplItem_DeleteClicked;
            lbChannels.Initialize(chplItem);
            lbChannels.BtnAddClicked += LbChannels_BtnAddClicked;
            FillDefaultWpDayParts();
            await FillComboBoxes();
        }

        private void LbChannels_BtnAddClicked(object? sender, EventArgs e)
        {
            var chplItem = lbChannels.Items[lbChannels.Items.Count-2] as ChannelPlItem;
            if (chplItem != null)
            {
                chplItem.Initialize(_allChannels);
                chplItem.DeleteClicked += ChplItem_DeleteClicked;
            }

        }

        private void ChplItem_DeleteClicked(object? sender, EventArgs e)
        {
            var chplItem = sender as ChannelPlItem;
            if (chplItem != null)
            {
                chplItem.DeleteClicked -= ChplItem_DeleteClicked;
                lbChannels.Items.Remove(chplItem);
            }
            pricelistChannelsModified = true;
        }

        private void FillDefaultWpDayParts()
        {
            var item = MakeEmptyDPItem();
            wpDayParts.Children.Add(item);
            Button addButton = MakeAddButton();
            wpDayParts.Children.Add(addButton);
        }

        // Adding Add Button and New TargetDPItem

        private Button MakeAddButton()
        {
            Button btnAddDP = new Button();
            btnAddDP.Click += new RoutedEventHandler(btnAddDP_Click);
            Image imgGreenPlus = new Image();
            ImageSource imageSource = (ImageSource)Application.Current.FindResource("plus_icon");
            imgGreenPlus.Source = imageSource;
            btnAddDP.Content = imgGreenPlus;
            btnAddDP.Width = 30;
            btnAddDP.Height = 30;
            btnAddDP.Background = Brushes.White;
            btnAddDP.BorderThickness = new Thickness(0);
            btnAddDP.HorizontalAlignment = HorizontalAlignment.Center;

            return btnAddDP;
        }

        private TargetDPItem MakeEmptyDPItem()
        {
            TargetDPItem item = MakeDPItem();

            item.tbFromH.Text = "02";
            item.tbFromM.Text = "00";
            item.tbToH.Text = "25";
            item.tbToM.Text = "59";

            item.tbCoef.Text = "1";
            item.cbIsPT.IsChecked = false;
            item.tbDays.Text = "1234567";

            item.modified = false;

            return item;
        }

        private TargetDPItem MakeDPItem()
        {
            TargetDPItem dpItem = new TargetDPItem();
            dpItem.btnDelete.Click += btnDeleteDP_Click;
            dpItem.Width = wpDayParts.Width;    

            return dpItem;
        }
        /*private void UpdateDayParts()
        {
            Button btnAddDP = MakeAddButton();
            TargetDPItem dpItem = MakeDPItem();
            dpItem.Width = wpDayParts.Width;

            wpDayParts.Children.Add(dpItem);
            wpDayParts.Children.Add(btnAddDP);
        }*/

        private async Task FillComboBoxes()
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

            List<SectableDTO> sectables = (List<SectableDTO>)await _sectableController.GetAllSectablesByOwnerId(_campaign.clid);
            

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

            List<SeasonalityDTO> seasonalities = (List<SeasonalityDTO>)await _seasonalityController.GetAllSeasonalitiesByOwnerId(_campaign.clid);
            foreach (var seasonality in seasonalities)
            {
                cbSeasonality.Items.Add(seasonality);
            }
            
            cbSeasonality.SelectedIndex = index;
        }
        private async Task FillCBTarget(int index = 0)
        {
            cbTarget.Items.Clear();

            List<TargetDTO> targets = (List<TargetDTO>)await _targetController.GetAllClientTargets(_campaign.clid);

            foreach (var target in targets)
            {
                cbTarget.Items.Add(target);
            }

            cbTarget.SelectedIndex = index;
        }

        #endregion

        #region Assign Fields
        private async Task AssignFields()
        {
            await AssignPricelistValues();
            await AssignDayPartsValues();
            await AssignChannelsValues();
            AssignSectableValues();
            AssignSeasonalityValues();
        }
        private async Task AssignPricelistValues()
        {
            tbName.Text = _pricelist.plname.Trim();
            cbType.SelectedIndex = _pricelist.pltype;
            cbSectable.SelectedItem = await _sectableController.GetSectableById(_pricelist.sectbid);
            cbSectable2.SelectedItem = await _sectableController.GetSectableById(_pricelist.sectbid);
            chbSectable2.IsChecked = _pricelist.use2;
            tbSec2From.Text = _pricelist.sectb2st.ToString().PadLeft(6,'0').Substring(0,4);
            tbSec2To.Text = _pricelist.sectb2en.ToString().PadLeft(6,'0').Substring(0,4);
            cbSeasonality.SelectedItem = await _seasonalityController.GetSeasonalityById(_pricelist.seastbid);
            tbCP.Text = _pricelist.price.ToString("0.0###");
            tbMinGRP.Text = _pricelist.minprice.ToString("0.0###");
            chbGRP.IsChecked = _pricelist.mgtype;
            if (_pricelist.fixprice != 0)
            {
                chbFixed.IsChecked = true;
                tbFixed.Text = _pricelist.fixprice.ToString("0.0###");
            }
            var target = await _targetController.GetTargetById(_pricelist.pltarg);
            for (int i=0; i<cbTarget.Items.Count; i++)
            {
                var targ = cbTarget.Items[i] as TargetDTO;
                if (targ != null && targ.targid == target.targid)
                    cbTarget.SelectedIndex = i;
            }
            //cbTarget.SelectedItem = await _targetController.GetTargetById(_pricelist.pltarg);
            dpValidityFrom.SelectedDate = TimeFormat.YMDStringToDateTime(_pricelist.valfrom.ToString());
            dpValidityTo.SelectedDate = TimeFormat.YMDStringToDateTime(_pricelist.valto.ToString());
        }
        private async Task AssignDayPartsValues()
        {
            wpDayParts.Children.Clear();

            var dpValues = await _pricesController.GetAllPricesByPlId(_pricelist.plid);
            dpValues = dpValues.OrderBy(dp => dp.dps).ThenBy(dp => dp.days);

            if (dpValues.Count() == 0)
            {
                var item = MakeEmptyDPItem();
                wpDayParts.Children.Add(item);
            }
            else
            {
                foreach (var dp in dpValues)
                {
                    TargetDPItem item = MakeDPItem();

                    string[] fromList = dp.dps.Split(':');
                    string[] toList = dp.dpe.Split(':');
                    item.tbFromH.Text = fromList[0];
                    item.tbFromM.Text = fromList[1];
                    item.tbToH.Text = toList[0];
                    item.tbToM.Text = toList[1];

                    item.tbCoef.Text = dp.price.ToString("0.0###");
                    item.cbIsPT.IsChecked = dp.ispt;
                    item.tbDays.Text = dp.days.ToString();

                    item.modified = false;

                    wpDayParts.Children.Add(item);
                }
            }
            

            Button addButton = MakeAddButton();
            wpDayParts.Children.Add(addButton);
        }
        private async Task AssignChannelsValues()
        {

            var pricelistChannels = await _pricelistChannelsController.GetAllPricelistChannelsByPlid(_pricelist.plid);
            if (pricelistChannels.Count() > 0)
            {
                ChannelPlItem channelItem = lbChannels.Items[0] as ChannelPlItem;
                if (channelItem != null)
                {
                    channelItem.DeleteClicked -= ChplItem_DeleteClicked;
                    lbChannels.Items.Remove(channelItem);
                }

            }
            foreach (var plchn in pricelistChannels)
            {
                var channel = await _channelController.GetChannelById(plchn.chid);
                ChannelPlItem channelItem = new ChannelPlItem();
                channelItem.Initialize(_allChannels, channel, plchn.chcoef);
                channelItem.DeleteClicked += ChplItem_DeleteClicked;
                lbChannels.Items.Insert(lbChannels.Items.Count - 1, channelItem);
            }
        }

        private void AssignSectableValues()
        {

            for (int index = 0; index < cbSectable.Items.Count; index++)
            {
                SectableDTO sectable = cbSectable.Items[index] as SectableDTO;
                if (sectable.sctid == _pricelist.sectbid)
                {
                    cbSectable.SelectedIndex = index;
                }
                if (sectable.sctid == _pricelist.sectbid2)
                {
                    cbSectable2.SelectedIndex = index;
                }
            }

        }
        private void AssignSeasonalityValues()
        {

            for (int index = 0; index < cbSeasonality.Items.Count; index++)
            {
                SeasonalityDTO seasonality = cbSeasonality.Items[index] as SeasonalityDTO;
                if (seasonality.seasid == _pricelist.seastbid)
                {
                    cbSeasonality.SelectedIndex = index;
                }
            }

        }
        
        #endregion

        #region Writing into base
        // for writing data in Database tblpricelist and tblpricelistchn
        private async Task CreateOrUpdatePricelist(PricelistDTO pricelist = null)
        {
            int clid = _campaign.clid;
            if ((bool)chbGlobal.IsChecked)
                clid = 0;
            string plname = tbName.Text.Trim();
            int pltype = cbType.SelectedIndex; // Place in combobox corresponds to int value
            int sectbid = (cbSectable.SelectedValue as SectableDTO)!.sctid; // By default, first value is selected
            int seasid = (cbSeasonality.SelectedValue as SeasonalityDTO)!.seasid;
            bool plactive = true;
            decimal price = decimal.Parse(tbCP.Text.Trim());
            decimal minprice = decimal.Parse(tbMinGRP.Text.Trim());
            bool prgcoef = false;
            int pltarg = (cbTarget.SelectedValue as TargetDTO)!.targid;
            bool use2 = (bool)chbSectable2.IsChecked;
            int sectbid2 = (cbSectable2.SelectedValue as SectableDTO)!.sctid;
            int sectb2st = int.Parse((tbSec2From.Text.Trim()+"00").PadLeft(6, '0'));
            int sectb2en = int.Parse((tbSec2To.Text.Trim()+"59").PadLeft(6,'0'));
            int valfrom = int.Parse(TimeFormat.DPToYMDString(dpValidityFrom));
            int valto = int.Parse(TimeFormat.DPToYMDString(dpValidityTo));
            bool mgtype = (bool)chbGRP.IsChecked;
            decimal fixPrice = 0.0M;
            if ((bool)chbFixed.IsChecked && !decimal.TryParse(tbFixed.Text, out fixPrice))
            {
                MessageBox.Show("Value for fixed price is invalid!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (pricelist == null)
            {
                _pricelist = await _pricelistController.CreatePricelist(new CreatePricelistDTO
                    (clid, plname, pltype, sectbid, seasid, plactive, price, minprice,
                    prgcoef, pltarg, use2, sectbid2, sectb2st, sectb2en,
                    valfrom, valto, mgtype, fixPrice));
            }
            else
            {
                await _pricelistController.UpdatePricelist(new UpdatePricelistDTO
                    (pricelist.plid, clid, plname, pltype, sectbid, seasid, plactive, price, minprice,
                    prgcoef, pltarg, use2, sectbid2, sectb2st, sectb2en,
                    valfrom, valto, mgtype, fixPrice));
                _pricelist = await _pricelistController.GetPricelistById(pricelist.plid);
            }
        }

        private async Task CreateOrUpdatePricelistChannels(PricelistDTO pricelist)
        {

            List<Tuple<int, decimal>> chIdCoefs = new List<Tuple<int, decimal>>();
            for (int i=0; i<lbChannels.Items.Count-1; i++)
            {
                ChannelPlItem chplItem = lbChannels.Items[i] as ChannelPlItem;
                var channel = chplItem.GetChannel();
                var chcoef = chplItem.GetCoef();
                // skip empty channels and already inserted channels
                if (channel == null || chcoef == null || chIdCoefs.Any(chidc => chidc.Item1 == channel.chid))
                {
                    continue;
                }
                chIdCoefs.Add(Tuple.Create(channel.chid, chcoef.Value));                              
            }
            await _pricelistChannelsController.DeleteAllPricelistChannelsByPlid(pricelist.plid);
            foreach (var chidCoef in chIdCoefs)
            {
                await _pricelistChannelsController.CreatePricelistChannels(
                    new CreatePricelistChannelsDTO(pricelist.plid, chidCoef.Item1, chidCoef.Item2));
            }
        }

        private async Task CreateOrUpdateDayparts(PricelistDTO pricelist)
        {
            int n = wpDayParts.Children.Count;

            await _pricesController.DeletePricesByPlid(pricelist.plid);
            int created = 0;
            for (int i = 0; i < n - 1; i++) // last item is Button
            {
                TargetDPItem item = wpDayParts.Children[i] as TargetDPItem;
                if (item.tbFromH.Text.Trim() == "" ||
                    item.tbFromM.Text.Trim() == "" ||
                    item.tbToH.Text.Trim() == "" ||
                    item.tbToM.Text.Trim() == "" ||
                    item.tbDays.Text.Trim() == "")
                {
                    continue;
                }
                else
                {
                    string dps = (item.tbFromH.Text.Trim() + ":" + item.tbFromM.Text.Trim()).PadLeft(2, '0');
                    string dpe = (item.tbToH.Text.Trim() + ":" + item.tbToM.Text.Trim()).PadLeft(2, '0');
                    decimal price = decimal.Parse(item.tbCoef.Text.Trim());
                    bool ispt = (bool)item.cbIsPT.IsChecked;
                    string days = item.tbDays.Text.Trim();

                    await _pricesController.CreatePrices(new CreatePricesDTO(pricelist.plid, dps, dpe, price, ispt, days));
                    created += 1; 
                }
            }
            if (created == 0)
            {
                await _pricesController.CreatePrices(new CreatePricesDTO(pricelist.plid, "02:00", "25:59", 1.0M, false, "1234567"));
            }
        }
        #endregion

        #region Check Values
        private async Task<bool> CheckValues(bool saveAs = false)
        {
            if (await CheckName(saveAs) == false)
                return false; 

            if (CheckCP() && CheckMinGRP() && 
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
        private async Task<bool> CheckName(bool checkSameName)
        {
            string name = tbName.Text.Trim();
            if (name.Length <= 0)
            {
                MessageBox.Show("Enter name");
                return false;
            }
            var pricelistWithSameName = await _pricelistController.GetClientPricelistByName(_campaign.clid, name);
            if (pricelistWithSameName != null && (_pricelist == null ||  
                    (_pricelist != null && pricelistWithSameName.plid != _pricelist.plid) || 
                    checkSameName))
            {
                MessageBox.Show("Name already exist");
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool CheckCP()
        {
            string cp = tbCP.Text.Trim();
            decimal cpdecimal = 0;
            if (cp.Length <= 0)
            {
                MessageBox.Show("Enter CP(/I/P/S)");
                return false;
            }else if(!decimal.TryParse(cp, out cpdecimal))
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
            decimal grpdecimal = 0;
            if (grp.Length <= 0)
            {
                MessageBox.Show("Enter MinGRP");
                return false;
            }
            else if (!decimal.TryParse(grp, out grpdecimal))
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
            if (toDate == null)
            {
                toDate = new DateTime(2999, 01, 01);
                dpValidityTo.SelectedDate = toDate;
            }
            if (!fromDate.HasValue)
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
            bool atLeastOne = false;
            List<TargetDPItem> items = new List<TargetDPItem>();

            for (int i=0; i<wpDayParts.Children.Count -1; i++)
            {
                TargetDPItem item = (TargetDPItem)wpDayParts.Children[i];
                items.Add(item);
                string validity = "";
                if ((validity = item.CheckValidity()) != "")
                {
                    if (validity == "empty")
                    {
                        continue;
                    }
                    MessageBox.Show(validity);
                    success = false;
                    break;
                }
                else
                {
                    atLeastOne = true;
                }
            }

            // If there are no values entered in dpItems, clear and add one, and fill it with default values
            /*if (!atLeastOne)
            {
                wpDayParts.Children.Clear();
                wpDayParts.Children.Add(MakeDPItem());
                wpDayParts.Children.Add(MakeAddButton());

                var item = wpDayParts.Children[0] as TargetDPItem;
                item.tbFromH.Text = "02";
                item.tbFromM.Text = "00";
                item.tbToH.Text = "25";
                item.tbToM.Text = "59";
                item.tbCoef.Text = "1.00";
            }*/

            items = items.OrderBy(item => item.GetSTime()).ToList();
            for (int i=0; i<items.Count()-1; i++)
            {
                for (int j=i+1; j<items.Count(); j++)
                {
                    if (String.Compare(items[i].GetETime(),items[j].GetSTime()) >= 0)
                    {
                        if (CheckOverlappingDays(items[i].GetDays(), items[j].GetDays()))
                        {
                            MessageBox.Show($"Intercepting intervals!\n{items[i]} and \n{items[j]}");
                            return false;                    
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return success;
        }

        private bool CheckOverlappingDays(string days1, string days2)
        {
            foreach (char day in days1)
            {
                if (days2.Contains(day))
                    return true;
            }
            return false;
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

            pricelistModified = true;
        }

        private void cbSeasonality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSeasonality.SelectedIndex <= 0)
                btnEditSeasonality.Visibility = Visibility.Hidden;
            else
                btnEditSeasonality.Visibility = Visibility.Visible;

            pricelistModified = true;
        }

        private void cbTarget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTarget.SelectedIndex <= 0)
                btnEditTarget.Visibility = Visibility.Hidden;
            else
                btnEditTarget.Visibility = Visibility.Visible;

            pricelistModified = true;
        }
        #endregion

        private async void btnNewTarget_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factoryNewTarget.Create();
            await factory.InitializeTree(_campaign);
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
                var success = await factory.InitializeTargetToEdit(_campaign, target);
            }

            factory.ShowDialog();

            if (factory.success)
                await FillCBTarget(index);
        }
        private async void btnNewSectable_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factorySectable.Create();
            int index = cbSectable.Items.Count;

            factory.Initialize(_campaign);
            factory.ShowDialog();
            if (factory.success)
            {
                var sec = factory.Sec;
                cbSectable.Items.Add(sec);
                cbSectable.SelectedIndex = cbSectable.Items.Count - 1;
                sectableModified = true;
            }

        }
        private async void btnEditSectable_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factorySectable.Create();
            SectableDTO sectable = (cbSectable.SelectedItem as SectableDTO)!;
            int index = cbSectable.SelectedIndex;

            factory.Initialize(_campaign, sectable);
            factory.ShowDialog();
            if (factory.success)
            {
                cbSectable.Items[index] = factory.Sec;
                cbSectable.SelectedIndex = index;
                sectableModified = true;
            }
        }

        private async void btnNewSeasonality_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factorySeasonality.Create();

            factory.Initialize(_campaign);
            factory.ShowDialog();
            if (factory.success)
            {
                var seas = factory.Seas;
                cbSeasonality.Items.Add(seas);
                cbSeasonality.SelectedIndex = cbSeasonality.Items.Count-1;
                seasonalityModified = true;
            }
        }
        private async void btnEditSeasonality_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factorySeasonality.Create();
            SeasonalityDTO seasonality = (cbSeasonality.SelectedItem as SeasonalityDTO)!;
            int index = cbSeasonality.SelectedIndex;

            factory.Initialize(_campaign, seasonality);
            factory.ShowDialog();
            if (factory.success)
            {
                cbSeasonality.Items[index] = factory.Seas;
                cbSeasonality.SelectedIndex = index;
                seasonalityModified = true;
            }
        }
        #endregion

        #region Save and Cancel Buttons
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (await CheckValues())
            {
                if (pricelistModified)
                    await CreateOrUpdatePricelist(_pricelist);
                if (isPricelistChannelsModified())
                    await CreateOrUpdatePricelistChannels(_pricelist);
                if (CheckDayPartsModified())
                    await CreateOrUpdateDayparts(_pricelist);
                if (pricelistModified || pricelistChannelsModified || dayPartsModified)
                {
                    pricelistChanged = true;
                }
                this.Close();
            }
        }

        private async void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (await CheckValues(true))
            {
                // Copy values
                // Make new pricelist
                // inside CreateOrUpdatePricelist function _pricelist will be updated 
                await CreateOrUpdatePricelist();
                await CreateOrUpdatePricelistChannels(_pricelist);
                await CreateOrUpdateDayparts(_pricelist);
                pricelistChanged = true;
                
                this.Close();
            }
        }

        private async void btnCopyPricelist_Click(object sender, RoutedEventArgs e)
        {
            if (_pricelist == null)
                return;
            var f = _factoryDuplicatePricelist.Create();
            await f.Initialize(_pricelist);
            f.ShowDialog();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
        }

        #endregion

        #region Modification checkers
        private bool CheckDayPartsModified()
        {
            int n = wpDayParts.Children.Count;
            for (int i = 0; i < n - 1; i++)
            {
                TargetDPItem item = wpDayParts.Children[i] as TargetDPItem;
                if (item.modified)
                    dayPartsModified = true;
            }
            return dayPartsModified;
        }
        private void tbName_TextChanged(object sender, TextChangedEventArgs e)
        {
            pricelistModified = true;
        }

        private void cbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pricelistModified = true;
        }

        private void cbSectable2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pricelistModified = true;
        }

        private void chbSectable2_Checked(object sender, RoutedEventArgs e)
        {
            pricelistModified = true;
        }

        private void chbSectable2_Unchecked(object sender, RoutedEventArgs e)
        {
            pricelistModified = true;
        }

        private void tbSec2From_TextChanged(object sender, TextChangedEventArgs e)
        {
            pricelistModified = true;
        }

        private void tbSec2To_TextChanged(object sender, TextChangedEventArgs e)
        {
            pricelistModified = true;
        }

        private void tbCP_TextChanged(object sender, TextChangedEventArgs e)
        {
            pricelistModified = true;
        }

        //private bool checkedFirst = true;
        private void chb_Checked(object sender, RoutedEventArgs e)
        {
            // Mutual excluding
            /*if (checkedFirst)
            {
                checkedFirst = false;
                return;
            }
            if (((CheckBox)sender).Name == "chbGRP")
            {
                chbFixed.IsChecked = false;
                tbFixed.IsEnabled = false;
            }
            else if(((CheckBox)sender).Name == "chbFixed")
            {
                chbGRP.IsChecked = false;
            }*/
            if (((CheckBox)sender).Name == "chbFixed")
            {
                tbFixed.IsEnabled = true;
            }
            pricelistModified = true;
        }

        private void chb_Unchecked(object sender, RoutedEventArgs e)
        {
            // Mutual excluding
            /*if (((CheckBox)sender).Name == "chbGRP")
            {
                tbFixed.IsEnabled = true;
            }
            else if (((CheckBox)sender).Name == "chbFixed")
            {
                tbFixed.IsEnabled = false;
            }*/
            if (((CheckBox)sender).Name == "chbFixed")
            {
                tbFixed.IsEnabled = false;
            }
            pricelistModified = true;
        }

        private void tbMinGRP_TextChanged(object sender, TextChangedEventArgs e)
        {
            pricelistModified = true;
        }

        private void dpValidityFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            pricelistModified = true;
        }

        private void dpValidityTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {   
            pricelistModified = true;
        }

        private bool isPricelistChannelsModified()
        {
            if (pricelistChannelsModified)
            {
                return true;
            }

            for (int i=0; i < lbChannels.Items.Count - 1; i++)
            {
                ChannelPlItem chplItem = lbChannels.Items[i] as ChannelPlItem;
                if (chplItem.GetIsModified())
                {
                    pricelistChannelsModified = true;
                    return true;
                }

            }
            
            pricelistChannelsModified = false;
            return false;
        }


        #endregion

        protected override void OnClosing(CancelEventArgs e)
        {
            for (int i = 0; i < lbChannels.Items.Count - 1; i++)
            {
                ChannelPlItem chplItem = lbChannels.Items[i] as ChannelPlItem;
                if (chplItem.GetIsModified())
                {
                    chplItem.DeleteClicked -= ChplItem_DeleteClicked;
                }

            }
            lbChannels.BtnAddClicked -= LbChannels_BtnAddClicked;
        }
    }
}
