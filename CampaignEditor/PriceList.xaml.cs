using CampaignEditor.Controllers;
using Database.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace CampaignEditor
{
    public partial class PriceList : Window
    {
        private ChannelController _channelController;

        // For Plus Icon
        private string appPath = Directory.GetCurrentDirectory();
        private string imgGreenPlusPath = "\\images\\GreenPlus.png";
        public PriceList(IChannelRepository channelRepository)
        {
            _channelController = new ChannelController(channelRepository);

            InitializeComponent();

            FillChannels();
            UpdateDayParts();
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
