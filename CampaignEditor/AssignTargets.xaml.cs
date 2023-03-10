using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using Database.DTOs.TargetCmpDTO;
using Database.DTOs.TargetDTO;
using Database.Entities;
using Database.Repositories;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace CampaignEditor
{
    public partial class AssignTargets : Window
    {
        private readonly IAbstractFactory<NewTarget> _factoryNewTarget;

        private TargetController _targetController;
        private TargetCmpController _targetCmpController;
        public bool success = false;
        public bool modified = false;

        private ObservableCollection<TargetDTO> _targetsList = new ObservableCollection<TargetDTO>();
        private ObservableCollection<TargetDTO> _selectedTargetsList = new ObservableCollection<TargetDTO>();

        int maxSelected = 3;

        CampaignDTO _campaign = null;

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

        public async Task Initialize(CampaignDTO campaign)
        {
            _campaign = campaign;
            _targetsList.Clear();
            _selectedTargetsList.Clear();

            var targets = await _targetController.GetAllClientTargets(_campaign.clid);
            targets = targets.OrderBy(t => t.targname);

            var selectedTargets = await _targetCmpController.GetTargetCmpByCmpid(_campaign.cmpid);
            foreach (var selectedTarget in selectedTargets)
            {
                _selectedTargetsList.Add(await _targetController.GetTargetById(selectedTarget.targid));
            }
               

            foreach (var target in targets)
            {
                bool inSelected = false;
                foreach (var selectedTarget in selectedTargets)
                {
                    if (selectedTarget.targid == target.targid)
                        inSelected = true;
                }
                if (!inSelected)
                    _targetsList.Add(target);
            }
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
            for (int i=0; i<maxSelected; i++) 
            {
                MoveTargetToSelected(selectedList[i] as TargetDTO);
            }
        }

        private void btnFromSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedList = lbSelectedTargets.SelectedItems;
            foreach (var selected in selectedList)
            {
                MoveTargetFromSelected(selected as TargetDTO);
            }
        }
        
        private ObservableCollection<TargetDTO> OrderCollection(ObservableCollection<TargetDTO> collection)
        {
            ObservableCollection<TargetDTO> temp;
            temp = new ObservableCollection<TargetDTO>(collection.OrderBy(p => p.targname));
            collection.Clear();
            foreach (TargetDTO j in temp) 
                collection.Add(j);
            return collection;
        }
        #endregion

        private async void btnNewTarget_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factoryNewTarget.Create();
            await factory.InitializeTree();
            factory.ShowDialog();
            if (factory.success)
                await Initialize(_campaign);
        }

        private async void btnEditTarget_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factoryNewTarget.Create();
            var target = lbTargets.SelectedItems.Count > 0 ? lbTargets.SelectedItems[0] as TargetDTO :
                                                             lbSelectedTargets.SelectedItems[0] as TargetDTO;

            
            if (target != null)
            {
                var success = await factory.InitializeTargetToEdit(target);
            }

            factory.ShowDialog();

            if (factory.success)
                await Initialize(_campaign);
                
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            success = true;
            modified = true;
            await _targetCmpController.DeleteTargetCmpByCmpid(_campaign.cmpid);
            for (int i=0; i<SelectedTargetsList.Count; i++)
            {
                int cmpid = _campaign.cmpid;
                int targid = SelectedTargetsList[i].targid;
                int priority = i;
                await _targetCmpController.CreateTargetCmp(new CreateTargetCmpDTO(cmpid, targid, priority));
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

        private void CheckEdit()
        {
            if ((lbTargets.SelectedItems.Count + lbSelectedTargets.SelectedItems.Count) == 1)
                btnEditTarget.IsEnabled = true;
            else
                btnEditTarget.IsEnabled = false;
        }
        private void TargetsItem_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var target = lbTargets.SelectedItem as TargetDTO;
            if (target != null)
                FillTargetTextBlock(target.targdefi);
            CheckEdit();
        }

        private void SelectedTargetsItem_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var target = lbSelectedTargets.SelectedItem as TargetDTO;
            if (target != null)
                FillTargetTextBlock(target.targdefi);
            CheckEdit();
        }

        private async void FillTargetTextBlock(string targetdefi)
        {
            var instance = _factoryNewTarget.Create();
            string text = await instance.ParseTargetdefi(targetdefi);
            tbTargetFilters.Text = text;
        }

        private void ListViewItem_LostFocus(object sender, RoutedEventArgs e)
        {
            tbTargetFilters.Text = "";
        }

        private void lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckEdit();
        }
    }
    
}
