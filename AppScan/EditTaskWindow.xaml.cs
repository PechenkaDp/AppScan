using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace AppScan
{
    public partial class EditTaskWindow : Window
    {
        private Task _task;

        public EditTaskWindow(Task task)
        {
            InitializeComponent();
            _task = task;
            LoadEmployees();
            PopulateFields();
        }

        private void LoadEmployees()
        {
            var users = DatabaseHelper.GetEmployeeUsers();
            AssignedToComboBox.ItemsSource = users;
            AssignedToComboBox.DisplayMemberPath = "Username";
            AssignedToComboBox.SelectedValuePath = "UserId";
        }

        private void PopulateFields()
        {
            TitleTextBox.Text = _task.Title;
            DescriptionTextBox.Text = _task.Description;
            AssignedToComboBox.SelectedValue = _task.AssignedTo;

            var priorityItem = PriorityComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == _task.Priority);
            if (priorityItem != null)
                PriorityComboBox.SelectedItem = priorityItem;

            var statusItem = StatusComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == _task.Status);
            if (statusItem != null)
                StatusComboBox.SelectedItem = statusItem;

            DueDatePicker.SelectedDate = _task.DueDate;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                _task.Title = TitleTextBox.Text;
                _task.Description = DescriptionTextBox.Text;
                _task.AssignedTo = (int)AssignedToComboBox.SelectedValue;
                _task.Priority = ((ComboBoxItem)PriorityComboBox.SelectedItem).Content.ToString();
                _task.Status = ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString();
                _task.DueDate = DueDatePicker.SelectedDate;

                bool success = DatabaseHelper.UpdateTask(_task);

                if (success)
                {
                    MessageBox.Show("Заявка успешно обновлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при обновлении заявки", "Ошибка",
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

            if (StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!DueDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите срок выполнения", "Ошибка",
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