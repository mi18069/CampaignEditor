﻿using Database.DTOs.ChannelDTO;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ClientDTO;
using System.Collections.ObjectModel;
using Database.Repositories;
using Database.DTOs.ChannelGroupDTO;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Media;
using Database.DTOs.ChannelGroupsDTO;

namespace CampaignEditor
{
    public partial class GroupChannels : Window
    {
        private ClientDTO _client;
        private CampaignDTO _campaign;

        private ChannelController _channelController;
        private ChannelGroupController _channelGroupController;
        private ChannelGroupsController _channelGroupsController;

        private ObservableCollection<ChannelDTO> _channelList = new ObservableCollection<ChannelDTO>();
        private ObservableCollection<ChannelGroupDTO> _channelGroupList = new ObservableCollection<ChannelGroupDTO>();
        private ObservableCollection<Tuple<ChannelDTO, ChannelGroupDTO>> _assigned =
                                new ObservableCollection<Tuple<ChannelDTO, ChannelGroupDTO>>();

        private bool channelsModified = false;

        private string tbNewChGrStr = "New Channel Group";
        private bool emptyTb = true;

        #region Getters and Setters for lists
        private ObservableCollection<ChannelDTO> ChannelList
        {
            get { return _channelList; }
            set { _channelList = value; }
        }
        private ObservableCollection<ChannelGroupDTO> ChannelGroupList
        {
            get { return _channelGroupList; }
            set { _channelGroupList = value; }
        }

        private ObservableCollection<Tuple<ChannelDTO, ChannelGroupDTO>> Assigned
        {
            get { return _assigned; }
            set { _assigned = value; }
        }

        #endregion
        public GroupChannels(IChannelRepository channelRepository, IChannelGroupRepository channelGroupRepository,
            IChannelGroupsRepository channelGroupsRepository)
        {
            this.DataContext = this;
            _channelController = new ChannelController(channelRepository);
            _channelGroupController = new ChannelGroupController(channelGroupRepository);
            _channelGroupsController = new ChannelGroupsController(channelGroupsRepository);

            InitializeComponent();
            emptyTb = true;
        }

        public async Task Initialize(ClientDTO client, CampaignDTO campaign)
        {
            _client = client;
            _campaign = campaign;


            var channelList = await _channelController.GetAllChannels();
            channelList = channelList.OrderBy(c => c.chname);
            foreach ( var channel in channelList )
            {
                ChannelList.Add(channel);
            }

            var channelGroups = await _channelGroupController.GetAllOwnerChannelGroups(_client.clid);
            channelGroups = channelGroups.OrderBy(c => c.chgrname);
            foreach ( var channelGroup in channelGroups)
            {
                ChannelGroupList.Add(channelGroup);
            }

            foreach (var channel in channelList ) 
            { 
                foreach (var channelGroup in channelGroups)
                {
                    if (await _channelGroupsController.GetChannelGroupsByIds(channelGroup.chgrid, channel.chid) != null)
                    {
                        MoveToAssigned(channel, channelGroup);
                    }
                }
            }

            lvChannels.ItemsSource = ChannelList;
            lvChannelGroups.ItemsSource = ChannelGroupList;
            dgAssigned.ItemsSource = Assigned;
        }

        #region From and To Assigned
        private void MoveToAssigned(ChannelDTO channel, ChannelGroupDTO channelGroup)
        {
            var list = Assigned.Select(t => t.Item2).ToList(); // making a list to pass to function FindIndex
            int index = FindAssignedIndex(list, channelGroup); // Finding the right index, to keep the list sorted
            Assigned.Insert(index, Tuple.Create(channel, channelGroup)!);
            ChannelList.Remove(channel);
            channelsModified = true;
        }
        private void MoveFromAssigned(Tuple<ChannelDTO, ChannelGroupDTO> tuple)
        {
            var channel = tuple.Item1;
            Assigned.Remove(tuple);
            int index = FindIndex(ChannelList, channel);
            ChannelList.Insert(index, channel);
            channelsModified = true;
        }

