using CampaignEditor.Controllers;
using CampaignEditor.StartupHelpers;
using Database.DTOs.TargetDTO;
using Database.Entities;
using Database.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;

namespace CampaignEditor
{
    public partial class AssignTargets : Window
    {
        private readonly IAbstractFactory<NewTarget> _factoryNewTarget;

        private TargetController _targetController;
        public bool success = false;
        private bool canBeEdited = false;

        private ObservableCollection<TargetDTO> _targetsList = new ObservableCollection<TargetDTO>();
        private ObservableCollection<TargetDTO> _selectedTargetsList = new ObservableCollection<TargetDTO>();

        int maxSelected = 3;

        public bool CanBeEdited { get; }
        public ObservableCollection<TargetDTO> TargetsList { get { return _targetsList; } }
        public ObservableCollection<TargetDTO> SelectedTargetsList { get { return _selectedTargetsList; } }

        public AssignTargets(ITargetRepository targetRepository, IAbstractFactory<NewTarget> factoryNewTarget)
        {
            _targetController = new TargetController(targetRepository);
            _ = InitializeListsAsync();
            _factoryNewTarget = factoryNewTarget;

            InitializeComponent();
            this.DataContext = this;
        }

        private async Task InitializeListsAsync()
        {
            _targetsList.Clear();
            _selectedTargetsList.Clear();

            var targets = await _targetController.GetAllTargets();
            targets = targets.OrderBy(t => t.targname);

            foreach (var target in targets)
            {
                _targetsList.Add(target);
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

        private ObservableCollection<TargetDTO> OrderCollection(ObservableCollection<TargetDTO> collection)
        {
            ObservableCollection<TargetDTO> temp;
            temp = new ObservableCollection<TargetDTO>(collection.OrderBy(p => p.targname));
            collection.Clear();
            foreach (TargetDTO j in temp) 
                collection.Add(j);
            return collection;
        }

        private async void btnNewTarget_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factoryNewTarget.Create();
            await factory.InitializeTree();
            factory.ShowDialog();
            if (factory.success)
                _ = InitializeListsAsync();
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
                _ = InitializeListsAsync();
                
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            success = true;
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
                maxSelected++;
            }
        }

        private void TargetsItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (maxSelected > 0)
            {
                var target = lbTargets.SelectedItem as TargetDTO;
                if (target != null)
                {
                    MoveTargetToSelected(target);
                    maxSelected--;
                }
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
            CheckEdit();
        }
    }
    
}
