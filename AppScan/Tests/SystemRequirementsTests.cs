using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;
using Npgsql;
using System.Text;
using System.Threading.Tasks;

namespace AppScan.Tests
{
    /// <summary>
    /// Класс для проверки минимальных системных требований приложения AppScan
    /// </summary>
    public class SystemRequirementsTest
    {
        private StringBuilder _logBuilder = new StringBuilder();
        private string _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system_requirements_test.log");

        // Минимальные требования
        private const int MIN_RAM_MB = 2048; // 2 ГБ RAM
        private const int MIN_FREE_DISK_SPACE_MB = 500; // 500 МБ свободного места
        private const string MIN_OS_VERSION = "6.1"; // Windows 7 или выше
        private const string MIN_NET_VERSION = "4.6.1"; // .NET Framework 4.6.1
        private double MIN_CPU_GHZ = 1.6; // 1.6 ГГц

        public async Task<bool> RunTests()
        {
            bool allTestsPassed = true;
            DateTime startTime = DateTime.Now;

            LogMessage($"AppScan System Requirements Test - {startTime}");
            LogMessage("============================================");

            // Проверка ОС Windows
            bool osResult = CheckOperatingSystem();
            LogResult("Операционная система", osResult);
            allTestsPassed &= osResult;

            // Проверка .NET Framework
            bool dotNetResult = CheckDotNetFramework();
            LogResult(".NET Framework", dotNetResult);
            allTestsPassed &= dotNetResult;

            // Проверка процессора
            bool cpuResult = CheckCPU();
            LogResult("Процессор", cpuResult);
            allTestsPassed &= cpuResult;

            // Проверка RAM
            bool ramResult = CheckRAM();
            LogResult("Оперативная память", ramResult);
            allTestsPassed &= ramResult;

            // Проверка свободного места на диске
            bool diskResult = CheckDiskSpace();
            LogResult("Свободное место на диске", diskResult);
            allTestsPassed &= diskResult;

            // Проверка подключения к БД PostgreSQL
            bool dbResult = await CheckDatabaseConnection();
            LogResult("Подключение к PostgreSQL", dbResult);
            allTestsPassed &= dbResult;

            // Проверка компонентов интерфейса WPF
            bool wpfResult = CheckWPFComponents();
            LogResult("Компоненты WPF", wpfResult);
            allTestsPassed &= wpfResult;

            // Проверка разрешения экрана
            bool screenResult = CheckScreenResolution();
            LogResult("Разрешение экрана", screenResult);
            allTestsPassed &= screenResult;

            LogMessage("\n============================================");
            LogMessage($"Тест завершен: {(allTestsPassed ? "ПРОЙДЕН" : "НЕ ПРОЙДЕН")}");
            LogMessage($"Дата и время: {DateTime.Now}");
            LogMessage($"Общее время тестирования: {(DateTime.Now - startTime).TotalSeconds:F2} сек.");

            // Сохраняем результаты в файл
            File.WriteAllText(_logFilePath, _logBuilder.ToString());

            return allTestsPassed;
        }

        private bool CheckOperatingSystem()
        {
            try
            {
                OperatingSystem os = Environment.OSVersion;
                Version osVersion = os.Version;

                string osName = GetWindowsVersionName();
                LogMessage($"Обнаружена ОС: {osName} (версия {osVersion})");

                // Проверяем версию ОС (Windows 7 или выше)
                Version minVersion = new Version(MIN_OS_VERSION);
                return osVersion >= minVersion;
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка при проверке ОС: {ex.Message}");
                return false;
            }
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

        private bool CheckDotNetFramework()
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
                        LogMessage($"Обнаружен .NET Framework версии: {version}");

                        // Check if installed version is at least 4.6.1
                        return releaseKey >= 394254; // Corresponds to .NET Framework 4.6.1
                    }
                    else
                    {
                        LogMessage("Не удалось определить версию .NET Framework.");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка при проверке версии .NET Framework: {ex.Message}");
                return false;
            }
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

        private bool CheckCPU()
        {
            try
            {
                double cpuGHz = 0;
                int coreCount = 0;
                string cpuName = "Неизвестно";

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        cpuName = obj["Name"].ToString();
                        cpuGHz = Convert.ToDouble(obj["MaxClockSpeed"]) / 1000.0;
                        coreCount = Environment.ProcessorCount;
                        break;
                    }
                }

                LogMessage($"Процессор: {cpuName}");
                LogMessage($"Частота: {cpuGHz:F2} ГГц");
                LogMessage($"Количество ядер: {coreCount}");

