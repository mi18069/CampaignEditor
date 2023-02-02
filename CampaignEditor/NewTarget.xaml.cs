using CampaignEditor.Controllers;
using CampaignEditor.Repositories;
using Database.DTOs.TargetClassDTO;
using Database.DTOs.TargetDTO;
using Database.DTOs.TargetValueDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
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
        List<TreeViewModel> treeViewList;

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

        #region TargetTree

        private async void InitializeTree()
        {
            var treeResult = await SetTree();
            tvTargets.Items.Clear();

            tvTargets.ItemsSource = treeResult;
        }
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
                    row.Append(parent.Name.Trim() + ":\n");
                }
                int i = 0;
                foreach (TreeViewModel child in parent.Children)
                {
                    if (child.IsChecked != false)
                    {
                        if (i != 0)
                        {
                            row.Append(",\n");
                        }
                        row.Append(' ', 5);
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

        #region Targets 

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddNewTarget();
        }

        private async void AddNewTarget()
        {
            if (await CheckValues())
            {
                string targname = tbName.Text.Trim();
                int targown = AddCampaign.instance.client.clid;
                string targdesc = tbDescription.Text.Trim();
                string targdefi = ParseSelectedTargdefi();
                string targdefp = ParseSelectedTargdefp();

                await _targetController.CreateTarget(new CreateTargetDTO(targname, targown, 
                                                                         targdesc, targdefi, targdefp));
                this.Close();
            }
        }

        #region Parsers
        private string ParseSelectedTargdefi()
        {
            StringBuilder sbTargetdefi = new StringBuilder("[");
            int i = 0;
            int j = 0;
            int k = 0;
            foreach (TreeViewModel parent in treeViewList)
            {
                foreach (TreeViewModel child in parent.Children)
                {
                    
                    int parentid = (parent.Item as TargetClassDTO).demoid;
                    if (parentid > 10 && k == 0)
                    {
                        string AgeRange = ParseAgeRangeTargetdefi();
                        string prefix = j == 0 ? "" : " ";
                        sbTargetdefi.Append(prefix + AgeRange);
                        k++;
                    }

                    if (child.IsChecked != false)
                    {
                        if (i != 0)
                        {
                            sbTargetdefi.Append(" ");
                        }
                        int childid = Convert.ToInt32((child.Item as TargetValueDTO).value);
                        sbTargetdefi.Append(parentid + "." + childid);
                        i++;
                        j++;
                    }
                }
            }
            if (sbTargetdefi.ToString() == "[")
                return "";

            sbTargetdefi.Append("][]");
            return sbTargetdefi.ToString();
        }

        private string ParseAgeRangeTargetdefi()
        {
            if ((bool)cbAgeRange.IsChecked == true)
            {
                string from = tbFrom.Text;
                string to = tbTo.Text;
                return "10." + from + "-" + to;
            }
            else
                return "";
        }

        private string ParseSelectedTargdefp()
        {
            StringBuilder sbTargetdefp = new StringBuilder("I;#;Y;N;&;");
            int i = 0;
            int j = 0;
            int k = 0;
            foreach (TreeViewModel parent in treeViewList)
            {
                foreach (TreeViewModel child in parent.Children)
                {
                    
                    int parentid = (parent.Item as TargetClassDTO).demoid;
                    if (parentid > 10 && k == 0)
                    {
                        string AgeRange = ParseAgeRangeTargetdefp();
                        string prefix = j == 0 ? "" : "&;";
                        sbTargetdefp.Append(prefix + AgeRange);
                        k++;
                    }

                    if (child.IsChecked != false)
                    {
                        if (i != 0)
                        {
                            sbTargetdefp.Append("&;");
                        }
                        int convertedId = ConvertToPlaceInTargetdefp(parentid);
                        int childid = Convert.ToInt32((child.Item as TargetValueDTO).value);
                        sbTargetdefp.Append("C;" + convertedId + ",1" + ";INL," + childid + ";100;");
                        i++;
                        j++;
                    }
                }
            }
            if (sbTargetdefp.ToString() == "I;#;Y;N;&;")
                return "I;";

            return sbTargetdefp.ToString();
        }

        private string ParseAgeRangeTargetdefp()
        {
            if ((bool)cbAgeRange.IsChecked == true)
            {
                string from = tbFrom.Text;
                string to = tbTo.Text;
                return "R;11,3;BET,0" + from + ",0" + to + ";100;";
            }
            else
                return "";
        }

        private int ConvertToPlaceInTargetdefp(int parentid)
        {
            if (parentid == 10)
                return 11;
            else if (parentid == 11)
                return 14;
            else
                return parentid;
        }

        #endregion

        private async Task<bool> CheckValues()
        {
            lblError.Content = "";
            if (tbName.Text == "")
            {
                lblError.Content = "Enter name";
                return false;
            }
            else if (tbDescription.Text == "")
            {
                lblError.Content = "Enter description";
                return false;
            }
            else if((bool)cbAgeRange.IsChecked == true)
            {
                if (!tbFrom.Text.All(Char.IsDigit) || !tbTo.Text.All(Char.IsDigit))
                {
                    lblError.Content = "Enter valid age range";
                    return false;
                }
                else if (Convert.ToInt32(tbFrom.Text) > Convert.ToInt32(tbTo.Text))
                {
                    lblError.Content = "Enter valid age range";
                    return false;
                }
                else if (tbFrom.Text.Trim() == "" || tbTo.Text.Trim() == "")
                {
                    lblError.Content = "Enter valid age range";
                    return false;
                }
            }
            else if (await _targetController.GetTargetByName(tbName.Text.Trim()) != null)
            {
                lblError.Content = "Target name already exist";
                return false;
            }
            return true;
        }

        #endregion


    }
}
