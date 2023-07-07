using CampaignEditor.Helpers;
using Database.DTOs.ChannelDTO;
using Database.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Interaction logic for ChannelsGoalsGrid.xaml
    /// </summary>
    public partial class ChannelsGoalsGrid : UserControl
    {
        Dictionary<int, ProgramGoals> _dictionary = new Dictionary<int, ProgramGoals>();
        ObservableRangeCollection<ProgramGoals> _values = new ObservableRangeCollection<ProgramGoals>();
        ObservableCollection<MediaPlan> _mediaPlans;

        public ChannelsGoalsGrid()
        {
            InitializeComponent();         
        }

        public void Initialize(ObservableCollection<MediaPlan> mediaPlans, List<ChannelDTO> channels)
        {
            _mediaPlans = mediaPlans;
            foreach (ChannelDTO channel in channels)
            {
                _dictionary.Add(channel.chid, new ProgramGoals(channel));
            }

            CalculateGoals();
            SubscribeToMediaPlans();
        }

        private void CalculateGoals()
        {
            ResetDictionaryValues();
            foreach (MediaPlan mediaPlan in _mediaPlans)
            {
                int chid = mediaPlan.chid;
                _dictionary[chid].Insertations += mediaPlan.Insertations;
                _dictionary[chid].Grp += mediaPlan.Insertations * (mediaPlan.Amrp1 + mediaPlan.Amrp2 + mediaPlan.Amrp3);
                _dictionary[chid].Budget += mediaPlan.price;
            }

            _values.ReplaceRange(_dictionary.Values);
            dgGrid.ItemsSource = _values;
        }

        private void RecalculateGoals(int chid)
        {
            ResetDictionaryValues(chid);
            var mediaPlans = _mediaPlans.Where(mp => mp.chid == chid);
            foreach (MediaPlan mediaPlan in mediaPlans)
            {
                _dictionary[chid].Insertations += mediaPlan.Insertations;
                _dictionary[chid].Grp += mediaPlan.Insertations * (mediaPlan.Amrp1 + mediaPlan.Amrp2 + mediaPlan.Amrp3);
                _dictionary[chid].Budget += mediaPlan.price;           
            }
            _values.ReplaceRange(_dictionary.Values);
            dgGrid.ItemsSource = _values;
        }

        private void ResetDictionaryValues(int chid = -1)
        {
            if (chid == -1)
            {
                foreach (ProgramGoals programGoal in _dictionary.Values)
                {
                    programGoal.Grp = 0;
                    programGoal.Insertations = 0;
                    programGoal.Budget = 0;
                }
            }
            else
            {
                _dictionary[chid].Budget = 0;
                _dictionary[chid].Grp = 0;
                _dictionary[chid].Insertations = 0;
            }
        }

        public void SubscribeToMediaPlans()
        {
            foreach (MediaPlan mediaPlan in _mediaPlans)
            { 
                mediaPlan.PropertyChanged += MediaPlan_PropertyChanged;
            }
        }

        private void MediaPlan_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Handle the changes in the MediaPlanTerm attributes here
            var mp = sender as MediaPlan;
            if (mp != null)
            {
                RecalculateGoals(mp.chid);
            }

        }
    }
}
