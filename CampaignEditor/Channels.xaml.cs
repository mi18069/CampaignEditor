﻿using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ActivityDTO;
using Database.DTOs.ChannelCmpDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.ChannelGroupDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.PricelistDTO;
using Database.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CampaignEditor.Helpers;
using Database.Entities;

namespace CampaignEditor
{
    public partial class Channels : Window
    {
        private ClientDTO _client;
        private CampaignDTO _campaign;

        private ChannelController _channelController;
        private PricelistController _pricelistController;
        private PricelistChannelsController _pricelistChannelsController;
        private ActivityController _activityController;
        private ChannelCmpController _channelCmpController;
        private ForecastDataManipulation _forecastDataManipulation;


        private readonly IAbstractFactory<PriceList> _factoryPriceList;
        private readonly IAbstractFactory<GroupChannels> _factoryGroupChannels;

        GroupChannels fGroupChannels;

        private List<ChannelDTO> _allChannelList;
        private ObservableCollection<ChannelDTO> _channelList;
        // This list will serve to get all Pricelists for client
        private List<PricelistDTO> _allPricelistsList;
        private ObservableCollection<PricelistDTO> _pricelistList;
        private ObservableCollection<ActivityDTO> _activityList;
        private ObservableCollection<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> _selected = 
                                new ObservableCollection<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>>();

        private List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> _selectedChannels =
                                new List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>>();
        private List<ChannelDTO> _originalSelectedChannels = new List<ChannelDTO>();



        public bool channelsModified = false;
        public bool channelsOrderChanged = false;
        public bool canEdit = false;
        private bool onlyActive = false; // For chbActive
        public bool pricelistChanged = false;

        public bool updateChannels = false; // for firing event in overview
        public List<int> channelsToDelete = new List<int>();
        public List<int> channelsToAdd = new List<int>();


        #region Getters and Setters for lists

        private List<ChannelDTO> AllChannelList
        {
            get { return _allChannelList; }
            set { _allChannelList = value; }
        }

        private ObservableCollection<ChannelDTO> ChannelList
        {
            get { return _channelList;  }
            set { _channelList = value; }
        }
        private ObservableCollection<PricelistDTO> PricelistList
        {
            get { return _pricelistList;  }
            set { _pricelistList = value; }
        }

        private List<PricelistDTO> AllPricelistsList
        {
            get { return _allPricelistsList; }
            set { _allPricelistsList = value; }
        }
        private ObservableCollection<ActivityDTO> ActivityList
        {
            get { return _activityList;  }
            set { _activityList = value; }
        }
        private ObservableCollection<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> Selected
        {
            get { return _selected;  }
            set { _selected = value; }
        }

        public List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> SelectedChannels
        {
            get { return _selectedChannels; }
            set { _selectedChannels = value; }
        }
        #endregion

        public event EventHandler<UpdatePricelistEventArgs> PricelistUpdatedEvent;
        public Channels(IChannelRepository channelRepository, IPricelistRepository pricelistRepository,
            IPricelistChannelsRepository pricelistChannelsRepository, IActivityRepository activityRepository,
            IAbstractFactory<PriceList> factoryPriceList, IChannelCmpRepository channelCmpRepository,
            IAbstractFactory<GroupChannels> factoryGroupChannels, IAbstractFactory<ForecastDataManipulation> factoryForecastDataManipulation)
        {
            this.DataContext = this;
            _channelController = new ChannelController(channelRepository);
            _pricelistController = new PricelistController(pricelistRepository);
            _pricelistChannelsController = new PricelistChannelsController(pricelistChannelsRepository);
            _activityController = new ActivityController(activityRepository);
            _channelCmpController = new ChannelCmpController(channelCmpRepository);

            _forecastDataManipulation = factoryForecastDataManipulation.Create();


            _factoryPriceList = factoryPriceList;
            _factoryGroupChannels = factoryGroupChannels;

            InitializeComponent();

            if (MainWindow.user.usrlevel >= 1)
            {
                btnNewPricelist.IsEnabled = false;
                btnEditPricelist.IsEnabled = false;
                btnEditChannelGroups.IsEnabled = false;
            }
        }

        public async Task Initialize(ClientDTO client, CampaignDTO campaign,
            List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> channelList)
        {
            this._client = client;
            this._campaign = campaign;
            //CampaignEventLinker.AddChannels(_campaign.cmpid, this);
            SelectedChannels = channelList;
            await FillLists();
            await FillChannelGroups();
        }