                return cpuGHz >= MIN_CPU_GHZ;
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка при проверке процессора: {ex.Message}");
                return false;
            }
        }

        private bool CheckRAM()
        {
            try
            {
                long totalRAM = 0;
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        totalRAM = Convert.ToInt64(obj["TotalPhysicalMemory"]) / (1024 * 1024);
                        break;
                    }
                }

                LogMessage($"Оперативная память: {totalRAM} МБ ({totalRAM / 1024.0:F2} ГБ)");
                return totalRAM >= MIN_RAM_MB;
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка при проверке оперативной памяти: {ex.Message}");
                return false;
            }
        }

        private bool CheckDiskSpace()
        {
            try
            {
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                string driveLetter = Path.GetPathRoot(appPath);
                DriveInfo drive = new DriveInfo(driveLetter);

                long freeSpaceMB = drive.AvailableFreeSpace / (1024 * 1024);

                LogMessage($"Диск: {drive.Name}");
                LogMessage($"Свободное место: {freeSpaceMB} МБ ({freeSpaceMB / 1024.0:F2} ГБ)");

                return freeSpaceMB >= MIN_FREE_DISK_SPACE_MB;
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка при проверке свободного места на диске: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> CheckDatabaseConnection()
        {
            try
            {
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;
                LogMessage($"Строка подключения: {MaskConnectionString(connectionString)}");

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogMessage($"Успешное подключение к PostgreSQL");
                    LogMessage($"Версия сервера: {connection.ServerVersion}");

                    // Проверяем наличие необходимых таблиц
                    bool tablesExist = await CheckDatabaseTables(connection);

                    return tablesExist;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка при подключении к базе данных: {ex.Message}");
                return false;
            }
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
                return builder.ToString();
            }
            catch
            {
                return "Невозможно замаскировать строку подключения";
            }
        }

        private async Task<bool> CheckDatabaseTables(NpgsqlConnection connection)
        {
            try
            {
                // Проверка наличия основных таблиц
                string[] requiredTables = new[] {
                    "users", "role", "asset", "assetmodel", "assettype",
                    "manufacturer", "employee", "room", "building", "task",
                    "supporttickets", "logs"
                };

                bool allTablesExist = true;
                LogMessage("Проверка таблиц базы данных:");

                foreach (string table in requiredTables)
                {
                    string query = "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = @tableName)";
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("tableName", table);
                        bool exists = (bool)await cmd.ExecuteScalarAsync();
                        LogMessage($"  - Таблица '{table}': {(exists ? "Существует" : "Отсутствует")}");

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
                LogMessage($"Ошибка при проверке таблиц базы данных: {ex.Message}");
                return false;
            }
        }

        private bool CheckWPFComponents()
        {
            try
            {
                // Проверка компонентов WPF
                Type wpfType = typeof(Window);
                LogMessage($"WPF компоненты доступны (версия {wpfType.Assembly.GetName().Version})");

                // Проверяем доступность всех необходимых сборок
                string[] requiredAssemblies = new[]
                {
                    "PresentationCore",
                    "PresentationFramework",
                    "WindowsBase",
                    "System.Xaml"
                };

                foreach (string assemblyName in requiredAssemblies)
                {
                    try
                    {
                        Assembly assembly = Assembly.Load(assemblyName);
                        LogMessage($"  - Сборка '{assemblyName}': Загружена (версия {assembly.GetName().Version})");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"  - Сборка '{assemblyName}': Ошибка загрузки ({ex.Message})");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка при проверке компонентов WPF: {ex.Message}");
                return false;
            }
        }

        private bool CheckScreenResolution()
        {
            try
            {
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                LogMessage($"Разрешение экрана: {screenWidth}x{screenHeight}");

                // Минимальное рекомендуемое разрешение 1024x768
                return screenWidth >= 1024 && screenHeight >= 768;
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка при проверке разрешения экрана: {ex.Message}");
                return false;
            }
        }

        private void LogMessage(string message)
        {
            _logBuilder.AppendLine(message);
            Debug.WriteLine(message);
        }

        private void LogResult(string testName, bool result)
        {
            string status = result ? "ПРОЙДЕН" : "НЕ ПРОЙДЕН";
            LogMessage($"\n{testName}: {status}");
        }

        // Метод для запуска теста из основного приложения
        public static async Task<bool> RunSystemTest()
        {
            var tester = new SystemRequirementsTest();
            bool result = await tester.RunTests();

            // Показываем результат пользователю
            string message = $"Тест системных требований {(result ? "пройден успешно" : "выявил проблемы")}.\n" +
                            $"Подробные результаты сохранены в файл:\n{tester._logFilePath}";

            MessageBox.Show(message, "Результаты тестирования",
                            result ? MessageBoxButton.OK : MessageBoxButton.OK,
                            result ? MessageBoxImage.Information : MessageBoxImage.Warning);

            return result;
        }
    }
}