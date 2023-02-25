using CampaignEditor.Controllers;
using CampaignEditor.StartupHelpers;
using Database.DTOs.ClientDTO;
using Database.DTOs.SeasonalityDTO;
using Database.DTOs.SectableDTO;
using Database.DTOs.TargetDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CampaignEditor
{
    public partial class PriceList : Window
    {
        private ChannelController _channelController;
        private PricelistChannelsController _pricelistChannelsController;
        private PricelistController _pricelistController;
        private SectableController _sectableController;
        private SeasonalityController _seasonalityController;
        private TargetController _targetController;


        private readonly IAbstractFactory<Sectable> _factorySectable;
        private readonly IAbstractFactory<Seasonality> _factorySeasonality;
        private ClientDTO client;

        // For Plus Icon
        private string appPath = Directory.GetCurrentDirectory();
        private string imgGreenPlusPath = "\\images\\GreenPlus.png";
        public PriceList(IAbstractFactory<Sectable> factorySectable, IAbstractFactory<Seasonality> factorySeasonality,
            IChannelRepository channelRepository,
            IPricelistRepository pricelistRepository, IPricelistChannelsRepository pricelistChannelsRepository,
            ISectableRepository sectableRepository, ISeasonalityRepository seasonalityRepository,
            ITargetRepository targetRepository)
        {
            _factorySectable = factorySectable;
            _factorySeasonality = factorySeasonality;

            _sectableController = new SectableController(sectableRepository);
            _seasonalityController = new SeasonalityController(seasonalityRepository);
            _targetController = new TargetController(targetRepository);
            _channelController = new ChannelController(channelRepository);
            _pricelistChannelsController = new PricelistChannelsController(pricelistChannelsRepository);
            _pricelistController = new PricelistController(pricelistRepository);


            InitializeComponent();

        }

        public void Initialize(ClientDTO client)
        {
            this.client = client;
            FillFields();
        }

        private void FillFields()
        {
            FillChannels();
            UpdateDayParts();
            FillComboBoxes();
        }

        private async void FillComboBoxes()
        {
            List<string> typeList = new List<string> { "CPP", "Seconds", "Package" };
            List<SectableDTO> sectables = (List<SectableDTO>) await _sectableController.GetAllSectablesByOwnerId(client.clid);
            List<SeasonalityDTO> seasonalities = (List<SeasonalityDTO>) await _seasonalityController.GetAllSeasonalitiesByOwnerId(client.clid);
            List<TargetDTO> targets = (List<TargetDTO>)await _targetController.GetAllClientTargets(client.clid);

            foreach (string type in typeList)
            {
                cbType.Items.Add(type);
            }

            foreach (var sectable in sectables)
            {
                cbSectable.Items.Add(sectable);
                cbSectable2.Items.Add(sectable);
            }

            foreach (var seasonality in seasonalities)
            {
                cbSeasonality.Items.Add(seasonality);
            }

            foreach (var target in targets)
            {
                cbTarget.Items.Add(target);
            }
        }

        // Adding Add Button and New TargetDPItem
        private void UpdateDayParts()
        {
            Button btnAddDP = new Button();
            btnAddDP.Click += new RoutedEventHandler(btnAddDP_Click);
            Image imgGreenPlus = new Image();
            imgGreenPlus.Source = new BitmapImage(new Uri(appPath + imgGreenPlusPath));
            btnAddDP.Content = imgGreenPlus;
            btnAddDP.Width = 30;
            btnAddDP.Height = 30;
            btnAddDP.Background = Brushes.White;
            btnAddDP.BorderThickness = new Thickness(0);
            btnAddDP.HorizontalAlignment = HorizontalAlignment.Center;

            TargetDPItem dpItem = new TargetDPItem();
            wpDayParts.Children.Add(dpItem);

            wpDayParts.Children.Add(btnAddDP);
        }

        private void btnAddDP_Click(object sender, RoutedEventArgs e)
        {
            var btnAdd = sender as Button;
            wpDayParts.Children.Remove(btnAdd);

            UpdateDayParts();
        }

        private async void FillChannels()
        {
            var channels = await _channelController.GetAllChannels();

            channels = channels.OrderBy(c => c.chname);

            foreach (var channel in channels)
            {
                CheckBox cb = new CheckBox();
                cb.Content = channel.chname;
                cb.Tag = channel;
                wpChannels.Children.Add(cb);
            }
        }

        // Selecting whole text when captured
        private void tbSec2_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var tb = sender as TextBox;
            
            tb.SelectAll();
            tb.Focus();
        }

        private void btnNewSectable_Click(object sender, RoutedEventArgs e)
        {
            _factorySectable.Create().Show();
        }

        private void btnNewSeasonality_Click(object sender, RoutedEventArgs e)
        {
            _factorySeasonality.Create().Show();
        }
    }
}
