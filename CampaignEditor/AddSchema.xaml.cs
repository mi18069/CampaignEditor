﻿using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.EmsTypesDTO;
using Database.DTOs.SchemaDTO;
using Database.Entities;
using Database.Repositories;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CampaignEditor
{
    public partial class AddSchema : Window
    {

        private CampaignDTO _campaign;

        private ChannelController _channelController;
        private MediaPlanController _mediaPlanController;
        private EmsTypesController _emsTypesController;
        private ChannelCmpController _channelCmpController;

        public CreateSchemaDTO _schema = null;
        private ChannelDTO _selectedChannel = null;
        private List<EmsTypes> _emsTypes = new List<EmsTypes>();

        private MediaPlan _mediaPlan;
        public bool updateMediaPlan = false;
        public AddSchema(IChannelRepository channelRepository,
            IMediaPlanRepository mediaPlanRepository,
            IEmsTypesRepository emsTypesRepository,
            IChannelCmpRepository channelCmpRepository)
        {
            this.DataContext = this;
            InitializeComponent();

            _channelController = new ChannelController(channelRepository);
            _mediaPlanController = new MediaPlanController(mediaPlanRepository);
            _emsTypesController = new EmsTypesController(emsTypesRepository);
            _channelCmpController = new ChannelCmpController(channelCmpRepository);
        }

        public async Task Initialize(CampaignDTO campaign, ChannelDTO selectedChannel = null, MediaPlan mediaPlan = null)
        {
            _campaign = campaign;
            if (mediaPlan != null)
                _mediaPlan = mediaPlan;

            _selectedChannel = selectedChannel;
            await FillComboboxes();
        }

        private async Task FillComboboxes()
        {
            await FillCbChannels();
            FillCbPosition();
            await FillLbType();
            FillDate();
            FillDays();

            if (_mediaPlan != null)
            {
                FillByMP(_mediaPlan);
            }
        }

        private void FillByMP(MediaPlan mediaPlan) 
        {
            tbProgram.Text = mediaPlan.Name.Trim();
            tbTimeFrom.Text = mediaPlan.stime.ToString();
            tbTimeTo.Text = mediaPlan.etime?.ToString();
            tbBlockTime.Text = mediaPlan.blocktime?.ToString();
            tbProgCoef.Text = mediaPlan.progcoef.ToString();
        }

        private async Task FillLbType()
        {
            var types = await _emsTypesController.GetAllEmsTypesEntities();
            _emsTypes = types.ToList();
            cbType.ItemsSource = _emsTypes;
            if (_mediaPlan != null)
            {
                cbType.SelectedValue = _mediaPlan.type;
            }
        }

        private void FillCbPosition()
        {
            cbPosition.Items.Clear();
            cbPosition.Items.Add("INS");
            cbPosition.Items.Add("BET");

            if (_mediaPlan == null || _mediaPlan.position == "INS")
                cbPosition.SelectedIndex = 0;
            else
                cbPosition.SelectedIndex = 1;
        }
        private void FillDays()
        {
            lbDays.Items.Clear();

            List<Tuple<string, int>> daysTuple = new List<Tuple<string, int>>
            {
                Tuple.Create("Monday", 1),
                Tuple.Create("Tuesday", 2),
                Tuple.Create("Wednesday", 3),
                Tuple.Create("Thursday", 4),
                Tuple.Create("Friday", 5),
                Tuple.Create("Saturday", 6),
                Tuple.Create("Sunday", 7)
            };

            lbDays.ItemsSource = daysTuple;
            lbDays.DisplayMemberPath = "Item1";

            if (_mediaPlan != null)
            {
                foreach (char day in _mediaPlan.days.Trim())
                {
                    int dayNum = int.Parse(day.ToString());
                    lbDays.SelectedItems.Add(daysTuple[dayNum - 1]);
                }
            }
        }

        private void FillDate()
        {
            dpFrom.SelectedDate = DateTime.Now;
            if (_mediaPlan != null)
                dpFrom.SelectedDate = new DateTime(_mediaPlan.sdate.Year, _mediaPlan.sdate.Month, _mediaPlan.sdate.Day);
            if (_mediaPlan != null && _mediaPlan.edate.HasValue)
                dpFrom.SelectedDate = new DateTime(_mediaPlan.edate.Value.Year, _mediaPlan.edate.Value.Month, _mediaPlan.edate.Value.Day);

        }

        private async Task FillCbChannels()
        {
            cbChannels.Items.Clear();

            var channelCmpIds = await _channelCmpController.GetChannelCmpsByCmpid(_campaign.cmpid);
            List<ChannelDTO> channels = new List<ChannelDTO>();
            foreach (var chCmpid in channelCmpIds)
            {
                var chid = chCmpid.chid;
                ChannelDTO channel = await _channelController.GetChannelById(chid);
                channels.Add(channel);
            }

            channels = channels.OrderBy(c => c.chname).ToList();

            foreach (ChannelDTO channel in channels)
            {
                cbChannels.Items.Add(channel);
                if (_mediaPlan != null)
                {
                    if (_mediaPlan.chid == channel.chid)
                        cbChannels.SelectedItem = channel;
                }
                else if (_selectedChannel != null && channel.chid == _selectedChannel.chid)
                {
                    cbChannels.SelectedItem = channel;
                }
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (await CheckFields())
            {
                if (_mediaPlan == null)
                    _schema = MakeSchemaDTO();
                else
                {
                    UpdateMediaPlan();
                    updateMediaPlan = true;
                }
                this.Close();
            }
        }

        private void UpdateMediaPlan()
        {
            var channelItem = cbChannels.SelectedItem as ChannelDTO;
            int chid = channelItem.chid;
            _mediaPlan.chid = chid;

            string name = tbProgram.Text.Trim();
            _mediaPlan.Name = name;

            string position = cbPosition.Text.Trim();
            _mediaPlan.Position = position;

            string timeFrom = tbTimeFrom.Text.Trim();
            if (!timeFrom.Contains(':'))
            {
                timeFrom = timeFrom.Insert(2, ":");
            }
            _mediaPlan.Stime = timeFrom;

            string? timeTo = tbTimeTo.Text.Trim().Length > 0 ? tbTimeTo.Text.Trim() : null;
            if (timeTo != null && !timeTo.Contains(':'))
            {
                timeTo = timeTo.Insert(2, ":");
            }
            _mediaPlan.Etime = timeTo;

            string? blockTime = tbBlockTime.Text.Trim().Length > 0 ? tbBlockTime.Text.Trim() : null;
            if (blockTime != null && !blockTime.Contains(':'))
            {
                blockTime = blockTime.Insert(2, ":");
            }
            _mediaPlan.Blocktime = blockTime;

            string days = "";
            foreach (Tuple<string, int> day in lbDays.SelectedItems)
            {
                days += day.Item2.ToString();
            }
            _mediaPlan.days = days;

            string type = cbType.Text.Trim().Length > 0 ? cbType.Text.ToUpper() : "000";
            _mediaPlan.Type = type;

            bool special = (bool)chbSpecial.IsChecked;
            _mediaPlan.Special = special;

            DateOnly dateFrom = DateOnly.FromDateTime(dpFrom.SelectedDate.Value);
            _mediaPlan.sdate = dateFrom;

            DateOnly? dateTo = dpTo.SelectedDate.HasValue ? DateOnly.FromDateTime(dpTo.SelectedDate.Value) : null;
            _mediaPlan.edate = dateTo;

            float progcoef = 1.0f;
            if (tbProgCoef.Text.Trim().Length > 0 && float.TryParse(tbProgCoef.Text.Trim(), out float progCoef))
            {
                progcoef = progCoef;
            }
            _mediaPlan.Progcoef = progcoef;

        }

        private CreateSchemaDTO MakeSchemaDTO()
        {
            var channelItem = cbChannels.SelectedItem as ChannelDTO;
            int chid = channelItem.chid;
            string name = tbProgram.Text.Trim();
            string position = cbPosition.Text.Trim();
            string timeFrom = tbTimeFrom.Text.Trim();
            if (!timeFrom.Contains(':'))
            {
                timeFrom = timeFrom.Insert(2, ":");
            }
            string? timeTo = tbTimeTo.Text.Trim().Length > 0 ? tbTimeTo.Text.Trim() : null;
            if (timeTo != null && !timeTo.Contains(':'))
            {
                timeTo = timeTo.Insert(2, ":");
            }
            string? blockTime = tbBlockTime.Text.Trim().Length > 0 ? tbBlockTime.Text.Trim() : null;
            if (blockTime != null && !blockTime.Contains(':'))
            {
                blockTime = blockTime.Insert(2, ":");
            }
            string days = "";
            foreach (Tuple<string, int> day in lbDays.SelectedItems)
            {
                days += day.Item2.ToString();
            }
            string type = cbType.Text.Trim().Length > 0 ? cbType.Text.ToUpper() : "000";
            bool special = (bool)chbSpecial.IsChecked;
            DateOnly dateFrom = DateOnly.FromDateTime(dpFrom.SelectedDate.Value);
            DateOnly? dateTo = dpTo.SelectedDate.HasValue ? DateOnly.FromDateTime(dpTo.SelectedDate.Value) : null;
            float progcoef = 1.0f;
            if (tbProgCoef.Text.Trim().Length > 0 && float.TryParse(tbProgCoef.Text.Trim(), out float progCoef))
            {
                progcoef = progCoef;
            }
            DateOnly created = DateOnly.FromDateTime(DateTime.Now);

            return new CreateSchemaDTO(chid, name, position, timeFrom, timeTo, blockTime, days, type,
                special, dateFrom, dateTo, progcoef, created, null);
        }

        private async Task<bool> CheckFields()
        {
            var timeRegex1 = new Regex(@"^[0-9]{2}:[0-9]{2}$");
            var timeRegex2 = new Regex(@"^[0-9]{4}$");
            if (cbChannels.SelectedIndex == -1)
            {
                MessageBox.Show("Assign channel");
                return false;
            }
            else if (tbProgram.Text.Trim().Length == 0)
            {
                MessageBox.Show("Assign program name");
                return false;
            }
            else if (cbPosition.SelectedIndex == -1)
            {
                MessageBox.Show("Assign position");
                return false;
            }
            else if (cbType.Text.Trim().Length > 0 && await _emsTypesController.GetEmsTypesByCode(cbType.Text.Trim()) == null)
            {
                MessageBox.Show("Invalid type value");
                return false;
            }
            else if (dpTo.SelectedDate.HasValue && dpFrom.SelectedDate > dpTo.SelectedDate)
            {
                MessageBox.Show("Start date is after end date");
                return false;
            }
            else if (tbTimeFrom.Text.Trim().Length == 0)
            {
                MessageBox.Show("Assign time from");
                return false;
            }
            else if (!timeRegex1.IsMatch(tbTimeFrom.Text.Trim()) && !timeRegex2.IsMatch(tbTimeFrom.Text.Trim()))
            {
                MessageBox.Show("Invalid time from value\nPossible formats: HH:mm or HHmm");
                return false;
            }
            else if (tbTimeTo.Text.Trim().Length > 0 &&
                !timeRegex1.IsMatch(tbTimeTo.Text.Trim()) && !timeRegex2.IsMatch(tbTimeFrom.Text.Trim()))
            {
                MessageBox.Show("Invalid time from value\nPossible formats: HH:mm or HHmm");
                return false;
            }
            else if (tbBlockTime.Text.Trim().Length > 0 &&
                !timeRegex1.IsMatch(tbBlockTime.Text) && !timeRegex2.IsMatch(tbTimeFrom.Text.Trim()))
            {
                MessageBox.Show("Invalid time from value\nPossible formats: HH:mm or HHmm");
                return false;
            }
            else if (tbProgCoef.Text.Trim().Length > 0 && !float.TryParse(tbProgCoef.Text.Trim(), out _))
            {
                MessageBox.Show("Invalid Prog coef value", "Result", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (lbDays.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select at least one day");
                return false;
            }
            else
            {
                return true;
            }
        }

        private void tbTime_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null)
            {
                try
                {
                    string timeString = textBox.Text.Trim();
                    textBox.Text = TimeFormat.ReturnGoodTimeFormat(timeString);
                }
                catch
                {
                    MessageBox.Show("Invalid time format", "Result", MessageBoxButton.OK, MessageBoxImage.Error);
                    textBox.Text = "";
                }
            }
        }

        private void tbProgCoef_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (!float.TryParse(tbProgCoef.Text.Trim(), out _))
            {
                MessageBox.Show("Invalid Prog coef value", "Result", MessageBoxButton.OK, MessageBoxImage.Error);
                tbProgCoef.Text = "";
            }
        }

        private async void cbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbType.Text.Length == 3)
            {
                if (cbType.SelectedItem != null)
                {
                    EmsTypes emsTypes = cbType.SelectedItem as EmsTypes;
                    if (emsTypes != null)
                    {
                        tbTypeDesc.Text = emsTypes.typoname.ToString().Trim();                        
                    }
                    else
                    {
                        tbTypeDesc.Text = "";
                    }
                }             
            }
            else
            {
                tbTypeDesc.Text = "";
            }
        }
    }
}
