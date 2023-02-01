﻿using CampaignEditor.Controllers;
using CampaignEditor.Repositories;
using Database.DTOs.TargetClassDTO;
using Database.DTOs.TargetValueDTO;
using Database.Repositories;
using System.Collections.Generic;
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

            List<TreeViewModel> treeViewList = new List<TreeViewModel>();

            foreach (TargetClassDTO node in nodes.Keys)
            {

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
        #endregion

    }
}
