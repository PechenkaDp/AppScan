using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Dapper;
using Npgsql;

namespace AppScan
{
    public static class SupportTicketHelper
    {
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        public static bool CreateSupportTicket(string title, string type, string description, int userId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO SupportTickets (
                            title, 
                            type, 
                            description, 
                            status, 
                            created_by, 
                            created_at
                        ) VALUES (
                            @title, 
                            @type, 
                            @description, 
                            'новая', 
                            @userId, 
                            CURRENT_TIMESTAMP
                        )";

                    int rowsAffected = connection.Execute(query, new
                    {
                        title,
                        type,
                        description,
                        userId
                    });

                    // Логируем создание заявки
                    if (rowsAffected > 0)
                    {
                        string logQuery = @"
                            INSERT INTO Logs (
                                timestamp, 
                                log_type, 
                                message, 
                                user_id
                            ) VALUES (
                                CURRENT_TIMESTAMP, 
                                'SUPPORT_TICKET_CREATED', 
                                @message, 
                                @userId
                            )";

                        connection.Execute(logQuery, new
                        {
                            message = $"Создана заявка в техподдержку: {title}",
                            userId
                        });
                    }

                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    // Логируем ошибку
                    try
                    {
                        string errorLogQuery = @"
                            INSERT INTO Logs (
                                timestamp, 
                                log_type, 
                                message, 
                                user_id
                            ) VALUES (
                                CURRENT_TIMESTAMP, 
                                'ERROR', 
                                @message, 
                                @userId
                            )";

                        connection.Execute(errorLogQuery, new
                        {
                            message = $"Ошибка при создании заявки в техподдержку: {ex.Message}",
                            userId
                        });
                    }
                    catch { /* Игнорируем ошибку при логировании */ }

                    return false;
                }
            }
        }

        public static List<SupportTicket> GetUserSupportTickets(int userId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
                    SELECT 
                        t.ticket_id AS TicketId,
                        t.title AS Title,
                        t.description AS Description,
                        t.type AS Type,
                        t.status AS Status,
                        t.created_by AS CreatedBy,
                        u.username AS CreatedByName,
                        t.assigned_to AS AssignedTo,
                        a.username AS AssignedToName,
                        t.response AS Response,
                        t.created_at AS CreatedAt,
                        t.updated_at AS UpdatedAt
                    FROM SupportTickets t
                    LEFT JOIN Users u ON t.created_by = u.user_id
                    LEFT JOIN Users a ON t.assigned_to = a.user_id
                    WHERE t.created_by = @userId
                    ORDER BY 
                        CASE 
                            WHEN t.status = 'новая' THEN 1
                            WHEN t.status = 'в работе' THEN 2
                            WHEN t.status = 'ответ дан' THEN 3
                            WHEN t.status = 'закрыта' THEN 4
                            ELSE 5
                        END,
                        t.created_at DESC";

                return connection.Query<SupportTicket>(query, new { userId }).ToList();
            }
        }

        public static List<SupportTicket> GetAllSupportTickets()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
                    SELECT 
                        t.ticket_id AS TicketId,
                        t.title AS Title,
                        t.description AS Description,
                        t.type AS Type,
                        t.status AS Status,
                        t.created_by AS CreatedBy,
                        u.username AS CreatedByName,
                        t.assigned_to AS AssignedTo,
                        a.username AS AssignedToName,
                        t.response AS Response,
                        t.created_at AS CreatedAt,
                        t.updated_at AS UpdatedAt
                    FROM SupportTickets t
                    LEFT JOIN Users u ON t.created_by = u.user_id
                    LEFT JOIN Users a ON t.assigned_to = a.user_id
                    ORDER BY 
                        CASE 
                            WHEN t.status = 'новая' THEN 1
                            WHEN t.status = 'в работе' THEN 2
                            WHEN t.status = 'ответ дан' THEN 3
                            WHEN t.status = 'закрыта' THEN 4
                            ELSE 5
                        END,
                        t.created_at DESC";

                return connection.Query<SupportTicket>(query).ToList();
            }
        }

        public static SupportTicket GetSupportTicketById(int ticketId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
                    SELECT 
                        t.ticket_id AS TicketId,
                        t.title AS Title,
                        t.description AS Description,
                        t.type AS Type,
                        t.status AS Status,
                        t.created_by AS CreatedBy,
                        u.username AS CreatedByName,
                        t.assigned_to AS AssignedTo,
                        a.username AS AssignedToName,
                        t.response AS Response,
                        t.created_at AS CreatedAt,
                        t.updated_at AS UpdatedAt
                    FROM SupportTickets t
                    LEFT JOIN Users u ON t.created_by = u.user_id
                    LEFT JOIN Users a ON t.assigned_to = a.user_id
                    WHERE t.ticket_id = @ticketId";

                return connection.QueryFirstOrDefault<SupportTicket>(query, new { ticketId });
            }
        }

        public static bool UpdateSupportTicketStatus(int ticketId, string status, int? assignedTo = null)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    string query = @"
                        UPDATE SupportTickets 
                        SET 
                            status = @status, 
                            assigned_to = @assignedTo,
                            updated_at = CURRENT_TIMESTAMP
                        WHERE ticket_id = @ticketId";

                    int rowsAffected = connection.Execute(query, new
                    {
                        ticketId,
                        status,
                        assignedTo
                    });

                    if (rowsAffected > 0)
                    {
                        string logQuery = @"
                            INSERT INTO Logs (
                                timestamp, 
                                log_type, 
                                message
                            ) VALUES (
                                CURRENT_TIMESTAMP, 
                                'SUPPORT_TICKET_UPDATED', 
                                @message
                            )";

                        connection.Execute(logQuery, new
                        {
                            message = $"Обновлен статус заявки #{ticketId} на '{status}'"
                        });
                    }

                    return rowsAffected > 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool RespondToSupportTicket(int ticketId, string response, int respondedBy)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    string query = @"
                        UPDATE SupportTickets 
                        SET 
                            response = @response, 
                            status = 'ответ дан',
                            assigned_to = @respondedBy,
                            updated_at = CURRENT_TIMESTAMP
                        WHERE ticket_id = @ticketId";

                    int rowsAffected = connection.Execute(query, new
                    {
                        ticketId,
                        response,
                        respondedBy
                    });

                    if (rowsAffected > 0)
                    {
                        string logQuery = @"
                            INSERT INTO Logs (
                                timestamp, 
                                log_type, 
                                message, 
                                user_id
                            ) VALUES (
                                CURRENT_TIMESTAMP, 
                                'SUPPORT_TICKET_RESPONDED', 
                                @message, 
                                @respondedBy
                            )";

                        connection.Execute(logQuery, new
                        {
                            message = $"Дан ответ на заявку #{ticketId}",
                            respondedBy
                        });
                    }

                    return rowsAffected > 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool CloseSupportTicket(int ticketId, int closedBy)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    string query = @"
                        UPDATE SupportTickets 
                        SET 
                            status = 'закрыта',
                            updated_at = CURRENT_TIMESTAMP
                        WHERE ticket_id = @ticketId";

                    int rowsAffected = connection.Execute(query, new { ticketId });

                    if (rowsAffected > 0)
                    {
                        string logQuery = @"
                            INSERT INTO Logs (
                                timestamp, 
                                log_type, 
                                message, 
                                user_id
                            ) VALUES (
                                CURRENT_TIMESTAMP, 
                                'SUPPORT_TICKET_CLOSED', 
                                @message, 
                                @closedBy
                            )";

                        connection.Execute(logQuery, new
                        {
                            message = $"Закрыта заявка #{ticketId}",
                            closedBy
                        });
                    }

                    return rowsAffected > 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        // Получение количества новых заявок для отображения в интерфейсе
        public static int GetNewTicketsCount()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM SupportTickets 
                    WHERE status = 'новая'";

                return connection.ExecuteScalar<int>(query);
            }
        }

        // Назначение себя ответственным за заявку
        public static bool AssignTicketToSupport(int ticketId, int supportId)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                try
                {
                    string query = @"
                        UPDATE SupportTickets 
                        SET 
                            status = 'в работе',
                            assigned_to = @supportId,
                            updated_at = CURRENT_TIMESTAMP
                        WHERE ticket_id = @ticketId";

                    int rowsAffected = connection.Execute(query, new
                    {
                        ticketId,
                        supportId
                    });

                    if (rowsAffected > 0)
                    {
                        string logQuery = @"
                            INSERT INTO Logs (
                                timestamp, 
                                log_type, 
                                message, 
                                user_id
                            ) VALUES (
                                CURRENT_TIMESTAMP, 
                                'SUPPORT_TICKET_ASSIGNED', 
                                @message, 
                                @supportId
                            )";

                        connection.Execute(logQuery, new
                        {
                            message = $"Заявка #{ticketId} назначена сотруднику поддержки",
                            supportId
                        });
                    }

                    return rowsAffected > 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}