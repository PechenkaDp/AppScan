using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AppScan
{
    public partial class ManagerWindow : Window
    {
        private List<Task> _allTasks;

        public ManagerWindow()
        {
            InitializeComponent();
            StatusFilterComboBox.SelectedIndex = 0;
            LoadTasks();
        }

        private void LoadTasks()
        {
            _allTasks = DatabaseHelper.GetAllTasks();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filteredTasks = _allTasks;

            // Применяем фильтр по статусу
            var selectedStatus = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (selectedStatus != "Все" && selectedStatus != null)
            {
                string statusToFilter = "";
                switch (selectedStatus.ToLower())
                {
                    case "новая":
                        statusToFilter = "new";
                        break;
                    case "в работе":
                        statusToFilter = "in_progress";
                        break;
                    case "завершена":
                        statusToFilter = "completed";
                        break;
                    case "отменена":
                        statusToFilter = "cancelled";
                        break;
                }

                if (!string.IsNullOrEmpty(statusToFilter))
                {
                    filteredTasks = filteredTasks.Where(t =>
                        t.Status.ToLower() == selectedStatus.ToLower()).ToList();
                }
            }

            var searchText = SearchTextBox.Text.ToLower();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredTasks = filteredTasks.Where(t =>
                    t.Title.ToLower().Contains(searchText) ||
                    t.Description.ToLower().Contains(searchText)
                ).ToList();
            }

            TasksDataGrid.ItemsSource = filteredTasks;
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TasksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var isTaskSelected = TasksDataGrid.SelectedItem != null;
        }

        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateTaskWindow();
            if (window.ShowDialog() == true)
            {
                LoadTasks();
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            var selectedTask = TasksDataGrid.SelectedItem as Task;
            if (selectedTask == null)
            {
                MessageBox.Show("Выберите заявку для редактирования", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new EditTaskWindow(selectedTask);
            if (window.ShowDialog() == true)
            {
                LoadTasks();
            }
        }

        private void CancelTask_Click(object sender, RoutedEventArgs e)
        {
            var selectedTask = TasksDataGrid.SelectedItem as Task;
            if (selectedTask == null)
            {
                MessageBox.Show("Выберите заявку для отмены", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите отменить выбранную заявку?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool success = DatabaseHelper.UpdateTaskStatus(selectedTask.TaskId, "Отменена");
                if (success)
                {
                    MessageBox.Show("Заявка успешно отменена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTasks();
                }
                else
                {
                    MessageBox.Show("Ошибка при отмене заявки", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshTasks_Click(object sender, RoutedEventArgs e)
        {
            LoadTasks();
        }
    }
}