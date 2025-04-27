using System;
using System.Windows;

namespace AppScan
{
    public partial class EditAssetTypeWindow : Window
    {
        private AssetType _assetType;

        public EditAssetTypeWindow(AssetType assetType)
        {
            InitializeComponent();
            _assetType = assetType;
            PopulateFields();
        }

        private void PopulateFields()
        {
            NameTextBox.Text = _assetType.Name;
            DescriptionTextBox.Text = _assetType.Description;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                string name = NameTextBox.Text;
                string description = DescriptionTextBox.Text;

                bool success = DatabaseHelper.UpdateAssetType(_assetType.AssetTypeId, name, description);

                if (success)
                {
                    MessageBox.Show("Тип актива успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при обновлении типа актива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название типа актива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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