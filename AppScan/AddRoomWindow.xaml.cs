using System;
using System.Windows;
using System.Windows.Controls;

namespace AppScan
{
    public partial class AddRoomWindow : Window
    {
        public AddRoomWindow()
        {
            InitializeComponent();
            LoadBuildings();
        }

        private void LoadBuildings()
        {
            var buildings = DatabaseHelper.GetBuildings();
            BuildingComboBox.ItemsSource = buildings;
            BuildingComboBox.DisplayMemberPath = "Name";
            BuildingComboBox.SelectedValuePath = "BuildingId";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                string roomNumber = RoomNumberTextBox.Text;
                int floor;
                if (!int.TryParse(FloorTextBox.Text, out floor))
                {
                    MessageBox.Show("Неверный формат этажа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                int buildingId = (int)BuildingComboBox.SelectedValue;

                bool success = DatabaseHelper.AddRoom(roomNumber, floor, buildingId);

                if (success)
                {
                    MessageBox.Show("Помещение успешно добавлено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении помещения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(RoomNumberTextBox.Text))
            {
                MessageBox.Show("Введите номер помещения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(FloorTextBox.Text))
            {
                MessageBox.Show("Введите этаж", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (BuildingComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите здание", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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