        private async Task FillLists()
        {
            Selected.Clear();

            _channelList = new ObservableCollection<ChannelDTO>((await _channelController.GetAllChannels()).OrderBy(c => c.chname));

            // Create a ListCollectionView and bind it to the ObservableCollection
            ListCollectionView channelListView = new ListCollectionView(ChannelList);
            channelListView.SortDescriptions.Add(new SortDescription("chname", ListSortDirection.Ascending));
            channelListView.Filter = ch => { return ((ChannelDTO)ch).chactive; };
            // Set the ListCollectionView as the view for your ObservableCollection
            lvChannels.ItemsSource = channelListView;

            // Initializing and sorting lists, so it don't have to be sorted later
            _allPricelistsList = (List<PricelistDTO>)await _pricelistController.GetAllClientPricelists(_client.clid);
            _allPricelistsList = _allPricelistsList.OrderBy(p => p.clid != 0).ThenByDescending(p => p.plname).ToList();
            _pricelistList = new ObservableCollection<PricelistDTO>((AllPricelistsList));
            _activityList = new ObservableCollection<ActivityDTO>((await _activityController.GetAllActivities()).OrderBy(a => a.act));

            // Binding to ListViews
            //lvChannels.ItemsSource = ChannelList;
            lvPricelists.ItemsSource = PricelistList;
            lvActivities.ItemsSource = ActivityList;

            AllChannelList = ChannelList.ToList();

            foreach (var selected in SelectedChannels)
            {
                // If we want to move some channel from channelList to Selected,
                // first we must find that object in ChannelList
                foreach (ChannelDTO channel in ChannelList)
                {
                    if (channel.chid == selected.Item1.chid)
                    {
                        MoveToSelected(channel, selected.Item2, selected.Item3, false);
                        break;
                    }

                }
            }
            channelsModified = false;

            dgSelected.ItemsSource = Selected;
            await RefreshPricelists();
        }

        #region ToSelected and FromSelected 

        private void MoveToSelected(ChannelDTO channel, PricelistDTO pricelist, ActivityDTO activity, bool addToAdded = true)
        {
            var list = Selected.Select(t => t.Item1).ToList(); // making a list to pass to function FindIndex
            // We don't want to insert alphabetically, but in adding order
            //int index = FindIndex(list, channel); // Finding the right index, to keep the list sorted
            Selected.Add(Tuple.Create(channel, pricelist, activity)!);
            ChannelList.Remove(channel);
            AllChannelList.Remove(channel);
            lbSelectedChannels.Items.Remove(channel);
            channelsModified = true;

            // If channel is added, but it's already in deleted list, then just remove it from deleted list
            // it it's not in deleted, then put it in added list
            if (!addToAdded)
                return;
            bool added = false;
            for (int i = 0; i < channelsToDelete.Count(); i++)
            {
                if (channelsToDelete[i] == channel.chid)
                {
                    channelsToDelete.RemoveAt(i);
                    added = true;
                    break;
                }
            }
            if (!added)
                channelsToAdd.Add(channel.chid);
        }
        private async void MoveFromSelected(Tuple<ChannelDTO, PricelistDTO, ActivityDTO> tuple)
        {
            if (!await _forecastDataManipulation.CheckIfChannelCanBeDeleted(_campaign.cmpid, tuple.Item1.chid))
            {
                return;
            }
            var channel = tuple.Item1;
            Selected.Remove(tuple);
            int index = FindIndex(ChannelList, channel);
            ChannelList.Insert(index, channel);
            AllChannelList.Insert(index, channel);
            channelsModified = true;

            // If channels in in list to add, but it's deleted, then just delete it from that list,
            // if it's not previously added, and now it's deleted, then add it in channelsToDelete list
            bool removed = false;
            for (int i = 0; i < channelsToAdd.Count(); i++)
            {
                if (channelsToAdd[i] == channel.chid)
                {
                    channelsToAdd.RemoveAt(i);
                    removed = true;
                    break;
                }
            }
            if (!removed)
                channelsToDelete.Add(channel.chid);
        }


