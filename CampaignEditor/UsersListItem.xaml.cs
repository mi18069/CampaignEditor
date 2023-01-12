using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace CampaignEditor
{

    public partial class UsersListItem : UserControl
    {
        public UsersListItem()
        {      
            InitializeComponent();
            SetImages();
        }

        private string appPath = Directory.GetCurrentDirectory();
        private string imgUserIconPath = "\\images\\UserIcon.png";
        private string imgRedXPath = "\\images\\Red_X.png";

        #region Properties

        private string _username;
        private string _userlevel;
        private Image _userIcon;
        private Button _btnUnassign;

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
            set { _userlevel = value; lblUserLevel.Content = value; }
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

        private void SetImages()
        {
            imgUserIcon.Source = new BitmapImage(new Uri(appPath + imgUserIconPath));

            Image imgRedX = new Image();
            imgRedX.Source = new BitmapImage(new Uri(appPath + imgRedXPath));
            btnUnassign.Content = imgRedX;
        }

        private void btnUnassign_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UsersOfClient.instance.UnassignUser_Click(Username);
        }
    }

}
