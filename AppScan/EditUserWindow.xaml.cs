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
    public partial class EditUserWindow : Window
    {
        private readonly string ConnectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;
        private readonly User _user;

        public EditUserWindow(User user)
        {
            InitializeComponent();

            if (user == null || user.UserId <= 0)
            {
                throw new ArgumentException("Invalid user data provided");
            }

            _user = user;
            System.Diagnostics.Debug.WriteLine($"EditUserWindow initialized with user: ID={_user.UserId}, Username={_user.Username}");
            LoadUserData();
        }

        private void LoadUserData()
        {
            System.Diagnostics.Debug.WriteLine($"Loading user data for ID={_user.UserId}");

            UsernameTextBox.Text = _user.Username;
            EmailTextBox.Text = _user.Email;
            IsBannedCheckBox.IsChecked = _user.IsBanned;

            switch (_user.RoleId)
            {
                case 1:
                    RoleComboBox.SelectedIndex = 0; 
                    break;
                case 2:
                    RoleComboBox.SelectedIndex = 1; 
                    break;
                case 3:
                    RoleComboBox.SelectedIndex = 2; 
                    break;
                default:
                    RoleComboBox.SelectedIndex = 2; 
                    break;
            }

            CreatedAtText.Text = $"Дата создания: {_user.CreatedAt:dd.MM.yyyy HH:mm:ss}";
            LastLoginText.Text = $"Последний вход: {(_user.LastLogin.HasValue ? _user.LastLogin.Value.ToString("dd.MM.yyyy HH:mm:ss") : "Нет данных")}";
            LoginAttemptsText.Text = $"Попыток входа: {_user.LoginAttempts}";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                try
                {
                    int roleId = GetRoleId();
                    bool isBanned = IsBannedCheckBox.IsChecked ?? false;
                    string email = EmailTextBox.Text.Trim();

                    System.Diagnostics.Debug.WriteLine($"Attempting to update user: ID={_user.UserId}, Email={email}, RoleId={roleId}, IsBanned={isBanned}");

                    bool success = DatabaseHelper.RedactUser(
                        userId: _user.UserId,
                        email: email,
                        roleId: roleId,
                        isBanned: isBanned
                    );

                    if (success)
                    {
                        MessageBox.Show("Пользователь успешно обновлен",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить информацию пользователя. Пожалуйста, проверьте введенные данные.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating user: {ex}");
                    MessageBox.Show($"Ошибка при обновлении пользователя: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Введите email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!IsValidEmail(EmailTextBox.Text.Trim()))
            {
                MessageBox.Show("Введите корректный email адрес", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите роль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
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