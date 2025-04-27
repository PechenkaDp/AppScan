using System;
using System.Windows;

namespace AppScan
{
    public partial class EditBuildingWindow : Window
    {
        private Building _building;

        public EditBuildingWindow(Building building)
        {
            InitializeComponent();
            _building = building;
            PopulateFields();
        }

        private void PopulateFields()
        {
            NameTextBox.Text = _building.Name;
            AddressTextBox.Text = _building.Address;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                string name = NameTextBox.Text;
                string address = AddressTextBox.Text;

                bool success = DatabaseHelper.UpdateBuilding(_building.BuildingId, name, address);

                if (success)
                {
                    MessageBox.Show("Здание успешно обновлено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при обновлении здания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название здания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(AddressTextBox.Text))
            {
                MessageBox.Show("Введите адрес здания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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