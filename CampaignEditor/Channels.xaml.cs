using CampaignEditor.Controllers;
using Database.DTOs.ActivityDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.PricelistDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CampaignEditor
{
    public partial class Channels : Window
    {
        private ClientDTO _client;

        private ChannelController _channelController;
        private PricelistController _pricelistController;
        private PricelistChannelsController _pricelistChannelsController;
        private ActivityController _activityController;

        private ObservableCollection<ChannelDTO> _channelList;
        private List<PricelistDTO> _allPricelistsList;
        private ObservableCollection<PricelistDTO> _pricelistList;
        private ObservableCollection<ActivityDTO> _activityList;
        private ObservableCollection<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> _selected = 
                                new ObservableCollection<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>>();
        public ObservableCollection<ChannelDTO> ChannelList
        {
            get { return _channelList;  }
            set { _channelList = value; }
        }
        public ObservableCollection<PricelistDTO> PricelistList
        {
            get { return _pricelistList;  }
            set { _pricelistList = value; }
        }
        public List<PricelistDTO> AllPricelistsList
        {
            get { return _allPricelistsList; }
            set { _allPricelistsList = value; }
        }
        public ObservableCollection<ActivityDTO> ActivityList
        {
            get { return _activityList;  }
            set { _activityList = value; }
        }
        public ObservableCollection<Tuple<ChannelDTO, PricelistDTO, ActivityDTO>> Selected
        {
            get { return _selected;  }
            set { _selected = value; }
        }
        public Channels(IChannelRepository channelRepository, IPricelistRepository pricelistRepository,
            IPricelistChannelsRepository pricelistChannelsRepository, IActivityRepository activityRepository)
        {
            this.DataContext = this;
            _channelController = new ChannelController(channelRepository);
            _pricelistController = new PricelistController(pricelistRepository);
            _pricelistChannelsController = new PricelistChannelsController(pricelistChannelsRepository);
            _activityController = new ActivityController(activityRepository);

            InitializeComponent();
            
            _ =  FillLists();
        }

        public void Initialize(ClientDTO client)
        {
            this._client = client;
        }

        private async Task FillLists()
        {
            // Initializing and sorting lists, so it don't have to be sorted later
            _channelList = new ObservableCollection<ChannelDTO>((await _channelController.GetAllChannels()).OrderBy(c => c.chname)); //_client.clid
            _allPricelistsList = (List<PricelistDTO>)(await _pricelistController.GetAllClientPricelists(22));
            _pricelistList = new ObservableCollection<PricelistDTO>((AllPricelistsList).OrderBy(p => p.plname));
            _activityList = new ObservableCollection<ActivityDTO>((await _activityController.GetAllActivities()).OrderBy(a => a.act));

            // Binding to ListViews
            lvChannels.ItemsSource = ChannelList;
            lvPricelists.ItemsSource = PricelistList;
            lvActivities.ItemsSource = ActivityList;

            dgSelected.ItemsSource = Selected;
        }

        #region ToSelected and FromSelected Buttons
        private void btnToSelected_Click(object sender, RoutedEventArgs e)
        {
            // At least one item from every listView needs to be selected to execute
            if (lvChannels.SelectedItems.Count > 0 &&
                lvPricelists.SelectedItems.Count > 0 &&
                lvActivities.SelectedItems.Count > 0) 
            {
                int n = lvChannels.SelectedItems.Count;
                var channels = lvChannels.SelectedItems;
                for(int i=0; i<n; i++)
                {
                    var channel = channels[0]; // 0 because we need n iterations, and in each we remove one item
                    ChannelDTO chn = channel as ChannelDTO;
                    PricelistDTO pl = lvPricelists.SelectedItem as PricelistDTO;
                    ActivityDTO act = lvActivities.SelectedItem as ActivityDTO;
                    var list = Selected.Select(t => t.Item1).ToList(); // making a list to pass to function FindIndex
                    int index = FindIndex(list, chn);
                    Selected.Insert(index, Tuple.Create(chn, pl, act)!);
                    ChannelList.Remove(chn);
                }
            }
        }

        private void btnFromSelected_Click(object sender, RoutedEventArgs e)
        {
            int n = dgSelected.SelectedItems.Count;
            if (n > 0)
            {
                var selectedItems = dgSelected.SelectedItems;
                for (int i=0; i<n; i++)
                {
                    var selectedItem = selectedItems[0]; // 0 because we need n iterations, and in each we remove one item
                    var item = selectedItem as Tuple<ChannelDTO, PricelistDTO, ActivityDTO>;
                    var channel = item.Item1;
                    Selected.Remove(item);
                    int index = FindIndex(ChannelList, channel);
                    ChannelList.Insert(index, channel);
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


        #endregion

        private async void lvChannels_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var br = AllPricelistsList;

            List<int> plids = new List<int>();
            List<int> chids = new List<int>();

            foreach (var plid in AllPricelistsList)
            {
                plids.Add(plid.plid);
            }
            foreach(ChannelDTO chid in lvChannels.SelectedItems){
                chids.Add(chid.chid);
            }

            var plIds = await _pricelistChannelsController.GetIntersectedPlIds(plids, chids);

            PricelistList.Clear();
            foreach (var plid in plIds)
            {
                PricelistList.Add(await _pricelistController.GetPricelistById(plid));
            }

        }
    }
}
