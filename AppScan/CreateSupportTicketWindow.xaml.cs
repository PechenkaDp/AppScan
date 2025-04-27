using System;
using System.Windows;
using System.Windows.Controls;

namespace AppScan
{
    public partial class CreateSupportTicketWindow : Window
    {
        private readonly int _userId;
        private readonly string _username;

        public CreateSupportTicketWindow(int userId, string username)
        {
            InitializeComponent();
            _userId = userId;
            _username = username;
            TicketTypeComboBox.SelectedIndex = 0;
            GenerateTemplateDescription();
        }

        private void TicketTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GenerateTemplateDescription();
        }

        private void GenerateTemplateDescription()
        {
            if (TicketTypeComboBox.SelectedItem == null)
                return;

            string selectedType = (TicketTypeComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            string template = "";

            switch (selectedType)
            {
                case "Ошибка в программе":
                    template = "Подробно опишите ошибку:\n\n" +
                               "1. Где возникает ошибка (модуль, окно, функция)\n" +
                               "2. Какие действия приводят к ошибке\n" +
                               "3. Текст сообщения об ошибке (если есть)\n" +
                               "4. Как часто возникает ошибка\n";
                    break;
                case "Проблема с оборудованием":
                    template = "Опишите проблему с оборудованием:\n\n" +
                               "1. Тип оборудования (компьютер, принтер, сканер и т.д.)\n" +
                               "2. Модель устройства\n" +
                               "3. Описание проблемы\n" +
                               "4. Когда возникла проблема\n";
                    break;
                case "Предложение по улучшению":
                    template = "Опишите ваше предложение:\n\n" +
                               "1. Какой функционал нужно улучшить\n" +
                               "2. Как именно должно работать улучшение\n" +
                               "3. Какую пользу принесет улучшение\n";
                    break;
                case "Вопрос по работе системы":
                    template = "Задайте ваш вопрос:\n\n" +
                               "1. К какому разделу системы относится вопрос\n" +
                               "2. Подробное описание вопроса\n";
                    break;
                case "Другое":
                    template = "Опишите ваше обращение подробно:\n";
                    break;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ||
                IsDescriptionMatchingTemplate(DescriptionTextBox.Text))
            {
                DescriptionTextBox.Text = template;
            }
        }

        private bool IsDescriptionMatchingTemplate(string text)
        {
            // Проверка, является ли текущий текст одним из шаблонов
            return text.StartsWith("Подробно опишите ошибку:") ||
                   text.StartsWith("Опишите проблему с оборудованием:") ||
                   text.StartsWith("Опишите ваше предложение:") ||
                   text.StartsWith("Задайте ваш вопрос:") ||
                   text.StartsWith("Опишите ваше обращение подробно:");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                string title = TitleTextBox.Text.Trim();
                string type = (TicketTypeComboBox.SelectedItem as ComboBoxItem).Content.ToString();
                string description = DescriptionTextBox.Text.Trim();

                bool success = SupportTicketHelper.CreateSupportTicket(title, type, description, _userId);

                if (success)
                {
                    MessageBox.Show("Заявка успешно отправлена в техническую поддержку",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при создании заявки. Пожалуйста, попробуйте позже",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите тему обращения",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (TicketTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип обращения",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text) || IsDescriptionMatchingTemplate(DescriptionTextBox.Text))
            {
                MessageBox.Show("Заполните описание вашего обращения",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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