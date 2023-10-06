using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.Repositories;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CampaignEditor
{
    /// <summary>
    /// Page in which we'll graphically see times of reserved and realized ads
    /// </summary>
    public partial class CampaignValidation : Page
    {

        private ChannelCmpController _channelCmpController;
        private ChannelController _channelController;
        private MediaPlanVersionController _mediaPlanVersionController;
        private MediaPlanController _mediaPlanController;
        private MediaPlanTermController _mediaPlanTermController;
        private SpotController _spotController;

        public CampaignValidation(
            IChannelCmpRepository channelCmpRepository,
            IChannelRepository channelRepository,
            IMediaPlanVersionRepository mediaPlanVersionRepository,
            IMediaPlanRepository mediaPlanRepository,
            IMediaPlanTermRepository mediaPlanTermRepository,
            ISpotRepository spotRepository)
        {
            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _channelController = new ChannelController(channelRepository);
            _mediaPlanVersionController = new MediaPlanVersionController(mediaPlanVersionRepository);

            _mediaPlanController = new MediaPlanController(mediaPlanRepository);
            _mediaPlanTermController = new MediaPlanTermController(mediaPlanTermRepository);
            _spotController = new SpotController(spotRepository);

            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign)
        {
            rulerExpected._channelCmpController = _channelCmpController;
            rulerExpected._channelController = _channelController;
            rulerExpected._mediaPlanVersionController = _mediaPlanVersionController;
            rulerExpected._mediaPlanController = _mediaPlanController;
            rulerExpected._mediaPlanTermController = _mediaPlanTermController;
            rulerExpected._spotController = _spotController;

            await rulerExpected.Initialize(campaign);
        }
    }
}
