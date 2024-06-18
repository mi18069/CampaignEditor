using CampaignEditor.Controllers;
using CampaignEditor.UserControls;
using Database.DTOs.BrandDTO;
using Database.DTOs.CampaignDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CampaignEditor
{
    /// <summary>
    /// Interaction logic for ClientBrands.xaml
    /// </summary>
    public partial class ClientBrands : Window
    {
        private BrandController _brandController;
        private CmpBrndController _cmpBrndController;

        private ObservableCollection<BrandDTO> _brands = new ObservableCollection<BrandDTO>();
        public List<BrandDTO> SelectedBrands = new List<BrandDTO>();


        CampaignDTO _campaign;
        BrandsItem? selectedBrandsItem = null;

        public bool isModified = false;

        public ClientBrands(IBrandRepository brandRepository, 
            ICmpBrndRepository cmpBrndRepository)
        {
            InitializeComponent();

            _brandController = new BrandController(brandRepository);
            _cmpBrndController = new CmpBrndController(cmpBrndRepository);
        }

        public async Task Initialize(CampaignDTO campaign, IEnumerable<BrandDTO> selectedBrands)
        {
            _campaign = campaign;
            SelectedBrands = selectedBrands.ToList();
            await FillBrands(selectedBrands);
        }



        private async Task FillBrands(IEnumerable<BrandDTO> selectedBrands)
        {
            var brands = (await _brandController.GetAllBrands()).OrderBy(b => b.brand);
            _brands = new ObservableCollection<BrandDTO>(brands);
            
            var cmpBrnds = await _cmpBrndController.GetCmpBrndsByCmpId(_campaign.cmpid);

            FillSelectedBrands(selectedBrands);
        }

        private void FillSelectedBrands(IEnumerable<BrandDTO> selectedBrands)
        {
            var defaultBrandItem = new BrandsItem();
            defaultBrandItem.Initialize(0);
            lbBrands.Initialize(defaultBrandItem);
            lbBrands.BtnAddClicked += LbBrands_BtnAddClicked;
            lbBrands.Items.Remove(defaultBrandItem);

            if (selectedBrands.Count() == 0)
            {
                var brandItem = new BrandsItem();
                brandItem.Initialize(1);
                lbBrands.Items.Insert(lbBrands.Items.Count - 1, brandItem);
                brandItem.TextChanged += BrandItem_TextChanged;
                brandItem.DeleteClicked += BrandItem_DeleteClicked;
            }
            else
            {
                int i = 1;
                foreach (var brand in selectedBrands)
                {
                    var brandItem = new BrandsItem();
                    brandItem.Initialize(i);
                    brandItem.Brand = brand;
                    brandItem.TextChanged += BrandItem_TextChanged;
                    brandItem.DeleteClicked += BrandItem_DeleteClicked;
                    lbBrands.Items.Insert(lbBrands.Items.Count - 1, brandItem);

                    i++;
                }
            }
        }

        private void LbBrands_BtnAddClicked(object? sender, System.EventArgs e)
        {
            var brandsItem = lbBrands.Items[lbBrands.Items.Count - 2] as BrandsItem;

            if (brandsItem != null)
            {
                brandsItem.Initialize(lbBrands.Items.Count - 1);
                brandsItem.DeleteClicked += BrandItem_DeleteClicked;
                brandsItem.TextChanged += BrandItem_TextChanged;
                selectedBrandsItem = brandsItem;
            }
        }

        private void BrandItem_TextChanged(object? sender, System.EventArgs e)
        {
            var currentBrandItem = sender as BrandsItem;
            if (currentBrandItem != null && currentBrandItem != selectedBrandsItem) 
            {
                selectedBrandsItem = currentBrandItem;
            }

            if (selectedBrandsItem != null)
                FilterLbBrands(selectedBrandsItem.GetText());
        }

        private void FilterLbBrands(string textString)
        {
            lbBrandSuggestions.ItemsSource = null;
            if (textString != "")
            {
                Regex regex = new Regex(textString, RegexOptions.IgnoreCase);

                var brands = _brands.Where(br => regex.IsMatch(br.brand));
                lbBrandSuggestions.ItemsSource = brands;               
            }
        }

        private void BrandItem_DeleteClicked(object? sender, System.EventArgs e)
        {
            var brandsItem = sender as BrandsItem;
            if (brandsItem != null)
            {
                brandsItem.TextChanged -= BrandItem_TextChanged;
                brandsItem.DeleteClicked -= BrandItem_DeleteClicked;
                lbBrands.Items.Remove(brandsItem);
                if (brandsItem.Brand != null)
                    isModified = true;
            }

            ResetIndexes();
        }

        private void ResetIndexes()
        {
            for (int i=0; i<lbBrands.Items.Count - 1; i++)
            {
                var brandItem = lbBrands.Items[i] as BrandsItem;
                brandItem.SetIndex(i+1);
            }
        }

        private void lbBrandSuggestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var brand = lbBrandSuggestions.SelectedItem as BrandDTO;
            if (brand != null)
            {
                if (selectedBrandsItem != null)
                    selectedBrandsItem.Brand = brand;
                else if(lbBrands.Items.Count > 1)
                {
                    for (int i=0; i<lbBrands.Items.Count - 1; i++)
                    {
                        if ((lbBrands.Items[i] as BrandsItem).Brand == null)
                        {
                            selectedBrandsItem = lbBrands.Items[i] as BrandsItem;
                            selectedBrandsItem.Brand = brand;
                        }
                    }
                }

                selectedBrandsItem = null;
                lbBrandSuggestions.ItemsSource = null;

            }
            isModified = true;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (isModified)
            {
                await _cmpBrndController.DeleteBrandByCmpId(_campaign.cmpid);
                SelectedBrands.Clear();

                for (int i = 0; i < lbBrands.Items.Count - 1; i++)
                {
                    var brand = lbBrands.Items[i] as BrandsItem;
                    if (brand.Brand != null)
                    {
                        bool existsDuplicate = false;
                        for (int j=0; j<i; j++)
                        {
                            var brandBefore = lbBrands.Items[j] as BrandsItem;
                            if (brandBefore.Brand != null && 
                                brandBefore.Brand.brbrand == brand.Brand.brbrand)
                            {
                                existsDuplicate = true;
                                break;
                            }
                        }
                        if (!existsDuplicate)
                        {
                            await _cmpBrndController.CreateCmpBrnd(
                                new Database.DTOs.CmpBrndDTO.CmpBrndDTO(_campaign.cmpid, brand.Brand.brbrand));
                            SelectedBrands.Add(brand.Brand);
                        }

                    }
                }
            }
            this.Close();

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            for (int i = 0; i < lbBrands.Items.Count - 1; i++)
            {
                BrandsItem brandsItem = lbBrands.Items[i] as BrandsItem;
                brandsItem.DeleteClicked -= BrandItem_DeleteClicked;
                brandsItem.TextChanged -= BrandItem_TextChanged;
            }
            lbBrands.BtnAddClicked -= LbBrands_BtnAddClicked;
        }



    }
}
