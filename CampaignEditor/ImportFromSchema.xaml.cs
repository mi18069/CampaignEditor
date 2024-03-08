using CampaignEditor.Controllers;
using CampaignEditor.Helpers;
using Database.DTOs.ChannelDTO;
using Database.DTOs.SchemaDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CampaignEditor
{
    /// <summary>
    /// Window that shows list of schemas, so user can directly import from database
    /// </summary>
    public partial class ImportFromSchema : Window
    {

        private SchemaController _schemaController;
        private List<ChannelDTO> _channels = new List<ChannelDTO>();
        private ObservableRangeCollection<SchemaDTO> _schemas = new ObservableRangeCollection<SchemaDTO>();
        private List<SchemaDTO> _schemasToImport = new List<SchemaDTO>();
        public bool success = false;
        public bool shouldReplace = false;
        public List<ChannelDTO> Channels
        {
            get { return _channels; }
            set { _channels = value; }
        }

        public ObservableRangeCollection<SchemaDTO> Schemas 
        { 
            get { return _schemas; } 
            set { _schemas = value; }
        }

        public List<SchemaDTO> SchemasToImport
        {
            get { return _schemasToImport; }
        }

        public ImportFromSchema(ISchemaRepository schemaRepository)
        {
            InitializeComponent();

            _schemaController = new SchemaController(schemaRepository);

            lvSchemas.ItemsSource = Schemas;
        }

        public void Initialize(IEnumerable<ChannelDTO> channels, ChannelDTO? selectedChannel = null)
        {
            Channels = channels.ToList();
            cbChannels.ItemsSource = Channels;

            if (selectedChannel != null)
            {
                int index = 0;
                foreach (var channel in Channels)
                {
                    if (channel.chid == selectedChannel.chid)
                    {
                        cbChannels.SelectedIndex = index;
                        break;
                    }
                    index++;
                }
            }
        }

        private async void cbChannel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedChannel = cbChannels.SelectedItem as ChannelDTO;
            if (selectedChannel == null)
                return;

            try
            {
                var channelSchemas = await _schemaController.GetAllChannelSchemas(selectedChannel.chid);
                Schemas.ReplaceRange(channelSchemas);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while getting channel schema\n" + ex.Message, "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lvSchemas_Loaded(object sender, RoutedEventArgs e)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(Schemas);

            dataView.SortDescriptions.Clear();
            dataView.SortDescriptions.Add(new SortDescription("stime", ListSortDirection.Ascending));
            dataView.Refresh();
        }

        // For handling sorting on clicked header
        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (GridViewColumnHeader)sender;
            string sortBy = column.Tag.ToString();

            ICollectionView dataView = CollectionViewSource.GetDefaultView(Schemas);

            ListSortDirection newDir = ListSortDirection.Ascending;

            if (dataView.SortDescriptions.Count > 0 && dataView.SortDescriptions[0].PropertyName == sortBy)
            {
                newDir = (dataView.SortDescriptions[0].Direction == ListSortDirection.Ascending)
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }

            dataView.SortDescriptions.Clear();
            dataView.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            dataView.Refresh();
        }

        private void YourListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                svListView.LineUp();
            }
            else
            {
                svListView.LineDown();
            }

            // Mark the event as handled to prevent further processing by the ListView.
            e.Handled = true;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (lvSchemas.SelectedItems.Count > 0)
            {
                foreach (SchemaDTO schema in lvSchemas.SelectedItems)
                    _schemasToImport.Add(schema);

                success = true;
                shouldReplace = chbReplaceDuplicates.IsChecked == true; 
            }

            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
