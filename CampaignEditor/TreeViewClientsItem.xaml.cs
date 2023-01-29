using Database.DTOs.ClientDTO;
using System.ComponentModel;
using System.Windows.Controls;


namespace CampaignEditor
{
    public partial class TreeViewClientsItem : UserControl
    {
        public TreeViewClientsItem()
        {
            InitializeComponent();
        }

        #region Properties

        private string _description;
        private ClientDTO _item;

        [Category("Custom Props")]
        public ClientDTO Item
        {
            get { return _item; }
            set { _item = value; tbName.Text = value.clname.Trim(); }
        }

        [Category("Custom Props")]
        public string Description
        {
            get { return _description; }
            set { _description = value; lblDescription.Content = value; }
        }

        #endregion
    }
}
