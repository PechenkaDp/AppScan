using System;
using System.Windows;

namespace AppScan
{
    public partial class EditManufacturerWindow : Window
    {
        private Manufacturer _manufacturer;

        public EditManufacturerWindow(Manufacturer manufacturer)
        {
            InitializeComponent();
            _manufacturer = manufacturer;
            PopulateFields();
        }

        private void PopulateFields()
        {
            NameTextBox.Text = _manufacturer.Name;
            ContactInfoTextBox.Text = _manufacturer.ContactInfo;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                string name = NameTextBox.Text;
                string contactInfo = ContactInfoTextBox.Text;

                bool success = DatabaseHelper.UpdateManufacturer(_manufacturer.ManufacturerId, name, contactInfo);

                if (success)
                {
                    MessageBox.Show("Производитель успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при обновлении производителя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название производителя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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