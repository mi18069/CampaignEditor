﻿using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ClientDTO;
using Database.DTOs.TargetDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CampaignEditor
{

    public partial class AddCampaign : Window
    {

        private CampaignController _campaignController;
        private TargetController _targetController;
        private ClientController _clientController;

        private CampaignDTO campaign;
        private ClientDTO client;

        public AddCampaign(ICampaignRepository campaignRepository, ITargetRepository targetRepository, 
            IClientRepository clientRepository)
        {
            _campaignController = new CampaignController(campaignRepository);
            _targetController = new TargetController(targetRepository);
            _clientController = new ClientController(clientRepository);
            InitializeComponent();            

        }

        public async void InitializeFields(string campaignName)
        {
            campaign = await _campaignController.GetCampaignByName(campaignName);
            client = await _clientController.GetClientById(campaign.clid);

            tbName.Text = campaignName;
            lblClientname.Content = client.clname;

            dpStartDate.SelectedDate = ConvertStringToDate(campaign.cmpsdate);
            dpEndDate.SelectedDate = ConvertStringToDate(campaign.cmpedate);

            FillTBTextBoxes();
            FillTargetsComboBox();
        }

        private void FillTBTextBoxes()
        {
            tbTbStartHours.Text = campaign.cmpstime[0].ToString() + campaign.cmpstime[1].ToString();
            tbTbStartMinutes.Text = campaign.cmpstime[3].ToString() + campaign.cmpstime[4].ToString();

            tbTbEndHours.Text = campaign.cmpetime[0].ToString() + campaign.cmpetime[1].ToString();
            tbTbEndMinutes.Text = campaign.cmpetime[3].ToString() + campaign.cmpetime[4].ToString();
        }

        private async void FillTargetsComboBox()
        {
            IEnumerable<TargetDTO> targets = await _targetController.GetAllTargets();
            targets = targets.OrderBy(t => t.targname);
            cbTargets.DisplayMemberPath = "targname";
            foreach (var target in targets)
            {
                cbTargets.Items.Add(target);
            }
        }

        #region Converters
        private string ConvertDateTimeToDateString(DateTime dateTime)
        {
            string year = dateTime.Year.ToString("0000");
            string month = dateTime.Month.ToString("00");
            string day = dateTime.Day.ToString("00");

            return year + month + day;
        }
        private string ConvertDateTimeToTimeString(DateTime dateTime)
        {
            string hour = dateTime.Hour.ToString("00");
            string minute = dateTime.Minute.ToString("00");
            string second = dateTime.Second.ToString("00");

            return hour + minute + second;
        }

        private DateTime ConvertStringToDate(string timeString)
        {
            int year = Convert.ToInt32(timeString[0].ToString() + timeString[1].ToString() + timeString[2].ToString() + timeString[3].ToString());
            int month = Convert.ToInt32(timeString[4].ToString() + timeString[5].ToString());
            int day = Convert.ToInt32(timeString[6].ToString() + timeString[7].ToString());

            return new DateTime(year, month, day);
        }

        #endregion

        private void TxtBoxes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).MaxLength == ((TextBox)sender).Text.Length)
            {
                // move focus to the next
                var ue = e.OriginalSource as FrameworkElement;
                e.Handled = true;
                ue.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cbTargets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTargets.SelectedIndex != -1)
            {
                var target = cbTargets.SelectedItem as TargetDTO;
                lblTargetDescription.Content = target.targdesc;
            }
        }
    }
}
