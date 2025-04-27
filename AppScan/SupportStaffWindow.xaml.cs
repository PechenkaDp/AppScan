using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AppScan
{
    public partial class SupportStaffWindow : Window
    {
        private readonly int _userId;
        private readonly string _username;
        private readonly DispatcherTimer _refreshTimer;

        public SupportStaffWindow(int userId, string username)
        {
            InitializeComponent();
            _userId = userId;
            _username = username;

            // Установка начальных значений
            StatusFilterComboBox.SelectedIndex = 0;

            // Отключаем кнопки, пока не выбрана заявка
            AssignToMeButton.IsEnabled = false;
            CloseTicketButton.IsEnabled = false;
            SendResponseButton.IsEnabled = false;
            ResponseTextBox.IsEnabled = false;

            // Загружаем заявки
            LoadTickets();
            UpdateNewTicketsCount();

            // Настраиваем таймер для периодического обновления
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromMinutes(1); // Обновление каждую минуту
            _refreshTimer.Tick += (s, e) =>
            {
                if (!IsTicketSelected()) // Не обновляем, если пользователь работает с заявкой
                {
                    LoadTickets();
                    UpdateNewTicketsCount();
                }
            };
            _refreshTimer.Start();
        }

        private bool IsTicketSelected()
        {
            return TicketsDataGrid.SelectedItem != null && ResponseTextBox.IsFocused;
        }

        private void UpdateNewTicketsCount()
        {
            int newTicketsCount = SupportTicketHelper.GetNewTicketsCount();
            NewTicketsCountTextBlock.Text = $"Новых заявок: {newTicketsCount}";
        }

        private void LoadTickets()
        {
            var tickets = SupportTicketHelper.GetAllSupportTickets();

            // Применяем фильтр по статусу
            if (StatusFilterComboBox.SelectedItem != null)
            {
                string selectedFilter = ((ComboBoxItem)StatusFilterComboBox.SelectedItem).Content.ToString();

                if (selectedFilter != "Все")
                {
                    string statusFilter = GetStatusFromFilter(selectedFilter);
                    tickets = tickets.Where(t => t.Status.ToLower() == statusFilter.ToLower()).ToList();
                }
            }

            // Применяем текстовый поиск
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                tickets = tickets.Where(t =>
                    t.Title.ToLower().Contains(searchText) ||
                    t.Description.ToLower().Contains(searchText) ||
                    (t.CreatedByName != null && t.CreatedByName.ToLower().Contains(searchText))
                ).ToList();
            }

            TicketsDataGrid.ItemsSource = tickets;

            StatusTextBlock.Text = $"Загружено заявок: {tickets.Count}";
        }

        private string GetStatusFromFilter(string filter)
        {
            switch (filter)
            {
                case "Новые": return "новая";
                case "В работе": return "в работе";
                case "Ответ дан": return "ответ дан";
                case "Закрытые": return "закрыта";
                default: return "";
            }
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadTickets();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadTickets();
        }

        private void RefreshTickets_Click(object sender, RoutedEventArgs e)
        {
            LoadTickets();
            UpdateNewTicketsCount();
        }

        private void TicketsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTicket = TicketsDataGrid.SelectedItem as SupportTicket;
            if (selectedTicket != null)
            {
                // Заполняем поля информацией о выбранной заявке
                DescriptionTextBox.Text = selectedTicket.Description;
                ResponseTextBox.Text = selectedTicket.Response ?? "";

                // Информация о пользователе
                UserInfoTextBox.Text = $"Пользователь: {selectedTicket.CreatedByName}\n" +
                                      $"Дата создания: {selectedTicket.CreatedAt:dd.MM.yyyy HH:mm}";

                // Включаем или отключаем кнопки в зависимости от статуса заявки
                AssignToMeButton.IsEnabled = selectedTicket.Status == "новая" ||
                                           (selectedTicket.Status == "в работе" &&
                                           selectedTicket.AssignedTo != _userId);

                CloseTicketButton.IsEnabled = selectedTicket.Status != "закрыта";

                bool canRespond = selectedTicket.Status != "закрыта" &&
                                 (selectedTicket.AssignedTo == _userId ||
                                 selectedTicket.Status == "новая");

                SendResponseButton.IsEnabled = canRespond;
                ResponseTextBox.IsEnabled = canRespond;
            }
            else
            {
                // Очищаем поля и отключаем кнопки, если заявка не выбрана
                DescriptionTextBox.Text = "";
                ResponseTextBox.Text = "";
                UserInfoTextBox.Text = "";

                AssignToMeButton.IsEnabled = false;
                CloseTicketButton.IsEnabled = false;
                SendResponseButton.IsEnabled = false;
                ResponseTextBox.IsEnabled = false;
            }
        }

        private void AssignToMe_Click(object sender, RoutedEventArgs e)
        {
            var selectedTicket = TicketsDataGrid.SelectedItem as SupportTicket;
            if (selectedTicket != null)
            {
                bool success = SupportTicketHelper.AssignTicketToSupport(selectedTicket.TicketId, _userId);
                if (success)
                {
                    MessageBox.Show("Заявка назначена вам", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTickets();

                    // Выбираем эту же заявку снова после обновления
                    foreach (var item in TicketsDataGrid.Items)
                    {
                        var ticket = item as SupportTicket;
                        if (ticket != null && ticket.TicketId == selectedTicket.TicketId)
                        {
                            TicketsDataGrid.SelectedItem = item;
                            break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось назначить заявку", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseTicket_Click(object sender, RoutedEventArgs e)
        {
            var selectedTicket = TicketsDataGrid.SelectedItem as SupportTicket;
            if (selectedTicket != null)
            {
                if (selectedTicket.Status == "новая" || selectedTicket.Status == "в работе")
                {
                    if (string.IsNullOrWhiteSpace(ResponseTextBox.Text))
                    {
                        MessageBox.Show("Нельзя закрыть заявку без ответа. Пожалуйста, напишите ответ или объяснение.",
                            "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                MessageBoxResult result = MessageBox.Show(
                    "Вы уверены, что хотите закрыть эту заявку?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Если есть ответ, то сначала отправляем его
                    if (!string.IsNullOrWhiteSpace(ResponseTextBox.Text) &&
                        (selectedTicket.Response == null || selectedTicket.Response != ResponseTextBox.Text))
                    {
                        SupportTicketHelper.RespondToSupportTicket(
                            selectedTicket.TicketId,
                            ResponseTextBox.Text,
                            _userId);
                    }

                    bool success = SupportTicketHelper.CloseSupportTicket(selectedTicket.TicketId, _userId);
                    if (success)
                    {
                        MessageBox.Show("Заявка успешно закрыта", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTickets();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось закрыть заявку", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SendResponse_Click(object sender, RoutedEventArgs e)
        {
            var selectedTicket = TicketsDataGrid.SelectedItem as SupportTicket;
            if (selectedTicket != null)
            {
                if (string.IsNullOrWhiteSpace(ResponseTextBox.Text))
                {
                    MessageBox.Show("Введите текст ответа", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Если заявка не назначена, то сначала назначаем ее текущему специалисту
                if (selectedTicket.Status == "новая")
                {
                    SupportTicketHelper.AssignTicketToSupport(selectedTicket.TicketId, _userId);
                }

                bool success = SupportTicketHelper.RespondToSupportTicket(
                    selectedTicket.TicketId,
                    ResponseTextBox.Text,
                    _userId);

                if (success)
                {
                    MessageBox.Show("Ответ успешно отправлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTickets();

                    // Выбираем эту же заявку снова после обновления
                    foreach (var item in TicketsDataGrid.Items)
                    {
                        var ticket = item as SupportTicket;
                        if (ticket != null && ticket.TicketId == selectedTicket.TicketId)
                        {
                            TicketsDataGrid.SelectedItem = item;
                            break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось отправить ответ", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Останавливаем таймер при закрытии окна
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
            }
        }
    }
}