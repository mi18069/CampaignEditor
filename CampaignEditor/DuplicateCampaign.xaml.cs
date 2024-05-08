using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using Database.Repositories;
using System.Threading.Tasks;
using System.Windows;

namespace CampaignEditor
{
    /// <summary>
    /// With given campaign, we'll make another campaign with it's informations
    /// </summary>
    public partial class DuplicateCampaign : Window
    {
        private CampaignController _campaignController;
        private CmpBrndController _cmpBrndController;

        private GoalsController _goalsController;
        private SpotController _spotController;
        private ChannelCmpController _channelCmpController;
        private TargetCmpController _targetCmpController;

        /*private MediaPlanRefController _mediaPlanRefController;
        private MediaPlanController _mediaPlanController;
        private MediaPlanTermController _mediaPlanTermController;
        private MediaPlanHistController _mediaPlanHistController;*/

        private CampaignDTO _campaign;
        private CampaignDTO _newCampaign;


        public DuplicateCampaign(
            ICampaignRepository campaignRepository,
            ICmpBrndRepository cmpBrndRepository,
            IGoalsRepository goalsRepository,
            ISpotRepository spotRepository,
            IChannelCmpRepository channelCmpRepository,
            ITargetCmpRepository targetCmpRepository)
        {
            InitializeComponent();

            _campaignController = new CampaignController(campaignRepository);
            _cmpBrndController = new CmpBrndController(cmpBrndRepository);

            _goalsController = new GoalsController(goalsRepository);
            _spotController = new SpotController(spotRepository);
            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _targetCmpController = new TargetCmpController(targetCmpRepository);
        }

        public async void Initialize(string campaignName)
        {
            _campaign = await _campaignController.GetCampaignByName(campaignName);

            tbDuplicateName.Text = campaignName.Trim();
            dpFromDate.SelectedDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            dpToDate.SelectedDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CheckValues())
            {
                try
                {
                    await CopyCampaign();
                    await CopyOverviewInfos();
                }
                catch
                {
                    MessageBox.Show("An error occured", "Result", MessageBoxButton.OK, MessageBoxImage.Error);
                    DeleteValues();
                }
            }
        }

        private async void DeleteValues()
        {
            await _channelCmpController.DeleteChannelCmpByCmpid(_newCampaign.cmpid);
            await _spotController.DeleteSpotsByCmpid(_newCampaign.cmpid);
            await _targetCmpController.DeleteTargetCmpByCmpid(_newCampaign.cmpid);
            await _goalsController.DeleteGoalsByCmpid(_newCampaign.cmpid);
            await _cmpBrndController.DeleteBrandByCmpId(_newCampaign.cmpid);
            await _campaignController.DeleteCampaignById(_newCampaign.cmpid);
        }

        private async Task CopyOverviewInfos()
        {
            // Copy Goals
            var goals = await _goalsController.GetGoalsByCmpid(_campaign.cmpid);
            if (goals != null)
            {
                goals.cmpid = _newCampaign.cmpid;
                await _goalsController.CreateGoals(new Database.DTOs.GoalsDTO.CreateGoalsDTO(goals));
            }

            // Copy Targets
            var targetCmps = await _targetCmpController.GetTargetCmpByCmpid(_campaign.cmpid);
            foreach (var targetCmp in targetCmps)
            {
                targetCmp.cmpid = _newCampaign.cmpid;
                await _targetCmpController.CreateTargetCmp(new Database.DTOs.TargetCmpDTO.CreateTargetCmpDTO(targetCmp));
            }

            // Copy Spots
            var spots = await _spotController.GetSpotsByCmpid(_campaign.cmpid);
            foreach (var spot in spots)
            {
                spot.cmpid = _newCampaign.cmpid;
                await _spotController.CreateSpot(new Database.DTOs.SpotDTO.CreateSpotDTO(spot));
            }

            // Copy Channels
            var channelCmps = await _channelCmpController.GetChannelCmpsByCmpid(_campaign.cmpid);
            foreach (var channelCmp in channelCmps)
            {
                channelCmp.cmpid = _newCampaign.cmpid;
                await _channelCmpController.CreateChannelCmp(new Database.DTOs.ChannelCmpDTO.CreateChannelCmpDTO(channelCmp));
            }
        }

        private async Task CopyCampaign()
        {
            _campaign.cmpsdate = TimeFormat.DPToYMDInt(dpFromDate)!.ToString();
            _campaign.cmpedate = TimeFormat.DPToYMDInt(dpToDate)!.ToString();
            _campaign.cmpname = tbNewName.Text.Trim();

            _newCampaign = await _campaignController.CreateCampaign(new CreateCampaignDTO(_campaign));
            var brands = await _cmpBrndController.GetCmpBrndsByCmpId(_campaign.cmpid);
            foreach (var brand in brands)
            {
                brand.cmpid = _newCampaign.cmpid;
                await _cmpBrndController.CreateCmpBrnd(brand);
            }
        }

        private bool CheckValues()
        {
            bool valid = true;
            if (tbDuplicateName.Text.Trim().Length == 0)
            {
                valid = false;
                MessageBox.Show("Invalid name of new campaign", "Result", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (dpFromDate.SelectedDate == null || dpToDate.SelectedDate == null || 
                dpFromDate.SelectedDate >= dpToDate.SelectedDate)
            {
                valid = false;
                MessageBox.Show("Invalid date values", "Result", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return valid;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
