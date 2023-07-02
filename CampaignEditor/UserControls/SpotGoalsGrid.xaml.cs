using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.SpotDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Grid which have multiple headers and is used in showing goals of campaign 
    /// </summary>
    public partial class SpotGoalsGrid : UserControl
    {

        public MediaPlanController _mediaPlanController { get; set; }
        public MediaPlanTermController _mediaPlanTermController { get; set; }
        public SpotController _spotController { get; set; }
        public ChannelController _channelController { get; set; }

        // for checking if certain character can be written in spot cells
        List<SpotDTO> _spots = new List<SpotDTO>();
        List<ChannelDTO> _channels = new List<ChannelDTO>();

        public ObservableCollection<MediaPlanTuple> _allMediaPlans =
            new ObservableCollection<MediaPlanTuple>();

        CampaignDTO _campaign;

        // for duration of campaign
        DateTime startDate;
        DateTime endDate;

        public SpotGoalsGrid()
        {
            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign)
        {

            _campaign = campaign;

            startDate = TimeFormat.YMDStringToDateTime(_campaign.cmpsdate);
            endDate = TimeFormat.YMDStringToDateTime(_campaign.cmpedate);

            var spots = await _spotController.GetSpotsByCmpid(_campaign.cmpid);
            foreach (SpotDTO spot in spots)
            {
                _spots.Add(spot);
            }
            _spots = _spots.OrderBy(s => s.spotcode).ToList();

            var channelIds = await _mediaPlanController.GetAllChannelsByCmpid(_campaign.cmpid);
            List<ChannelDTO> channels = new List<ChannelDTO>();
            foreach (var chid in channelIds)
            {
                ChannelDTO channel = await _channelController.GetChannelById(chid);
                channels.Add(channel);
            }
            _channels = channels.OrderBy(c => c.chname).ToList();

            CreateOutboundHeaders();
            SetWidth();

        }

        private void CreateOutboundHeaders()
        {
            var firstWeekNum = GetWeekOfYear(startDate);
            var lastWeekNum = GetWeekOfYear(endDate);

            // Add headers
            // Weeks
            ugWeeks.Columns = lastWeekNum - firstWeekNum + 1;
            for (int i = firstWeekNum; i <= lastWeekNum; i++)
            {

                Border border = new Border();
                border.BorderBrush = Brushes.Black;
                border.Background = Brushes.LightGoldenrodYellow;
                if (i == firstWeekNum)
                {
                    border.BorderThickness = new Thickness(3, 3, 3, 1);
                }
                else
                {
                    border.BorderThickness = new Thickness(1, 3, 3, 1);
                }
                TextBlock textBlock = new TextBlock();

                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.Text = "Week " + i.ToString();

                border.Child = textBlock;

                ugWeeks.Children.Add(border);
            }

            //Channels
            ugChannels.Columns = _channels.Count * ugWeeks.Columns;
            for (int j = 0; j < ugWeeks.Columns; j++)
            {
                for (int i = 0; i < _channels.Count; i++)
                {

                    Border border = new Border();
                    border.BorderBrush = Brushes.Black;
                    border.Background = Brushes.LightGoldenrodYellow;
                    if (i == 0 && j == 0)
                    {
                        border.BorderThickness = new Thickness(3, 1, 3, 1);
                    }
                    else
                    {
                        border.BorderThickness = new Thickness(1, 1, 3, 1);
                    }
                    TextBlock textBlock = new TextBlock();

                    textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.Text = _channels[i].chname.Trim();
                    border.Child = textBlock;

                    ugChannels.Children.Add(border);

                }
            }


            //Goals
            ugGoals.Columns = ugChannels.Columns * 3;
            for (int j=0; j<ugChannels.Columns; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    Border border = new Border();
                    border.BorderBrush = Brushes.Black;
                    border.Background = Brushes.LightGoldenrodYellow;
                    if (i == 0 && j == 0)
                    {
                        border.BorderThickness = new Thickness(3, 1, 3, 1);
                    }
                    else
                    {
                        border.BorderThickness = new Thickness(1, 1, 3, 1);
                    }
                    TextBlock textBlock = new TextBlock();

                    textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    switch (i % 3)
                    {
                        case 0:
                            textBlock.Text = "INS";
                            break;
                        case 1:
                            textBlock.Text = "GRP";
                            break;
                        case 2:
                            textBlock.Text = "LEN";
                            break;
                    }
                    border.Child = textBlock;
                    ugGoals.Children.Add(border);
                }
            }       

            //Spot
            ugSpots.Rows = _spots.Count();
            for (int i = 0; i < _spots.Count; i++)
            {
                Border border = new Border();
                border.BorderBrush = Brushes.Black;
                border.Background = Brushes.LightGoldenrodYellow;
                border.BorderThickness = new Thickness(3, 3, 3, 3);
                TextBlock textBlock = new TextBlock();

                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.Text = _spots[i].spotcode + ": " + _spots[i].spotname.Trim();

                border.Child = textBlock;

                ugSpots.Children.Add(border);
                
            }

        }
        private int GetWeekOfYear(DateTime date)
        {
            System.Globalization.Calendar calendar = CultureInfo.CurrentCulture.Calendar;
            return calendar.GetWeekOfYear(
                date,
                CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
                CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek
            );
        }

        private void SetWidth()
        {
            double weekWidth = 200;
            int weeksNum = ugWeeks.Children.Count;
            double headerWidth = weeksNum * weekWidth;

            ugWeeks.Width = headerWidth;
            ugChannels.Width = headerWidth;
            ugGoals.Width = headerWidth;
        }
    }
}
