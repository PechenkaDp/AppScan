using System.Net.Mail;
using System.Windows;
using System;

namespace AppScan
{
    public partial class ResetPasswordWindow : Window
    {
        public ResetPasswordWindow()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Введите email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Введите корректный email адрес", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string currentPassword = DatabaseHelper.GetUserPassword(email);

                if (currentPassword != null)
                {
                    try
                    {
                        using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
                        {
                            smtpClient.EnableSsl = true;

                            smtpClient.Credentials = new System.Net.NetworkCredential(
                                "e_d.s.produvalov@mpt.ru",
                                "ваш_пароль_приложения");

                            MailMessage mailMessage = new MailMessage();
                            mailMessage.From = new MailAddress("e_d.s.produvalov@mpt.ru");
                            mailMessage.To.Add(email);
                            mailMessage.Subject = "Восстановление пароля";
                            mailMessage.Body = $"Ваш текущий пароль: {currentPassword}";

                            smtpClient.Timeout = 20000; 

                            smtpClient.Send(mailMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при отправке email: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Пользователь с таким email не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при восстановлении пароля: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }

        
    }
}