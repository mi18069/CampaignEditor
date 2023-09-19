using CampaignEditor.Controllers;
using CampaignEditor.DTOs.CampaignDTO;
using CampaignEditor.StartupHelpers;
using Database.DTOs.TargetCmpDTO;
using Database.DTOs.TargetDTO;
using Database.Repositories;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
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
        public bool targetsModified = false;
        public bool shouldClose = false;

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

        public async Task Initialize(CampaignDTO campaign, List<TargetDTO> selectedlist = null, bool initialize = false)
        {
            _campaign = campaign;

            if (selectedlist == null || initialize == true)
            {
                _targetsList.Clear();
                _selectedTargetsList.Clear();
                var targets = await _targetController.GetAllClientTargets(_campaign.clid);
                targets = targets.OrderBy(t => t.targown != 0).ThenBy(t => t.targname);
                var selectedTargets = await _targetCmpController.GetTargetCmpByCmpid(_campaign.cmpid);
                selectedTargets.OrderBy(s => s.priority);
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
                    {
                        _targetsList.Add(target);
                    }
                }
            }
            else
            {
                // First return all elements to targetsList, then by order place it in selected
                while (SelectedTargetsList.Count() > 0)
                {
                    MoveTargetFromSelected(SelectedTargetsList[0]); // n times move element from first location
                }

                foreach (var selectedTarget in selectedlist)
                {
                    for (int i = 0; i < TargetsList.Count(); i++)
                    {
                        if (TargetsList[i].targid == selectedTarget.targid)
                        {
                            MoveTargetToSelected(TargetsList[i]);
                            i--;
                        }
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
                await Initialize(_campaign, SelectedTargetsList.ToList(), true);
        }

        private async void btnEditTarget_Click(object sender, RoutedEventArgs e)
        {
            var factory = _factoryNewTarget.Create();
            var target = lbTargets.SelectedItems.Count > 0 ? lbTargets.SelectedItems[0] as TargetDTO :
                                                             lbSelectedTargets.SelectedItems[0] as TargetDTO;

            
            if (target != null)
            {
                var success = await factory.InitializeTargetToEdit(_campaign, target);
            }

            factory.ShowDialog();

            if (factory.success)
                await Initialize(_campaign);
                
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            targetsModified = true;
            await UpdateDatabase(SelectedTargetsList.ToList());
            this.Hide();
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
            this.Hide();
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
            if ((lbTargets.SelectedItems.Count + lbSelectedTargets.SelectedItems.Count) == 1)
                btnEditTarget.IsEnabled = true;
            else
                btnEditTarget.IsEnabled = false;
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

        #endregion

        // Overriding OnClosing because click on x button should only hide window
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!shouldClose)
            {
                e.Cancel = true;
                Hide();
            }

        }
    }

}
