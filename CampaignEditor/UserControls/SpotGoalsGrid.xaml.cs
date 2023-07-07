using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.SpotDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        public ObservableCollection<MediaPlanTuple> _allMediaPlans;

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
            int firstWeekNum = GetWeekOfYear(startDate);
            int lastWeekNum = GetWeekOfYear(endDate);

            // Add headers
            // Weeks
            ugWeeks.Columns = lastWeekNum - firstWeekNum + 1 + 1; // last + 1 is for Header Total
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

            AddWeeksHeaderTotal();

            //Channels
            ugChannels.Columns = (_channels.Count + 1) * ugWeeks.Columns;
            for (int j = 0; j < ugWeeks.Columns; j++)
            {
                for (int i = 0; i < _channels.Count + 1; i++)
                {

                    Border border = new Border();
                    border.BorderBrush = Brushes.Black;
                    if ( i == _channels.Count || j == ugWeeks.Columns - 1 )
                    {
                        border.Background = Brushes.Yellow;
                    }
                    else
                    {
                        border.Background = Brushes.LightGoldenrodYellow;
                    }

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
                            textBlock.Text = "BUD";
                            break;
                    }
                    border.Child = textBlock;
                    ugGoals.Children.Add(border);
                }
            }

            // Grid Goals
            for (int i=0; i < ugWeeks.Children.Count; i++)
            {
                int weekNum = firstWeekNum + i;
                
                if (i == ugWeeks.Children.Count - 1)
                {
                    AddTotalSubGrids();
                    continue;
                }

                for (int j=0; j <= _channels.Count; j++)
                {
                    // Filtering mediaPlans by channels 
                    ObservableCollection<MediaPlanTuple> channelMpTuples;
                    if (j == _channels.Count)
                    {
                        ObservableCollection<ObservableCollection<SpotGoals>> valuesList = new ObservableCollection<ObservableCollection<SpotGoals>>();
                        int n = ugGrid.Children.Count;
                        for (int k=0; k<_channels.Count; k++)
                        { 
                            SpotGoalsSubGrid sg = ugGrid.Children[n-1-k] as SpotGoalsSubGrid;
                            ObservableCollection<SpotGoals> dg = sg.Values;
                            valuesList.Add(dg);
                        }
                        var totalSubGrid = new SpotGoalsTotalSubGrid(valuesList);
                        ugGrid.Children.Add(totalSubGrid);
                        continue;
                    }
                    else
                    {
                        int channelId = _channels[j].chid;
                        channelMpTuples = new ObservableCollection<MediaPlanTuple>(_allMediaPlans.Where(tuple => tuple.MediaPlan.chid == channelId));
                    }

                    var mpTuples = new ObservableCollection<MediaPlanTuple>();
                    foreach (var mpTuple in channelMpTuples)
                    {
                        var allMpTerms = mpTuple.Terms;
                        ObservableCollection<MediaPlanTerm> mpTerms;
                        
                        if (weekNum <= lastWeekNum)
                        {
                            mpTerms = new ObservableCollection<MediaPlanTerm>(mpTuple.Terms.Where(t => t != null &&
                                        GetWeekOfYear(t.Date.ToDateTime(TimeOnly.Parse("00:00 AM"))) == weekNum));
                        }
                        else
                        {
                            mpTerms = new ObservableCollection<MediaPlanTerm>(mpTuple.Terms.Where(t => t != null));
                        }
                        mpTuples.Add(new MediaPlanTuple(mpTuple.MediaPlan, mpTerms));
                    }
                    var subGrid = new SpotGoalsSubGrid(_spots, mpTuples);
                    ugGrid.Children.Add(subGrid);
                }
            }

            // Spots
            foreach (SpotDTO spot in _spots)
            {
                string label = spot.spotcode.Trim() + ": " + spot.spotname.Trim();
                dgSpots.Items.Add(new SpotLabel { Label = label });
            }
           

        }

        public class SpotLabel
        {
            public string Label { get; set; }
        }

        private void AddTotalSubGrids()
        {
            for (int j = 0; j <= _channels.Count; j++)
            {

                if (j == _channels.Count)
                {
                    ObservableCollection<ObservableCollection<SpotGoals>> totalValuesList = new ObservableCollection<ObservableCollection<SpotGoals>>();
                    int m = ugGrid.Children.Count;
                    for (int k = 0; k < _channels.Count; k++)
                    {
                        SpotGoalsTotalSubGrid tsg = ugGrid.Children[m - 1 - k] as SpotGoalsTotalSubGrid;
                        ObservableCollection<SpotGoals> totalValues = tsg.Values;
                        totalValuesList.Add(totalValues);
                    }
                    var totalsSubGrid = new SpotGoalsTotalSubGrid(totalValuesList);
                    ugGrid.Children.Add(totalsSubGrid);
                }
                else
                {
                    ObservableCollection<ObservableCollection<SpotGoals>> valuesList = new ObservableCollection<ObservableCollection<SpotGoals>>();
                    int n = ugGrid.Children.Count;

                    for (int k =  n - (_channels.Count + 1); k >= 0; k -= _channels.Count + 1)
                    {
                        SpotGoalsSubGrid sg = ugGrid.Children[k] as SpotGoalsSubGrid;
                        ObservableCollection<SpotGoals> values = sg.Values;
                        valuesList.Add(values);
                    }
                    var totalSubGrid = new SpotGoalsTotalSubGrid(valuesList);
                    ugGrid.Children.Add(totalSubGrid);
                }             
            }
        }

        private void AddWeeksHeaderTotal()
        {
            Border border = new Border();
            border.BorderBrush = Brushes.Black;
            border.Background = Brushes.Yellow;

            border.BorderThickness = new Thickness(1, 3, 3, 1);

            TextBlock textBlock = new TextBlock();

            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
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
            int weeksNum = ugWeeks.Children.Count;
            int channelsNum = ugChannels.Children.Count;

            double weekWidth = 140;
            double headerWidth = weeksNum * weekWidth * (_channels.Count + 1);

            ugWeeks.Width = headerWidth;
            ugChannels.Width = headerWidth;
            ugGoals.Width = headerWidth;
            ugGrid.Width = headerWidth;
            ugGrid.Height = dgSpots.Height;
        }
    }
}
