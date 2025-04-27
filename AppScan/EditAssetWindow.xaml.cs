using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace AppScan
{
    public partial class EditAssetWindow : Window
    {
        private Asset _asset;

        public EditAssetWindow(Asset asset)
        {
            InitializeComponent();
            _asset = asset;
            LoadComboBoxes();
            PopulateFields();
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

        private void PopulateFields()
        {
            ModelComboBox.SelectedValue = _asset.ModelId;
            InventoryNumberTextBox.Text = _asset.InventoryNumber;
            SerialNumberTextBox.Text = _asset.SerialNumber;
            AssetNumberTextBox.Text = _asset.AssetNumber;
            BarcodeTextBox.Text = _asset.Barcode;
            EmployeeComboBox.SelectedValue = _asset.EmployeeId;
            DepartmentTextBox.Text = _asset.Department;
            RoomComboBox.SelectedValue = _asset.RoomId;
            StatusComboBox.SelectedValue = _asset.Status;
            IssueDatePicker.SelectedDate = _asset.IssueDate;
            AccountingInfoTextBox.Text = _asset.AccountingInfo;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                _asset.ModelId = (int)ModelComboBox.SelectedValue;
                _asset.InventoryNumber = InventoryNumberTextBox.Text;
                _asset.SerialNumber = SerialNumberTextBox.Text;
                _asset.AssetNumber = AssetNumberTextBox.Text;
                _asset.Barcode = BarcodeTextBox.Text;
                _asset.EmployeeId = (int?)EmployeeComboBox.SelectedValue;
                _asset.Department = DepartmentTextBox.Text;
                _asset.RoomId = (int?)RoomComboBox.SelectedValue;
                _asset.Status = ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString();
                _asset.IssueDate = IssueDatePicker.SelectedDate;
                _asset.AccountingInfo = AccountingInfoTextBox.Text;

                bool success = DatabaseHelper.EditAsset(
                    _asset.AssetId,
                    _asset.ModelId.ToString(),
                    _asset.Barcode,
                    _asset.SerialNumber
                );

                if (success)
                {
                    MessageBox.Show("Актив успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при обновлении актива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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