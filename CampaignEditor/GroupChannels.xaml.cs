using Database.DTOs.ChannelDTO;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ClientDTO;
using System.Collections.ObjectModel;
using Database.Repositories;
using Database.DTOs.ChannelGroupDTO;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Media;
using Database.DTOs.ChannelGroupsDTO;
using System.Windows.Data;
using Database.Entities;

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

        public bool channelsModified = false;
        public bool shouldClose = false;

        private string tbNewChGrStr = "Enter new group name";
        private bool emptyTb = true;

        #region Getters and Setters for lists
        private ObservableCollection<ChannelDTO> ChannelList
        {
            get { return _channelList; }
            set { _channelList = value; }
        }
        public ObservableCollection<ChannelGroupDTO> ChannelGroupList
        {
            get { return _channelGroupList; }
            set { _channelGroupList = value; }
        }

        public ObservableCollection<Tuple<ChannelDTO, ChannelGroupDTO>> Assigned
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

        }

        public async Task Initialize(ClientDTO client, CampaignDTO campaign)
        {
            _client = client;
            _campaign = campaign;

            tbNewGroup.Text = tbNewChGrStr;
            emptyTb = true;

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

            lvAssigned.ItemsSource = Assigned;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvAssigned.ItemsSource);
            // Add GroupDescription
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Item2.chgrname");
            view.GroupDescriptions.Add(groupDescription);

            // Add SortDescription for groups
            SortDescription groupSortDescription = new SortDescription("Item2.chgrname", ListSortDirection.Ascending);
            view.SortDescriptions.Add(groupSortDescription);

            // Add SortDescription
            SortDescription sortDescription = new SortDescription("Item1.chname", ListSortDirection.Ascending);
            view.SortDescriptions.Add(sortDescription);

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

        private async void btnToAssigned_Click(object sender, RoutedEventArgs e)
        {
            // At least one item from every listView needs to be selected to execute
            if (lvChannels.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select at least one Channel", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else if (lvChannelGroups.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select Channel Group", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                int n = lvChannels.SelectedItems.Count;
                var channels = lvChannels.SelectedItems;
                for (int i = 0; i < n; i++)
                {
                    ChannelDTO channel = channels[0] as ChannelDTO; // 0 because we need n iterations, and in each we remove one item
                    ChannelGroupDTO channelGroup = lvChannelGroups.SelectedItem as ChannelGroupDTO;
                    MoveToAssigned(channel, channelGroup);
                    await AddChannelToGroupDatabase(channel.chid, channelGroup.chgrid);
                }
            }
        }

        private async void btnFromAssigned_Click(object sender, RoutedEventArgs e)
        {
            int n = lvAssigned.SelectedItems.Count;
            if (n == 0)
            {
                MessageBox.Show("Select at least one Channel", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                var selectedItems = lvAssigned.SelectedItems;
                for (int i = 0; i < n; i++)
                {
                    // 0 because we need n iterations, and in each we remove one item
                    var tuple = selectedItems[0] as Tuple<ChannelDTO, ChannelGroupDTO>;
                    MoveFromAssigned(tuple);
                    await RemoveChannelFromGroupDatabase(tuple.Item1.chid, tuple.Item2.chgrid);
                }
            }
        }
        private async Task AddChannelToGroupDatabase(int chid, int chgrid)
        {
            await _channelGroupsController.CreateChannelGroups(
                        new CreateChannelGroupsDTO(chgrid, chid));
        }

        private async Task RemoveChannelFromGroupDatabase(int chid, int chgrid)
        {
            await _channelGroupsController.DeleteChannelGroupsByIds(chgrid, chid);
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

        private async void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DependencyObject originalSource = (DependencyObject)e.OriginalSource;
            ListViewItem clickedItem = FindAncestor<ListViewItem>(originalSource);

            if (clickedItem != null)
            {
                try
                {
                    if (clickedItem.DataContext is Tuple<ChannelDTO, ChannelGroupDTO> clickedTuple)
                    {
                        MoveFromAssigned(clickedTuple);
                        await RemoveChannelFromGroupDatabase(clickedTuple.Item1.chid, clickedTuple.Item2.chgrid);
                    }
                }
                catch
                {
                    return;
                }
            }

        }

        private static T FindAncestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);

            return null;
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

        private async void btnNewChGr_Click(object sender, RoutedEventArgs e)
        {
            string name = tbNewGroup.Text.Trim();
            if (String.Equals(name.Trim(), tbNewChGrStr))
            {
                MessageBox.Show("Enter name of channel group", "Message", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        private void lvChannelGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvChannelGroups.SelectedItems.Count == 1)
            {
                btnDeleteChGr.IsEnabled = true;
            }
            else
            {
                btnDeleteChGr.IsEnabled = false;
            }
 
        }

        private async Task DeleteChGr(ChannelGroupDTO channelGroupDTO)
        {
            bool hasAssignedChannels = false;
            foreach (var tuple in Assigned)
            {
                if (tuple.Item2.chgrid == channelGroupDTO.chgrid)
                {
                    hasAssignedChannels = true;
                    break;
                }
            }

            if (hasAssignedChannels)
            {
                if (MessageBox.Show("Do you want to delete channel group?\nIt has dedicated channels to it.", 
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var dedicatedChannels = Assigned.Where(t => t.Item2.chgrid == channelGroupDTO.chgrid).ToList();
                    int n = dedicatedChannels.Count();
                    for (int i = 0; i < n; i++)
                    {
                        var tuple = dedicatedChannels[i];
                        MoveFromAssigned(tuple);
                        await RemoveChannelFromGroupDatabase(tuple.Item1.chid, tuple.Item2.chgrid);
                    }

                    await _channelGroupController.DeleteChannelGroupById(channelGroupDTO.chgrid);
                    ChannelGroupList.Remove(channelGroupDTO);
                }
            }

        }

        private async void btnDeleteChGr_Click(object sender, RoutedEventArgs e)
        {
            var item = lvChannelGroups.SelectedItems[0] as ChannelGroupDTO;
            await DeleteChGr(item);

        }
    }
}
