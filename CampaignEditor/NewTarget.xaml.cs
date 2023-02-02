using CampaignEditor.Controllers;
using CampaignEditor.Repositories;
using Database.DTOs.TargetClassDTO;
using Database.DTOs.TargetValueDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TreeViewModels;

namespace CampaignEditor
{

    public partial class NewTarget : Window
    {
        private TargetController _targetController;
        private TargetClassController _targetClassController;
        private TargetValueController _targetValueController;
        List<TreeViewModel> treeViewList;// = new List<TreeViewModel>();

        public bool isDataRangeChecked { get; set; } = false;
        public NewTarget(ITargetRepository targetRepository, 
                         ITargetClassRepository targetClassRepository,
                         ITargetValueRepository targetValueRepository)
        {
            InitializeComponent();
            this.DataContext = this;

            _targetController = new TargetController(targetRepository);
            _targetClassController = new TargetClassController(targetClassRepository);
            _targetValueController = new TargetValueController(targetValueRepository);

            InitializeTree();
        }

        private async void InitializeTree()
        {
            var treeResult = await SetTree();
            tvTargets.Items.Clear();
            
            tvTargets.ItemsSource = treeResult;
            
        }

        #region TargetTree
        private async Task<Dictionary<TargetClassDTO, IEnumerable<TargetValueDTO>>> GetNodes()
        {

            IEnumerable<TargetClassDTO> targetClassDTONodes = await _targetClassController.GetAllTargetClasses();
            Dictionary<TargetClassDTO, IEnumerable<TargetValueDTO>> treeNodes = new Dictionary<TargetClassDTO, IEnumerable<TargetValueDTO>>();

            foreach (TargetClassDTO targetClass in targetClassDTONodes)
            {
                treeNodes[targetClass] = await _targetValueController.GetAllTargetValuesWithId(targetClass.demoid);
            }
            return treeNodes;
        }

        private async Task<List<TreeViewModel>> SetTree()
        {
            Dictionary<TargetClassDTO, IEnumerable<TargetValueDTO>> nodes = await GetNodes();

            treeViewList = new List<TreeViewModel>();

            foreach (TargetClassDTO node in nodes.Keys)
            {
                // Don't want to include range fields in treeView
                if (node.type == "R")
                {
                    continue;
                }

                TreeViewModel tv1 = new TreeViewModel(node, node.name);
                treeViewList.Add(tv1);

                foreach (TargetValueDTO subNode in nodes[node])
                {
                    tv1.Children.Add(new TreeViewModel(subNode, subNode.name));
                }

            }
            foreach (TreeViewModel tv in treeViewList)
            {
                tv.Initialize();
            }

            return treeViewList;
        }

        private void tvTargets_LostMouseCapture(object sender, MouseEventArgs e)
        {
            Dictionary<string, List<string>> treeDict = new Dictionary<string, List<string>>();
            foreach (TreeViewModel tv in tvTargets.ItemsSource)
            {
                if (tv.IsChecked != false)
                {
                    List<string> strings = TreeViewModel.GetSelected(tv);
                    treeDict[tv.Name] = strings;
                }
            }

        }

        // For writing in textBox element
        private List<string> GetSelectedStrings()
        {
            
            List<string> selectedStrings = new List<string>();
            foreach (TreeViewModel parent in treeViewList)
            {
                StringBuilder row = new StringBuilder("");
                if (parent.IsChecked != false){
                    row.Append(parent.Name.Trim() + ": ");
                }
                int i = 0;
                foreach (TreeViewModel child in parent.Children)
                {
                    if (child.IsChecked != false)
                    {
                        if (i != 0)
                        {
                            row.Append(",\n");
                            int numOfWhitespaces = parent.Name.Trim().Length + 2;
                            row.Append(' ', numOfWhitespaces);
                        }
                        row.Append(child.Name.Trim());
                        i++;
                    }
                }
                if (parent.IsChecked != false)
                    selectedStrings.Add(row.ToString());
            }

            return selectedStrings;
        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            tbSelected.Text = "";
            List<string> selectedStrings = GetSelectedStrings();
            foreach (string str in selectedStrings)
            {
                tbSelected.Text += str + "\n";
            }
        }

        #endregion


    }
}
