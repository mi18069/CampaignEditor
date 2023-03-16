﻿using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ClientDTO;
using Database.Entities;
using Database.Repositories;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CampaignEditor
{
    public partial class CmpInfo : Window
    {
        private CampaignDTO _campaign = null;
        private ClientDTO _client = null;

        private CampaignController _campaignController;

        public bool infoModified = false;
        public CampaignDTO Campaign {
            get { return _campaign; }
            set { _campaign = value; }
            }
        public CmpInfo(ICampaignRepository campaignRepository)
        {
            _campaignController = new CampaignController(campaignRepository);

            InitializeComponent();
        }

        public void Initialize(ClientDTO client, CampaignDTO campaign = null)
        {
            _campaign = campaign;
            _client = client;

            if (campaign != null)
            {
                cbActive.IsChecked = campaign.active;
                tbName.Text = campaign.cmpname.Trim();
                tbClientname.Text = client.clname.Trim();

                dpStartDate.SelectedDate = TimeFormat.YMDStringToDateTime(campaign.cmpsdate);
                dpEndDate.SelectedDate = TimeFormat.YMDStringToDateTime(campaign.cmpedate);

                FillTBTextBoxes();
            }
        }

        private void FillTBTextBoxes()
        {
            tbTbStartHours.Text = _campaign.cmpstime[0].ToString() + _campaign.cmpstime[1].ToString();
            tbTbStartMinutes.Text = _campaign.cmpstime[3].ToString() + _campaign.cmpstime[4].ToString();

            tbTbEndHours.Text = _campaign.cmpetime[0].ToString() + _campaign.cmpetime[1].ToString();
            tbTbEndMinutes.Text = _campaign.cmpetime[3].ToString() + _campaign.cmpetime[4].ToString();
        }

        private void TextBoxes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).MaxLength == ((TextBox)sender).Text.Length)
            {
                // move focus to the next
                var ue = e.OriginalSource as FrameworkElement;
                e.Handled = true;
                ue.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
            infoModified = true;
        }

        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            infoModified = true;
        }

        private void dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            infoModified = true;
        }

        private void cb_Checked(object sender, RoutedEventArgs e)
        {
            infoModified = true;
        }

        #region Save and Cancel buttons
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (infoModified)
            {
                int cmpid = Campaign.cmpid;
                int cmprev = 0;
                int cmpown = 1; // Don't know what this is
                string cmpname = tbName.Text.Trim();
                int clid = _client.clid;
                string cmpsdate = TimeFormat.DPToYMDString(dpStartDate);
                string cmpedate = TimeFormat.DPToYMDString(dpEndDate);
                string cmpstime = tbTbStartHours.Text.PadLeft(2, '0')+ ":" +tbTbStartMinutes.Text.PadLeft(2, '0')+":00";
                string cmpetime = tbTbEndHours.Text.PadLeft(2, '0') + ":" +tbTbEndMinutes.Text.PadLeft(2, '0') + ":59";
                int cmpstatus = 0;
                string sostring = "1;999;F;01234;012345";
                int activity = 0;
                int cmpaddedon = _campaign.cmpaddedon;
                int cmpaddedat = _campaign.cmpaddedat;
                bool active = (bool)cbActive.IsChecked;
                bool forcec = _campaign.forcec;

                Campaign = new CampaignDTO(cmpid, cmprev, cmpown, cmpname, clid, cmpsdate, cmpedate, 
                    cmpstime, cmpetime, cmpstatus, sostring, activity, cmpaddedon, cmpaddedat,
                    active, forcec);
            }

            this.Hide();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        #endregion

        public async Task UpdateDatabase(CampaignDTO campaign)
        {
            await _campaignController.UpdateCampaign(new UpdateCampaignDTO(campaign.cmpid,
                campaign.cmprev, campaign.cmpown, campaign.cmpname, campaign.clid, 
                campaign.cmpsdate, campaign.cmpedate, campaign.cmpstime, campaign.cmpetime,
                campaign.cmpstatus, campaign.sostring, campaign.activity, campaign.cmpaddedon,
                campaign.cmpaddedat, campaign.active, campaign.forcec));
        }
    }
}
