using System;
using System.Windows;
using System.Windows.Controls;

namespace AppScan
{
    public partial class AddAssetWindow : Window
    {
        public AddAssetWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
            IssueDatePicker.SelectedDate = DateTime.Today;
        }

        private void LoadComboBoxes()
        {
            var models = DatabaseHelper.GetAssetModels();
            ModelComboBox.ItemsSource = models;
            ModelComboBox.DisplayMemberPath = "Name";
            ModelComboBox.SelectedValuePath = "ModelId";

            var employees = DatabaseHelper.GetEmployees();
            EmployeeComboBox.ItemsSource = employees;
            EmployeeComboBox.DisplayMemberPath = "LastName";
            EmployeeComboBox.SelectedValuePath = "EmployeeId";

            var rooms = DatabaseHelper.GetRooms();
            RoomComboBox.ItemsSource = rooms;
            RoomComboBox.DisplayMemberPath = "RoomNumber";
            RoomComboBox.SelectedValuePath = "RoomId";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                var model = (AssetModel)ModelComboBox.SelectedItem;
                string barcode = BarcodeTextBox.Text;
                string serialNumber = SerialNumberTextBox.Text;

                bool success = DatabaseHelper.AddAsset(model.Name, barcode, serialNumber);

                if (success)
                {
                    MessageBox.Show("Актив успешно добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении актива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            if (ModelComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите модель актива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(InventoryNumberTextBox.Text))
            {
                MessageBox.Show("Введите инвентарный номер", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(SerialNumberTextBox.Text))
            {
                MessageBox.Show("Введите серийный номер", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус актива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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