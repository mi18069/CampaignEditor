using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.UserControls;
using Database.DTOs.SpotDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
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
        private List<SpotDTO> _spotlist;
        private CampaignDTO _campaign;

        public List<SpotDTO> Spotlist
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
            Campaign = campaign;
            wpSpots.Children.Add(MakeAddButton());

            var spots = await _spotController.GetSpotsByCmpid(Campaign.cmpid);
            Spotlist = (List<SpotDTO>)spots;
            foreach (var spot in spots)
            {
                AddSpotItem(spot);
            }
            AddSpotItem();
        }

        #region SpotItems

        private void btnAddSpot_Click(object sender, RoutedEventArgs e)
        {
            AddSpotItem();
        }
        private void btnDeleteSpot_Click(object sender, RoutedEventArgs e)
        {
            spotsModified = true;
            UpdateSpots();
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
            spotItem.Width = 550; // FIX THIS HARDCODING
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
                Spotlist.Clear();
                await _spotController.DeleteSpotsByCmpid(Campaign.cmpid);
                for (int i = 0; i < n - 1; i++)
                {
                    SpotItem item = wpSpots.Children[i] as SpotItem;
                    if (!(i == n - 2 &&
                    item.tbLength.Text.Trim().Length == 0 &&
                    item.tbName.Text.Trim().Length == 0))
                    {
                        CreateSpotDTO newSpot = new CreateSpotDTO(Campaign.cmpid, item.lblCode.Content.ToString().Trim(),
                        item.tbName.Text.ToString().Trim(), int.Parse(item.tbLength.Text.Trim()), false);
                        Spotlist.Add(await _spotController.CreateSpot(newSpot));
                    }                
                }
            }
            this.Close();
        }

        private bool CheckValues()
        {
            int n = wpSpots.Children.Count;
            for(int i=0; i<n-1; i++)
            {
                SpotItem spotItem = wpSpots.Children[i] as SpotItem;
                // Don't check last item if it's empty
                if (!(i == n - 2 && 
                    spotItem.tbLength.Text.Trim().Length == 0 && 
                    spotItem.tbName.Text.Trim().Length == 0))
                {
                    continue;
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
            this.Close();
        }
    }
}
