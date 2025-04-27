using AppScan;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows;

namespace AppScan
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private static ConcurrentDictionary<string, int> _loginAttempts =
            new ConcurrentDictionary<string, int>();

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;
            string ipAddress = GetClientIPAddress();

            // Проверка частоты попыток входа
            if (IsLoginBlocked(ipAddress, username))
            {
                MessageBox.Show("Слишком много попыток входа. Подождите.");
                return;
            }

            var user = DatabaseHelper.LoginUser(username, password);

            if (user != null)
            {
                // Успешный вход
                ResetLoginAttempts(ipAddress, username);
                OpenUserWindow(user);
                this.Close();
            }
            else
            {
                // Неудачная попытка
                IncrementLoginAttempts(ipAddress, username);
                MessageBox.Show("Неверный логин или пароль");
            }
        }

        private string GetClientIPAddress()
        {
            return System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?.ToString() ?? "unknown";
        }

        private bool IsLoginBlocked(string ipAddress, string username)
        {
            string key = $"{ipAddress}_{username}";
            return _loginAttempts.TryGetValue(key, out int attempts) && attempts >= 5;
        }

        private void IncrementLoginAttempts(string ipAddress, string username)
        {
            string key = $"{ipAddress}_{username}";
            _loginAttempts.AddOrUpdate(key, 1, (k, oldValue) => oldValue + 1);
        }

        private void ResetLoginAttempts(string ipAddress, string username)
        {
            string key = $"{ipAddress}_{username}";
            _loginAttempts.TryRemove(key, out _);
        }

        private void OpenUserWindow(User user)
        {
            Window window;
            switch (user.RoleId)
            {
                case 1:
                    window = new AdminWindow();
                    break;
                case 2: 
                    window = new ManagerWindow();
                    break;
                case 3:
                    window = new EmployeeWindow(user.UserId);
                    new UserSupportTicketsWindow(user.UserId, user.Username).Show();
                    break;
                case 4:
                    window = new SupportStaffWindow(user.UserId, user.Username);
                    break;
                default:
                    MessageBox.Show("Неизвестная роль");
                    return;
            }
            window.Show();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            new RegisterWindow().Show();
            this.Close();
        }

        private string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            new ResetPasswordWindow().Show();
            Close();
        }
    }
}