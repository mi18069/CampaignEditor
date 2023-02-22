using CampaignEditor.Controllers;
using Database.DTOs.PricelistChannels;
using Database.DTOs.PricelistDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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


        // For Plus Icon
        private string appPath = Directory.GetCurrentDirectory();
        private string imgGreenPlusPath = "\\images\\GreenPlus.png";
        public PriceList(IChannelRepository channelRepository,
            IPricelistRepository pricelistRepository, IPricelistChannelsRepository pricelistChannelsRepository)
        {
            _channelController = new ChannelController(channelRepository);
            _pricelistChannelsController = new PricelistChannelsController(pricelistChannelsRepository);
            _pricelistController = new PricelistController(pricelistRepository);


            InitializeComponent();

            Initialize();
            //DoOnce();
            
        }

        Regex rg = new Regex(@"[\d+]");
        private async void DoOnce()
        {
            List<PricelistDTO> pricelists = (List<PricelistDTO>)await _pricelistController.GetAllPricelists();
            foreach (PricelistDTO pricelist in pricelists)
            {
                string channelsString = pricelist.a2chn;
                var matches = rg.Matches(channelsString);

                foreach (var match in matches)
                {
                    string m = match.ToString();
                    m.Trim('[');
                    m.Trim(']');
                    int chid = Convert.ToInt32(m);
                    await _pricelistChannelsController.CreatePricelistChannels(
                        new CreatePricelistChannelsDTO(pricelist.plid, chid));
                }
            }
        }

        private void Initialize()
        {
            FillChannels();
            UpdateDayParts();
            FillComboBoxes();
        }

        private void FillComboBoxes()
        {
            List<string> typeList = new List<string> { "CPP", "Seconds", "Package" };
            List<string> sectable = new List<string> { "LINEAR" };


            foreach (string type in typeList)
            {
                cbType.Items.Add(type);
            }

            foreach (string sect in sectable)
            {
                cbSectable.Items.Add(sect);
                cbSectable2.Items.Add(sect);
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
    }
}
