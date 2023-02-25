using CampaignEditor.Controllers;
using Database.DTOs.SectablesDTO;
using Database.Repositories;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace CampaignEditor
{
    public partial class Sectable : Window
    {
        private SectablesController _sectablesController;
        private SectableController _sectableController;

        private readonly Regex regInteger = new Regex("[0-9]+"); //regex that matches integers
        private readonly Regex regDouble = new Regex("[0-9]+(\\.[0-9]+)?"); //regex that matches integers

        public Sectable(ISectableRepository sectableRepository, 
            ISectablesRepository sectablesRepository)
        {
            InitializeComponent();

            _sectablesController = new SectablesController(sectablesRepository);
            _sectableController = new SectableController(sectableRepository);
            FillBySctIdAsync(1);
        }

        private async Task FillBySctIdAsync(int id)
        {
            List<SectablesDTO> seccoefs = (List<SectablesDTO>)await _sectablesController.GetSectablesById(id);
            dgSectables.ItemsSource = seccoefs;
        }

        private void tbAddFrom_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            tbAddTo.Text = tbAddFrom.Text;
        }

        private void tbDeleteFrom_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            tbDeleteTo.Text = tbDeleteFrom.Text;
        }

        private void tbIntegerOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            int num = 0;
            bool success = int.TryParse(e.Text, out num);
            e.Handled = !success;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int from = 0;
            int to = 0;
            double coef = 0;

            if (int.TryParse(tbAddFrom.Text, out from) && 
                int.TryParse(tbAddTo.Text, out to) && 
                double.TryParse(tbCoef.Text, out coef))
            {
                
            }
        }
    }
}
