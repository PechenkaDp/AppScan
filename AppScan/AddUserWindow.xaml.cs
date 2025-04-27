using System;
using System.Windows;
using System.Security.Cryptography;
using System.Text;
using Npgsql;
using System.Configuration;
using Dapper;
using System.Windows.Controls;

namespace AppScan
{
    public partial class AddUserWindow : Window
    {
        private readonly string ConnectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        public AddUserWindow()
        {
            InitializeComponent();
            RoleComboBox.SelectedIndex = 2;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                string username = UsernameTextBox.Text;
                string email = EmailTextBox.Text;
                string password = PasswordBox.Password;
                int roleId = GetRoleId();

                try
                {
                    using (var connection = new NpgsqlConnection(ConnectionString))
                    {
                        connection.Open();

                        string checkQuery = "SELECT COUNT(*) FROM Users WHERE username = @username";
                        int count = connection.ExecuteScalar<int>(checkQuery, new { username });

                        if (count > 0)
                        {
                            MessageBox.Show("Пользователь с таким логином уже существует",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        string insertQuery = @"
                            INSERT INTO Users (username, password_hash, email, role_id, created_at, updated_at)
                            VALUES (@username, @passwordHash, @email, @roleId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";

                        var parameters = new
                        {
                            username,
                            passwordHash = HashPassword(password),
                            email,
                            roleId
                        };

                        connection.Execute(insertQuery, parameters);

                        string logQuery = @"
                            INSERT INTO Logs (timestamp, log_type, message)
                            VALUES (CURRENT_TIMESTAMP, 'USER_CREATED', @message)";

                        connection.Execute(logQuery, new { message = $"Created new user: {username}" });

                        MessageBox.Show("Пользователь успешно создан",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании пользователя: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Введите email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите роль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private int GetRoleId()
        {
            var selectedRole = (RoleComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            switch (selectedRole)
            {
                case "Администратор":
                    return 1;
                case "Менеджер":
                    return 2;
                case "Сотрудник":
                    return 3;
                default:
                    return 3;
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}