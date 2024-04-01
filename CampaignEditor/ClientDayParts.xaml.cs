using CampaignEditor.Controllers;
using CampaignEditor.UserControls;
using Database.DTOs.ClientDTO;
using Database.DTOs.DayPartDTO;
using Database.DTOs.DPTimeDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CampaignEditor
{
    public partial class ClientDayParts : Window
    {
        private DayPartController _dayPartController;
        private DPTimeController _dpTimeController;

        private ClientDTO _client;
        private Dictionary<DayPartDTO, List<DPTimeDTO>> _dayPartsDict = null;

        public Dictionary<DayPartDTO, List<DPTimeDTO>> DayPartsDict
        {
            get { return _dayPartsDict; }
        }

        public bool dpModified = false;

        public ClientDayParts(IDayPartRepository dayPartRepository, IDPTimeRepository dPTimeRepository)
        {
            InitializeComponent();

            _dayPartController = new DayPartController(dayPartRepository);
            _dpTimeController = new DPTimeController(dPTimeRepository);
        }

        public void Initialize(ClientDTO client, Dictionary<DayPartDTO, List<DPTimeDTO>> dayPartsDict)
        {
            _client = client;
            _dayPartsDict = dayPartsDict;

            lblClientDayParts.Content += client.clname.Trim();
            lbDayParts.Initialize(new DayPartItem());
            FillDayParts();
        }

        private void FillDayParts()
        {
            if (_dayPartsDict.Count > 0)
            {
                lbDayParts.Items.RemoveAt(0);
            }

            foreach (DayPartDTO dayPartDTO in _dayPartsDict.Keys)
            {
                DayPartItem dayPartItem = new DayPartItem();
                dayPartItem.Initialize(dayPartDTO, _dayPartsDict[dayPartDTO]);
                dayPartItem.DayPartItemDeleted += DayPartItem_DayPartItemDeleted;
                lbDayParts.Items.Insert(lbDayParts.Items.Count - 1, dayPartItem);
            }
        }

        private void DayPartItem_DayPartItemDeleted(object? sender, EventArgs e)
        {
            DayPartItem dayPartItem = (DayPartItem)sender;
            dayPartItem.DayPartItemDeleted -= DayPartItem_DayPartItemDeleted;
            dpModified = true;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (IsModified())
            {
                if (!IsGoodTime())
                {
                    return;
                }
                else
                {
                    await DeleteAllDayParts();
                    await CreateAllDayParts();
                }
            }
            this.Close();
        }

        private bool IsModified()
        {
            foreach (DayPartItem dayPartItem in lbDayParts.GetItems())
            {
                if (dayPartItem.CheckModified())
                {
                    dpModified = true;
                    return true;
                }
            }

            return dpModified;
        }

        private bool IsIntersecting()
        {
            List<Tuple<string, string>> timePairs = new List<Tuple<string, string>>();

            foreach (DayPartItem dayPartItem in lbDayParts.GetItems())
            {
                var dayPartTimePairs = dayPartItem.GetTimeStringPairs();
                foreach (var timePair in dayPartTimePairs)
                {
                    timePairs.Add(timePair);
                }
            }

            timePairs = timePairs.OrderBy(tp => tp.Item1).ToList();

            for (int i=0; i<timePairs.Count - 1; i++)
            {
                var timePair = timePairs[i];
                var nextTimePair = timePairs[i+1];

                if (String.Compare(timePair.Item2, nextTimePair.Item1) >= 0)
                {
                    MessageBox.Show("Time periods are intersecting", "Message",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return true;
                }
            }

            return false;
        }

        private bool IsGoodTime()
        {
            foreach (DayPartItem dayPartItem in lbDayParts.GetItems())
            {
                if (!dayPartItem.isGoodTime())
                {
                    return false;
                }
            }
            if (IsIntersecting())
                return false;

            return true;
        }

        private async Task DeleteAllDayParts()
        {
            var dayParts = await _dayPartController.GetAllClientDayParts(_client.clid);
            foreach (var dayPart in dayParts)
            {
                await _dpTimeController.DeleteDPTimeByDPId(dayPart.dpid);
                await _dayPartController.DeleteDayPart(dayPart.dpid);
            }

        }

        private async Task CreateAllDayParts()
        {
            _dayPartsDict.Clear();
            for (int i=0; i<lbDayParts.Items.Count - 1; i++)
            {
                DayPartItem dpItem = lbDayParts.Items[i] as DayPartItem;
                if (dpItem.GetName() == "")
                {
                    continue;
                }
                DayPartDTO dayPart = null;
                if (dpItem != null)
                {
                    dayPart = await _dayPartController.CreateDayPart( new CreateDayPartDTO(_client.clid, dpItem.GetName(), "1234567"));
                    _dayPartsDict[dayPart] = null;
                }
                if (dayPart != null)
                {
                    List<DPTimeDTO> dpTimes = new List<DPTimeDTO>();
                    var dpItems = dpItem.GetTimeStringPairs();                  
                    foreach (var dpTime in dpItems)
                    {
                        if (dpTime.Item1 == "" || dpTime.Item2 == "")
                            continue;
                        var createdDpTime = await _dpTimeController.CreateDPTime(new CreateDPTimeDTO(dayPart.dpid, dpTime.Item1, dpTime.Item2));
                        dpTimes.Add(createdDpTime);
                    }
                    _dayPartsDict[dayPart] = dpTimes;
                }

            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            dpModified = false;
            this.Close();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (DayPartItem dayPartItem in lbDayParts.GetItems())
            {
                dayPartItem.DayPartItemDeleted -= DayPartItem_DayPartItemDeleted;
            }
        }
    }
}