        private async void btnToSelected_Click(object sender, RoutedEventArgs e)
        {
            // At least one item from every listView needs to be selected to execute
            if (lbSelectedChannels.Items.Count == 0)
            {
                MessageBox.Show("Select at least one Channel", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else if (lvPricelists.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select Pricelist", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else if (lvActivities.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select Activity", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                int n = lbSelectedChannels.Items.Count;
                var channels = lbSelectedChannels.Items;

                PricelistDTO pricelist = lvPricelists.SelectedItem as PricelistDTO;
                ActivityDTO activity = lvActivities.SelectedItem as ActivityDTO;
                for (int i=0; i<n; i++)
                {
                    ChannelDTO channel = channels[0] as ChannelDTO; // 0 because we need n iterations, and in each we remove one item
                    MoveToSelected(channel, pricelist, activity);
                }

                await RefreshPricelists();
            }
        }

        private void btnFromSelected_Click(object sender, RoutedEventArgs e)
        {
            int n = dgSelected.SelectedItems.Count;
            if (n == 0)
            {
                MessageBox.Show("Select at least one Channel", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                var selectedItems = dgSelected.SelectedItems;
                for (int i=0; i<n; i++)
                {
                    // 0 because we need n iterations, and in each we remove one item
                    var tuple = selectedItems[0] as Tuple<ChannelDTO, PricelistDTO, ActivityDTO>;
                    MoveFromSelected(tuple);
                }
            }
        }

        // Used to find the correct position of element in collection
        private int FindIndex(IEnumerable<ChannelDTO> ChannelList, ChannelDTO channelDTO)
        {
            int index = ChannelList.Count();
            for (int i = 0; i < ChannelList.Count(); i++)
            {
                if (channelDTO.chname.CompareTo(ChannelList.ElementAt(i).chname) < 0)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        // On row double click, moves clicked item to Channel list
        private async void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            var tuple = row.Item as Tuple<ChannelDTO, PricelistDTO, ActivityDTO>;
            //MoveFromSelected(tuple);
            if (tuple != null)
            {
                var pricelist = tuple.Item2;
                await UpdatePricelist(pricelist);
            }
        }

        #endregion

        #region Selection Changed

        private async void lvChannels_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            lvChannels.SelectedItems.Clear();

        }

        private async void lvChannels_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            if (lvChannels.SelectedItems.Count > 0)
            {
                ChannelDTO channel = lvChannels.SelectedItem as ChannelDTO;
                if (channel != null)
                {
                    lbSelectedChannels.Items.Add(channel);
                    ChannelList.Remove(channel);
                }

            }

            await RefreshPricelists();

        }

        private async Task RefreshPricelists()
        {
            if (PricelistList == null)
                return;
            PricelistList.Clear();
            // Making lists of integers for faster transition of elements
            List<int> plids = new List<int>();
            List<int> chids = new List<int>();

            // taking plids from AllPricelistsList because this elements already shows 
            // only pricelists available for current client
            foreach (var pricelist in AllPricelistsList)
            {
                // for adding only id-s of pricelists withing Date range
                if (onlyActive)
                {
                    if (TimeFormat.YMDStringToDateTime(pricelist.valfrom.ToString().Trim()).AddDays(-1) > DateTime.Now ||
                        TimeFormat.YMDStringToDateTime(pricelist.valto.ToString().Trim()).AddDays(1) < DateTime.Now)
                    {
                        continue;
                    }
                    else
                    {
                        plids.Add(pricelist.plid);
                    }
                }
                else
                {
                    plids.Add(pricelist.plid);
                }
            }
            foreach (ChannelDTO chid in lbSelectedChannels.Items)
            {
                chids.Add(chid.chid);
            }

            // Getting the right pricelistChannels ids which should be displayed
            if (plids.Count == 0)
                return;

            var plIds = await _pricelistChannelsController.GetIntersectedPlIds(plids, chids);

            // Clearing and filling with the available pricelists for selected channels
            // == 0 because when more channels are selected, only one should modify lvPricelists
            int selectedIndex = 0;
            List<PricelistDTO> pricelists = new List<PricelistDTO>();

            foreach (int plid in plIds)
            {
                PricelistDTO pricelist = await _pricelistController.GetPricelistById(plid);
                pricelists.Add(pricelist);
            }

            /*PricelistComparer comparer = new PricelistComparer();
            pricelists.Sort((a, b) => comparer.Compare(a, b));*/
            pricelists = pricelists.OrderBy(p => p.clid != 0).ThenBy(p => p.plname).ToList();

            foreach (var pricelist in pricelists)
            {
                PricelistList.Add(pricelist);
            }
        }

        // In order to select ListView on mouse right click
        private void lvPricelists_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListViewItem listViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (listViewItem != null)
            {
                listViewItem.Focus();
                listViewItem.IsSelected = true;
                lvPricelists.ContextMenu = lvPricelists.Resources["PricelistContext"] as System.Windows.Controls.ContextMenu;
                e.Handled = true;
            }
        }
        static ListViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is ListViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as ListViewItem;
        }

