using System;
using System.Windows;

namespace AppScan
{
    public partial class AssetWindow : Window
    {
        public AssetWindow()
        {
            InitializeComponent();
            LoadAssets();
        }

        private void LoadAssets()
        {
            var assets = DatabaseHelper.GetAssets();
            AssetsDataGrid.ItemsSource = assets;
        }

        private void AddAssetButton_Click(object sender, RoutedEventArgs e)
        {
            AddAssetModelWindow addAssetWindow = new AddAssetModelWindow();
            addAssetWindow.ShowDialog();
            LoadAssets();
        }

        private void EditAssetButton_Click(object sender, RoutedEventArgs e)
        {
            if (AssetsDataGrid.SelectedItem is Asset selectedAsset)
            {
                EditAssetWindow editAssetWindow = new EditAssetWindow(selectedAsset);
                editAssetWindow.ShowDialog();
                LoadAssets();
            }
            else
            {
                MessageBox.Show("Please select an asset to edit.");
            }
        }

        private void DeleteAssetButton_Click(object sender, RoutedEventArgs e)
        {
            if (AssetsDataGrid.SelectedItem is Asset selectedAsset)
            {
                var result = MessageBox.Show("Are you sure you want to delete this asset?", "Delete Asset", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    DatabaseHelper.DeleteAsset(selectedAsset.AssetId);
                    LoadAssets(); 
                }
            }
            else
            {
                MessageBox.Show("Please select an asset to delete.");
            }
        }
    }
}
