using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using Database.Repositories;
using System.Threading.Tasks;
using System.Windows;
using System;

namespace CampaignEditor
{
    public class CampaignManipulations
    {
        private CampaignController _campaignController;
        private MediaPlanRefController _mediaPlanRefController;
        private CmpBrndController _cmpBrndController;
        private GoalsController _goalsController;
        private TargetCmpController _targetCmpController;
        private SpotController _spotController;
        private ChannelCmpController _channelCmpController;
        private ClientCoefsController _clientProgcoefController;
        private CobrandController _cobrandController;
        private SpotPairController _spotPairController;
        private MediaPlanRealizedController _mediaPlanRealizedController;

        public CampaignManipulations(ICampaignRepository campaignRepository,
            IMediaPlanRefRepository mediaPlanRefRepository,
            ICmpBrndRepository cmpBrndRepository, IGoalsRepository goalsRepository,
            ITargetCmpRepository targetCmpRepository, ISpotRepository spotRepository,
            IChannelCmpRepository channelCmpRepository, IClientCoefsRepository clientProgCoefRepository,
            ICobrandRepository cobrandRepository, ISpotPairRepository spotPairRepository,
            IMediaPlanRealizedRepository mediaPlanRealizedRepository)
        {
            _campaignController = new CampaignController(campaignRepository);
            _mediaPlanRefController = new MediaPlanRefController(mediaPlanRefRepository);
            _cmpBrndController = new CmpBrndController(cmpBrndRepository);
            _goalsController = new GoalsController(goalsRepository);
            _targetCmpController = new TargetCmpController(targetCmpRepository);
            _spotController = new SpotController(spotRepository);
            _channelCmpController = new ChannelCmpController(channelCmpRepository);
            _clientProgcoefController = new ClientCoefsController(clientProgCoefRepository);
            _cobrandController = new CobrandController(cobrandRepository);
            _spotPairController = new SpotPairController(spotPairRepository);
            _mediaPlanRealizedController = new MediaPlanRealizedController(mediaPlanRealizedRepository);    
        }

        public async Task<bool> DuplicateCampaign(CampaignDTO oldCampaign, CampaignDTO newCampaign)
        {
            try
            {
                // Duplicate connections with campaign
                await _channelCmpController.DuplicateChannelCmp(oldCampaign.cmpid, newCampaign.cmpid);
                await _spotController.DuplicateSpot(oldCampaign.cmpid, newCampaign.cmpid);
                await _targetCmpController.DuplicateTargetCmp(oldCampaign.cmpid, newCampaign.cmpid);
                await _goalsController.DuplicateGoals(oldCampaign.cmpid, newCampaign.cmpid);
                // This shouldn't be done since it's done while saving campaign
                //await _cmpBrndController.DuplicateCmpBrnd(oldCampaign.cmpid, newCampaign.cmpid);

                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while duplicating campaign!\n" + ex.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteCampaign(CampaignDTO campaign)
        {
            // If campaign is active, ask to set to inactive
            bool isCampaignActive = campaign.active;
            if (isCampaignActive)
            {
                if (MessageBox.Show("Cannot delete active campaign.\nDo you want to set it to inactive and delete?", "Question",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    try
                    {
                        campaign.active = false;
                        await _campaignController.UpdateCampaign(new UpdateCampaignDTO(campaign));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Cannot set campaign to inactive", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            // If campaign is inactive and doesn't have initialized mediaPlan, delete it
            var mediaPlanRef = await _mediaPlanRefController.GetMediaPlanRef(campaign.cmpid);
            // If campaign is inactive and have initialized MediaPlan, but hasn't started, delete it
            if (TimeFormat.YMDStringToDateTime(campaign.cmpsdate) >= DateTime.Now)
            {
                return await DeleteCampaignFromBase(campaign);
            }
            // Else campaign can't be deleted
            else
            {
                MessageBox.Show("Cannot delete campaigns that are initialized and started!", "Message",
                    MessageBoxButton.OK, MessageBoxImage.Stop);
                return false;
            }

        }

        private async Task<bool> DeleteCampaignFromBase(CampaignDTO campaign)
        {


            try
            {
                // Deleting Forecast
                await _campaignController.DeleteCampaignInitialization(campaign.cmpid);
                await _mediaPlanRefController.DeleteMediaPlanRefById(campaign.cmpid);
                //await _clientProgcoefController.DeleteClientCoefsByClientId(campaign.clid);

                // Deleting Validation
                await _mediaPlanRealizedController.DeleteMediaPlanRealizedForCampaign(campaign.cmpid);
                await _spotPairController.DeleteAllCampaignSpotPairs(campaign.cmpid);

                // Deleting connections with campaign
                await _channelCmpController.DeleteChannelCmpByCmpid(campaign.cmpid);
                await _spotController.DeleteSpotsByCmpid(campaign.cmpid);
                await _targetCmpController.DeleteTargetCmpByCmpid(campaign.cmpid);
                await _goalsController.DeleteGoalsByCmpid(campaign.cmpid);
                await _cmpBrndController.DeleteBrandByCmpId(campaign.cmpid);
                await _cobrandController.DeleteCampaignCobrands(campaign.cmpid);

                // Deleting campaign
                await _campaignController.DeleteCampaignById(campaign.cmpid);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while deleting campaign!\n" + ex.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;

        }

    }
}
