using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.Entities;
using CampaignEditor.UserControls;
using Database.DTOs.MediaPlanRef;
using Database.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor
{
    /// <summary>
    /// Interaction logic for CampaignForecastDates.xaml
    /// </summary>
    public partial class CampaignForecastDates : Page
    {

        MediaPlanRefController _mediaPlanRefController;

        private List<DateTime> unavailableDates = new List<DateTime>();
        private CampaignDTO _campaign;

        public CampaignForecastDates(IMediaPlanRefRepository mediaPlanRefRepository)
        {
            _mediaPlanRefController = new MediaPlanRefController(mediaPlanRefRepository);

            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign, List<DateTime> unavailableDates)
        {
            this._campaign = campaign;
            this.unavailableDates = unavailableDates;

            await LoadGridInit();
        }

        public async Task LoadGridInit()
        {
            lbDateRanges.Items.Clear();
            var dateRanges = await _mediaPlanRefController.GetAllMediaPlanRefsByCmpid(_campaign.cmpid);

            if (dateRanges.Count() <= 0)
            {
                var dri = new DateRangeItem();
                DateTime now = DateTime.Now;
                dri.SetDates(now, now);
                dri.DisableDates(unavailableDates);
                lbDateRanges.Initialize(unavailableDates, dri);
            }
            else
            {
                lbDateRanges.Initialize(unavailableDates, new DateRangeItem());

                bool first = true;
                foreach (var dateRange in dateRanges)
                {
                    DateTime start = TimeFormat.YMDStringToDateTime(dateRange.datestart.ToString());
                    DateTime end = TimeFormat.YMDStringToDateTime(dateRange.dateend.ToString());

                    if (first)
                    {
                        DateRangeItem dri = lbDateRanges.Items[0] as DateRangeItem;
                        dri.SetDates(start, end);
                        dri.DisableDates(unavailableDates);
                        first = false;
                    }
                    else
                    {
                        DateRangeItem dri = new DateRangeItem();
                        dri.SetDates(start, end);
                        dri.DisableDates(unavailableDates);
                        lbDateRanges.Items.Insert(lbDateRanges.Items.Count - 1, dri);
                    }

                }

                lbDateRanges.ResizeItems(lbDateRanges.Items);

                btnInitCancel.Visibility = Visibility.Visible;
            }
           
        }

        public async Task<bool> InsertMediaPlanRefs()
        {

            bool validRanges = CheckDateRanges();
            if (!validRanges)
            {
                return false;
            }

            for (int i = 0; i < lbDateRanges.Items.Count - 1; i++)
            {
                DateRangeItem dri = lbDateRanges.Items[i] as DateRangeItem;

                int? ymdFrom = TimeFormat.DPToYMDInt(dri.dpFrom);
                int? ymdTo = TimeFormat.DPToYMDInt(dri.dpTo);

                if (ymdFrom.HasValue && ymdTo.HasValue)
                {
                    await _mediaPlanRefController.CreateMediaPlanRef(
                    new MediaPlanRefDTO(_campaign.cmpid, ymdFrom.Value, ymdTo.Value));
                }
            }

            return true;
        }

        private bool CheckDateRanges()
        {
            // no range entered
            if (lbDateRanges.Items.Count == 1)
            {
                MessageBox.Show("Enter date range");
                return false;
            }

            // check intercepting or invalid date values
            for (int i = 0; i < lbDateRanges.Items.Count - 1; i++)
            {
                DateRangeItem dri = lbDateRanges.Items[i] as DateRangeItem;
                if (!dri.CheckValidity())
                {
                    MessageBox.Show("Invalid dates");
                    return false;
                }
                for (int j = i + 1; j < lbDateRanges.Items.Count - 1; j++)
                {
                    DateRangeItem dri2 = lbDateRanges.Items[j] as DateRangeItem;
                    if (dri.checkIntercepting(dri2))
                    {
                        MessageBox.Show("Dates are intercepting");
                        return false;
                    }
                }
            }

            return true;
        }

        public event EventHandler CancelButtonClicked;

        private void OnCancelButtonClicked()
        {
            CancelButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Trigger the event
            OnCancelButtonClicked();
        }

        public event EventHandler InitializeButtonClicked;

        private void OnInitializeButtonClicked()
        {
            InitializeButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void InitializeButton_Click(object sender, RoutedEventArgs e)
        {
            // Trigger the event
            OnInitializeButtonClicked();
        }
    }
}
