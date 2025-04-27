using ClosedXML.Excel;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using DocumentFormat.OpenXml.Wordprocessing;

namespace AppScan
{
    public partial class EmployeeWindow : Window
    {
        private readonly int _userId;
        private readonly List<string> _taskStatuses = new List<string>
        {
            "Назначено",
            "В работе",
            "Выполнено"
        };

        public EmployeeWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            StatusColumn.ItemsSource = _taskStatuses;
            LoadData();
        }

        private void LoadData()
        {
            LoadTasks();
            LoadAssets();
            LoadAssetModels();
            LoadManufacturers();
            LoadAssetTypes();
            LoadRooms();
            LoadBuildings();
            LoadEmployees();
        }

        #region LoadDataMethods
        private void LoadTasks()
        {
            var columns = new[]
            {
                new { Header = "Заголовок", Binding = new Binding("Title"), Width = new DataGridLength(2, DataGridLengthUnitType.Star) },
                new { Header = "Описание", Binding = new Binding("Description"), Width = new DataGridLength(3, DataGridLengthUnitType.Star) },
                new { Header = "Приоритет", Binding = new Binding("Priority"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) },
                new { Header = "Статус", Binding = new Binding("Status"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) },
                new { Header = "Срок выполнения", Binding = new Binding("DueDate") { StringFormat = "dd.MM.yyyy" }, Width = new DataGridLength(1, DataGridLengthUnitType.Star) }
            };
            TasksDataGrid.AutoGenerateColumns = false;
            TasksDataGrid.Columns.Clear();

            foreach (var columnDef in columns)
            {
                TasksDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = columnDef.Header,
                    Binding = columnDef.Binding,
                    Width = columnDef.Width
                });
            }
            TasksDataGrid.ItemsSource = DatabaseHelper.GetTasksForEmployee(_userId);
        }

        private void LoadAssets()
        {
            AssetsDataGrid.AutoGenerateColumns = false;
            AssetsDataGrid.Columns.Clear();

            AssetsDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Модель",
                Binding = new Binding("AssetModel")
            });
            AssetsDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Инв. номер",
                Binding = new Binding("InventoryNumber")
            });
            AssetsDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Серийный номер",
                Binding = new Binding("SerialNumber")
            });
            AssetsDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Штрих-код",
                Binding = new Binding("Barcode")
            });
            AssetsDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Статус",
                Binding = new Binding("Status")
            });

            AssetsDataGrid.ItemsSource = DatabaseHelper.GetAssets();
        }


        private void LoadAssetModels()
        {
            AssetModelsDataGrid.AutoGenerateColumns = false;
            AssetModelsDataGrid.Columns.Clear();

            AssetModelsDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Название",
                Binding = new Binding("Name")
            });
            AssetModelsDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Производитель",
                Binding = new Binding("ManufacturerName")
            });
            AssetModelsDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Тип актива",
                Binding = new Binding("AssetTypeName")
            });
            AssetModelsDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Штрих-код",
                Binding = new Binding("Barcode")
            });

            AssetModelsDataGrid.ItemsSource = DatabaseHelper.GetAssetModels();
        }

        private void LoadManufacturers()
        {
            ManufacturersDataGrid.AutoGenerateColumns = false;
            ManufacturersDataGrid.Columns.Clear();

            ManufacturersDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Название",
                Binding = new Binding("Name"),
                Width = new DataGridLength(2, DataGridLengthUnitType.Star)
            });
            ManufacturersDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Контактная информация",
                Binding = new Binding("ContactInfo"),
                Width = new DataGridLength(3, DataGridLengthUnitType.Star)
            });

            ManufacturersDataGrid.ItemsSource = DatabaseHelper.GetManufacturers();
        }

        private void LoadAssetTypes()
        {
            AssetTypesDataGrid.AutoGenerateColumns = false;
            AssetTypesDataGrid.Columns.Clear();

            AssetTypesDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Название",
                Binding = new Binding("Name"),
                Width = new DataGridLength(2, DataGridLengthUnitType.Star)
            });
            AssetTypesDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Описание",
                Binding = new Binding("Description"),
                Width = new DataGridLength(3, DataGridLengthUnitType.Star)
            });

            AssetTypesDataGrid.ItemsSource = DatabaseHelper.GetAssetTypes();
        }

        private void LoadRooms()
        {
            RoomsDataGrid.AutoGenerateColumns = false;
            RoomsDataGrid.Columns.Clear();

            RoomsDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Номер помещения",
                Binding = new Binding("RoomNumber"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });
            RoomsDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Этаж",
                Binding = new Binding("Floor"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });
            RoomsDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Здание",
                Binding = new Binding("BuildingName"),
                Width = new DataGridLength(2, DataGridLengthUnitType.Star)
            });

            RoomsDataGrid.ItemsSource = DatabaseHelper.GetRooms();
        }

        private void LoadBuildings()
        {
            BuildingsDataGrid.AutoGenerateColumns = false;
            BuildingsDataGrid.Columns.Clear();

            BuildingsDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Название",
                Binding = new Binding("Name"),
                Width = new DataGridLength(2, DataGridLengthUnitType.Star)
            });
            BuildingsDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Адрес",
                Binding = new Binding("Address"),
                Width = new DataGridLength(3, DataGridLengthUnitType.Star)
            });

            BuildingsDataGrid.ItemsSource = DatabaseHelper.GetBuildings();
        }

        private void LoadEmployees()
        {
            EmployeesDataGrid.AutoGenerateColumns = false;
            EmployeesDataGrid.Columns.Clear();

            EmployeesDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Фамилия",
                Binding = new Binding("LastName"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });
            EmployeesDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Имя",
                Binding = new Binding("FirstName"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });
            EmployeesDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Отчество",
                Binding = new Binding("MiddleName"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });
            EmployeesDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Табельный номер",
                Binding = new Binding("TabNumber"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });
            EmployeesDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Должность",
                Binding = new Binding("Position"),
                Width = new DataGridLength(1.5, DataGridLengthUnitType.Star)
            });
            EmployeesDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Отдел",
                Binding = new Binding("Department"),
                Width = new DataGridLength(1.5, DataGridLengthUnitType.Star)
            });

            EmployeesDataGrid.ItemsSource = DatabaseHelper.GetEmployees();
        }
        #endregion

        #region TaskEvents
        private void UpdateTaskStatus_Click(object sender, RoutedEventArgs e)
        {
            if (TasksDataGrid.SelectedItem is Task selectedTask)
            {
                int currentIndex = _taskStatuses.IndexOf(selectedTask.Status);
                if (currentIndex < _taskStatuses.Count - 1)
                {
                    string newStatus = _taskStatuses[currentIndex + 1];
                    if (DatabaseHelper.UpdateTaskStatus(selectedTask.TaskId, newStatus))
                    {
                        LoadTasks();
                        MessageBox.Show("Статус задачи обновлен");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите задачу для обновления статуса");
            }
        }
        #endregion

        #region AssetEvents
        private void AddAsset_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddAssetWindow();
            if (window.ShowDialog() == true)
            {
                LoadAssets();
            }
        }

        private void EditAsset_Click(object sender, RoutedEventArgs e)
        {
            if (AssetsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите актив для редактирования");
                return;
            }
            var window = new EditAssetWindow(AssetsDataGrid.SelectedItem as Asset);
            if (window.ShowDialog() == true)
            {
                LoadAssets();
            }
        }

        private void DeleteAsset_Click(object sender, RoutedEventArgs e)
        {
            if (AssetsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите актив для удаления");
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить этот актив?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var asset = AssetsDataGrid.SelectedItem as Asset;
                if (DatabaseHelper.DeleteAsset(asset.AssetId))
                {
                    LoadAssets();
                    MessageBox.Show("Актив успешно удален");
                }
            }
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            var data = DatabaseHelper.GetInventoryForExport();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Инвентаризация");

                if (data.Any())
                {
                    var headers = ((IDictionary<string, object>)data.First()).Keys;
                    int col = 1;
                    foreach (string header in headers)
                    {
                        worksheet.Cell(1, col).Value = header;
                        col++;
                    }

                    int row = 2;
                    foreach (var item in data)
                    {
                        col = 1;
                        foreach (var value in ((IDictionary<string, object>)item).Values)
                        {
                            worksheet.Cell(row, col).Value = value?.ToString() ?? "";
                            col++;
                        }
                        row++;
                    }
                }

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    DefaultExt = ".xlsx",
                    FileName = "Инвентаризация"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    workbook.SaveAs(saveDialog.FileName);
                    MessageBox.Show("Файл успешно сохранен");
                }
            }
        }
        #endregion

        #region AssetModelEvents
        private void AddAssetModel_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddAssetModelWindow();
            if (window.ShowDialog() == true)
            {
                LoadAssetModels();
            }
        }

        private void EditAssetModel_Click(object sender, RoutedEventArgs e)
        {
            if (AssetModelsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите модель для редактирования");
                return;
            }
            var window = new EditAssetModelWindow(AssetModelsDataGrid.SelectedItem as AssetModel);
            if (window.ShowDialog() == true)
            {
                LoadAssetModels();
            }
        }

        private void DeleteAssetModel_Click(object sender, RoutedEventArgs e)
        {
            if (AssetModelsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите модель для удаления");
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить эту модель?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var model = AssetModelsDataGrid.SelectedItem as AssetModel;
                if (DatabaseHelper.DeleteAssetModel(model.ModelId))
                {
                    LoadAssetModels();
                    MessageBox.Show("Модель успешно удалена");
                }
            }
        }
        #endregion

        #region ManufacturerEvents
        private void AddManufacturer_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddManufacturerWindow();
            if (window.ShowDialog() == true)
            {
                LoadManufacturers();
            }
        }

        private void EditManufacturer_Click(object sender, RoutedEventArgs e)
        {
            if (ManufacturersDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите производителя для редактирования");
                return;
            }
            var window = new EditManufacturerWindow(ManufacturersDataGrid.SelectedItem as Manufacturer);
            if (window.ShowDialog() == true)
            {
                LoadManufacturers();
            }
        }

        private void DeleteManufacturer_Click(object sender, RoutedEventArgs e)
        {
            if (ManufacturersDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите производителя для удаления");
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить этого производителя?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var manufacturer = ManufacturersDataGrid.SelectedItem as Manufacturer;
                if (DatabaseHelper.DeleteManufacturer(manufacturer.ManufacturerId))
                {
                    LoadManufacturers();
                    MessageBox.Show("Производитель успешно удален");
                }
            }
        }
        #endregion

        #region AssetTypeEvents
        private void AddAssetType_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddAssetTypeWindow();
            if (window.ShowDialog() == true)
            {
                LoadAssetTypes();
            }
        }

        private void EditAssetType_Click(object sender, RoutedEventArgs e)
        {
            if (AssetTypesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип актива для редактирования");
                return;
            }
            var window = new EditAssetTypeWindow(AssetTypesDataGrid.SelectedItem as AssetType);
            if (window.ShowDialog() == true)
            {
                LoadAssetTypes();
            }
        }

        private void DeleteAssetType_Click(object sender, RoutedEventArgs e)
        {
            if (AssetTypesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип актива для удаления");
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить этот тип актива?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var assetType = AssetTypesDataGrid.SelectedItem as AssetType;
                if (DatabaseHelper.DeleteAssetType(assetType.AssetTypeId))
                {
                    LoadAssetTypes();
                    MessageBox.Show("Тип актива успешно удален");
                }
            }
        }
        #endregion

        #region RoomEvents
        private void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddRoomWindow();
            if (window.ShowDialog() == true)
            {
                LoadRooms();
            }
        }

        private void EditRoom_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите помещение для редактирования");
                return;
            }
            var window = new EditRoomWindow(RoomsDataGrid.SelectedItem as Room);
            if (window.ShowDialog() == true)
            {
                LoadRooms();
            }
        }

        private void DeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите помещение для удаления");
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить это помещение?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var room = RoomsDataGrid.SelectedItem as Room;
                if (DatabaseHelper.DeleteRoom(room.RoomId))
                {
                    LoadRooms();
                    MessageBox.Show("Помещение успешно удалено");
                }
            }
        }
        #endregion

        #region BuildingEvents
        private void AddBuilding_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddBuildingWindow();
            if (window.ShowDialog() == true)
            {
                LoadBuildings();
            }
        }

        private void EditBuilding_Click(object sender, RoutedEventArgs e)
        {
            if (BuildingsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите здание для редактирования");
                return;
            }
            var window = new EditBuildingWindow(BuildingsDataGrid.SelectedItem as Building);
            if (window.ShowDialog() == true)
            {
                LoadBuildings();
            }
        }

        private void DeleteBuilding_Click(object sender, RoutedEventArgs e)
        {
            if (BuildingsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите здание для удаления");
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить это здание?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var building = BuildingsDataGrid.SelectedItem as Building;
                if (DatabaseHelper.DeleteBuilding(building.BuildingId))
                {
                    LoadBuildings();
                    MessageBox.Show("Здание успешно удалено");
                }
            }
        }
        #endregion

        #region EmployeeEvents
        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEmployeeWindow();
            if (window.ShowDialog() == true)
            {
                LoadEmployees();
            }
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите сотрудника для редактирования");
                return;
            }
            var window = new EditEmployeeWindow(EmployeesDataGrid.SelectedItem as Employee);
            if (window.ShowDialog() == true)
            {
                LoadEmployees();
            }
        }

        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите сотрудника для удаления");
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить этого сотрудника?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var employee = EmployeesDataGrid.SelectedItem as Employee;
                if (DatabaseHelper.DeleteEmployee(employee.EmployeeId))
                {
                    LoadEmployees();
                    MessageBox.Show("Сотрудник успешно удален");
                }
            }
        }
        #endregion

        #region RefreshMethods
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
            MessageBox.Show("Данные успешно обновлены");
        }
        #endregion

        #region WindowEvents
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
        #endregion

        private void OpenSupport_Click(object sender, RoutedEventArgs e)
        {
            // Получаем информацию о текущем пользователе
            var users = DatabaseHelper.GetEmployeeUsers();
            var currentUser = users.FirstOrDefault(u => u.UserId == _userId);
            string username = currentUser?.Username ?? "Пользователь";

            var supportWindow = new UserSupportTicketsWindow(_userId, username);
            supportWindow.Show();
        }
    }
}