        private async Task UpdatePricelist(PricelistDTO pricelist = null)
        {
            var f = _factoryPriceList.Create();
            await f.Initialize(_campaign, pricelist);
            f.ShowDialog();
            if (f.pricelistChanged || f.sectableModified || f.seasonalityModified)
            {
                _allPricelistsList = ((await _pricelistController.GetAllClientPricelists(_client.clid))).ToList<PricelistDTO>();
                //lvChannels_SelectionChanged(lvChannels, null);
                await RefreshPricelists();

                for (int i = 0; i < Selected.Count(); i++)
                {
                    Tuple<ChannelDTO, PricelistDTO, ActivityDTO> tuple = Selected[i];
                    if (tuple.Item2.plid == f._pricelist.plid)
                    {
                        Tuple<ChannelDTO, PricelistDTO, ActivityDTO> newTuple = Tuple.Create(tuple.Item1, f._pricelist, tuple.Item3);
                        Selected.RemoveAt(i);
                        Selected.Insert(i, newTuple);
                        pricelistChanged = true;
                        break;
                    }
                }

                if (f._pricelist != null)
                {
                    PricelistUpdatedEvent?.Invoke(this, new UpdatePricelistEventArgs(f._pricelist));
                }
            }
           
          
        }

        private async void lvPricelists_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (MainWindow.user.usrlevel == 2)
            {
                return;
            }

            PricelistDTO pricelist = null;
            if (lvPricelists.SelectedItems.Count > 0)
                pricelist = lvPricelists.SelectedItem as PricelistDTO;

            await UpdatePricelist(pricelist);
            /*var f = _factoryPriceList.Create();
            if (lvPricelists.SelectedItems.Count > 0)
                await f.Initialize(_campaign, lvPricelists.SelectedItem as PricelistDTO);
            else
                await f.Initialize(_campaign);
            f.ShowDialog();
            if (f.pricelistChanged || f.sectableModified || f.seasonalityModified)
            {
                _allPricelistsList = ((await _pricelistController.GetAllClientPricelists(_client.clid))).ToList<PricelistDTO>();
                //lvChannels_SelectionChanged(lvChannels, null);
                await RefreshPricelists();
                pricelistChanged = true;
                if (f._pricelist != null)
                {
                    PricelistUpdatedEvent?.Invoke(this, new UpdatePricelistEventArgs(f._pricelist));
                }
            }*/
        }

        #endregion

        #region Pricelist
        private async void btnEditPricelist_Click(object sender, RoutedEventArgs e)
        {
            btnEditPricelist.IsEnabled = false;

            PricelistDTO pricelist = null;
            if (lvPricelists.SelectedItems.Count > 0)
                pricelist = lvPricelists.SelectedItem as PricelistDTO;

            await UpdatePricelist(pricelist);

            /*var f = _factoryPriceList.Create();
            if (lvPricelists.SelectedItems.Count > 0)
                await f.Initialize(_campaign, lvPricelists.SelectedItem as PricelistDTO);
            else
                await f.Initialize(_campaign);
            f.ShowDialog();
            if (f.pricelistChanged)
            {
                _allPricelistsList = ((await _pricelistController.GetAllClientPricelists(_client.clid))).ToList<PricelistDTO>();                
                await RefreshPricelists();
                if (f._pricelist == null)
                    return;
                for (int i= 0; i<Selected.Count(); i++)
                {
                    Tuple<ChannelDTO, PricelistDTO, ActivityDTO> tuple = Selected[i];
                    if (tuple.Item2.plid == f._pricelist.plid)
                    {
                        Tuple<ChannelDTO, PricelistDTO, ActivityDTO> newTuple = Tuple.Create(tuple.Item1, f._pricelist, tuple.Item3);
                        Selected.RemoveAt(i);
                        Selected.Insert(i, newTuple);
                        pricelistChanged = true;
                        break;
                    }
                }

                if (f._pricelist != null)
                {
                    PricelistUpdatedEvent?.Invoke(this, new UpdatePricelistEventArgs(f._pricelist));
                }
            }*/
            btnEditPricelist.IsEnabled = true;

        }
        private async void btnNewPricelist_Click(object sender, RoutedEventArgs e)
        {
            btnNewPricelist.IsEnabled = false;

            var f = _factoryPriceList.Create();
            await f.Initialize(_campaign);
            f.ShowDialog();
            if (f.pricelistChanged)
            {
                _allPricelistsList = (List<PricelistDTO>)(await _pricelistController.GetAllClientPricelists(_client.clid));
                //lvChannels_SelectionChanged(lvChannels, null);
                await RefreshPricelists();
                pricelistChanged = true;
            }
            btnNewPricelist.IsEnabled = true;

        }
        private async void chbActivePricelists_Checked(object sender, RoutedEventArgs e)
        {
            onlyActive = true;
            //lvChannels_SelectionChanged(lvChannels, null);
            await RefreshPricelists();

        }

