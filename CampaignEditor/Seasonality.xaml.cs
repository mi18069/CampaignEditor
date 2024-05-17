using CampaignEditor.Controllers;
using Database.DTOs.SeasonalitiesDTO;
using Database.DTOs.SeasonalityDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using Database.DTOs.CampaignDTO;

namespace CampaignEditor
{
    public partial class Seasonality : Window
    {
        private SeasonalityController _seasonalityController;
        private SeasonalitiesController _seasonalitiesController;

        private SeasonalityDTO _seasonality = null;

        public SeasonalityDTO Seas { get { return _seasonality; } }

        public bool success = false;
        private bool modifiedSeasonalities = false;
        private bool modifiedSeasonality = false;

        private CampaignDTO _campaign = null;

        // For Plus Icon
        private string appPath = Directory.GetCurrentDirectory();
        private string imgGreenPlusPath = "\\images\\GreenPlus.png";

        public Seasonality(ISeasonalitiesRepository seasonalitiesRepository,
                           ISeasonalityRepository seasonalityRepository)
        {
            InitializeComponent();

            _seasonalitiesController = new SeasonalitiesController(seasonalitiesRepository);
            _seasonalityController = new SeasonalityController(seasonalityRepository);

            if (MainWindow.user.usrlevel <= 0)
            {
                cbGlobal.Visibility = Visibility.Visible;
            }
        }

        public async void Initialize(CampaignDTO campaign, SeasonalityDTO seasonality = null)
        {
            if (seasonality != null)
            {
                _seasonality = seasonality;
                await FillBySeasAsync(_seasonality);
            }
            else
            {
                var seasItem = MakeSeasItem();
                wpSeasonalities.Children.Add(seasItem);
                var addBtn = MakeAddButton();
                wpSeasonalities.Children.Add(addBtn);
            }
            _campaign = campaign;
        }
        
        #region Fill Fields
        private async Task FillBySeasAsync(SeasonalityDTO seasonality)
        {
            tbName.Text = seasonality.seasname.Trim();
            cbActive.IsChecked = seasonality.seasactive;
            cbGlobal.IsChecked = seasonality.ownedby == 0 ? true : false;
            wpSeasonalities.Children.Clear();

            List<SeasonalitiesDTO> seascoefs = (List<SeasonalitiesDTO>)await _seasonalitiesController.GetSeasonalitiesById(seasonality.seasid);
            seascoefs = seascoefs.OrderBy(s => s.stdt).ToList();
            foreach (var seascoef in seascoefs)
            {
                var seasItem = MakeSeasItem();
                seasItem.dpFrom.SelectedDate = seasItem.FromStringToDateTime(seascoef.stdt);
                seasItem.dpTo.SelectedDate = seasItem.FromStringToDateTime(seascoef.endt);
                seasItem.tbCoef.Text = seascoef.coef.ToString("0.0###").Trim();
                wpSeasonalities.Children.Add(seasItem);
            }
            var addBtn = MakeAddButton();
            wpSeasonalities.Children.Add(addBtn);
            modifiedSeasonalities = false;

        }
        #endregion

        #region Text Boxes mechanism
        private void tbName_TextChanged(object sender, TextChangedEventArgs e)
        {
            modifiedSeasonality = true;
        }

        private void cb_Changed(object sender, RoutedEventArgs e)
        {
            modifiedSeasonality = true;
        }

        #endregion

        #region Wrap Pannel
        private SeasonalitiesItem MakeSeasItem()
        {
            SeasonalitiesItem seasItem = new SeasonalitiesItem();
            seasItem.Width = wpSeasonalities.Width;
            seasItem.PreviewTextInput += seasItem_PreviewTextInput;
            seasItem.btnDelete.Click += btnItemDelete_Click;
            seasItem.dpFrom.SelectedDateChanged += Dp_SelectedDateChanged;
            seasItem.dpTo.SelectedDateChanged += Dp_SelectedDateChanged;
            return seasItem;
        }
        private Button MakeAddButton()
        {
            Button btnAddSeas = new Button();
            btnAddSeas.Click += new RoutedEventHandler(btnAddSeas_Click);
            Image imgGreenPlus = new Image();
            ImageSource imageSource = (ImageSource)Application.Current.FindResource("plus_icon");
            imgGreenPlus.Source = imageSource;
            btnAddSeas.Content = imgGreenPlus;
            btnAddSeas.Width = 30;
            btnAddSeas.Height = 30;
            btnAddSeas.Background = Brushes.White;
            btnAddSeas.BorderThickness = new Thickness(0);
            btnAddSeas.HorizontalAlignment = HorizontalAlignment.Center;

            return btnAddSeas;
        }
        private void btnAddSeas_Click(object sender, RoutedEventArgs e)
        {
            var btnAdd = sender as Button;
            wpSeasonalities.Children.Remove(btnAdd);

            var seasItem = MakeSeasItem();
            wpSeasonalities.Children.Add(seasItem);
            var addBtn = MakeAddButton();
            wpSeasonalities.Children.Add(addBtn);

        }
        private void btnItemDelete_Click(object sender, RoutedEventArgs e)
        {
            modifiedSeasonalities = true;
        }

