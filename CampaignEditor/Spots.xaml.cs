using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.UserControls;
using Database.DTOs.SpotDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CampaignEditor
{
    public partial class Spots : Window
    {

        private SpotController _spotController;

        public bool spotsModified = false;
        public bool shouldClose = false;
        private ObservableCollection<SpotDTO> _spotlist = new ObservableCollection<SpotDTO>();
        private CampaignDTO _campaign;

        public ObservableCollection<SpotDTO> Spotlist
        {
            get { return _spotlist; }
            set { _spotlist = value; }
        }
        public CampaignDTO Campaign
        {
            get { return _campaign; }
            set { _campaign = value; }
        }

        // For Plus Icon
        private string appPath = Directory.GetCurrentDirectory();
        private string imgGreenPlusPath = "\\images\\GreenPlus.png";
        public Spots(ISpotRepository spotRepository)
        {
            _spotController = new SpotController(spotRepository);

            InitializeComponent();
        }

        public async Task Initialize(CampaignDTO campaign, List<SpotDTO> spotlist = null)
        {
            wpSpots.Children.Clear();
            Campaign = campaign;
            wpSpots.Children.Add(MakeAddButton());

            // If spots are called for the first time
            if (spotlist == null)
            {
                var spots = await _spotController.GetSpotsByCmpid(Campaign.cmpid);
                spots.OrderBy(s => s.spotcode);
                foreach (var spot in spots)
                {
                    Spotlist.Add(spot);
                    AddSpotItem(spot);
                }
            }
            // Just filling up wpSpots, Spotlist already contains informations
            else
            {
                foreach (var spot in spotlist)
                {
                    AddSpotItem(spot);
                }
            }
            AddSpotItem();
            ResizeSpotItems();
        }

        #region SpotItems

        private void btnAddSpot_Click(object sender, RoutedEventArgs e)
        {
            AddSpotItem();
            spotsModified = true;
        }
        private void btnDeleteSpot_Click(object sender, RoutedEventArgs e)
        {
            spotsModified = true;
            UpdateSpots();
            // When it's only button left, it needs to be placed in a center
            if (wpSpots.Children.Count == 1)
            {
                Button button = wpSpots.Children[0] as Button;
                button.Margin = new Thickness(wpSpots.ActualWidth / 2, 0, 0, 0);
            }
        }

        private Button MakeAddButton()
        {
            Button btnAddSpot = new Button();
            btnAddSpot.Click += new RoutedEventHandler(btnAddSpot_Click);
            Image imgGreenPlus = new Image();
            imgGreenPlus.Source = new BitmapImage(new Uri(appPath + imgGreenPlusPath));
            btnAddSpot.Content = imgGreenPlus;
            btnAddSpot.Width = 30;
            btnAddSpot.Height = 30;
            btnAddSpot.Background = Brushes.White;
            btnAddSpot.BorderThickness = new Thickness(0);
            btnAddSpot.HorizontalAlignment = HorizontalAlignment.Center;

            return btnAddSpot;
        }

        private SpotItem MakeSpotItem(SpotDTO spot = null)
        {
            SpotItem spotItem = new SpotItem();
            spotItem.btnDelete.Click += btnDeleteSpot_Click;
            spotItem.lblCode.Content = ((char)('A' + wpSpots.Children.Count-1)).ToString();

            if (spot != null)
            {
                spotItem.lblCode.Content = spot.spotcode.ToString().Trim();
                spotItem.tbName.Text = spot.spotname.ToString().Trim();
                spotItem.tbLength.Text = spot.spotlength.ToString().Trim();
                spotItem.modified = false;
            }

            return spotItem;
        }
        private void AddSpotItem(SpotDTO spot = null)
        {
            SpotItem item = MakeSpotItem(spot);
            int index = wpSpots.Children.Count - 1;
            wpSpots.Children.Insert(index, item);
        }

        private void UpdateSpots()
        {
            int n = wpSpots.Children.Count;
            for(int i=0; i<n-1; i++)
            {
                SpotItem item = wpSpots.Children[i] as SpotItem;
                item.lblCode.Content = ((char)('A' + i)).ToString();
            }
        }

        #region Resizing
        // In order to UserControl items follow width of parent element
        private void wpSpots_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeSpotItems();
        }
        // Resizing Items when WP is Loaded
        private void wpSpots_Loaded(object sender, RoutedEventArgs e)
        {
            ResizeSpotItems();
        }
        private void ResizeSpotItems()
        {
            int n = wpSpots.Children.Count;
            for (int i = 0; i < n - 1; i++)
            {
                SpotItem spotItem = wpSpots.Children[i] as SpotItem;
                spotItem.Width = wpSpots.ActualWidth;
            }
        }
        #endregion
        #endregion

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int n = wpSpots.Children.Count;

            for (int i=0; i<n-1; i++)
            {
                SpotItem item = wpSpots.Children[i] as SpotItem;
                if (item.modified)
                {
                    spotsModified = true;
                    break;
                }
            }
            if (spotsModified && CheckValues())
            {
                int skipped = 0;
                Spotlist.Clear();
                for (int i = 0; i < n - 1; i++)
                {
                    SpotItem item = wpSpots.Children[i] as SpotItem;
                    if (item.tbLength.Text.Trim().Length == 0 ||
                    item.tbName.Text.Trim().Length == 0)
                    {
                        skipped += 1;
                        continue;
                    }
                    else
                    {
                        SpotDTO spot = new SpotDTO(Campaign.cmpid, ((char)('A' + i - skipped)).ToString(),
                        item.tbName.Text.ToString().Trim(), int.Parse(item.tbLength.Text.Trim()), false);
                        Spotlist.Add(spot);
                    }
                }
                await UpdateDatabase(Spotlist.ToList());
                this.Hide();
            }
            
        }

        private bool CheckValues()
        {
            int n = wpSpots.Children.Count;
            for(int i=0; i<n-1; i++)
            {
                SpotItem spotItem = wpSpots.Children[i] as SpotItem;
                // Don't check item if it's empty
                if (spotItem.tbLength.Text.Trim().Length == 0 && 
                    spotItem.tbName.Text.Trim().Length == 0)
                {
                    continue;
                }
                if (spotItem.tbName.Text.Trim() == "" || spotItem.tbName.Text.Trim() == null)
                {
                    MessageBox.Show("Enter name");
                    return false;
                }
                if (spotItem.tbLength.Text.Trim() == "" || spotItem.tbLength.Text.Trim() == null)
                {
                    MessageBox.Show("Enter length");
                    return false;
                }
                if (!spotItem.tbLength.Text.Trim().All(Char.IsDigit))
                {
                    MessageBox.Show("Invalid value for length (integers only)");
                    return false;
                }
            }
            return true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        public async Task UpdateDatabase(List<SpotDTO> spotlist)
        {
            await _spotController.DeleteSpotsByCmpid(Campaign.cmpid);

            Spotlist.Clear();

            foreach (var spot in spotlist)
            {
                CreateSpotDTO newSpot = new CreateSpotDTO(spot.cmpid, spot.spotcode, spot.spotname, spot.spotlength, spot.ignore);
                Spotlist.Add(await _spotController.CreateSpot(newSpot));
            }
        }


        // Overriding OnClosing because click on x button should only hide window
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!shouldClose)
            {
                e.Cancel = true;
                Hide();
            }

        }
   
    }
}
