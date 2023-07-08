using CampaignEditor.Helpers;
using Database.DTOs.SpotDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// This is part of dataGrid which contains only datas for one channel and week
    /// </summary>
    public partial class SpotGoalsSubGrid : UserControl
    {

        private Dictionary<Char, SpotGoals> _dictionary = new Dictionary<char, SpotGoals>();
        private ObservableRangeCollection<SpotGoals> _values = new ObservableRangeCollection<SpotGoals>();
        private ObservableCollection<MediaPlanTuple> _mpTuples;
        private Dictionary<Char, int> _spotLengths = new Dictionary<char, int>();

        public Dictionary<Char, SpotGoals> Dict
        {
            get { return _dictionary; }
        }

        public ObservableCollection<SpotGoals> Values
        {
            get { return _values; }
        }

        public SpotGoalsSubGrid(IEnumerable<SpotDTO> spots, ObservableCollection<MediaPlanTuple> mpTuples)
        {
            InitializeComponent();
            _mpTuples = mpTuples;

            foreach (var spot in spots)
            {
                _dictionary.Add(spot.spotcode[0], new SpotGoals());
                _spotLengths.Add(spot.spotcode[0], spot.spotlength);
            }
            _dictionary.OrderBy(kv => kv.Key);
            CalculateGoals();
            SubscribeToMediaPlanTerms();
            dgGrid.ItemsSource = _dictionary.Values;
        }

        private void CalculateGoals()
        {
            ResetDictionaryValues();
            foreach (var mpTuple in _mpTuples)
            {
                var mediaPlan = mpTuple.MediaPlan;
                foreach (var mpTerm in mpTuple.Terms)
                {
                    var spotcodes = mpTerm.Spotcode;

                    if (spotcodes != null && spotcodes.Length > 0)
                    {
                        foreach (char spotcode in spotcodes)
                        {
                            if (spotcode != ' ')
                            {
                                _dictionary[spotcode].Insertations += 1;
                                _dictionary[spotcode].Budget += (mediaPlan.Price / mediaPlan.Length) * _spotLengths[spotcode];
                                _dictionary[spotcode].Grp += mediaPlan.Amrp1 + mediaPlan.Amrp2 + mediaPlan.Amrp3;
                            }
                            
                        }
                    }
                }
            }
            _values.ReplaceRange(_dictionary.Values);
            dgGrid.ItemsSource = _values;

        }

        private void ResetDictionaryValues()
        {
            foreach (SpotGoals spotGoal in _dictionary.Values)
            {
                spotGoal.Grp = 0;
                spotGoal.Insertations = 0;
                spotGoal.Budget = 0;
            }
        }


        public void SubscribeToMediaPlanTerms()
        {
            foreach (MediaPlanTuple mediaPlanTuple in _mpTuples)
            {
                foreach (MediaPlanTerm mpTerm in mediaPlanTuple.Terms)
                {
                    mpTerm.PropertyChanged += MediaPlanTerm_PropertyChanged;
                }             
            }
        }

        private void MediaPlanTerm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Handle the changes in the MediaPlanTerm attributes here
            CalculateGoals();

        }

    }
}
