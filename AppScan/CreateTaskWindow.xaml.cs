using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AppScan
{
    public partial class CreateTaskWindow : Window
    {
        public CreateTaskWindow()
        {
            InitializeComponent();
            LoadEmployees();
            PriorityComboBox.SelectedIndex = 1;
            DueDatePicker.SelectedDate = DateTime.Today.AddDays(1);
        }

        private void LoadEmployees()
        {
            try
            {
                var users = DatabaseHelper.GetEmployeeUsers();
                if (users != null && users.Any())
                {
                    AssignedToComboBox.ItemsSource = users;
                    AssignedToComboBox.DisplayMemberPath = "Username";
                    AssignedToComboBox.SelectedValuePath = "UserId";
                }
                else
                {
                    MessageBox.Show("Нет доступных сотрудников для назначения задачи.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка сотрудников: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                var task = new Task
                {
                    Title = TitleTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    AssignedTo = (int)AssignedToComboBox.SelectedValue,
                    Priority = ((ComboBoxItem)PriorityComboBox.SelectedItem).Content.ToString(),
                    Status = "Новая",
                    DueDate = DueDatePicker.SelectedDate,
                    CreatedAt = DateTime.Now
                };

                bool success = DatabaseHelper.CreateTask(task);

                if (success)
                {
                    MessageBox.Show("Заявка успешно создана", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при создании заявки", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите заголовок заявки", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (AssignedToComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите исполнителя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!DueDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите срок выполнения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (DueDatePicker.SelectedDate < DateTime.Today)
            {
                MessageBox.Show("Срок выполнения не может быть в прошлом", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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