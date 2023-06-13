using System;
using System.Collections.Generic;
using System.Windows;

namespace CampaignEditor.Helpers
{
    public partial class LabelWindow : Window
    {
        public LabelWindow(string message)
        {
            InitializeComponent();
            lblMessage.Content = message;

        }
    }
}
