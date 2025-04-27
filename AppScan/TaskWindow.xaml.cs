using System;
using System.Windows;
using System.Windows.Controls;

namespace AppScan
{
    public partial class TaskWindow : Window
    {
        private int _userId;

        public TaskWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadTasks();
        }

        private void LoadTasks()
        {
            var tasks = DatabaseHelper.GetTasksForEmployee(_userId); 
            TasksDataGrid.ItemsSource = tasks;
        }

        private void UpdateStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (TasksDataGrid.SelectedItem is Task selectedTask)
            {
                var selectedStatus = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                if (!string.IsNullOrEmpty(selectedStatus))
                {
                    selectedTask.Status = selectedStatus;
                    DatabaseHelper.UpdateTaskStatus(selectedTask.TaskId, selectedStatus); 
                    LoadTasks(); 
                }
                else
                {
                    MessageBox.Show("Please select a status.");
                }
            }
            else
            {
                MessageBox.Show("Please select a task to update.");
            }
        }
    }
}
