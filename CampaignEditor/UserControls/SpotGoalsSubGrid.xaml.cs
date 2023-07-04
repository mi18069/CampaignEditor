using CampaignEditor.Controllers;
using Database.DTOs.ChannelDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.SpotDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// This is part of dataGrid which contains only datas for one channel and week
    /// </summary>
    public partial class SpotGoalsSubGrid : UserControl
    {

        private Dictionary<Char, SpotGoals> _dictionary = new Dictionary<Char, SpotGoals>();
        private IEnumerable<MediaPlanTuple> _mpTuples;
        private Dictionary<Char, int> _spotLengths = new Dictionary<char, int>();

        public Dictionary<Char, SpotGoals> Dict
        {
            get { return _dictionary; }
        }

        public SpotGoalsSubGrid(IEnumerable<SpotDTO> spots, IEnumerable<MediaPlanTuple> mpTuples)
        {
            InitializeComponent();
            _mpTuples = mpTuples;
            foreach (var spot in spots)
            {
                _dictionary.Add(spot.spotcode[0], new SpotGoals());
                _spotLengths.Add(spot.spotcode[0], spot.spotlength);
            }

            CalculateProperties();

            dgGrid.ItemsSource = _dictionary.Values;
        }

        private void CalculateProperties()
        {
            foreach (var mpTuple in _mpTuples)
            {
                var mediaPlan = mpTuple.MediaPlan;
                foreach (var mpTerm in mpTuple.Terms)
                {
                    var spotcodes = mpTerm.spotcode;

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

        }
    }
}
