using CampaignEditor.DTOs.UserDTO;
using System;
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

        public event EventHandler<UserDTO> BtnUnassignedClicked;
        public event EventHandler<UserDTO> UserLevelSelectionChanged;

        #region Properties

        private UserDTO _user;
        private Image _userIcon;
        private Button _btnUnassign;

        public bool authorizationChanged = false;      

        public UserDTO User
        {
            get { return _user; }
            set 
            { 
                _user = value;
                lblUsername.Content = _user.usrname.Trim();
                cbUserLevel.SelectedIndex = _user.usrlevel;
                authorizationChanged = false;
            }
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
            UserLevelSelectionChanged?.Invoke(this, this.User);
            authorizationChanged = true;
        }

        private void btnUnassign_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BtnUnassignedClicked?.Invoke(this, this.User);
        }
    }

}
