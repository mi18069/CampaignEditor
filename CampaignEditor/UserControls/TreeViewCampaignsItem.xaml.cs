using Database.DTOs.CampaignDTO;
using System.ComponentModel;
using System.Windows.Controls;


namespace CampaignEditor
{

    public partial class TreeViewCampaignsItem : UserControl
    {
        public TreeViewCampaignsItem()
        {
            InitializeComponent();
        }

        #region Properties

        private string _description;
        private CampaignDTO _item;

        [Category("Custom Props")]
        public CampaignDTO Item
        {
            get { return _item; }
            set { _item = value; tbName.Text = value.cmpname.Trim(); }
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
