using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.SpotDTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using static CampaignEditor.UserControls.SpotGoalsGrid;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Data;
using Database.Entities;
using System.Reflection.Emit;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// This grid is very similar to SpotGoalsGrid, so I'll pass it some SpotGoalsGrid, and make
    /// this grid based on that, on that way, there's no need to calculate same things more than once
    /// </summary>
    public partial class SpotWeekGoalsGrid : UserControl
    {

        CampaignDTO _campaign;
        // for duration of campaign
        DateTime startDate;
        DateTime endDate;

        List<SpotDTO> _spots = new List<SpotDTO>();
        List<ChannelDTO> _channels = new List<ChannelDTO>();

        public SpotWeekGoalsGrid()
        {
            InitializeComponent();
        }

        public void Initialize(SpotGoalsGrid sgGrid)
        {
            _campaign = sgGrid.Campaign;

            startDate = sgGrid.StartDate;
            endDate = sgGrid.EndDate;

            var spots = sgGrid.Spots;
            foreach (SpotDTO spot in spots)
            {
                _spots.Add(spot);
            }

            var channels = sgGrid.Channels;
            foreach (ChannelDTO channel in channels)
            {
                _channels.Add(channel);

            }

            CreateOutboundHeaders();
            AddGrid(sgGrid.ugGrid);
            SetWidth();

        }

        private void CreateOutboundHeaders()
        {
            int firstWeekNum = GetWeekOfYear(startDate);
            int lastWeekNum = GetWeekOfYear(endDate);

            // Add headers
            // Weeks
            ugWeeks.Rows = lastWeekNum - firstWeekNum + 1 + 1; // last + 1 is for Header Total
            for (int i = firstWeekNum; i <= lastWeekNum; i++)
            {

                Border border = new Border();
                border.BorderBrush = Brushes.Black;
                border.Background = Brushes.LightGoldenrodYellow;
                if (i == firstWeekNum)
                {
                    border.BorderThickness = new Thickness(1, 3, 1, 1);
                }
                else
                {
                    border.BorderThickness = new Thickness(1, 1, 1, 1);
                }
                TextBlock textBlock = new TextBlock();

                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.FontWeight = FontWeights.Bold;
                textBlock.Text = "Week " + i.ToString();

                border.Child = textBlock;

                ugWeeks.Children.Add(border);
            }

            AddWeeksHeaderTotal();

            // Spots
            for (int i = firstWeekNum; i < lastWeekNum + 1 + 1; i++) // +1 because of Total row 
            {
                for (int j=0; j<_spots.Count; j++)
                {
                    Border border = new Border();
                    border.BorderBrush = Brushes.Black;
                    border.Background = Brushes.LightGoldenrodYellow;
                    if (j == _spots.Count - 1)
                    {
                        border.BorderThickness = new Thickness(1, 1, 1, 2);
                    }
                    else
                    {
                        border.BorderThickness = new Thickness(1, 1, 1, 1);
                    }
                    TextBlock textBlock = new TextBlock();

                    textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.FontWeight = FontWeights.Bold;

                    SpotDTO spot = _spots[j];
                    string label = spot.spotcode.Trim() + ": " + spot.spotname.Trim();
                    textBlock.Text = label;

                    border.Child = textBlock;

                    ugSpots.Children.Add(border);
                }

                /*foreach (SpotDTO spot in _spots)
                {
                    string label = spot.spotcode.Trim() + ": " + spot.spotname.Trim();
                    dgSpots.Items.Add(new SpotLabel { Label = label });           
                }*/
            }

           

            //Channels
            ugChannels.Columns = (_channels.Count + 1);

            for (int i = 0; i < _channels.Count + 1; i++)
            {

                Border border = new Border();
                border.BorderBrush = Brushes.Black;
                if (i == _channels.Count)
                {
                    border.Background = Brushes.Yellow;
                }
                else
                {
                    border.Background = Brushes.LightGoldenrodYellow;
                }

                if (i == 0)
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

                if (i == _channels.Count)
                {
                    textBlock.Text = "Total";
                }
                else
                {
                    textBlock.Text = _channels[i].chname.Trim();
                }

                border.Child = textBlock;

                ugChannels.Children.Add(border);

            }



            //Goals
            ugGoals.Columns = ugChannels.Columns * 3;
            for (int j = 0; j < ugChannels.Columns; j++)
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
                            textBlock.Text = "BUD";
                            break;
                    }
                    border.Child = textBlock;
                    ugGoals.Children.Add(border);
                }
            }

        }
        

        private void AddGrid(UniformGrid grid)
        {
            // Set num of Columns
            int nCol = ugChannels.Children.Count;
            ugGrid.Columns = nCol;

            // Iterate through the child elements in the UniformGrid
            foreach (var child in grid.Children)
            {
                if (child is SpotGoalsSubGrid sg)
                {
                   ugGrid.Children.Add(new SpotGoalsSubGrid(sg));
                }
                else if (child is SpotGoalsTotalSubGrid tsg)
                {
                    ugGrid.Children.Add(new SpotGoalsTotalSubGrid(tsg));
                }
                else
                {
                    continue;
                }
            }
        }

        private void AddWeeksHeaderTotal()
        {
            Border border = new Border();
            border.BorderBrush = Brushes.Black;
            border.Background = Brushes.Yellow;

            border.BorderThickness = new Thickness(1, 2, 1, 1);

            TextBlock textBlock = new TextBlock();

            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.Text = "Total";

            border.Child = textBlock;

            ugWeeks.Children.Add(border);
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
            int channelsNum = ugChannels.Children.Count;

            int channelWidth = 150;
            int headerWidth = channelsNum * channelWidth;


            ugChannels.Width = headerWidth;
            ugGoals.Width = headerWidth;
            ugGrid.Width = headerWidth;

            //double spotRowHeight = ugWeeks.Height / dgSpots.Items.Count;
            //dgSpots.RowHeight = spotRowHeight + 20;
            //ugGrid.Height = dgSpots.Height;
            //ugWeeks.Height = ugGrid.Height;
        }
    }
}
