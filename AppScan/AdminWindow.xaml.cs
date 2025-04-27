using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using Microsoft.Win32;
using System.IO;
using Npgsql;
using System.Configuration;
using ClosedXML.Excel;
using Dapper;

namespace AppScan
{
    public partial class AdminWindow : Window
    {
        private readonly string ConnectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        public AdminWindow()
        {
            InitializeComponent();
            LoadUsers();
            LoadLogs();
            InitializeLogFilters();
            UpdateDbInfo();
        }



        private NpgsqlConnectionStringBuilder GetConnectionInfo()
        {
            return new NpgsqlConnectionStringBuilder(ConnectionString);
        }

        #region Users Management

        private void LoadSupportStaff()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
                    SELECT 
                        u.user_id AS UserId, 
                        u.username AS Username,
                        u.email AS Email,
                        u.is_banned AS IsBanned
                    FROM Users u
                    WHERE u.role_id = 4 -- Роль техподдержки
                    ORDER BY u.username";

                var supportStaff = connection.Query<User>(query);
                SupportStaffDataGrid.ItemsSource = supportStaff;
            }
        }

        private void LoadUsers()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
            SELECT 
                u.user_id AS UserId, 
                u.username AS Username,
                u.email AS Email,
                u.role_id AS RoleId,
                r.role_name AS RoleName,
                u.is_banned AS IsBanned,
                u.created_at AS CreatedAt,
                u.last_login AS LastLogin,
                u.login_attempts AS LoginAttempts
            FROM Users u
            LEFT JOIN Role r ON u.role_id = r.role_id
            ORDER BY u.user_id";

                var users = connection.Query<User>(query);
                UsersDataGrid.ItemsSource = users;

                foreach (var user in users)
                {
                    System.Diagnostics.Debug.WriteLine($"Loaded user: ID={user.UserId}, Username={user.Username}");
                }
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddUserWindow();
            if (window.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersDataGrid.SelectedItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Selected user for edit: ID={selectedUser.UserId}, Username={selectedUser.Username}");

            var window = new EditUserWindow(selectedUser);
            if (window.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void BanUser_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersDataGrid.SelectedItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для блокировки");
                return;
            }

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "UPDATE Users SET is_banned = true WHERE user_id = @userId";
                connection.Execute(query, new { userId = selectedUser.UserId });
                LoadUsers();
            }
        }

        private void UnbanUser_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersDataGrid.SelectedItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для разблокировки");
                return;
            }

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "UPDATE Users SET is_banned = false WHERE user_id = @userId";
                connection.Execute(query, new { userId = selectedUser.UserId });
                LoadUsers();
            }
        }

        #endregion

        #region Logs Management

        private void InitializeLogFilters()
        {
            LogTypeFilter.Items.Add("Все");
            LogTypeFilter.Items.Add("Информация");
            LogTypeFilter.Items.Add("Предупреждение");
            LogTypeFilter.Items.Add("Ошибка");
            LogTypeFilter.SelectedIndex = 0;

            StartDatePicker.SelectedDate = DateTime.Today.AddDays(-7);
            EndDatePicker.SelectedDate = DateTime.Today;
        }

        private void LoadLogs()
        {
            string logType = LogTypeFilter.SelectedItem?.ToString();
            DateTime? startDate = StartDatePicker.SelectedDate;
            DateTime? endDate = EndDatePicker.SelectedDate;

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
                    SELECT * FROM Logs 
                    WHERE (@logType = 'Все' OR log_type = @logType)
                    AND timestamp BETWEEN @startDate AND @endDate
                    ORDER BY timestamp DESC";

                var logs = connection.Query<Log>(query, new
                {
                    logType,
                    startDate,
                    endDate = endDate?.AddDays(1)
                });

                LogsDataGrid.ItemsSource = logs;
            }
        }

        private void LogFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadLogs();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadLogs();
        }

        private void ExportLogs_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                DefaultExt = "xlsx",
                FileName = "Logs_Export_" + DateTime.Now.ToString("yyyy-MM-dd")
            };

            if (saveDialog.ShowDialog() == true)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Logs");
                    var logs = LogsDataGrid.ItemsSource as IEnumerable<Log>;

                    worksheet.Cell(1, 1).Value = "Время";
                    worksheet.Cell(1, 2).Value = "Тип";
                    worksheet.Cell(1, 3).Value = "Сообщение";
                    worksheet.Cell(1, 4).Value = "Пользователь";
                    worksheet.Cell(1, 5).Value = "Актив";

                    int row = 2;
                    foreach (var log in logs)
                    {
                        worksheet.Cell(row, 1).Value = log.Timestamp;
                        worksheet.Cell(row, 2).Value = log.LogType;
                        worksheet.Cell(row, 3).Value = log.Message;
                        worksheet.Cell(row, 4).Value = log.UserId;
                        worksheet.Cell(row, 5).Value = log.AssetId;
                        row++;
                    }

                    workbook.SaveAs(saveDialog.FileName);
                }

                MessageBox.Show("Логи успешно экспортированы", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RefreshLogs_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }

        #endregion

        #region Database Management

        private void UpdateDbInfo()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string sizeQuery = @"
                    SELECT pg_size_pretty(pg_database_size(current_database()))
                    AS size";
                string dbSize = connection.QueryFirstOrDefault<string>(sizeQuery);
                DbSizeText.Text = $"Размер базы данных: {dbSize}";

                string backupQuery = @"
                    SELECT MAX(timestamp) 
                    FROM Logs 
                    WHERE log_type = 'BACKUP'";
                DateTime? lastBackup = connection.QueryFirstOrDefault<DateTime?>(backupQuery);
                LastBackupText.Text = $"Последнее резервное копирование: {(lastBackup.HasValue ? lastBackup.Value.ToString("dd.MM.yyyy HH:mm:ss") : "Нет данных")}";
            }
        }

        private void CreateBackup_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Backup files (*.backup)|*.backup|All files (*.*)|*.*",
                DefaultExt = "backup",
                FileName = $"database_backup_{DateTime.Now:yyyy-MM-dd_HH-mm}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    using (var connection = new NpgsqlConnection(ConnectionString))
                    {
                        connection.Open();
                        var connectionInfo = GetConnectionInfo();

                        // Получаем версию сервера PostgreSQL
                        string versionQuery = "SELECT version()";
                        string versionString = connection.ExecuteScalar<string>(versionQuery);
                        string serverVersion = "";

                        // Извлекаем номер версии из строки
                        if (versionString.Contains("PostgreSQL"))
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(versionString, @"PostgreSQL (\d+)");
                            if (match.Success)
                            {
                                serverVersion = match.Groups[1].Value;
                            }
                        }

                        if (string.IsNullOrEmpty(serverVersion))
                        {
                            MessageBox.Show("Не удалось определить версию PostgreSQL сервера",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Путь к соответствующей версии pg_dump
                        string pgDumpPath = $@"C:\Program Files\PostgreSQL\{serverVersion}\bin\pg_dump.exe";

                        if (!System.IO.File.Exists(pgDumpPath))
                        {
                            MessageBox.Show($"Не найдена утилита pg_dump для PostgreSQL {serverVersion}. " +
                                $"Убедитесь, что установлены клиентские утилиты PostgreSQL {serverVersion}.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var pgDump = new System.Diagnostics.Process();
                        pgDump.StartInfo.FileName = pgDumpPath;
                        pgDump.StartInfo.Arguments = $"-h {connectionInfo.Host} -p {connectionInfo.Port} -U {connectionInfo.Username} -F c -b -v -f \"{saveDialog.FileName}\" {connectionInfo.Database}";
                        pgDump.StartInfo.UseShellExecute = false;
                        pgDump.StartInfo.RedirectStandardOutput = true;
                        pgDump.StartInfo.RedirectStandardError = true;
                        pgDump.StartInfo.CreateNoWindow = true;
                        pgDump.StartInfo.EnvironmentVariables["PGPASSWORD"] = connectionInfo.Password;

                        pgDump.Start();
                        string error = pgDump.StandardError.ReadToEnd();
                        pgDump.WaitForExit();

                        if (pgDump.ExitCode == 0)
                        {
                            string logQuery = @"
                        INSERT INTO Logs (timestamp, log_type, message)
                        VALUES (CURRENT_TIMESTAMP, 'BACKUP', 'Database backup created successfully')";
                            connection.Execute(logQuery);

                            MessageBox.Show("Резервная копия успешно создана", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            UpdateDbInfo();
                        }
                        else
                        {
                            MessageBox.Show($"Ошибка при создании резервной копии: {error}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании резервной копии: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Backup files (*.backup)|*.backup|All files (*.*)|*.*",
                DefaultExt = "backup"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    var result = MessageBox.Show(
                        "Восстановление базы данных приведет к потере всех текущих данных. Продолжить?",
                        "Предупреждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        using (var connection = new NpgsqlConnection(ConnectionString))
                        {
                            connection.Open();
                            var connectionInfo = GetConnectionInfo();

                            // Получаем версию сервера PostgreSQL
                            string versionQuery = "SELECT version()";
                            string versionString = connection.ExecuteScalar<string>(versionQuery);
                            string serverVersion = "";

                            // Извлекаем номер версии из строки
                            if (versionString.Contains("PostgreSQL"))
                            {
                                var match = System.Text.RegularExpressions.Regex.Match(versionString, @"PostgreSQL (\d+)");
                                if (match.Success)
                                {
                                    serverVersion = match.Groups[1].Value;
                                }
                            }

                            if (string.IsNullOrEmpty(serverVersion))
                            {
                                MessageBox.Show("Не удалось определить версию PostgreSQL сервера",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // Путь к pg_restore
                            string pgRestorePath = $@"C:\Program Files\PostgreSQL\{serverVersion}\bin\pg_restore.exe";

                            if (!System.IO.File.Exists(pgRestorePath))
                            {
                                MessageBox.Show($"Не найдена утилита pg_restore для PostgreSQL {serverVersion}. " +
                                    $"Убедитесь, что установлены клиентские утилиты PostgreSQL {serverVersion}.",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // Отключаем всех пользователей от базы данных
                            string disconnectQuery = @"
                        SELECT pg_terminate_backend(pid)
                        FROM pg_stat_activity
                        WHERE datname = @dbName
                        AND pid <> pg_backend_pid()";
                            connection.Execute(disconnectQuery, new { dbName = connectionInfo.Database });

                            var pgRestore = new System.Diagnostics.Process();
                            pgRestore.StartInfo.FileName = pgRestorePath;
                            pgRestore.StartInfo.Arguments = $"-h {connectionInfo.Host} -p {connectionInfo.Port} -U {connectionInfo.Username} -d {connectionInfo.Database} -c -v \"{openDialog.FileName}\"";
                            pgRestore.StartInfo.UseShellExecute = false;
                            pgRestore.StartInfo.RedirectStandardOutput = true;
                            pgRestore.StartInfo.RedirectStandardError = true;
                            pgRestore.StartInfo.CreateNoWindow = true;
                            pgRestore.StartInfo.EnvironmentVariables["PGPASSWORD"] = connectionInfo.Password;

                            pgRestore.Start();
                            string error = pgRestore.StandardError.ReadToEnd();
                            pgRestore.WaitForExit();

                            if (pgRestore.ExitCode == 0)
                            {
                                string logQuery = @"
                            INSERT INTO Logs (timestamp, log_type, message)
                            VALUES (CURRENT_TIMESTAMP, 'RESTORE', 'Database restored successfully')";
                                connection.Execute(logQuery);

                                MessageBox.Show("База данных успешно восстановлена", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                                LoadUsers();
                                LoadLogs();
                                UpdateDbInfo();
                            }
                            else
                            {
                                MessageBox.Show($"Ошибка при восстановлении базы данных: {error}",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при восстановлении базы данных: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshDbInfo_Click(object sender, RoutedEventArgs e)
        {
            UpdateDbInfo();
        }

        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        private void OpenAssetWindow_Click(object sender, RoutedEventArgs e)
        {
            var assetWindow = new AssetWindow();
            assetWindow.Show();
        }

        private void OpenEmployeeWindow_Click(object sender, RoutedEventArgs e)
        {
            var employeeWindow = new EmployeeWindow(0); // 0 для админа
            employeeWindow.Show();
        }

        private void OpenTaskWindow_Click(object sender, RoutedEventArgs e)
        {
            var taskWindow = new ManagerWindow();
            taskWindow.Show();
        }
        private void OpenSupportStaffWindow_Click(object sender, RoutedEventArgs e)
        {
            // Получаем данные текущего администратора
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
                    SELECT 
                        user_id AS UserId,
                        username AS Username
                    FROM Users
                    WHERE role_id = 1
                    LIMIT 1";

                var adminUser = connection.QueryFirstOrDefault<User>(query);

                if (adminUser != null)
                {
                    var supportWindow = new SupportStaffWindow(adminUser.UserId, adminUser.Username);
                    supportWindow.Show();
                }
                else
                {
                    MessageBox.Show("Не удалось получить данные администратора",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddSupportStaff_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаем окно для выбора пользователя
                var selectedUser = UsersDataGrid.SelectedItem as User;

                if (selectedUser == null)
                {
                    MessageBox.Show("Выберите пользователя для назначения роли техподдержки",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (selectedUser.RoleId == 4)
                {
                    MessageBox.Show("Данный пользователь уже имеет роль техподдержки",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Назначить пользователю {selectedUser.Username} роль сотрудника техподдержки?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var connection = new NpgsqlConnection(ConnectionString))
                    {
                        string updateQuery = @"
                            UPDATE Users
                            SET role_id = 4
                            WHERE user_id = @userId";

                        connection.Execute(updateQuery, new { userId = selectedUser.UserId });

                        // Логируем изменение
                        string logQuery = @"
                            INSERT INTO Logs (timestamp, log_type, message, user_id)
                            VALUES (CURRENT_TIMESTAMP, 'USER_ROLE_CHANGED', @message, @userId)";

                        connection.Execute(logQuery, new
                        {
                            message = $"Пользователю {selectedUser.Username} назначена роль 'Техподдержка'",
                            userId = selectedUser.UserId
                        });

                        MessageBox.Show("Роль пользователя успешно изменена",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadUsers();
                        LoadSupportStaff();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении роли пользователя: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}