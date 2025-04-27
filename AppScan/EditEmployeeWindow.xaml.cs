using System;
using System.Windows;

namespace AppScan
{
    public partial class EditEmployeeWindow : Window
    {
        private Employee _employee;

        public EditEmployeeWindow(Employee employee)
        {
            InitializeComponent();
            _employee = employee;
            PopulateFields();
        }

        private void PopulateFields()
        {
            LastNameTextBox.Text = _employee.LastName;
            FirstNameTextBox.Text = _employee.FirstName;
            MiddleNameTextBox.Text = _employee.MiddleName;
            TabNumberTextBox.Text = _employee.TabNumber;
            PositionTextBox.Text = _employee.Position;
            DepartmentTextBox.Text = _employee.Department;
            PhoneTextBox.Text = _employee.Phone;
            EmailTextBox.Text = _employee.Email;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                string lastName = LastNameTextBox.Text;
                string firstName = FirstNameTextBox.Text;
                string middleName = MiddleNameTextBox.Text;
                string tabNumber = TabNumberTextBox.Text;
                string position = PositionTextBox.Text;
                string department = DepartmentTextBox.Text;
                string phone = PhoneTextBox.Text;
                string email = EmailTextBox.Text;

                bool success = DatabaseHelper.UpdateEmployee(_employee.EmployeeId, lastName, firstName,
                                                          middleName, tabNumber, position,
                                                          department, phone, email);

                if (success)
                {
                    MessageBox.Show("Сотрудник успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при обновлении сотрудника", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                MessageBox.Show("Введите фамилию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                MessageBox.Show("Введите имя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TabNumberTextBox.Text))
            {
                MessageBox.Show("Введите табельный номер", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(PositionTextBox.Text))
            {
                MessageBox.Show("Введите должность", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(DepartmentTextBox.Text))
            {
                MessageBox.Show("Введите отдел", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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