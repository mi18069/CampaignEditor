using CampaignEditor.Controllers;
using CampaignEditor.Helpers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CampaignEditor.UserControls.ValidationItems
{
    /// <summary>
    /// Two stacks within the same ScrollViewer
    /// </summary>
    public partial class ValidationStack : UserControl
    {

        private CampaignDTO _campaign;
        private int _campaignVersion;

        public List<ChannelDTO> _channels = new List<ChannelDTO>();
        public List<DateOnly> _dates = new List<DateOnly>();

        private List<Tuple<DateOnly, List<TermTuple>>> _dayTuples = new List<Tuple<DateOnly, List<TermTuple>>>();

        public ChannelCmpController _channelCmpController;
        public ChannelController _channelController;
        public MediaPlanVersionController _mediaPlanVersionController;

        public MediaPlanController _mediaPlanController;
        public MediaPlanTermController _mediaPlanTermController;
        public SpotController _spotController;
        public MediaPlanConverter _mpConverter;
        public MediaPlanForecastData _forecastData;
        public ObservableRangeCollection<MediaPlanTuple> _allMediaPlans;

        public ValidationStack()
        {
            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;
            var mpVersion = await _mediaPlanVersionController.GetLatestMediaPlanVersion(campaign.cmpid);
            if (mpVersion != null)
            {
                int version = mpVersion.version;
                _campaignVersion = version;

            }

        }

        public async Task LoadData(int chid)
        {
            List<TermTuple> termTuples = new List<TermTuple>();

            //var mediaPlans = await _mediaPlanController.GetAllChannelCmpMediaPlans(chid, _campaign.cmpid, _campaignVersion);
            var mediaPlanTuples = _allMediaPlans.Where(mpt => mpt.MediaPlan.chid == chid);
            foreach (var mediaPlanTuple in mediaPlanTuples)
            {
                //var mediaPlanTerms = await _mediaPlanTermController.GetAllNotNullMediaPlanTermsByXmpid(mediaPlan.xmpid);
                var mediaPlanTerms = mediaPlanTuple.Terms.Where(t => t != null && t.Spotcode != null);
                foreach (var mediaPlanTerm in mediaPlanTerms)
                {
                    foreach (char spotcode in mediaPlanTerm!.Spotcode!.Trim())
                    {
                        var spot = _forecastData.SpotcodeSpotDict[spotcode];
                        // It's better to have info from allMediaPlans
                        var mediaPlan = mediaPlanTuple.MediaPlan;
                        var price = _mpConverter.GetProgramSpotPrice(mediaPlan, mediaPlanTerm, spot);
                        TermTuple termTuple = new TermTuple(mediaPlan, mediaPlanTerm, spot, price);
                        termTuples.Add(termTuple);
                    }
                   
                }
            }

            _dayTuples.Clear();

            termTuples = termTuples.OrderBy(tt=>tt.MediaPlanTerm.Date).ThenBy(tt => tt.MediaPlan.stime).ToList();
            foreach (DateOnly date in _dates)
            {
                List<TermTuple> dayTuples = termTuples.Where(tt => tt.MediaPlanTerm.Date == date).ToList();
                _dayTuples.Add(Tuple.Create(date, dayTuples));
            }

            AddIntoExpected(_dayTuples);
        }

        private void AddIntoExpected(List<Tuple<DateOnly, List<TermTuple>>> dayTuples)
        {
            spExpected.Children.Clear();

            foreach (var tuple in dayTuples)
            {
                /*if (tuple.Item2.Count == 0)
                    continue;*/

                string date = tuple.Item1.ToString();
                DateLabelItem dli = new DateLabelItem(date);

                spExpected.Children.Add(dli);

                foreach (var termTuple in tuple.Item2)
                {
                    VTermItem vTermItem = new VTermItem();
                    vTermItem.Initialize(termTuple);

                    spExpected.Children.Add(vTermItem);
                }

            }
        }
    }
}
