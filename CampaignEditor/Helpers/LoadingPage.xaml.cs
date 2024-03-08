using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor.Helpers
{
    /// <summary>
    /// Interaction logic for LoadingPage.xaml
    /// </summary>
    public partial class LoadingPage : Page
    {
        public LoadingPage()
        {
            InitializeComponent();
        }

        public void SetContent(string content)
        {
            tbMessage.Text = content;
        }

        public void SetProgressBarVisibility(Visibility visibility)
        {
            pbBar.Visibility = visibility;
        }

        public void SetProgressBarValue(int value)
        {
            pbBar.Value = value;
        }

    }
}
