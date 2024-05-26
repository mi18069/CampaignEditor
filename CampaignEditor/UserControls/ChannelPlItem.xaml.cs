using Database.DTOs.ChannelDTO;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// This control is used in pricelist 
    /// </summary>
    public partial class ChannelPlItem : UserControl
    {
        public IEnumerable<ChannelDTO> _channels;
        public bool isModified = false;
        public ChannelPlItem()
        {
            InitializeComponent();
            
        }

        public void Initialize(IEnumerable<ChannelDTO> channels, ChannelDTO selectedChannel = null, decimal value = 1.0M)
        {
            _channels = channels;
            cbChannels.ItemsSource = _channels;
            if (selectedChannel != null)
            {
                cbChannels.SelectedItem = selectedChannel;
                if (value != 1)
                {
                    tbCoef.SetValue(value);
                    tbCoef.isModified = false;
                }
                isModified = false;
            }
        }

        public ChannelDTO? GetChannel()
        {
           if (cbChannels.SelectedItem == null)
            {
                return null;
            }
            else
            {
                return cbChannels.SelectedItem as ChannelDTO;
            }
        }

        public decimal? GetCoef()
        {
            return tbCoef.GetValue();
        }

        public bool GetIsModified()
        {
            return isModified || tbCoef.isModified;
        }

        public event EventHandler DeleteClicked;

        private void btnDelete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Raise the event
            DeleteClicked?.Invoke(this, EventArgs.Empty);
        }

        private void cbChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isModified = true;
        }
    }
}