        private void btnToAssigned_Click(object sender, RoutedEventArgs e)
        {
            // At least one item from every listView needs to be selected to execute
            if (lvChannels.SelectedItems.Count > 0 &&
                lvChannelGroups.SelectedItems.Count > 0)
            {
                int n = lvChannels.SelectedItems.Count;
                var channels = lvChannels.SelectedItems;
                for (int i = 0; i < n; i++)
                {
                    ChannelDTO channel = channels[0] as ChannelDTO; // 0 because we need n iterations, and in each we remove one item
                    ChannelGroupDTO channelGroup = lvChannelGroups.SelectedItem as ChannelGroupDTO;
                    MoveToAssigned(channel, channelGroup);
                }
            }
        }

        private void btnFromAssigned_Click(object sender, RoutedEventArgs e)
        {
            int n = dgAssigned.SelectedItems.Count;
            if (n > 0)
            {
                var selectedItems = dgAssigned.SelectedItems;
                for (int i = 0; i < n; i++)
                {
                    // 0 because we need n iterations, and in each we remove one item
                    var tuple = selectedItems[0] as Tuple<ChannelDTO, ChannelGroupDTO>;
                    MoveFromAssigned(tuple);
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

        private int FindAssignedIndex(IEnumerable<ChannelGroupDTO> ChannelGroupList, ChannelGroupDTO channelGroupDTO)
        {
            int index = Assigned.Count();
            for (int i = 0; i < Assigned.Count(); i++)
            {
                if (channelGroupDTO.chgrname.CompareTo(Assigned.ElementAt(i).Item2.chgrname) < 0)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            var tuple = row.Item as Tuple<ChannelDTO, ChannelGroupDTO>;
            MoveFromAssigned(tuple);
        }

        #endregion


        #region TextBox mechanism

        private void tbNewGroup_GotFocus(object sender, RoutedEventArgs e)
        {
            if (emptyTb)
            {
                tbNewGroup.Text = "";
                tbNewGroup.Foreground = Brushes.Black;
                emptyTb = false;
            }
        }

        private void tbNewGroup_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbNewGroup.Text == "" && emptyTb == false)
            {
                emptyTb = true;
                tbNewGroup.Foreground = Brushes.Gray;
                tbNewGroup.Text = tbNewChGrStr;
            }
        }

        private async void btnNewChhGr_Click(object sender, RoutedEventArgs e)
        {
            string name = tbNewGroup.Text.Trim();
            if (name != "" &&
                await _channelGroupController.GetChannelGroupByNameAndOwner(name, _client.clid) == null)
            {
                ChannelGroupDTO chGroup = await _channelGroupController.CreateChannelGroup(new CreateChannelGroupDTO(name, _client.clid));
                ChannelGroupList.Add(chGroup);

                emptyTb = true;
                tbNewGroup.Foreground = Brushes.Gray;
                tbNewGroup.Text = tbNewChGrStr;
            }
            else
            {
                if (name == "")
                    MessageBox.Show("Enter name");
                else
                    MessageBox.Show("Name already exists");
            }
        }

        #endregion

        #region Save and Cancel

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (channelsModified)
            {
                foreach (ChannelGroupDTO channelGroup in ChannelGroupList)
                {
                    await _channelGroupsController.DeleteChannelGroupsByChgrid(channelGroup.chgrid);
                }
                foreach (var tuple in Assigned)
                {
                    await _channelGroupsController.CreateChannelGroups(
                        new CreateChannelGroupsDTO(tuple.Item2.chgrid, tuple.Item1.chid));
                }
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        // Overriding OnClosing because click on x button should only hide window
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private async void lvChannelGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvChannelGroups.SelectedItems.Count > 0)
            {
                var item = lvChannelGroups.SelectedItems[0] as ChannelGroupDTO;
                int chgr = item.chgrid;
                var channelGroups = await _channelGroupsController.GetAllChannelGroupsByChgrid(chgr);

                if (channelGroups.Count() == 0)
                {
                    btnDeleteChhGr.IsEnabled = true;
                }
                else
                {
                    btnDeleteChhGr.IsEnabled = false;
                }
            }
            else
            {
                btnDeleteChhGr.IsEnabled = false;
            }
        }

        private async void btnDeleteChhGr_Click(object sender, RoutedEventArgs e)
        {
            var item = lvChannelGroups.SelectedItems[0] as ChannelGroupDTO;
            await _channelGroupController.DeleteChannelGroupById(item.chgrid);
            ChannelGroupList.Remove(item);
        }
    }
}
