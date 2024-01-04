using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.TargetClassDTO;
using Database.DTOs.TargetDTO;
using Database.DTOs.TargetValueDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public List<TreeViewModel> treeViewList;

        private TargetDTO targetToEdit = null;

        private CampaignDTO _campaign = null;

        public bool success = false;

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
        }

        #region TargetTree

        public async Task InitializeTree(CampaignDTO campaign)
        {
            _campaign = campaign;
            var treeResult = await SetTree();

            bool isAdmin = MainWindow.user.usrlevel == 0;
            chbGlobal.Visibility = isAdmin ? Visibility.Visible : Visibility.Hidden;
            if (chbGlobal.Visibility == Visibility.Visible && targetToEdit != null)
                chbGlobal.IsChecked = targetToEdit.targown == 0;

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

        public async Task<bool> InitializeTargetToEdit(CampaignDTO campaign, TargetDTO target)
        {
            targetToEdit = target;
            await InitializeTree(campaign);
            tbName.Text = target.targname.Trim();
            btnSaveAs.Visibility = Visibility.Visible;         
            
            var res = await CheckTreeUsingTargetdefi(target.targdefi);
            PrintInTbDescription();

            if (res == false)
                return false;
            else
                return true;
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
                if (parent.IsChecked != false)
                {
                    row.Append(parent.Name.Trim() + "(");
                }
                int i = 0;
                foreach (TreeViewModel child in parent.Children)
                {
                    if (child.IsChecked != false)
                    {
                        if (i != 0)
                        {
                            row.Append(",");
                        }
                        row.Append(child.Name.Trim());
                        i++;
                    }
                }
                row.Append(')');

                if (parent.IsChecked != false)
                    selectedStrings.Add(row.ToString());
            }

            return selectedStrings;
        }
        public void PrintInTbDescription()
        {
            tbDescription.Text = "";
            List<string> selectedStrings = GetSelectedStrings();
            StringBuilder sb = new StringBuilder("");

            if (cbAgeRange.IsChecked == true)
            {
                string from = tbFrom.Text;
                string to = tbTo.Text;
                string ageString = "Age Range(" + from + "-" + to + ")";
                selectedStrings.Add(ageString);
            }
            foreach (string str in selectedStrings)
            {
                sb.Append(str + "&");
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1); // removes last "&"

            tbDescription.Text = sb.ToString();
        }
        private void tb_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            PrintInTbDescription();
        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            PrintInTbDescription();
        }

        #endregion

        #region Targets 

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (targetToEdit == null)
                AddNewTarget();
            else
                UpdateTarget();
        }
        private void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            AddNewTarget();
        }

        private async void AddNewTarget()
        {
            if (await CheckValues(true))
            {
                string targname = tbName.Text.Trim();
                int targown = _campaign.clid;
                if ((bool)chbGlobal.IsChecked)
                    targown = 0;
                string targdesc = tbDescription.Text.Trim();
                string targdefi = ParseSelectedTargdefi();
                string targdefp = ParseSelectedTargdefp();

                _ = await _targetController.CreateTarget(new CreateTargetDTO(targname, targown, 
                                                                         targdesc, targdefi, targdefp));
                success = true;
                this.Close();
            }
        }

        private async void UpdateTarget()
        {
            if (await CheckValues())
            {
                int targid = targetToEdit.targid;
                string targname = tbName.Text.Trim();
                int targown = _campaign.clid;
                if ((bool)chbGlobal.IsChecked)
                    targown = 0;
                string targdesc = tbDescription.Text.Trim();
                string targdefi = ParseSelectedTargdefi();
                string targdefp = ParseSelectedTargdefp();

                _ = await _targetController.UpdateTarget(new UpdateTargetDTO(targid, targname, targown,
                                                         targdesc, targdefi, targdefp));
                success = true;
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
                        if (AgeRange != "")
                        {
                            string prefix = j == 0 ? "" : " ";
                            sbTargetdefi.Append(prefix + AgeRange);
                        }
                        k++;
                    }

                    if (child.IsChecked != false)
                    {
                        if (i != 0)
                        {
                            sbTargetdefi.Append(" ");
                        }
                        string childid = (child.Item as TargetValueDTO).value;
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

        /*private string ParseSelectedTargdefp()
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
                        if (AgeRange != "")
                        {
                            string prefix = j == 0 ? "" : "&;";
                            sbTargetdefp.Append(prefix + AgeRange);
                        }
                        k++;
                    }

                    if (child.IsChecked != false)
                    {
                        if (i != 0)
                        {
                            sbTargetdefp.Append("&;");
                        }
                        int id = ConvertToPlaceInTargetdefp(parentid);
                        string childid = (child.Item as TargetValueDTO).value;
                        sbTargetdefp.Append("C;" + id + ",1" + ";INL," + childid + ";100;");
                        i++;
                        j++;
                    }
                }
            }
            if (sbTargetdefp.ToString() == "I;#;Y;N;&;")
                return "I;";

            return sbTargetdefp.ToString();
        }*/

        private string ParseSelectedTargdefp()
        {
            StringBuilder sbTargetdefp = new StringBuilder("I;#;Y;N;"); 
            bool firstOver10 = true;
            foreach (TreeViewModel parent in treeViewList)
            {
                bool firstValue = true;
                foreach (TreeViewModel child in parent.Children)
                {

                    int parentid = (parent.Item as TargetClassDTO).demoid;
                    if (parentid > 10 && firstOver10) // because there are no parent with demoid == 10
                    {
                        string AgeRange = ParseAgeRangeTargetdefp();
                        if (AgeRange != "")
                        {
                            sbTargetdefp.Append("&;" + AgeRange);
                        }
                        firstOver10 = false;
                    }

                    if (child.IsChecked != false)
                    {
                        if (firstValue)
                        {
                            int id = ConvertToPlaceInTargetdefp(parentid);
                            string childid = (child.Item as TargetValueDTO).value;
                            sbTargetdefp.Append("&;C;" + id + ",1" + ";INL," + childid + ";100;");
                            firstValue = false;
                        }
                        else
                        {
                            sbTargetdefp.Length = sbTargetdefp.Length - 5; // removing ";100;"
                            string childid = (child.Item as TargetValueDTO).value;
                            sbTargetdefp.Append("," + childid + ";100;");
                        }

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
                string from = tbFrom.Text.PadLeft(3, '0');
                string to = tbTo.Text.PadLeft(3, '0');
                return "R;11,3;BET," + from + "," + to + ";100;";
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

        public async Task<string> ParseTargetdefi(string targetdefi)
        {
            // Trimming [ and ][]
            targetdefi = targetdefi.Trim();
            if (targetdefi.Length < 4 || !CheckTargetdefiFormat(targetdefi))
                return "";
            targetdefi = targetdefi.Substring(1, targetdefi.Length - 4);
            
            Regex regex = new Regex(@"\S+");
            StringBuilder sb = new StringBuilder("");

            var matches = regex.Matches(targetdefi);
            int lastClassId = -1;
            foreach (var match in matches)
            {
                // match = num.num || num.range
                string[] splitted = match.ToString()!.Split('.');
                int classid = Convert.ToInt32(splitted[0]);
                var classDTO = await _targetClassController.GetTargetClassById(classid);

                // if class is the same as last, there's no need to write it's name again
                if (lastClassId != classid) 
                { 
                    sb.Append(classDTO.name.Trim() + ":\n");
                    lastClassId = classid;
                }

                if (classDTO.type == "S")
                {
                    string value = splitted[1];
                    var valueDTO = await _targetValueController.GetTargetValueByIdAndValue(classid, value);
                    sb.Append(' ', 5);
                    sb.Append(valueDTO.name.Trim() + "\n");
                }
                else if (classDTO.type == "R")
                {
                    sb.Append(' ', 5);
                    sb.Append(splitted[1]);
                }
            }

            return sb.ToString();
        }

        private async Task<bool> CheckTreeUsingTargetdefi(string targetdefi)
        {
            // Trimming [ and ][]
            targetdefi = targetdefi.Trim();
            if (targetdefi.Length < 4 || !CheckTargetdefiFormat(targetdefi))
                return false;

            targetdefi = targetdefi.Substring(1, targetdefi.Length - 4);

            Regex regex = new Regex(@"\S+");
            //StringBuilder sb = new StringBuilder("");

            var matches = regex.Matches(targetdefi);
            foreach (var match in matches)
            {
                // match = num.num || num.range
                string[] splitted = match.ToString()!.Split('.');
                int classid = Convert.ToInt32(splitted[0]);
                var classDTO = await _targetClassController.GetTargetClassById(classid);

                if (classDTO.type == "S")
                {
                    string value = splitted[1];
                    var valueDTO = await _targetValueController.GetTargetValueByIdAndValue(classid, value);
                    CheckInTree(classDTO, valueDTO);
                }
                else if (classDTO.type == "R")
                {
                    string from = (splitted[1].Split('-'))[0];
                    string to = (splitted[1].Split('-'))[1];

                    cbAgeRange.IsChecked = true;
                    tbFrom.Text = from;
                    tbTo.Text = to;
                }
            }
            return true;
        }

        private void CheckInTree(TargetClassDTO classDTO, TargetValueDTO valueDTO)
        {
            foreach (TreeViewModel parent in tvTargets.ItemsSource)
            {
                if (parent.Name == classDTO.name)
                    foreach (TreeViewModel child in parent.Children)
                    {
                        if (child.Name == valueDTO.name)
                        {
                            child.IsChecked = true;
                        }
                    }
            }
        }

        private bool CheckTargetdefiFormat(string targetdefi)
        {
            Regex checkFormat = new Regex(@"\[.*\]\[\]");
            if (checkFormat.IsMatch(targetdefi))
                return true;
            return false;
        }

        #endregion

        private async Task<bool> CheckValues(bool checkname = false)
        {
            lblError.Content = "";
            if (tbName.Text == "")
            {
                lblError.Content = "Enter name";
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
            else if (checkname && await _targetController.CheckClientTargetName(tbName.Text, _campaign.clid))
            {
                lblError.Content = "Target name already exist";
                return false;
            }
            return true;
        }



        #endregion

        
    }
}
