using System;
using System.Windows;

namespace AppScan
{
    public partial class AddManufacturerWindow : Window
    {
        public AddManufacturerWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                string name = NameTextBox.Text;
                string contactInfo = ContactInfoTextBox.Text;

                bool success = DatabaseHelper.AddManufacturer(name, contactInfo);

                if (success)
                {
                    MessageBox.Show("Производитель успешно добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении производителя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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