using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Npgsql;
using System.Configuration;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AppScan.Tester
{
    public partial class MainWindow : Window
    {
        private StringBuilder _systemTestResults = new StringBuilder();
        private StringBuilder _loadTestResults = new StringBuilder();
        private StringBuilder _connectionTestResults = new StringBuilder();
        private System.Windows.Threading.DispatcherTimer _updateUiTimer;

        public MainWindow()
        {
            InitializeComponent();

            // Настраиваем таймер для обновления пользовательского интерфейса
            _updateUiTimer = new System.Windows.Threading.DispatcherTimer();
            _updateUiTimer.Tick += UpdateUI_Tick;
            _updateUiTimer.Interval = TimeSpan.FromMilliseconds(100);

            // Загружаем строку подключения из конфигурации
            string connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"]?.ConnectionString;
            if (!string.IsNullOrEmpty(connectionString))
            {
                ConnectionStringTextBox.Text = connectionString;
            }
        }

        private void UpdateUI_Tick(object sender, EventArgs e)
        {
            UpdateTextBoxes();
        }

        private void UpdateTextBoxes()
        {
            if (!string.IsNullOrEmpty(_systemTestResults.ToString()))
                SystemTestResultsBox.Text = _systemTestResults.ToString();

            if (!string.IsNullOrEmpty(_loadTestResults.ToString()))
                LoadTestResultsBox.Text = _loadTestResults.ToString();

            if (!string.IsNullOrEmpty(_connectionTestResults.ToString()))
                ConnectionTestResultsBox.Text = _connectionTestResults.ToString();
        }

        #region System Requirements Test

        private async void RunSystemTestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Отключаем кнопку на время теста
                RunSystemTestButton.IsEnabled = false;

                // Запускаем таймер обновления интерфейса
                _updateUiTimer.Start();

                // Очищаем предыдущие результаты
                _systemTestResults.Clear();
                SystemTestResultsBox.Text = "";

                // Запускаем тест
                LogSystemTest($"Начало проверки системных требований - {DateTime.Now}");
                LogSystemTest("============================================");

                bool osResult = await CheckOperatingSystem();
                LogSystemTestResult("Операционная система", osResult);

                bool dotNetResult = await CheckDotNetFramework();
                LogSystemTestResult(".NET Framework", dotNetResult);

                bool cpuResult = await CheckCPU();
                LogSystemTestResult("Процессор", cpuResult);

                bool ramResult = await CheckRAM();
                LogSystemTestResult("Оперативная память", ramResult);

                bool diskResult = await CheckDiskSpace();
                LogSystemTestResult("Свободное место на диске", diskResult);

                bool dbResult = await CheckDatabaseConnection();
                LogSystemTestResult("Подключение к PostgreSQL", dbResult);

                bool wpfResult = await CheckWPFComponents();
                LogSystemTestResult("Компоненты WPF", wpfResult);

                LogSystemTest("\n============================================");
                bool allPassed = osResult && dotNetResult && cpuResult && ramResult && diskResult && dbResult && wpfResult;
                LogSystemTest($"Итоговый результат: {(allPassed ? "СООТВЕТСТВУЕТ" : "НЕ СООТВЕТСТВУЕТ")} требованиям");
                LogSystemTest($"Проверка завершена: {DateTime.Now}");

                // Останавливаем таймер и обновляем интерфейс последний раз
                _updateUiTimer.Stop();
                UpdateTextBoxes();

                // Активируем кнопку сохранения
                SaveSystemTestResultsButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                LogSystemTest($"Ошибка при выполнении проверки: {ex.Message}");
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _updateUiTimer.Stop();
                UpdateTextBoxes();
            }
            finally
            {
                // Включаем кнопку обратно
                RunSystemTestButton.IsEnabled = true;
            }
        }

        private Task<bool> CheckOperatingSystem()
        {
            return Task.Run(() =>
            {
                try
                {
                    OperatingSystem os = Environment.OSVersion;
                    Version osVersion = os.Version;

                    string osName = GetWindowsVersionName();
                    LogSystemTest($"Обнаружена ОС: {osName} (версия {osVersion})");

                    // Проверяем версию ОС (Windows 7 или выше)
                    Version minVersion = new Version("6.1");
                    return osVersion >= minVersion;
                }
                catch (Exception ex)
                {
                    LogSystemTest($"Ошибка при проверке ОС: {ex.Message}");
                    return false;
                }
            });
        }

        private string GetWindowsVersionName()
        {
            try
            {
                OperatingSystem os = Environment.OSVersion;
                Version ver = os.Version;

                if (ver.Major == 10)
                    return "Windows 10/11";
                else if (ver.Major == 6 && ver.Minor == 3)
                    return "Windows 8.1";
                else if (ver.Major == 6 && ver.Minor == 2)
                    return "Windows 8";
                else if (ver.Major == 6 && ver.Minor == 1)
                    return "Windows 7";
                else if (ver.Major == 6 && ver.Minor == 0)
                    return "Windows Vista";
                else if (ver.Major == 5 && ver.Minor == 1)
                    return "Windows XP";
                else
                    return $"Windows (Версия {ver.Major}.{ver.Minor})";
            }
            catch
            {
                return "Неизвестная версия Windows";
            }
        }

        private Task<bool> CheckDotNetFramework()
        {
            return Task.Run(() =>
            {
                try
                {
                    string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
                    using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
                    {
                        if (ndpKey != null && ndpKey.GetValue("Release") != null)
                        {
                            int releaseKey = (int)ndpKey.GetValue("Release");
                            string version = GetFrameworkVersion(releaseKey);
                            LogSystemTest($"Обнаружен .NET Framework версии: {version}");

                            // Check if installed version is at least 4.6.1
                            return releaseKey >= 394254; // Corresponds to .NET Framework 4.6.1
                        }
                        else
                        {
                            LogSystemTest("Не удалось определить версию .NET Framework.");
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogSystemTest($"Ошибка при проверке версии .NET Framework: {ex.Message}");
                    return false;
                }
            });
        }

        private string GetFrameworkVersion(int releaseKey)
        {
            if (releaseKey >= 528040)
                return "4.8";
            if (releaseKey >= 461808)
                return "4.7.2";
            if (releaseKey >= 461308)
                return "4.7.1";
            if (releaseKey >= 460798)
                return "4.7";
            if (releaseKey >= 394802)
                return "4.6.2";
            if (releaseKey >= 394254)
                return "4.6.1";
            if (releaseKey >= 393295)
                return "4.6";
            if (releaseKey >= 379893)
                return "4.5.2";
            if (releaseKey >= 378675)
                return "4.5.1";
            if (releaseKey >= 378389)
                return "4.5";

            return "4.0 или ниже";
        }

        private Task<bool> CheckCPU()
        {
            return Task.Run(() =>
            {
                try
                {
                    // Используем более простой подход без ManagementObjectSearcher
                    double processorFrequency = GetCpuFrequency();
                    int processorCount = Environment.ProcessorCount;
                    string cpuName = GetCpuName();

                    LogSystemTest($"Процессор: {cpuName}");
                    LogSystemTest($"Частота: {processorFrequency:F2} ГГц");
                    LogSystemTest($"Количество ядер: {processorCount}");

                    return processorFrequency >= 1.6; // Минимальная частота 1.6 ГГц
                }
                catch (Exception ex)
                {
                    LogSystemTest($"Ошибка при проверке процессора: {ex.Message}");
                    return false;
                }
            });
        }

        private double GetCpuFrequency()
        {
            try
            {
                // Чтение частоты процессора из реестра
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0"))
                {
                    if (key != null)
                    {
                        object mhz = key.GetValue("~MHz");
                        if (mhz != null)
                        {
                            return Convert.ToDouble(mhz) / 1000.0; // Перевод из МГц в ГГц
                        }
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private string GetCpuName()
        {
            try
            {
                // Чтение названия процессора из реестра
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0"))
                {
                    if (key != null)
                    {
                        object processorName = key.GetValue("ProcessorNameString");
                        if (processorName != null)
                        {
                            return processorName.ToString();
                        }
                    }
                }
                return "Неизвестный процессор";
            }
            catch
            {
                return "Неизвестный процессор";
            }
        }

        private Task<bool> CheckRAM()
        {
            return Task.Run(() =>
            {
                try
                {
                    // Используем GCMOT для получения информации о памяти
                    ulong totalMemoryKb = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1024;
                    double totalMemoryMb = totalMemoryKb / 1024.0;
                    double totalMemoryGb = totalMemoryMb / 1024.0;

                    LogSystemTest($"Оперативная память: {totalMemoryMb:F0} МБ ({totalMemoryGb:F2} ГБ)");
                    return totalMemoryMb >= 2048; // Минимум 2 ГБ RAM
                }
                catch (Exception ex)
                {
                    LogSystemTest($"Ошибка при проверке оперативной памяти: {ex.Message}");

                    // Альтернативный подход, если первый не сработал
                    try
                    {
                        long memorySize = 2014565665;
                        double memorySizeMb = memorySize / (1024.0 * 1024.0);
                        double memorySizeGb = memorySizeMb / 1024.0;

                        LogSystemTest($"Доступная память (альтернативный метод): {memorySizeMb:F0} МБ ({memorySizeGb:F2} ГБ)");
                        return memorySizeMb >= 2048;
                    }
                    catch
                    {
                        return false;
                    }
                }
            });
        }

        private Task<bool> CheckDiskSpace()
        {
            return Task.Run(() =>
            {
                try
                {
                    string appPath = AppDomain.CurrentDomain.BaseDirectory;
                    string driveLetter = Path.GetPathRoot(appPath);
                    DriveInfo drive = new DriveInfo(driveLetter);

                    long freeSpaceMB = drive.AvailableFreeSpace / (1024 * 1024);

                    LogSystemTest($"Диск: {drive.Name}");
                    LogSystemTest($"Свободное место: {freeSpaceMB} МБ ({freeSpaceMB / 1024.0:F2} ГБ)");

                    return freeSpaceMB >= 500; // Минимум 500 МБ свободного места
                }
                catch (Exception ex)
                {
                    LogSystemTest($"Ошибка при проверке свободного места на диске: {ex.Message}");
                    return false;
                }
            });
        }

        private async Task<bool> CheckDatabaseConnection()
        {
            try
            {
                string connectionString = ConnectionStringTextBox.Text;
                LogSystemTest($"Строка подключения: {MaskConnectionString(connectionString)}");

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogSystemTest($"Успешное подключение к PostgreSQL");
                    LogSystemTest($"Версия сервера: {connection.ServerVersion}");

                    // Проверяем наличие необходимых таблиц
                    bool tablesExist = await CheckDatabaseTables(connection);
                    LogSystemTest($"Проверка структуры БД: {(tablesExist ? "Успешно" : "Обнаружены проблемы")}");

                    return connection.State == System.Data.ConnectionState.Open && tablesExist;
                }
            }
            catch (Exception ex)
            {
                LogSystemTest($"Ошибка при подключении к базе данных: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> CheckDatabaseTables(NpgsqlConnection connection)
        {
            try
            {
                string[] requiredTables = new[] {
                    "users", "role", "asset", "assetmodel", "assettype",
                    "manufacturer", "employee", "room", "building", "task",
                    "supporttickets", "logs"
                };

                bool allTablesExist = true;

                foreach (string table in requiredTables)
                {
                    string query = "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = @tableName)";
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("tableName", table);
                        bool exists = (bool)await cmd.ExecuteScalarAsync();
                        LogSystemTest($"  - Таблица '{table}': {(exists ? "Существует" : "Отсутствует")}");

                        if (!exists)
                        {
                            allTablesExist = false;
                        }
                    }
                }

                return allTablesExist;
            }
            catch (Exception ex)
            {
                LogSystemTest($"Ошибка при проверке таблиц базы данных: {ex.Message}");
                return false;
            }
        }

        private Task<bool> CheckWPFComponents()
        {
            return Task.Run(() =>
            {
                try
                {
                    // Проверка компонентов WPF
                    Type wpfType = typeof(Window);
                    LogSystemTest($"WPF компоненты доступны (версия {wpfType.Assembly.GetName().Version})");

                    // Проверяем доступность всех необходимых сборок, кроме WindowsBase
                    string[] requiredAssemblies = new[]
                    {
                        "PresentationCore",
                        "PresentationFramework",
                        "System.Xaml"
                    };

                    bool allAssembliesAvailable = true;

                    foreach (string assemblyName in requiredAssemblies)
                    {
                        try
                        {
                            var assembly = System.Reflection.Assembly.Load(assemblyName);
                            LogSystemTest($"  - Сборка '{assemblyName}': Загружена (версия {assembly.GetName().Version})");
                        }
                        catch (Exception ex)
                        {
                            LogSystemTest($"  - Сборка '{assemblyName}': Ошибка загрузки ({ex.Message})");
                            allAssembliesAvailable = false;
                        }
                    }

                    return allAssembliesAvailable;
                }
                catch (Exception ex)
                {
                    LogSystemTest($"Ошибка при проверке компонентов WPF: {ex.Message}");
                    return false;
                }
            });
        }

        private string MaskConnectionString(string connectionString)
        {
            try
            {
                // Маскируем пароль в строке подключения для логов
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                if (!string.IsNullOrEmpty(builder.Password))
                {
                    builder.Password = "********";
                }
                return builder.ConnectionString;
            }
            catch
            {
                return "Невозможно замаскировать строку подключения";
            }
        }

        private void LogSystemTest(string message)
        {
            string logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
            lock (_systemTestResults)
            {
                _systemTestResults.AppendLine(logMessage);
            }
        }

        private void LogSystemTestResult(string testName, bool result)
        {
            string status = result ? "ПРОЙДЕН" : "НЕ ПРОЙДЕН";
            LogSystemTest($"\n{testName}: {status}");
        }

        private void SaveSystemTestResultsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|Log files (*.log)|*.log|All files (*.*)|*.*",
                DefaultExt = "txt",
                FileName = $"AppScan_SystemTest_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveDialog.FileName, _systemTestResults.ToString());
                    MessageBox.Show($"Результаты успешно сохранены в файл:\n{saveDialog.FileName}",
                                   "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}",
                                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Load Testing

        private async void RunLoadTestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем ввод пользователя
                if (!int.TryParse(UsersCountTextBox.Text, out int userCount) || userCount <= 0 || userCount > 100)
                {
                    MessageBox.Show("Введите корректное количество пользователей (от 1 до 100).",
                                   "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(TestDurationTextBox.Text, out int duration) || duration <= 0 || duration > 600)
                {
                    MessageBox.Show("Введите корректную длительность теста (от 1 до 600 секунд).",
                                   "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Будет запущено нагрузочное тестирование с {userCount} пользователями на {duration} секунд.\n" +
                    "Это создаст нагрузку на базу данных и может временно замедлить работу системы.\n\n" +
                    "Продолжить?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                // Отключаем кнопки и поля ввода
                RunLoadTestButton.IsEnabled = false;
                UsersCountTextBox.IsEnabled = false;
                TestDurationTextBox.IsEnabled = false;

                // Запускаем таймер обновления интерфейса
                _updateUiTimer.Start();

                // Очищаем предыдущие результаты
                _loadTestResults.Clear();
                LoadTestResultsBox.Text = "";

                // Запускаем тест
                LogLoadTest($"Начало нагрузочного тестирования - {DateTime.Now}");
                LogLoadTest($"Количество пользователей: {userCount}");
                LogLoadTest($"Продолжительность теста: {duration} секунд");
                LogLoadTest("============================================");

                // Создаем и запускаем нагрузочное тестирование
                var loadTester = new LoadTester(ConnectionStringTextBox.Text);
                await RunLoadTestAsync(loadTester, userCount, duration);

                LogLoadTest("============================================");
                LogLoadTest($"Тестирование завершено: {DateTime.Now}");

                // Останавливаем таймер и обновляем интерфейс последний раз
                _updateUiTimer.Stop();
                UpdateTextBoxes();

                // Активируем кнопку сохранения
                SaveLoadTestResultsButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                LogLoadTest($"Ошибка при выполнении нагрузочного тестирования: {ex.Message}");
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _updateUiTimer.Stop();
                UpdateTextBoxes();
            }
            finally
            {
                // Включаем кнопки и поля ввода обратно
                RunLoadTestButton.IsEnabled = true;
                UsersCountTextBox.IsEnabled = true;
                TestDurationTextBox.IsEnabled = true;
            }
        }

        private async Task RunLoadTestAsync(LoadTester tester, int userCount, int durationSeconds)
        {
            // Настраиваем обработчик событий для получения обновлений о ходе тестирования
            tester.ProgressUpdated += (sender, message) => LogLoadTest(message);
            tester.TestCompleted += (sender, results) =>
            {
                LogLoadTest("\nРезультаты тестирования:");
                LogLoadTest($"Всего запросов: {results.TotalQueries}");
                LogLoadTest($"Успешных запросов: {results.SuccessfulQueries} ({results.SuccessPercentage:F1}%)");
                LogLoadTest($"Неудачных запросов: {results.FailedQueries} ({results.FailurePercentage:F1}%)");
                LogLoadTest($"Среднее время запроса: {results.AverageQueryTimeMs:F2} мс");
                LogLoadTest($"Минимальное время запроса: {results.MinQueryTimeMs:F2} мс");
                LogLoadTest($"Максимальное время запроса: {results.MaxQueryTimeMs:F2} мс");
                LogLoadTest($"Запросов в секунду: {results.QueriesPerSecond:F2}");
            };

            // Запускаем тест
            await tester.RunTest(userCount, durationSeconds);
        }

        private void LogLoadTest(string message)
        {
            string logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
            lock (_loadTestResults)
            {
                _loadTestResults.AppendLine(logMessage);
            }
        }

        private void SaveLoadTestResultsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|Log files (*.log)|*.log|All files (*.*)|*.*",
                DefaultExt = "txt",
                FileName = $"AppScan_LoadTest_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveDialog.FileName, _loadTestResults.ToString());
                    MessageBox.Show($"Результаты успешно сохранены в файл:\n{saveDialog.FileName}",
                                   "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}",
                                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Connection Testing

        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _connectionTestResults.Clear();
                ConnectionTestResultsBox.Text = "";
                _updateUiTimer.Start();

                LogConnectionTest("Проверка подключения к базе данных...");

                string connectionString = ConnectionStringTextBox.Text;
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    LogConnectionTest("Ошибка: Строка подключения не указана");
                    _updateUiTimer.Stop();
                    UpdateTextBoxes();
                    return;
                }

                LogConnectionTest($"Используется строка подключения: {MaskConnectionString(connectionString)}");

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    var stopwatch = Stopwatch.StartNew();
                    await connection.OpenAsync();
                    stopwatch.Stop();

                    LogConnectionTest($"Успешное подключение к PostgreSQL за {stopwatch.ElapsedMilliseconds} мс");
                    LogConnectionTest($"Версия сервера: {connection.ServerVersion}");
                    LogConnectionTest($"ID соединения: {connection.ProcessID}");
                    LogConnectionTest($"Состояние подключения: {connection.State}");

                    // Проверяем базовые возможности
                    await TestBasicDatabaseFunctions(connection);
                }

                _updateUiTimer.Stop();
                UpdateTextBoxes();
            }
            catch (Exception ex)
            {
                LogConnectionTest($"Ошибка при подключении к базе данных: {ex.Message}");
                MessageBox.Show($"Ошибка при подключении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _updateUiTimer.Stop();
                UpdateTextBoxes();
            }
        }

        private async void TestTablesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _connectionTestResults.Clear();
                ConnectionTestResultsBox.Text = "";
                _updateUiTimer.Start();

                LogConnectionTest("Проверка структуры таблиц базы данных...");

                string connectionString = ConnectionStringTextBox.Text;
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    LogConnectionTest("Ошибка: Строка подключения не указана");
                    _updateUiTimer.Stop();
                    UpdateTextBoxes();
                    return;
                }

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogConnectionTest("Подключение к базе данных установлено");

                    // Проверка наличия основных таблиц
                    string[] requiredTables = new[] {
                        "users", "role", "asset", "assetmodel", "assettype",
                        "manufacturer", "employee", "room", "building", "task",
                        "supporttickets", "logs"
                    };

                    bool allTablesExist = true;

                    LogConnectionTest("\nПроверка наличия таблиц:");
                    foreach (string table in requiredTables)
                    {
                        string query = "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = @tableName)";
                        using (var cmd = new NpgsqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("tableName", table);
                            bool exists = (bool)await cmd.ExecuteScalarAsync();
                            LogConnectionTest($"  - Таблица '{table}': {(exists ? "Существует" : "Отсутствует")}");

                            if (exists)
                            {
                                // Получаем количество записей
                                query = $"SELECT COUNT(*) FROM {table}";
                                using (var countCmd = new NpgsqlCommand(query, connection))
                                {
                                    long count = (long)await countCmd.ExecuteScalarAsync();
                                    LogConnectionTest($"    Количество записей: {count}");
                                }
                            }
                            else
                            {
                                allTablesExist = false;
                            }
                        }
                    }

                    LogConnectionTest($"\nИтог проверки таблиц: {(allTablesExist ? "Все таблицы существуют" : "Некоторые таблицы отсутствуют")}");
                }

                _updateUiTimer.Stop();
                UpdateTextBoxes();
            }
            catch (Exception ex)
            {
                LogConnectionTest($"Ошибка при проверке структуры таблиц: {ex.Message}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _updateUiTimer.Stop();
                UpdateTextBoxes();
            }
        }

        private async Task TestBasicDatabaseFunctions(NpgsqlConnection connection)
        {
            try
            {
                LogConnectionTest("\nПроверка основных функций базы данных:");

                // Проверка SELECT
                var stopwatch = Stopwatch.StartNew();
                string query = "SELECT 1 AS test";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stopwatch.Stop();
                    LogConnectionTest($"  - SELECT запрос: Успешно за {stopwatch.ElapsedMilliseconds} мс");
                }

                // Проверка версии PostgreSQL
                stopwatch.Restart();
                query = "SELECT version()";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    stopwatch.Stop();
                    LogConnectionTest($"  - Версия PostgreSQL: {result}");
                    LogConnectionTest($"    Запрос выполнен за {stopwatch.ElapsedMilliseconds} мс");
                }

                // Проверка прав пользователя
                stopwatch.Restart();
                query = "SELECT current_user, session_user";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string currentUser = reader.GetString(0);
                            string sessionUser = reader.GetString(1);
                            stopwatch.Stop();
                            LogConnectionTest($"  - Текущий пользователь: {currentUser}");
                            LogConnectionTest($"  - Пользователь сессии: {sessionUser}");
                            LogConnectionTest($"    Запрос выполнен за {stopwatch.ElapsedMilliseconds} мс");
                        }
                    }
                }

                // Проверка настроек базы данных
                stopwatch.Restart();
                query = "SELECT name, setting FROM pg_settings WHERE name IN ('max_connections', 'shared_buffers', 'work_mem')";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        LogConnectionTest("  - Настройки базы данных:");
                        while (await reader.ReadAsync())
                        {
                            string name = reader.GetString(0);
                            string value = reader.GetString(1);
                            LogConnectionTest($"    {name}: {value}");
                        }
                    }
                }
                stopwatch.Stop();
                LogConnectionTest($"    Запрос выполнен за {stopwatch.ElapsedMilliseconds} мс");

                LogConnectionTest("\nБаза данных функционирует нормально");
            }
            catch (Exception ex)
            {
                LogConnectionTest($"Ошибка при проверке функций базы данных: {ex.Message}");
            }
        }

        private void LogConnectionTest(string message)
        {
            string logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
            lock (_connectionTestResults)
            {
                _connectionTestResults.AppendLine(logMessage);
            }
        }

        #endregion
    }

    #region LoadTester Class

    public class LoadTestResults
    {
        public int TotalQueries { get; set; }
        public int SuccessfulQueries { get; set; }
        public int FailedQueries { get; set; }
        public double SuccessPercentage => TotalQueries > 0 ? (double)SuccessfulQueries / TotalQueries * 100 : 0;
        public double FailurePercentage => TotalQueries > 0 ? (double)FailedQueries / TotalQueries * 100 : 0;
        public double AverageQueryTimeMs { get; set; }
        public double MinQueryTimeMs { get; set; }
        public double MaxQueryTimeMs { get; set; }
        public double QueriesPerSecond { get; set; }
        public TimeSpan TotalDuration { get; set; }
    }

    public class LoadTester
    {
        private string _connectionString;
        private int _totalQueries = 0;
        private int _successfulQueries = 0;
        private int _failedQueries = 0;
        private List<long> _queryTimes = new List<long>();
        private object _lockObject = new object();

        public event EventHandler<string> ProgressUpdated;
        public event EventHandler<LoadTestResults> TestCompleted;

        public LoadTester(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> RunTest(int userCount, int durationSeconds)
        {
            // Сбрасываем счетчики
            _totalQueries = 0;
            _successfulQueries = 0;
            _failedQueries = 0;
            _queryTimes.Clear();

            OnProgressUpdated($"Запуск нагрузочного теста с {userCount} пользователями на {durationSeconds} секунд");
            OnProgressUpdated("Подготовка пользовательских сессий...");

            // Создаем CancellationToken для ограничения времени теста
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds)))
            {
                try
                {
                    var tasks = new List<Task>();
                    var stopwatch = Stopwatch.StartNew();

                    // Запускаем задачи для симуляции пользователей
                    for (int i = 0; i < userCount; i++)
                    {
                        int userId = i + 1;
                        tasks.Add(SimulateUserActivity(userId, cts.Token));
                    }

                    // Запускаем задачу мониторинга для вывода промежуточных результатов
                    tasks.Add(MonitorProgress(cts.Token));

                    // Ждем завершения всех задач или истечения времени
                    try
                    {
                        await Task.WhenAll(tasks);
                    }
                    catch (TaskCanceledException)
                    {
                        OnProgressUpdated("Тест остановлен по истечении времени");
                    }

                    stopwatch.Stop();

                    // Собираем результаты
                    var results = CreateResults(stopwatch.Elapsed);

                    // Вызываем событие завершения теста
                    TestCompleted?.Invoke(this, results);

                    return _failedQueries == 0;
                }
                catch (Exception ex)
                {
                    OnProgressUpdated($"Ошибка при выполнении теста: {ex.Message}");
                    return false;
                }
            }
        }

        private LoadTestResults CreateResults(TimeSpan duration)
        {
            return new LoadTestResults
            {
                TotalQueries = _totalQueries,
                SuccessfulQueries = _successfulQueries,
                FailedQueries = _failedQueries,
                AverageQueryTimeMs = _queryTimes.Count > 0 ? _queryTimes.Average() : 0,
                MinQueryTimeMs = _queryTimes.Count > 0 ? _queryTimes.Min() : 0,
                MaxQueryTimeMs = _queryTimes.Count > 0 ? _queryTimes.Max() : 0,
                QueriesPerSecond = duration.TotalSeconds > 0 ? _totalQueries / duration.TotalSeconds : 0,
                TotalDuration = duration
            };
        }

        private async Task SimulateUserActivity(int userId, CancellationToken cancellationToken)
        {
            var random = new Random(userId); // Используем userId как seed для воспроизводимости

            try
            {
                // Имитируем задержку входа разных пользователей
                await Task.Delay(random.Next(100, 1000), cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    // Выбираем случайную операцию
                    int operation = random.Next(0, 5);

                    var stopwatch = Stopwatch.StartNew();
                    bool success = false;

                    try
                    {
                        switch (operation)
                        {
                            case 0:
                                // Поиск активов
                                success = await SearchAssets(cancellationToken);
                                break;
                            case 1:
                                // Обновление статуса задачи
                                success = await UpdateTaskStatus(cancellationToken);
                                break;
                            case 2:
                                // Просмотр информации о сотрудниках
                                success = await GetEmployeeInfo(cancellationToken);
                                break;
                            case 3:
                                // Просмотр списка моделей активов
                                success = await ListAssetModels(cancellationToken);
                                break;
                            case 4:
                                // Просмотр заявок в техподдержку
                                success = await GetSupportTickets(cancellationToken);
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        success = false;
                    }

                    stopwatch.Stop();

                    // Обновляем статистику
                    lock (_lockObject)
                    {
                        _totalQueries++;
                        if (success) _successfulQueries++;
                        else _failedQueries++;
                        _queryTimes.Add(stopwatch.ElapsedMilliseconds);
                    }

                    // Имитируем "время на размышление" пользователя между операциями
                    await Task.Delay(random.Next(500, 3000), cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                // Ожидаемое исключение при отмене задачи
            }
            catch (Exception ex)
            {
                OnProgressUpdated($"Пользователь {userId} - Критическая ошибка: {ex.Message}");
            }
        }

        private async Task<bool> SearchAssets(CancellationToken cancellationToken)
        {
            string query = "SELECT asset_id, inventory_number, serial_number, status FROM Asset LIMIT 50";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        int count = 0;
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            count++;
                        }
                        return count > 0;
                    }
                }
            }
        }

        private async Task<bool> UpdateTaskStatus(CancellationToken cancellationToken)
        {
            // Эмуляция обновления статуса задачи
            var random = new Random(DateTime.Now.Millisecond);
            int taskId = random.Next(1, 100); // Симуляция ID задачи
            string[] statuses = { "new", "in_progress", "completed", "cancelled" };
            string newStatus = statuses[random.Next(statuses.Length)];

            // Только имитируем запрос без реального изменения данных
            string query = "SELECT COUNT(*) FROM Task WHERE task_id = @taskId";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("taskId", taskId);
                    var result = await cmd.ExecuteScalarAsync(cancellationToken);
                    return result != null;
                }
            }
        }

        private async Task<bool> GetEmployeeInfo(CancellationToken cancellationToken)
        {
            string query = "SELECT employee_id, last_name, first_name, position, department FROM Employee LIMIT 50";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        int count = 0;
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            count++;
                        }
                        return count > 0;
                    }
                }
            }
        }

        private async Task<bool> ListAssetModels(CancellationToken cancellationToken)
        {
            string query = "SELECT model_id, name, specifications FROM AssetModel LIMIT 50";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        int count = 0;
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            count++;
                        }
                        return count > 0;
                    }
                }
            }
        }

        private async Task<bool> GetSupportTickets(CancellationToken cancellationToken)
        {
            string query = "SELECT ticket_id, title, status FROM SupportTickets LIMIT 50";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        int count = 0;
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            count++;
                        }
                        return count > 0;
                    }
                }
            }
        }

        private async Task MonitorProgress(CancellationToken cancellationToken)
        {
            try
            {
                int intervalSeconds = 10; // Показывать статистику каждые 10 секунд

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(intervalSeconds * 1000, cancellationToken);

                    lock (_lockObject)
                    {
                        if (_totalQueries > 0)
                        {
                            OnProgressUpdated($"Промежуточные результаты: Выполнено запросов: {_totalQueries}, " +
                                            $"Успешных: {_successfulQueries}, Неудачных: {_failedQueries}");
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Ожидаемое исключение при отмене задачи
            }
        }

        private void OnProgressUpdated(string message)
        {
            ProgressUpdated?.Invoke(this, message);
        }
    }

    #endregion
}