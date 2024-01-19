using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using Database.DTOs.TargetCmpDTO;
using Database.DTOs.TargetDTO;
using Database.Repositories;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;

namespace CampaignEditor
{
    public partial class AssignTargets : Window
    {
        private readonly IAbstractFactory<NewTarget> _factoryNewTarget;

        private TargetController _targetController;
        private TargetCmpController _targetCmpController;
        public bool targetsModified = false;

        private ObservableCollection<TargetDTO> _targetsList = new ObservableCollection<TargetDTO>();
        private ObservableCollection<TargetDTO> _selectedTargetsList = new ObservableCollection<TargetDTO>();

        int maxSelected = 3;

        CampaignDTO _campaign = null;
        private ListView _selectedListView = null;
        public bool CanBeEdited { get; }
        public ObservableCollection<TargetDTO> TargetsList { get { return _targetsList; } }
        public ObservableCollection<TargetDTO> SelectedTargetsList { get { return _selectedTargetsList; } }


        public AssignTargets(ITargetRepository targetRepository, 
                             IAbstractFactory<NewTarget> factoryNewTarget,
                             ITargetCmpRepository targetCmpRepository)
        {
            _targetController = new TargetController(targetRepository);
            _targetCmpController = new TargetCmpController(targetCmpRepository);
            _factoryNewTarget = factoryNewTarget;
            
            InitializeComponent();

            this.DataContext = this;
        }

        public async Task Initialize(CampaignDTO campaign, List<TargetDTO> selectedlist)
        {
            _campaign = campaign;

            _targetsList.Clear();
            _selectedTargetsList.Clear();

            var targets = await _targetController.GetAllClientTargets(_campaign.clid);
            targets = targets.OrderBy(t => t.targown != 0).ThenBy(t => t.targname);

            foreach (var target in targets)
            {
                _targetsList.Add(target);
            }

            
            foreach (var selectedTarget in selectedlist)
            {
                foreach (var target in _targetsList)
                {
                    if (target.targid == selectedTarget.targid)
                    {
                        MoveTargetToSelected(target);
                        break;
                    }
                }
            }
            

            targetsModified = false;
        }

        #region From And To Selected
        private void MoveTargetToSelected(TargetDTO target)
        {
            if (SelectedTargetsList.Count < maxSelected)
            {
                _targetsList.Remove(target);
                _selectedTargetsList.Add(target);
            }
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
            for (int i=0; i<maxSelected && i<selectedList.Count; i++) 
            {
                MoveTargetToSelected(selectedList[i] as TargetDTO);
            }
        }

        private void btnFromSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = lbSelectedTargets.SelectedItems;
            for (int i = 0;  i < selectedList.Count; i++)
            {
                MoveTargetFromSelected(selectedList[i] as TargetDTO);
            }
        }
        
        private ObservableCollection<TargetDTO> OrderCollection(ObservableCollection<TargetDTO> collection)
        {
            ObservableCollection<TargetDTO> temp;
            temp = new ObservableCollection<TargetDTO>(collection.OrderBy(t => t.targown != 0).ThenBy(t => t.targname));
            collection.Clear();
            foreach (TargetDTO j in temp) 
                collection.Add(j);
            return collection;
        }

        private void SelectedTargetsItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var target = lbSelectedTargets.SelectedItem as TargetDTO;
            if (target != null)
            {
                MoveTargetFromSelected(target);
            }
        }

        private void TargetsItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            var target = lbTargets.SelectedItem as TargetDTO;
            if (target != null)
            {
                MoveTargetToSelected(target);

            }

        }

        #endregion

        private async void btnNewTarget_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factoryNewTarget.Create();
            await factory.InitializeTree(_campaign);
            factory.ShowDialog();
            if (factory.success)
            {
                if (factory.newTarget != null)
                {
                    _targetsList.Insert(0, factory.newTarget);
                }
            }
        }

        private async void btnEditTarget_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedListView.SelectedItems.Count == 0) 
            {
                MessageBox.Show("No target selected!", "Message", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            try
            {
                var factory = _factoryNewTarget.Create();
                var target = _selectedListView.SelectedItems[0] as TargetDTO;
                await factory.InitializeTargetToEdit(_campaign, target);
                factory.ShowDialog();

                if (factory.success)
                {
                    target = await _targetController.GetTargetById(target.targid);
                    int index = -1;
                    var source = (ObservableCollection<TargetDTO>)_selectedListView.ItemsSource;
                    for (int i=0; i< source.Count(); i++)
                    {
                        var viewTarget = source[i];
                        if (viewTarget.targid == target.targid)
                        {
                            index = i;
                            break;
                        }
                    }
                    if (index != -1)
                    {
                        source.RemoveAt(index);
                        source.Insert(index, target);
                        targetsModified = true;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
           

        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            targetsModified = true;
            await UpdateDatabase(SelectedTargetsList.ToList());
            this.Close();
        }

        public async Task UpdateDatabase(List<TargetDTO> targetlist)
        {
            await _targetCmpController.DeleteTargetCmpByCmpid(_campaign.cmpid);
            for (int i = 0; i < targetlist.Count; i++)
            {
                int cmpid = _campaign.cmpid;
                int targid = targetlist[i].targid;
                int priority = i;
                await _targetCmpController.CreateTargetCmp(new CreateTargetCmpDTO(cmpid, targid, priority));
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region Drag and Drop selected Targets
        private void ListViewItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is ListViewItem)
                {
                    ListViewItem draggedItem = sender as ListViewItem;
                    DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                    draggedItem.IsSelected = true;
                }
            }
        }

        private void ListViewItem_Drop(object sender, DragEventArgs e)
        {
            TargetDTO droppedData = e.Data.GetData(typeof(TargetDTO)) as TargetDTO;
            TargetDTO target = ((ListBoxItem)(sender)).DataContext as TargetDTO;

            int removedIdx = lbSelectedTargets.Items.IndexOf(droppedData);
            int targetIdx = lbSelectedTargets.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                _selectedTargetsList.Insert(targetIdx + 1, droppedData);
                _selectedTargetsList.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (_selectedTargetsList.Count + 1 > remIdx)
                {
                    _selectedTargetsList.Insert(targetIdx, droppedData);
                    _selectedTargetsList.RemoveAt(remIdx);
                }
            }
        }
        #endregion

        #region Edit button mechanism
        private void CheckEdit()
        {
            if (_selectedListView.SelectedItems.Count > 0)
                btnEditTarget.IsEnabled = true;
            else
                btnEditTarget.IsEnabled = false;
        }

        private void lb_GotFocus(object sender, RoutedEventArgs e)
        {
            _selectedListView = sender as ListView;
        }

        private void lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckEdit();
        }


        #endregion

        #region Description TextBlock
        private void TargetsItem_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var target = lbTargets.SelectedItem as TargetDTO;
            if (target != null)
                //FillTargetTextBlock(target.targdefi);
                tbDescription.Text = target.targdesc.Trim();
            CheckEdit();
        }

        private void SelectedTargetsItem_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var target = lbSelectedTargets.SelectedItem as TargetDTO;
            if (target != null)
                //FillTargetTextBlock(target.targdefi);
                tbDescription.Text = target.targdesc.Trim();
            CheckEdit();
        }

        /*private async void FillTargetTextBlock(string targetdefi)
        {
            var instance = _factoryNewTarget.Create();
            string text = await instance.ParseTargetdefi(targetdefi);
            tbTargetFilters.Text = text;
        }*/


        #endregion

 
    }

}
