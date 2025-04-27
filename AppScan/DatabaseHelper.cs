using Dapper;
using Npgsql;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Windows.Data;

namespace AppScan
{
    public class DatabaseHelper
    {
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        #region Пользователи

        public static bool UserExists(string username)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "SELECT COUNT(*) FROM users WHERE username = @Username";
                return connection.ExecuteScalar<int>(query, new { Username = username }) > 0;
            }
        }

        public static bool RegisterUser(string username, string passwordHash, string email)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                int roleId = 3;

                string query = "INSERT INTO Users (username, password_hash, email, role_id) VALUES (@username, @password, @email, @roleId)";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", passwordHash);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@roleId", roleId);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }


        public static User LoginUser(string username, string password)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                string query = @"
            SELECT 
                u.user_id AS UserId, 
                u.username AS Username, 
                u.role_id AS RoleId,
                r.role_name AS RoleName
            FROM Users u
            LEFT JOIN Role r ON u.role_id = r.role_id
            WHERE u.username = @username AND u.password_hash = @password";

                var passwordHash = Convert.ToBase64String(
                    System.Security.Cryptography.SHA256.Create()
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password))
                );

                var user = connection.QuerySingleOrDefault<User>(
                    query,
                    new { username, password = passwordHash }
                );

                if (user != null)
                {
                    Console.WriteLine($"Номер роли пользователя {user.Username}: {user.RoleId}");
                }

                return user;
            }
        }






        #endregion

        #region Активы


        public static List<Asset> GetAssets()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
            SELECT 
                a.asset_id AS AssetId,
                a.model_id AS ModelId,
                am.name AS AssetModel,
                a.inventory_number AS InventoryNumber,
                a.serial_number AS SerialNumber,
                a.asset_number AS AssetNumber,
                a.barcode AS Barcode,
                a.employee_id AS EmployeeId,
                a.department AS Department,
                a.room_id AS RoomId,
                a.status AS Status,
                a.issue_date AS IssueDate,
                a.accounting_info AS AccountingInfo
            FROM Asset a
            LEFT JOIN AssetModel am ON a.model_id = am.model_id";

                return connection.Query<Asset>(query).ToList();
            }
        }

        public static bool AddAsset(string model, string barcode, string serialNumber)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
               
                string getModelIdQuery = "SELECT model_id FROM AssetModel WHERE name = @model";
                int? modelId = connection.QueryFirstOrDefault<int?>(getModelIdQuery, new { model });

                if (!modelId.HasValue)
                {
                    return false;
                }

                string query = @"
            INSERT INTO Asset (
                model_id, 
                barcode, 
                serial_number, 
                status
            ) 
            VALUES (
                @modelId, 
                @barcode, 
                @serialNumber,
                'active'
            )";

                int rowsAffected = connection.Execute(query, new
                {
                    modelId = modelId.Value,
                    barcode,
                    serialNumber
                });

                return rowsAffected > 0;
            }
        }


        public static bool EditAsset(int assetId, string model, string barcode, string serialNumber)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string getModelIdQuery = "SELECT model_id FROM AssetModel WHERE name = @model";
                int? modelId = connection.QueryFirstOrDefault<int?>(getModelIdQuery, new { model });

                if (!modelId.HasValue)
                {
                    return false;
                }

                string query = @"
            UPDATE Asset 
            SET 
                model_id = @modelId, 
                barcode = @barcode, 
                serial_number = @serialNumber,
                updated_at = CURRENT_TIMESTAMP
            WHERE asset_id = @assetId";

                int rowsAffected = connection.Execute(query, new
                {
                    modelId = modelId.Value,
                    barcode,
                    serialNumber,
                    assetId
                });

                return rowsAffected > 0;
            }
        }


        public static bool DeleteAsset(int assetId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "DELETE FROM Asset WHERE asset_id = @assetId";
                int rowsAffected = connection.Execute(query, new { assetId });
                return rowsAffected > 0;
            }
        }

        #endregion

        #region Заявки

        public static bool AddAsset(string assetName, string assetModel, string manufacturer, string assetType, string room)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "INSERT INTO assets (asset_name, asset_model, manufacturer, asset_type, room) VALUES (@AssetName, @AssetModel, @Manufacturer, @AssetType, @Room)";
                int rowsAffected = connection.Execute(query, new { AssetName = assetName, AssetModel = assetModel, Manufacturer = manufacturer, AssetType = assetType, Room = room });
                return rowsAffected > 0;
            }
        }

        public static bool UpdateAsset(int assetId, string assetName, string assetModel, string manufacturer, string assetType, string room)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "UPDATE assets SET asset_name = @AssetName, asset_model = @AssetModel, manufacturer = @Manufacturer, asset_type = @AssetType, room = @Room WHERE asset_id = @AssetId";
                int rowsAffected = connection.Execute(query, new { AssetId = assetId, AssetName = assetName, AssetModel = assetModel, Manufacturer = manufacturer, AssetType = assetType, Room = room });
                return rowsAffected > 0;
            }
        }

        public static List<Task> GetTasksForEmployee(int userId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
            SELECT 
                task_id AS TaskId,
                title AS Title,
                description AS Description,
                priority AS Priority,
                status AS Status,
                start_date AS StartDate,
                due_date AS DueDate,
                assigned_by AS AssignedBy,
                assigned_to AS AssignedTo,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM Task 
            WHERE assigned_to = @userId
            ORDER BY due_date DESC";

                return connection.Query<Task>(query, new { userId }).ToList();
            }
        }



        #endregion


        public static List<dynamic> GetInventoryForExport()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
            SELECT 
                a.department AS ""Отдел"",
                b.name AS ""Здание"",
                r.room_number AS ""Номер помещения"",
                CONCAT(e.last_name, ' ', e.first_name, ' ', e.middle_name) AS ""ФИО сотрудника"",
                e.position AS ""Должность"",
                at.name AS ""Тип актива"",
                am.name AS ""Модель"",
                m.name AS ""Производитель"",
                a.inventory_number AS ""Инвентарный номер"",
                a.serial_number AS ""Серийный номер"",
                a.status AS ""Статус"",
                a.issue_date AS ""Дата выдачи""
            FROM asset a
            LEFT JOIN employee e ON a.employee_id = e.employee_id
            LEFT JOIN room r ON a.room_id = r.room_id
            LEFT JOIN building b ON r.building_id = b.building_id
            LEFT JOIN assetmodel am ON a.model_id = am.model_id
            LEFT JOIN manufacturer m ON am.manufacturer_id = m.manufacturer_id
            LEFT JOIN assettype at ON am.asset_type_id = at.asset_type_id
            ORDER BY a.department, b.name, r.room_number";

                return connection.Query(query).ToList();
            }
        }

        #region AssetModels
        public static List<AssetModel> GetAssetModels()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
            SELECT 
                am.model_id AS ModelId,
                am.name,
                am.barcode,
                am.specifications,
                m.name AS ManufacturerName,
                at.name AS AssetTypeName
            FROM AssetModel am
            LEFT JOIN Manufacturer m ON am.manufacturer_id = m.manufacturer_id
            LEFT JOIN AssetType at ON am.asset_type_id = at.asset_type_id";
                return connection.Query<AssetModel>(query).ToList();
            }
        }

        public static bool AddAssetModel(string name, int manufacturerId, int assetTypeId, string specifications, string barcode)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "INSERT INTO AssetModel (name, manufacturer_id, asset_type_id, specifications, barcode) VALUES (@name, @manufacturerId, @assetTypeId, @specifications, @barcode)";
                int rowsAffected = connection.Execute(query, new { name, manufacturerId, assetTypeId, specifications, barcode });
                return rowsAffected > 0;
            }
        }

        public static bool UpdateAssetModel(int modelId, string name, int manufacturerId, int assetTypeId, string specifications, string barcode)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "UPDATE AssetModel SET name = @name, manufacturer_id = @manufacturerId, asset_type_id = @assetTypeId, specifications = @specifications, barcode = @barcode WHERE model_id = @modelId";
                int rowsAffected = connection.Execute(query, new { modelId, name, manufacturerId, assetTypeId, specifications, barcode });
                return rowsAffected > 0;
            }
        }

        public static bool DeleteAssetModel(int modelId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "DELETE FROM AssetModel WHERE model_id = @modelId";
                int rowsAffected = connection.Execute(query, new { modelId });
                return rowsAffected > 0;
            }
        }
        #endregion

        #region Manufacturers
        public static List<Manufacturer> GetManufacturers()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
            SELECT 
                manufacturer_id AS ManufacturerId,
                name AS Name,
                contact_info AS ContactInfo,
                created_at AS CreatedAt
            FROM Manufacturer
            ORDER BY name";

                return connection.Query<Manufacturer>(query).ToList();
            }
        }


        public static bool AddManufacturer(string name, string contactInfo)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "INSERT INTO Manufacturer (name, contact_info) VALUES (@name, @contactInfo)";
                int rowsAffected = connection.Execute(query, new { name, contactInfo });
                return rowsAffected > 0;
            }
        }

        public static bool UpdateManufacturer(int manufacturerId, string name, string contactInfo)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "UPDATE Manufacturer SET name = @name, contact_info = @contactInfo WHERE manufacturer_id = @manufacturerId";
                int rowsAffected = connection.Execute(query, new { manufacturerId, name, contactInfo });
                return rowsAffected > 0;
            }
        }

        public static bool DeleteManufacturer(int manufacturerId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "DELETE FROM Manufacturer WHERE manufacturer_id = @manufacturerId";
                int rowsAffected = connection.Execute(query, new { manufacturerId });
                return rowsAffected > 0;
            }
        }
        #endregion

        #region AssetTypes
        public static List<AssetType> GetAssetTypes()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
            SELECT 
                asset_type_id AS AssetTypeId,
                name AS Name,
                description AS Description,
                created_at AS CreatedAt
            FROM AssetType
            ORDER BY name";

                return connection.Query<AssetType>(query).ToList();
            }
        }

        public static bool AddAssetType(string name, string description)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "INSERT INTO AssetType (name, description) VALUES (@name, @description)";
                int rowsAffected = connection.Execute(query, new { name, description });
                return rowsAffected > 0;
            }
        }

        public static bool UpdateAssetType(int assetTypeId, string name, string description)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "UPDATE AssetType SET name = @name, description = @description WHERE asset_type_id = @assetTypeId";
                int rowsAffected = connection.Execute(query, new { assetTypeId, name, description });
                return rowsAffected > 0;
            }
        }

        public static bool DeleteAssetType(int assetTypeId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "DELETE FROM AssetType WHERE asset_type_id = @assetTypeId";
                int rowsAffected = connection.Execute(query, new { assetTypeId });
                return rowsAffected > 0;
            }
        }
        #endregion

        #region Rooms
        public static List<Room> GetRooms()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
            SELECT 
                r.room_id AS RoomId,
                r.room_number AS RoomNumber,
                r.floor AS Floor,
                r.building_id AS BuildingId,
                b.name AS BuildingName
            FROM Room r
            LEFT JOIN Building b ON r.building_id = b.building_id
            ORDER BY b.name, r.floor, r.room_number";

                return connection.Query<Room>(query).ToList();
            }
        }

        public static bool AddRoom(string roomNumber, int floor, int buildingId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "INSERT INTO Room (room_number, floor, building_id) VALUES (@roomNumber, @floor, @buildingId)";
                int rowsAffected = connection.Execute(query, new { roomNumber, floor, buildingId });
                return rowsAffected > 0;
            }
        }

        public static bool UpdateRoom(int roomId, string roomNumber, int floor, int buildingId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "UPDATE Room SET room_number = @roomNumber, floor = @floor, building_id = @buildingId WHERE room_id = @roomId";
                int rowsAffected = connection.Execute(query, new { roomId, roomNumber, floor, buildingId });
                return rowsAffected > 0;
            }
        }

        public static bool DeleteRoom(int roomId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "DELETE FROM Room WHERE room_id = @roomId";
                int rowsAffected = connection.Execute(query, new { roomId });
                return rowsAffected > 0;
            }
        }
        #endregion

        #region Buildings
        public static List<Building> GetBuildings()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
            SELECT 
                building_id AS BuildingId,
                name AS Name,
                address AS Address,
                created_at AS CreatedAt
            FROM Building
            ORDER BY name";

                return connection.Query<Building>(query).ToList();
            }
        }

        public static bool AddBuilding(string name, string address)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "INSERT INTO Building (name, address) VALUES (@name, @address)";
                int rowsAffected = connection.Execute(query, new { name, address });
                return rowsAffected > 0;
            }
        }

        public static bool UpdateBuilding(int buildingId, string name, string address)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "UPDATE Building SET name = @name, address = @address WHERE building_id = @buildingId";
                int rowsAffected = connection.Execute(query, new { buildingId, name, address });
                return rowsAffected > 0;
            }
        }

        public static bool DeleteBuilding(int buildingId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "DELETE FROM Building WHERE building_id = @buildingId";
                int rowsAffected = connection.Execute(query, new { buildingId });
                return rowsAffected > 0;
            }
        }
        #endregion

        #region Employees
        public static List<Employee> GetEmployees()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
            SELECT 
                employee_id AS EmployeeId,
                last_name AS LastName,
                first_name AS FirstName,
                middle_name AS MiddleName,
                tab_number AS TabNumber,
                position AS Position,
                department AS Department,
                phone AS Phone,
                email AS Email,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM Employee
            ORDER BY last_name, first_name";

                return connection.Query<Employee>(query).ToList();
            }
        }

        public static bool AddEmployee(string lastName, string firstName, string middleName, string tabNumber, string position, string department, string phone, string email)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
            INSERT INTO Employee 
            (last_name, first_name, middle_name, tab_number, position, department, phone, email) 
            VALUES 
            (@lastName, @firstName, @middleName, @tabNumber, @position, @department, @phone, @email)";
                int rowsAffected = connection.Execute(query, new { lastName, firstName, middleName, tabNumber, position, department, phone, email });
                return rowsAffected > 0;
            }
        }

        public static bool UpdateEmployee(int employeeId, string lastName, string firstName, string middleName, string tabNumber, string position, string department, string phone, string email)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
            UPDATE Employee 
            SET last_name = @lastName, 
                first_name = @firstName, 
                middle_name = @middleName, 
                tab_number = @tabNumber, 
                position = @position, 
                department = @department, 
                phone = @phone, 
                email = @email 
            WHERE employee_id = @employeeId";
                int rowsAffected = connection.Execute(query, new { employeeId, lastName, firstName, middleName, tabNumber, position, department, phone, email });
                return rowsAffected > 0;
            }
        }

        public static bool DeleteEmployee(int employeeId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = "DELETE FROM Employee WHERE employee_id = @employeeId";
                int rowsAffected = connection.Execute(query, new { employeeId });
                return rowsAffected > 0;
            }
        }
        #endregion
        public static bool CreateTask(Task task)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                // Приоритет должен быть: low, medium, high
                string priority = "medium"; // значение по умолчанию
                string priorityLower = task.Priority.ToLower();

                if (priorityLower == "низкий")
                    priority = "low";
                else if (priorityLower == "средний")
                    priority = "medium";
                else if (priorityLower == "высокий")
                    priority = "high";

                // Статус должен быть: new, in_progress, completed, cancelled
                string status = "new"; // значение по умолчанию
                string statusLower = task.Status.ToLower();

                if (statusLower == "новая")
                    status = "new";
                else if (statusLower == "в работе")
                    status = "in_progress";
                else if (statusLower == "завершена")
                    status = "completed";
                else if (statusLower == "отменена")
                    status = "cancelled";

                string query = @"
            INSERT INTO Task (
                title, 
                description, 
                priority, 
                status, 
                due_date, 
                assigned_to,
                created_at,
                updated_at
            ) VALUES (
                @Title, 
                @Description, 
                @Priority, 
                @Status, 
                @DueDate, 
                @AssignedTo,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP
            )";

                int rowsAffected = connection.Execute(query, new
                {
                    task.Title,
                    task.Description,
                    Priority = priority,
                    Status = status,
                    task.DueDate,
                    task.AssignedTo
                });

                return rowsAffected > 0;
            }
        }


        public static bool UpdateTask(Task task)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string priority = "medium"; // значение по умолчанию
                string priorityLower = task.Priority.ToLower();

                if (priorityLower == "низкий")
                    priority = "low";
                else if (priorityLower == "средний")
                    priority = "medium";
                else if (priorityLower == "высокий")
                    priority = "high";

                string status = "new"; // значение по умолчанию
                string statusLower = task.Status.ToLower();

                if (statusLower == "новая")
                    status = "new";
                else if (statusLower == "в работе")
                    status = "in_progress";
                else if (statusLower == "завершена")
                    status = "completed";
                else if (statusLower == "отменена")
                    status = "cancelled";

                string query = @"
            UPDATE Task 
            SET 
                title = @Title,
                description = @Description,
                priority = @Priority,
                status = @Status,
                due_date = @DueDate,
                assigned_to = @AssignedTo,
                updated_at = CURRENT_TIMESTAMP
            WHERE task_id = @TaskId";

                int rowsAffected = connection.Execute(query, new
                {
                    task.TaskId,
                    task.Title,
                    task.Description,
                    Priority = priority,
                    Status = status,
                    task.DueDate,
                    task.AssignedTo
                });

                return rowsAffected > 0;
            }
        }

        public static bool UpdateTaskStatus(int taskId, string newStatus)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                // Преобразуем русский текст в значения из БД
                string dbStatus;
                switch (newStatus.ToLower().Trim())
                {
                    case "отменена":
                        dbStatus = "cancelled";
                        break;
                    case "в работе":
                        dbStatus = "in_progress";
                        break;
                    case "завершена":
                        dbStatus = "completed";
                        break;
                    case "новая":
                        dbStatus = "new";
                        break;
                    default:
                        dbStatus = "new";
                        break;
                }

                string query = @"
            UPDATE Task 
            SET 
                status = @dbStatus,
                updated_at = CURRENT_TIMESTAMP
            WHERE task_id = @taskId";

                int rowsAffected = connection.Execute(query, new { taskId, dbStatus });
                return rowsAffected > 0;
            }
        }


        public static Task GetTaskById(int taskId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
            SELECT 
                t.task_id AS TaskId,
                t.title AS Title,
                t.description AS Description,
                t.priority AS Priority,
                t.status AS Status,
                t.start_date AS StartDate,
                t.due_date AS DueDate,
                t.assigned_by AS AssignedBy,
                t.assigned_to AS AssignedTo,
                CONCAT(e.last_name, ' ', e.first_name) AS AssignedToName
            FROM Task t
            LEFT JOIN Employee e ON t.assigned_to = e.employee_id
            WHERE t.task_id = @taskId";

                return connection.QueryFirstOrDefault<Task>(query, new { taskId });
            }
        }

        public static List<Task> GetAllTasks()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
            SELECT 
                t.task_id AS TaskId,
                t.title AS Title,
                t.description AS Description,
                CASE t.priority 
                    WHEN 'low' THEN 'Низкий'
                    WHEN 'medium' THEN 'Средний'
                    WHEN 'high' THEN 'Высокий'
                    ELSE t.priority
                END AS Priority,
                CASE t.status 
                    WHEN 'new' THEN 'Новая'
                    WHEN 'in_progress' THEN 'В работе'
                    WHEN 'completed' THEN 'Завершена'
                    WHEN 'cancelled' THEN 'Отменена'
                    ELSE t.status
                END AS Status,
                t.start_date AS StartDate,
                t.due_date AS DueDate,
                t.assigned_to AS AssignedTo,
                u.username AS AssignedToName
            FROM Task t
            LEFT JOIN Users u ON t.assigned_to = u.user_id
            WHERE u.role_id = 3
            ORDER BY t.created_at DESC";

                return connection.Query<Task>(query).ToList();
            }
        }

        public static List<User> GetEmployeeUsers()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
            SELECT 
                user_id AS UserId,
                username AS Username,
                email AS Email
            FROM Users 
            WHERE role_id = 3
            ORDER BY username";

                return connection.Query<User>(query).ToList();
            }
        }

        public static bool RedactUser(int userId, string email, int roleId, bool isBanned)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();


                    string updateQuery = @"
                UPDATE Users 
                SET email = @email,
                    role_id = @roleId,
                    is_banned = @isBanned,
                    updated_at = CURRENT_TIMESTAMP
                WHERE user_id = @userId";

                    int rowsAffected = connection.Execute(updateQuery, new
                    {
                        userId,
                        email,
                        roleId,
                        isBanned
                    });

                    if (rowsAffected > 0)
                    {

                        string roleText;
                        switch (roleId)
                        {
                            case 1:
                                roleText = "Администратор";
                                break;
                            case 2:
                                roleText = "Менеджер";
                                break;
                            case 3:
                                roleText = "Сотрудник";
                                break;
                            default:
                                roleText = "Неизвестная роль";
                                break;
                        }


                        string logQuery = @"
                    INSERT INTO Logs (timestamp, log_type, message, user_id)
                    VALUES (CURRENT_TIMESTAMP, 'USER_UPDATED', @message, @userId)";

                        connection.Execute(logQuery, new
                        {
                            message = $"Обновлен пользователь (ID: {userId}). Роль: {roleText}, Статус блокировки: {(isBanned ? "Заблокирован" : "Активен")}",
                            userId
                        });

                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {

                    try
                    {
                        string errorLogQuery = @"
                    INSERT INTO Logs (timestamp, log_type, message, user_id)
                    VALUES (CURRENT_TIMESTAMP, 'ERROR', @message, @userId)";

                        connection.Execute(errorLogQuery, new
                        {
                            message = $"Ошибка при обновлении пользователя: {ex.Message}",
                            userId
                        });
                    }
                    catch
                    {
  
                    }

                    throw;
                }
            }
        }

        public static string GetUserPassword(string email)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    string query = "SELECT password_hash FROM Users WHERE email = @email";
                    var password = connection.QueryFirstOrDefault<string>(query, new { email });

                    if (password != null)
                    {
                        string logQuery = @"
                    INSERT INTO Logs (timestamp, log_type, message)
                    VALUES (CURRENT_TIMESTAMP, 'PASSWORD_RECOVERY', @message)";

                        connection.Execute(logQuery, new
                        {
                            message = $"Password recovery requested for email: {email}"
                        });
                    }

                    return password;
                }
                catch (Exception ex)
                {
                    string errorLogQuery = @"
                INSERT INTO Logs (timestamp, log_type, message)
                VALUES (CURRENT_TIMESTAMP, 'ERROR', @message)";

                    connection.Execute(errorLogQuery, new
                    {
                        message = $"Error during password recovery for {email}: {ex.Message}"
                    });

                    throw;
                }
            }
        }
    }
}
