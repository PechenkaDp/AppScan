using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;

namespace AppScan
{
    public partial class AddAssetModelWindow : Window
    {
        public AddAssetModelWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            var manufacturers = DatabaseHelper.GetManufacturers();
            ManufacturerComboBox.ItemsSource = manufacturers;
            ManufacturerComboBox.DisplayMemberPath = "Name";
            ManufacturerComboBox.SelectedValuePath = "ManufacturerId";

            var assetTypes = DatabaseHelper.GetAssetTypes();
            AssetTypeComboBox.ItemsSource = assetTypes;
            AssetTypeComboBox.DisplayMemberPath = "Name";
            AssetTypeComboBox.SelectedValuePath = "AssetTypeId";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                string name = NameTextBox.Text;
                int manufacturerId = (int)ManufacturerComboBox.SelectedValue;
                int assetTypeId = (int)AssetTypeComboBox.SelectedValue;
                string barcode = BarcodeTextBox.Text;
                string specifications = SpecificationsTextBox.Text;

                bool success = DatabaseHelper.AddAssetModel(name, manufacturerId, assetTypeId, specifications, barcode);

                if (success)
                {
                    MessageBox.Show("Модель актива успешно добавлена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении модели актива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название модели", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (ManufacturerComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите производителя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (AssetTypeComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите тип актива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}