        private void seasItem_PreviewTextInput(object sender, EventArgs e)
        {
            modifiedSeasonalities = true;
        }
        private void Dp_SelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            modifiedSeasonalities = true;
        }

        #endregion

        #region Save and Calcel Buttons
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (modifiedSeasonality == false && modifiedSeasonalities == false)
            {
                this.Close();
            }

            string errorMessage = CheckSeasonalities();

            if ( errorMessage != string.Empty)
            {
                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error );
                return;
            }
            else
            {
                if (_seasonality == null)
                {
                    _seasonality = await _seasonalityController.CreateSeasonality(new CreateSeasonalityDTO
                        (tbName.Text.Trim(), (bool)cbActive.IsChecked, _campaign.clid));
                }
                int id = _seasonality.seasid;

                if (modifiedSeasonalities)
                {
                    await _seasonalitiesController.DeleteSeasonalitiesById(id);

                    int n = wpSeasonalities.Children.Count;
                    for (int i = 0; i < n - 1; i++)
                    {
                        SeasonalitiesItem seasonalitiesItem = (wpSeasonalities.Children[i] as SeasonalitiesItem)!;
                        string? stdt = seasonalitiesItem.FromDPToString(seasonalitiesItem.dpFrom);
                        string? endt = seasonalitiesItem.FromDPToString(seasonalitiesItem.dpTo);
                        decimal coef = decimal.Parse(seasonalitiesItem.tbCoef.Text.Trim());

                        if (stdt != null && endt != null)
                        {
                            await _seasonalitiesController.CreateSeasonalities(new CreateSeasonalitiesDTO(
                            id, stdt, endt, coef));
                        }
                    }
                }
                if (modifiedSeasonality)
                {
                    int owner = (bool)cbGlobal.IsChecked ? 0 : _campaign.clid;
                    await _seasonalityController.UpdateSeasonality(new UpdateSeasonalityDTO(id, tbName.Text.Trim(), (bool)cbActive.IsChecked, owner));
                }
                success = true;
            }
            this.Close();
        }

        private string CheckSeasonalities()
        {
            string errorString = string.Empty;
            for (int i = 0; i < wpSeasonalities.Children.Count - 1; i++)
            {
                SeasonalitiesItem seasonalitiesItem = (wpSeasonalities.Children[i] as SeasonalitiesItem)!;
                errorString = seasonalitiesItem.CheckFields();
                if (errorString != string.Empty)
                {
                    return errorString; 
                }

            }

            if (CheckOverlapping())
            {
                return "Overlapping date values";
            }

            return errorString;
        }

        private bool CheckOverlapping()
        {
            if (wpSeasonalities.Children.Count <= 1)
            {
                return false;
            }

            List<Tuple<DateTime, DateTime>> startEndDate = new List<Tuple<DateTime, DateTime>>();

            for (int i = 0; i < wpSeasonalities.Children.Count - 1; i++)
            {
                SeasonalitiesItem seasonalitiesItem = (wpSeasonalities.Children[i] as SeasonalitiesItem)!;
                var startDate = seasonalitiesItem.dpFrom.SelectedDate!.Value;
                var endDate = seasonalitiesItem.dpTo.SelectedDate!.Value;
                startEndDate.Add(Tuple.Create(startDate, endDate));
            }

            startEndDate = startEndDate.OrderBy(sed => sed.Item1).ToList();

            for (int i = 0; i < startEndDate.Count() - 1; i++)
            {
                if (startEndDate[i].Item2 > startEndDate[i + 1].Item1)
                {
                    return true;
                }
            }

            return false;
        } 


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
