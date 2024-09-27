using CampaignEditor.Controllers;
using CampaignEditor.Entities;
using Database.DTOs.CampaignDTO;
using Database.DTOs.ChannelDTO;
using Database.DTOs.CobrandDTO;
using Database.DTOs.SpotDTO;
using Database.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace CampaignEditor
{
    /// <summary>
    /// Interaction logic for Cobranding.xaml
    /// </summary>
    public partial class Cobranding : Window
    {
        private CampaignDTO _campaign;
        private CobrandController _cobrandController;

        List<char> _spotcodes;
        decimal _previousValue = 1.0M;

        private bool _modified = false;
        public bool Modified { get { return _modified; } } 
        public Cobranding(ICobrandRepository cobrandRepository)
        {
            InitializeComponent();

            _cobrandController = new CobrandController(cobrandRepository);
        }

        public void Initialize(CampaignDTO campaign, IEnumerable<CobrandDTO> cobrands, IEnumerable<ChannelDTO> channels, IEnumerable<char> spotcodes)
        {
            _campaign = campaign;

            // Making all necessary ChannelSpotModels
            _spotcodes = spotcodes.ToList();
            List<ChannelSpotModel> data = new List<ChannelSpotModel>();

            foreach (var channel in channels)
            {
                var channelSpotModel = new ChannelSpotModel(channel, _spotcodes);
                data.Add(channelSpotModel);
            }

            // Adding initial coefs
            foreach (var cobrand in cobrands)
            {
                var channelSpotModel = data.FirstOrDefault(d => d.Channel.chid == cobrand.chid);
                
                if (channelSpotModel == null)
                    continue; // This shouldn't happen, when we delete channel then delete cobrand

                if (!spotcodes.Contains(cobrand.spotcode))
                    continue; // This shouldn't happen, when we delete spot then delete cobrand

                channelSpotModel.InitializeCoef(cobrand.spotcode, cobrand.coef);
            }

            GenerateDynamicColumns(spotcodes);
            dgGrid.ItemsSource = data;
        }

        private void GenerateDynamicColumns(IEnumerable<char> spotcodes)
        {
            foreach (var spotcode in spotcodes)
            {
                // Create a new DataGridTextColumn for each spot
                var column = new DataGridTextColumn
                {
                    Header = spotcode,
                    // Bind to the dictionary value
                    Binding = new Binding($"SpotCoefficients[{spotcode}]")
                    {
                        StringFormat = "N2"
                    }
                };
                dgGrid.Columns.Add(column);
            }
        }

        private void dgGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Get the item (ChannelSpotModel) for the row that was edited
            var editedItem = (ChannelSpotModel)e.Row.Item;
            char spotcoef = e.Column.Header.ToString()![0];
            // Access the edited value from the editing control
            var editingElement = e.EditingElement as TextBox; // Assuming a TextBox is used for editing

            if (editingElement != null)
            {
                if (!decimal.TryParse(editingElement.Text, out decimal newValue))
                {
                    MessageBox.Show("Invalid value!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    editingElement.Text = _previousValue.ToString();
                    return;
                }

                if (newValue != _previousValue)
                {
                    if (newValue >= 100)
                    {
                        MessageBox.Show("Invalid value!\nValue must be less than 100", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        editingElement.Text = _previousValue.ToString();
                    }
                    else
                    {
                        editedItem.SetCoef(spotcoef, newValue);
                    }
                }

            }
        }

        private void dgGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            char spotcode = e.Column.Header.ToString()[0];
            var editedItem = (ChannelSpotModel)e.Row.Item;

            if (_spotcodes.Contains(spotcode))
            {
                _previousValue = editedItem.SpotCoefficients[spotcode];
            }
        }

        public IEnumerable<CobrandDTO> GetChangedCobrands()
        {
            List<CobrandDTO> cobrands = new List<CobrandDTO>();

            var data = (List<ChannelSpotModel>)dgGrid.ItemsSource;
            foreach (var channelSpotModel in data)
            {
                foreach (var spotcode in _spotcodes)
                {
                    switch (channelSpotModel.Statuses[spotcode])
                    {
                        case -1: case 0: break;
                        default:
                            cobrands.Add(new CobrandDTO(_campaign.cmpid, channelSpotModel.Channel.chid, spotcode, channelSpotModel.SpotCoefficients[spotcode]));
                            break;
                    }
                }
            }

            return cobrands;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var data = (List<ChannelSpotModel>)dgGrid.ItemsSource;
            foreach (var channelSpotModel in data)
            {
                foreach (var spotcode in _spotcodes)
                {
                    switch (channelSpotModel.Statuses[spotcode])
                    {
                        case -1: case 0: break;
                        case 1:
                            await _cobrandController.CreateCobrand(
                                new CreateCobrandDTO(_campaign.cmpid, channelSpotModel.Channel.chid, spotcode, channelSpotModel.SpotCoefficients[spotcode]));
                            _modified = true;
                            break;
                        case 2:
                            await _cobrandController.UpdateCobrand(
                                new UpdateCobrandDTO(_campaign.cmpid, channelSpotModel.Channel.chid, spotcode, channelSpotModel.SpotCoefficients[spotcode]));
                            _modified = true;
                            break;
                        case 3:
                            await _cobrandController.DeleteCobrand(_campaign.cmpid, channelSpotModel.Channel.chid, spotcode);
                            _modified = true;
                            break;
                        default:
                            break;
                    }
                }
            }

            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
