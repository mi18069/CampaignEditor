using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.EmsTypesDTO;
using Database.DTOs.SchemaDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace CampaignEditor
{
    public partial class AddSchema : Window
    {

        private CampaignDTO _campaign;

        private ChannelController _channelController;
        private MediaPlanController _mediaPlanController;
        private EmsTypesController _emsTypesController;

        public CreateSchemaDTO _schema = null;


        public AddSchema(IChannelRepository channelRepository,
            IMediaPlanRepository mediaPlanRepository,
            IEmsTypesRepository emsTypesRepository)
        {
            InitializeComponent();

            _channelController = new ChannelController(channelRepository);
            _mediaPlanController = new MediaPlanController(mediaPlanRepository);
            _emsTypesController = new EmsTypesController(emsTypesRepository);
        }

        public async Task Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;

            await FillComboboxes();
        }

        private async Task FillComboboxes()
        {
            await FillCbChannels();
            FillCbPosition();
            FillDate();
            FillDays();
        }

        private void FillCbPosition()
        {
            cbPosition.Items.Clear();
            cbPosition.Items.Add("INS");
            cbPosition.Items.Add("BET");
        }
        private void FillDays()
        {
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
        }

        private void FillDate()
        {
            dpFrom.SelectedDate = DateTime.Now;
            dpTo.SelectedDate = DateTime.Now;
        }

        private async Task FillCbChannels()
        {
            cbChannels.Items.Clear();

            var channelIds = await _mediaPlanController.GetAllChannelsByCmpid(_campaign.cmpid);
            List<ChannelDTO> channels = new List<ChannelDTO>();
            foreach (var chid in channelIds)
            {
                ChannelDTO channel = await _channelController.GetChannelById(chid);
                channels.Add(channel);
            }

            channels = channels.OrderBy(c => c.chname).ToList();

            foreach (ChannelDTO channel in channels)
            {
                cbChannels.Items.Add(channel);
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
                _schema = MakeSchemaDTO();
                this.Close();
            }
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
            string? type = tbType.Text.Trim().Length > 0 ? tbType.Text.ToUpper() : null;
            bool special = (bool)chbSpecial.IsChecked;
            DateOnly dateFrom = DateOnly.FromDateTime(dpFrom.SelectedDate.Value);
            DateOnly dateTo = DateOnly.FromDateTime(dpTo.SelectedDate.Value);
            float progcoef = 1.0f;
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
            else if (await _emsTypesController.GetEmsTypesByCode(tbType.Text.Trim()) == null)
            {
                MessageBox.Show("Invalid type value");
                return false;
            }
            else if (dpFrom.SelectedDate > dpTo.SelectedDate)
            {
                MessageBox.Show("Invalid date values");
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
            else if ( tbTimeTo.Text.Trim().Length > 0 && 
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

        private async void tbType_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (tbType.Text.Length == 3)
            {
                EmsTypesDTO emsType = null;
                if ((emsType = await _emsTypesController.GetEmsTypesByCode(tbType.Text.ToUpper())) != null)
                {
                    tbTypeDesc.Text = emsType.typoname.ToString().Trim();
                }
                else
                {
                    tbTypeDesc.Text = "";
                }
            }
            else
            {
                tbTypeDesc.Text = "";
            }
        }
    }
}