        private async void chbActivePricelists_Unchecked(object sender, RoutedEventArgs e)
        {
            onlyActive = false;
            //lvChannels_SelectionChanged(lvChannels, null);
            await RefreshPricelists();

        }
        #endregion

        #region Channel Groups

        private async Task FillChannelGroups()
        {
            fGroupChannels = _factoryGroupChannels.Create();
            await fGroupChannels.Initialize(_client, _campaign);
            lbChannelGroups.ItemsSource = fGroupChannels.ChannelGroupList;
        }
        private async void btnEditChannelGroups_Click(object sender, RoutedEventArgs e)
        {
            btnEditChannelGroups.IsEnabled = false;
            var f = _factoryGroupChannels.Create();
            await f.Initialize(_client, _campaign);

            f.ShowDialog();
            if (f.channelsModified)
            {
                await FillChannelGroups();
            }
            btnEditChannelGroups.IsEnabled = true;
        }

        private async void lbChannelGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbChannelGroups.SelectedItems.Count == 0)
            {
                ChannelList.Clear();
                foreach (ChannelDTO channel in AllChannelList)
                {
                    ChannelList.Add(channel);
                }
            }
            else if (e.AddedItems.Count > 0)
            {
                // Assuring that the only one element is selected
                ListBox listBox = sender as ListBox;
                var valid = e.AddedItems[0];
                foreach (var item in new ArrayList(listBox.SelectedItems))
                {
                    if (item != valid)
                    {
                        listBox.SelectedItems.Remove(item);
                    }
                }

                //Implementation of logic
                ChannelGroupDTO channelGroup = lbChannelGroups.SelectedItems[0] as ChannelGroupDTO;
                int chgrid = channelGroup.chgrid;
                ChannelList.Clear();
                var f = _factoryGroupChannels.Create();
                await f.Initialize(_client, _campaign);
                var tuples = f.Assigned.Where(t => t.Item2.chgrid == chgrid);
                for (int i = 0; i < AllChannelList.Count; i++)
                {
                    ChannelDTO channel = AllChannelList[i] as ChannelDTO;
                    if (tuples.Any(t => t.Item1.chid == channel.chid))
                    {
                        ChannelList.Add(channel);
                        continue;
                    }
                }


            }
            // Remove channels which are in dgSelected and lbSelectedChannels
            foreach (Tuple<ChannelDTO, PricelistDTO, ActivityDTO> tuple in dgSelected.Items)
            {
                ChannelDTO chn = tuple.Item1 as ChannelDTO;
                for (int i = 0; i < ChannelList.Count; i++)
                {
                    ChannelDTO channel = ChannelList[i];
                    if (chn.chid == channel.chid)
                    {
                        ChannelList.RemoveAt(i);
                        i = ChannelList.Count; // in order to step out of loop
                    }
                }
            }

            foreach (ChannelDTO chn in lbSelectedChannels.Items)
            {
                for (int i = 0; i < ChannelList.Count; i++)
                {
                    ChannelDTO channel = ChannelList[i];
                    if (chn.chid == channel.chid)
                    {
                        ChannelList.RemoveAt(i);
                        i = ChannelList.Count; // in order to step out of loop
                    }
                }

            }

        }

        #endregion


        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            btnSave.IsEnabled = false;
            if (channelsModified)
            {
                SelectedChannels = Selected.ToList();
                await UpdateDatabase(SelectedChannels);
                if (channelsToAdd.Count() > 0 || channelsToDelete.Count() > 0)
                {
                    updateChannels = true;
                }
            }
            else
            {
                await CheckIfPositionChanged();
            }
            btnSave.IsEnabled = true;

            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            channelsModified = false;
            this.Close();
        }

        private async Task CheckIfPositionChanged()
        {
            var channelCmps = (await _channelCmpController.GetChannelCmpsByCmpid(_campaign.cmpid)).ToList();

            for (int i = 0; i < Selected.Count; i++)
            {
                var channel = Selected[i].Item1;
                var channelCmp = channelCmps.FirstOrDefault(cmpchn => cmpchn.chid == channel.chid);
                if (channelCmp == null)
                {
                    continue;
                }
                if (channelCmp.pos != i)
                {
                    channelsOrderChanged = true;
                    await _channelCmpController.UpdateChannelCmp(
                        new UpdateChannelCmpDTO(channelCmp.cmpid, channelCmp.chid, channelCmp.plid,
                            channelCmp.actid, channelCmp.plidbuy, channelCmp.actidbuy, i));
                }

            }
        }     

        public async Task UpdateDatabase(List<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> channelList)
        {
            await _channelCmpController.DeleteChannelCmpByCmpid(_campaign.cmpid);

            for (int i=0; i<SelectedChannels.Count; i++)
            {
                var channel = SelectedChannels[i];
                CreateChannelCmpDTO channelCmp = new CreateChannelCmpDTO
                    (_campaign.cmpid, channel.Item1.chid, channel.Item2.plid, channel.Item3.actid, -1, -1, i);
                await _channelCmpController.CreateChannelCmp(channelCmp);
            }
        }

        #region Context Menu
        private async void miDeletePricelist_Click(object sender, RoutedEventArgs e)
        {
            PricelistDTO pricelist = lvPricelists.SelectedItem as PricelistDTO;
            if (pricelist == null)
                return;
            if (!await CanPricelistBeDeleted(pricelist.plid))
            {
                MessageBox.Show("Cannot delete pricelist because it's in use!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                await _pricelistChannelsController.DeleteAllPricelistChannelsByPlid(pricelist.plid);
                await _pricelistController.DeletePricelistById(pricelist.plid);
                PricelistList.Remove(pricelist);
                channelsModified = true;
            }

        }

        private async Task<bool> CanPricelistBeDeleted(int plid)
        {
            bool isInUse = await _pricelistController.IsPricelistInUse(plid);
            bool isInSelected = Selected.Where(tuple => tuple.Item2.plid == plid).Count() > 0;
            return !isInUse && !isInSelected;
        }

        #endregion

        private async void lbSelectedChannels_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lbSelectedChannels.SelectedItems.Count < 1)
                return;

            var channel = lbSelectedChannels.SelectedItems[0] as ChannelDTO;
            if (channel != null)
            {
                lbSelectedChannels.Items.Remove(channel);
                ChannelList.Add(channel);
            }
            await RefreshPricelists();
        }

        public class PricelistComparer : IComparer<PricelistDTO>
        {
            public int Compare(PricelistDTO x, PricelistDTO y)
            {
                // First, compare the clid values
                int clidComparison = x.clid.CompareTo(y.clid);

                // If clid values are not equal, sort by clid in ascending order
                if (clidComparison != 0)
                {
                    return clidComparison;
                }

                // If clid values are equal, check the condition p.clid != 0
                bool xClidNotZero = x.clid != 0;
                bool yClidNotZero = y.clid != 0;

                // If both have clid != 0 or both have clid == 0, sort by valfrom in descending order
                if (xClidNotZero == yClidNotZero)
                {
                    return y.valfrom.CompareTo(x.valfrom);
                }

                // Sort by clid != 0 in descending order
                return yClidNotZero.CompareTo(xClidNotZero);
            }
        }

        // Overriding OnClosing because we need to close all opened windows
        protected override void OnClosing(CancelEventArgs e)
        {

            fGroupChannels.shouldClose = true;
            fGroupChannels.Close();
            
        }

    }
}
