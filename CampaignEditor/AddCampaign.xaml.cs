using CampaignEditor.Controllers;
using CampaignEditor.DTOs.UserDTO;
using CampaignEditor.Repositories;
using Database.DTOs.UserClients;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CampaignEditor
{

    public partial class AddCampaign : Window
    {

        private CampaignController _campaignController;
        private UserController _userController;
        private ClientController _clientController;
        private UserClientsController _userClientsController;
        //private CampaignInfo _ci;

        public AddCampaign(ICampaignRepository campaignRepository, IUserRepository userRepository, 
            IClientRepository clientRepository, IUserClientsRepository userClientsRepository)
        {
            _campaignController = new CampaignController(campaignRepository);
            _userController = new UserController(userRepository);
            _clientController = new ClientController(clientRepository);
            _userClientsController = new UserClientsController(userClientsRepository);
            InitializeComponent();

            //_ci = new CampaignInfo();
            InitializeFields();
            

        }

        private void InitializeFields()
        {
            dpStartDate.SelectedDate = DateTime.Now;
            dpEndDate.SelectedDate = DateTime.Now;

            FillUsersComboBox();
        }

        private async void FillUsersComboBox()
        {
            IEnumerable<string> usernames = await _userController.GetAllUsernames();
            usernames = usernames.OrderBy(u => u);
            foreach (var username in usernames)
            {
                cbUsers.Items.Add(username);
            }

            cbUsers.SelectedItem = MainWindow.instance.user.usrname;
            if (MainWindow.instance.user.usrlevel != 0)
                cbUsers.IsEnabled = false;
        }
        private async void FillClientsComboBox()
        {
            cbClients.Items.Clear();
            if (cbUsers.SelectedIndex != -1)
            {
                var username = cbUsers.SelectedItem.ToString().Trim();
                GetAllClientsByUsername(username);
            }

        }

        private async void GetAllClientsByUsername(string username)
        {

            List<string> clients = new List<string>();
            UserDTO user = await _userController.GetUserByUsername(username);

            foreach (UserClientsDTO userClients in await _userClientsController.GetAllUserClientsByUserId(user.usrid))
            {
                var client = await _clientController.GetClientById(userClients.cliid);
                clients.Add(client.clname);
            }

            clients.Sort();
            foreach (string clientname in clients)
                cbClients.Items.Add(clientname);

            cbClients.SelectedIndex = 0;

            if (clients.Count <= 0)
                cbClients.IsEnabled = false;
            else
                cbClients.IsEnabled = true;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            lblError.Content = "";

            string campaignname = tbName.Text.Trim();

            DateTime startDate = (DateTime)dpStartDate.SelectedDate!;
            DateTime endDate = (DateTime)dpEndDate.SelectedDate!;

            if (!(int.TryParse(tbTbStartHours.Text, out int startHours) && int.TryParse(tbTbStartMinutes.Text, out int startMinutes) && 
                int.TryParse(tbTbEndHours.Text, out int endHours) && int.TryParse(tbTbEndMinutes.Text, out int endMinutes)))
            {
                lblError.Content = "Unsupported TB time format";
            }
            else
            {
                startDate = startDate.AddHours(startHours).AddMinutes(startMinutes);
                endDate = endDate.AddHours(endHours).AddMinutes(endMinutes);
            }

            if (cbUsers.SelectedIndex == -1)
            {
                lblError.Content = "User must be selected";
            }
            else if (cbClients.SelectedIndex == -1)
            {
                lblError.Content = "Client not selected";
            }
            else if (campaignname == "")
            {
                lblError.Content = "Enter campaign name";
            }
            else if (await _campaignController.GetCampaignByName(campaignname) != null)
            {
                lblError.Content = "Campaign name already exists";
            }
            else if (startDate >= endDate)
            {
                lblError.Content = "Start time must be before end time";
            }
            else
            {
                string username = cbUsers.SelectedValue.ToString()!.Trim();
                string clientname = cbClients.SelectedValue.ToString()!.Trim();
                AddNewCampaign(username, clientname, campaignname, startDate, endDate);
            }         

        }

        private void AddNewCampaign(string username, string clientname, string campaignname, DateTime startDate, DateTime endDate)
        {
            
        }

        private string ConvertDateTimeToDateString(DateTime dateTime)
        {
            string year = dateTime.Year.ToString("0000");
            string month = dateTime.Month.ToString("00");
            string day = dateTime.Day.ToString("00");

            return year + month + day;
        }
        private string ConvertDateTimeToTimeString(DateTime dateTime)
        {
            string hour = dateTime.Hour.ToString("00");
            string minute = dateTime.Minute.ToString("00");
            string second = dateTime.Second.ToString("00");

            return hour + minute + second;
        }

        private void cbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillClientsComboBox();
        }

        private void TxtBoxes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).MaxLength == ((TextBox)sender).Text.Length)
            {
                // move focus to the next
                var ue = e.OriginalSource as FrameworkElement;
                e.Handled = true;
                ue.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
