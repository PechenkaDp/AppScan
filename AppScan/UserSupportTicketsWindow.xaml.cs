using System.Windows;
using System.Windows.Controls;

namespace AppScan
{
    public partial class UserSupportTicketsWindow : Window
    {
        private readonly int _userId;
        private readonly string _username;

        public UserSupportTicketsWindow(int userId, string username)
        {
            InitializeComponent();
            _userId = userId;
            _username = username;
            LoadTickets();
        }

        private void LoadTickets()
        {
            var tickets = SupportTicketHelper.GetUserSupportTickets(_userId);
            TicketsDataGrid.ItemsSource = tickets;
        }

        private void CreateTicket_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateSupportTicketWindow(_userId, _username);
            if (window.ShowDialog() == true)
            {
                LoadTickets();
            }
        }

        private void TicketsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTicket = TicketsDataGrid.SelectedItem as SupportTicket;
            if (selectedTicket != null)
            {
                DescriptionTextBox.Text = selectedTicket.Description;
                ResponseTextBox.Text = selectedTicket.Response ?? "Ожидается ответ специалиста...";
                CloseTicketButton.IsEnabled = selectedTicket.Status == "ответ дан";
            }
            else
            {
                DescriptionTextBox.Text = "";
                ResponseTextBox.Text = "";
                CloseTicketButton.IsEnabled = false;
            }
        }

        private void CloseTicket_Click(object sender, RoutedEventArgs e)
        {
            var selectedTicket = TicketsDataGrid.SelectedItem as SupportTicket;
            if (selectedTicket != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Вы уверены, что хотите закрыть эту заявку? После закрытия вы не сможете продолжить обсуждение.",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
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

        private void RefreshTickets_Click(object sender, RoutedEventArgs e)
        {
            LoadTickets();
        }
    }
}