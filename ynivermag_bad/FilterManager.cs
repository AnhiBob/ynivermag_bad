using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ynivermag_bad
{
    public class FilterManager
    {
        private string _connectionString;
        private List<OrderData> _allOrders;

        public FilterManager(string connectionString)
        {
            _connectionString = connectionString;
            _allOrders = new List<OrderData>();

            try
            {
                _allOrders = GetAllOrdersFromDB();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания FilterManager: {ex.Message}",
                    "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<OrderData> GetAllOrdersFromDB()
        {
            var orders = new List<OrderData>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    // Полный запрос с JOIN
                    string query = @"
                        SELECT 
                            o.order_id,
                            CONCAT(c.first_name, ' ', c.last_name) as client_name,
                            CONCAT(u.first_name, ' ', u.last_name) as user_name,
                            o.order_date,
                            o.total_amount,
                            o.status,
                            GROUP_CONCAT(CONCAT(p.name, ' (', op.quantity, ' шт.)') SEPARATOR ', ') as products
                        FROM `order` o
                        LEFT JOIN client c ON o.client_id = c.client_id
                        LEFT JOIN user u ON o.user_id = u.user_id
                        LEFT JOIN order_product op ON o.order_id = op.order_id
                        LEFT JOIN product p ON op.product_id = p.product_id
                        WHERE o.order_date IS NOT NULL
                        GROUP BY o.order_id
                        ORDER BY o.order_date DESC
                        LIMIT 100";

                    using (var cmd = new MySqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new OrderData
                            {
                                OrderId = reader.GetInt32("order_id"),
                                ClientName = reader.IsDBNull(1) ? "Не указан" : reader.GetString("client_name"),
                                UserName = reader.IsDBNull(2) ? "Не указан" : reader.GetString("user_name"),
                                OrderDate = reader.GetDateTime("order_date"),
                                TotalAmount = reader.GetDecimal("total_amount"),
                                Status = reader.IsDBNull(5) ? "Не указан" : reader.GetString("status"),
                                Products = reader.IsDBNull(6) ? "Нет товаров" : reader.GetString("products")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }

            return orders;
        }

        public DateRange GetDateRange()
        {
            if (_allOrders == null || !_allOrders.Any())
            {
                var today = DateTime.Today;
                return new DateRange { MinDate = today.AddMonths(-1), MaxDate = today };
            }

            try
            {
                var validOrders = _allOrders.Where(o => o.OrderDate.Year > 1900).ToList();

                if (!validOrders.Any())
                {
                    var today = DateTime.Today;
                    return new DateRange { MinDate = today.AddMonths(-1), MaxDate = today };
                }

                var minDate = validOrders.Min(o => o.OrderDate).Date;
                var maxDate = validOrders.Max(o => o.OrderDate).Date;

                return new DateRange { MinDate = minDate, MaxDate = maxDate };
            }
            catch
            {
                var today = DateTime.Today;
                return new DateRange { MinDate = today.AddMonths(-1), MaxDate = today };
            }
        }

        public List<OrderData> GetFilteredOrders(
            string searchText = "",
            string userFilter = "Все продавцы",
            string statusFilter = "Все статусы",
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string sortBy = "OrderDate",
            bool ascending = false)
        {
            if (_allOrders == null || !_allOrders.Any())
                return new List<OrderData>();

            var filtered = _allOrders.AsEnumerable();

            // Поиск
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(o =>
                    o.ClientName.ToLower().Contains(searchText.ToLower()) ||
                    o.UserName.ToLower().Contains(searchText.ToLower()) ||
                    o.Products.ToLower().Contains(searchText.ToLower()) ||
                    o.OrderId.ToString().Contains(searchText));
            }

            // Фильтрация по продавцу
            if (userFilter != "Все продавцы")
            {
                filtered = filtered.Where(o => o.UserName == userFilter);
            }

            // Фильтрация по статусу
            if (statusFilter != "Все статусы")
            {
                filtered = filtered.Where(o => o.Status == statusFilter);
            }

            // Фильтр по дате
            if (fromDate.HasValue)
            {
                filtered = filtered.Where(o => o.OrderDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                filtered = filtered.Where(o => o.OrderDate <= toDate.Value.Date.AddDays(1).AddSeconds(-1));
            }

            // Сортировка
            switch (sortBy)
            {
                case "TotalAmount":
                    filtered = ascending ?
                        filtered.OrderBy(o => o.TotalAmount) :
                        filtered.OrderByDescending(o => o.TotalAmount);
                    break;
                case "ClientName":
                    filtered = ascending ?
                        filtered.OrderBy(o => o.ClientName) :
                        filtered.OrderByDescending(o => o.ClientName);
                    break;
                case "UserName":
                    filtered = ascending ?
                        filtered.OrderBy(o => o.UserName) :
                        filtered.OrderByDescending(o => o.UserName);
                    break;
                default: // OrderDate
                    filtered = ascending ?
                        filtered.OrderBy(o => o.OrderDate) :
                        filtered.OrderByDescending(o => o.OrderDate);
                    break;
            }

            return filtered.ToList();
        }

        public void PopulateUsersComboBox(ComboBox comboBox)
        {
            if (_allOrders == null || !_allOrders.Any())
                return;

            var users = new List<string> { "Все продавцы" };
            var uniqueUsers = _allOrders
                .Select(o => o.UserName)
                .Where(u => !string.IsNullOrEmpty(u))
                .Distinct()
                .OrderBy(u => u);

            users.AddRange(uniqueUsers);
            comboBox.DataSource = users;
        }

        public void PopulateStatusComboBox(ComboBox comboBox)
        {
            var statuses = new List<string> { "Все статусы", "обработка", "отправлен", "доставлен" };
            comboBox.DataSource = statuses;
        }

        public void RefreshData()
        {
            try
            {
                _allOrders = GetAllOrdersFromDB();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления данных: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}