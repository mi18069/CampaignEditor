using CampaignEditor.Controllers;
using Database.DTOs.TargetDTO;
using Database.Repositories;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CampaignEditor
{
    public partial class AssignTargets : Window
    {
        private TargetController _targetController;

        private ObservableCollection<TargetDTO> _targetsList = new ObservableCollection<TargetDTO>();
        private ObservableCollection<TargetDTO> _selectedTargetsList = new ObservableCollection<TargetDTO>();

        int maxSelected = 3;

        public ObservableCollection<TargetDTO> TargetsList { get { return _targetsList; } }
        public ObservableCollection<TargetDTO> SelectedTargetsList { get { return _selectedTargetsList; } }

        public AssignTargets(ITargetRepository targetRepository)
        {
            _targetController = new TargetController(targetRepository);
            _ = InitializeListsAsync();

            InitializeComponent();
            this.DataContext = this;
        }

        private async Task InitializeListsAsync()
        {
            var targets = await _targetController.GetAllTargets();
            targets = targets.OrderBy(t => t.targname);

            foreach (var target in targets)
            {
                _targetsList.Add(target);
            }

        }

        private void lbTargets_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DependencyObject obj = (DependencyObject)e.OriginalSource;

            while (obj != null && obj != lbTargets)
            {
                if (obj.GetType() == typeof(ListViewItem))
                {
                    // Do something here
                    if (maxSelected > 0)
                    {
                        var target = lbTargets.SelectedItem as TargetDTO;
                        if (target != null)
                        {
                            MoveTargetToSelected(target);
                            maxSelected--;
                        }
                    }

                    break;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }

        }

        private void lbSelectedTargets_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DependencyObject obj = (DependencyObject)e.OriginalSource;

            while (obj != null && obj != lbTargets)
            {
                if (obj.GetType() == typeof(ListViewItem))
                {
                    var target = lbSelectedTargets.SelectedItem as TargetDTO;
                    if (target != null)
                    {
                        MoveTargetFromSelected(target);
                        maxSelected++;
                    }
                    
                    break;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
        }
        private void MoveTargetToSelected(TargetDTO target)
        {
            _targetsList.Remove(target);
            _selectedTargetsList.Add(target);
        }
        private void MoveTargetFromSelected(TargetDTO target)
        {
            _selectedTargetsList.Remove(target);
            _targetsList.Add(target);
            OrderCollection(_targetsList);
        }

        private void btnToSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = lbTargets.SelectedItems;
            while (selectedList.Count > 0 && maxSelected > 0) 
            {
                MoveTargetToSelected(selectedList[0] as TargetDTO);
                maxSelected--;
            }
        }

        private void btnFromSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = lbSelectedTargets.SelectedItems;
            while (selectedList.Count > 0)
            {
                MoveTargetFromSelected(selectedList[0] as TargetDTO);
                maxSelected++;
            }
        }

        public ObservableCollection<TargetDTO> OrderCollection(ObservableCollection<TargetDTO> collection)
        {
            ObservableCollection<TargetDTO> temp;
            temp = new ObservableCollection<TargetDTO>(collection.OrderBy(p => p.targname));
            collection.Clear();
            foreach (TargetDTO j in temp) 
                collection.Add(j);
            return collection;
        }
    }
    
}
