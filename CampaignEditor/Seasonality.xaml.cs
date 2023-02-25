using CampaignEditor.Controllers;
using Database.DTOs.SeasonalitiesDTO;
using Database.DTOs.SectablesDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace CampaignEditor
{
    public partial class Seasonality : Window
    {
        private SeasonalitiesController _seasonalitiesController;
        public Seasonality(ISeasonalitiesRepository seasonalitiesRepository)
        {
            InitializeComponent();

            _seasonalitiesController = new SeasonalitiesController(seasonalitiesRepository);
            FillBySeasIdAsync(6);
        }

        private async Task FillBySeasIdAsync(int id)
        {
            List<SeasonalitiesDTO> seascoefs = (List<SeasonalitiesDTO>)await _seasonalitiesController.GetSeasonalitiesById(id);
            dgSeasonalities.ItemsSource = seascoefs;
        }
    }
}
