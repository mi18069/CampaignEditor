using AutoMapper.Configuration.Conventions;
using CampaignEditor.Controllers;
using Database.DTOs.BrandDTO;
using System;
using System.Windows.Controls;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Interaction logic for BrandsItem.xaml
    /// </summary>
    public partial class BrandsItem : UserControl
    {

        BrandDTO? _brand = null;

        public BrandDTO? Brand 
        {
            get { return _brand; } 
            set { _brand = value; tbBrand.Text = _brand.brand.Trim(); } }

        public BrandsItem()
        {
            InitializeComponent();
        }

        public void Initialize(int num)
        {
            lblBrandNum.Content = num.ToString();
        }

        public string GetText()
        {
            return tbBrand.Text.Trim();
        }

        public void SetIndex(int index)
        {
            lblBrandNum.Content = index.ToString();
        }

        public event EventHandler TextChanged;
        private void tbBrand_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler DeleteClicked;

        private void btnDelete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Raise the event
            DeleteClicked?.Invoke(this, EventArgs.Empty);
        }


    }
}
