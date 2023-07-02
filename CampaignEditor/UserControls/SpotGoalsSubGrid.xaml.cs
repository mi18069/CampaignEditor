using Database.DTOs.ChannelDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.SpotDTO;
using Database.Entities;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// This is part of dataGrid which contains only datas for one channel and week
    /// </summary>
    public partial class SpotGoalsSubGrid : UserControl
    {

        private List<MediaPlanTuple> _mpTuples;
        private List<SpotDTO> _spots;
        public SpotGoalsSubGrid()
        {
            InitializeComponent();
        }

        public void Initialize(List<SpotDTO> spots, List<MediaPlanTuple> mpTuples)
        {
            _spots = spots;
            _mpTuples = mpTuples;
        }

    }
}
