using System.ComponentModel;
using System.Windows.Controls;


namespace CampaignEditor
{

    public partial class UsersListItem : UserControl
    {
        public UsersListItem()
        {      
            InitializeComponent();
        }

        #region Properties

        private string _username;
        private string _userlevel;
        private Image _userIcon;
        private Button _btnUnassign;

        public bool authorizationChanged = false;

        [Category("Custom Props")]
        public string Username
        {
            get { return _username; }
            set { _username = value.Trim() ; lblUsername.Content = value.Trim(); }
        }

        [Category("Custom Props")]
        public string Userlevel
        {
            get { return _userlevel; }      
            set { _userlevel = value; cbUserLevel.SelectedIndex = value == "0" ? 0 : value == "1" ? 1 : 2; }
        }

        [Category("Custom Props")]
        public Image Icon
        {
            get { return _userIcon; }
            set { _userIcon = value; imgUserIcon = value; }
        }

        [Category("Custom Props")]
        public Button BtnUnassign
        {
            get { return _btnUnassign; }
            set { _btnUnassign = value; btnUnassign = value; }
        }

        #endregion

        private void cbUserLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            authorizationChanged = true;
        }
    }

}
