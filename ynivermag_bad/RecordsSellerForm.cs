using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ynivermag_bad
{
    /// <summary>
    /// Форма для оформления заказов продавцом.
    /// Предоставляет функционал для:
    /// - Выбора клиента из списка (или поиска через отдельную форму)
    /// - Просмотра доступных товаров с подсветкой остатков
    /// - Добавления товаров в заказ
    /// - Редактирования количества товаров
    /// - Автоматического подсчета суммы
    /// - Оформления заказа с проверкой остатков
    /// - Генерации чека
    /// </summary>
    public partial class RecordsSellerForm : Form
    {
        // ============ ПОЛЯ КЛАССА ============

        /// <summary>
        /// ФИО текущего пользователя (продавца)
        /// </summary>
        private string _fio;

        /// <summary>
        /// ID роли текущего пользователя
        /// </summary>
        private int _roleID;

        /// <summary>
        /// Строка подключения к базе данных
        /// </summary>
        private string _connection;

        /// <summary>
        /// Таблица со всеми доступными товарами
        /// </summary>
        private DataTable _allProductsTable;

        /// <summary>
        /// Таблица с товарами в текущем заказе (не используется напрямую)
        /// </summary>
        private DataTable _orderProductsTable;

        /// <summary>
        /// ID текущего пользователя в базе данных
        /// </summary>
        private int _currentUserId;

        /// <summary>
        /// ID выбранного клиента (по умолчанию -1, не выбран)
        /// </summary>
        private int _selectedClientId = -1;

        /// <summary>
        /// Общая сумма текущего заказа
        /// </summary>
        private decimal _totalAmount = 0;

        /// <summary>
        /// Флаг для предотвращения рекурсивного обновления поля поиска
        /// </summary>
        private bool _isUpdatingSearch = false;

        // ============ КОНСТРУКТОР ============

        /// <summary>
        /// Конструктор формы оформления заказа
        /// </summary>
        /// <param name="FIO">ФИО продавца</param>
        /// <param name="roleID">ID роли пользователя</param>
        public RecordsSellerForm(string FIO, int roleID)
        {
            InitializeComponent();
            _fio = FIO;
            _roleID = roleID;
            _connection = Connection.ConnectionString;

            // Настройка формы
            this.Text = "Оформление заказа";
            FIOlabel.Text = $"Продавец: {_fio}";

            // Получаем ID текущего пользователя
            _currentUserId = GetCurrentUserId();

            // Настройка таблиц
            SetupAllProductsGrid();
            SetupOrderProductsGrid();

            // Загрузка клиентов и товаров
            LoadClients();
            LoadAllProducts();

            // Подписка на события
            SubscribeToEvents();
        }

        // ============ ПОДПИСКА НА СОБЫТИЯ ============

        /// <summary>
        /// Подписывается на все необходимые события формы
        /// </summary>
        private void SubscribeToEvents()
        {
            // События таблицы товаров
            dataGridViewAllProducts.CellDoubleClick += DataGridViewAllProducts_CellDoubleClick;

            // События таблицы заказа
            dataGridViewOrderProducts.CellDoubleClick += DataGridViewOrderProducts_CellDoubleClick;
            dataGridViewOrderProducts.CellEndEdit += DataGridViewOrderProducts_CellEndEdit;
            dataGridViewOrderProducts.CellValidating += DataGridViewOrderProducts_CellValidating;
            dataGridViewOrderProducts.EditingControlShowing += DataGridViewOrderProducts_EditingControlShowing;

            // Фильтрация ввода в поле поиска
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearch.KeyPress += TxtSearch_KeyPress;

            // Подсказка для поля поиска
            toolTip1.SetToolTip(txtSearch, "Поиск по названию товара (буквы, цифры, пробел, дефис)");
        }

        // ============ ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ============

        /// <summary>
        /// Получает ID текущего пользователя из базы данных по ФИО
        /// </summary>
        /// <returns>ID пользователя или 1, если не найден</returns>
        private int GetCurrentUserId()
        {
            using (var connection = new MySqlConnection(_connection))
            {
                try
                {
                    connection.Open();
                    // Разбиваем ФИО на фамилию и имя
                    string[] nameParts = _fio.Split(' ');
                    string lastName = nameParts.Length > 0 ? nameParts[0] : "";
                    string firstName = nameParts.Length > 1 ? nameParts[1] : "";

                    string query = "SELECT user_id FROM user WHERE last_name = @LastName AND first_name = @FirstName";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@LastName", lastName);
                    cmd.Parameters.AddWithValue("@FirstName", firstName);

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка получения ID пользователя: {ex.Message}");
                }
            }
            return 1; // Возвращаем 1 по умолчанию если не нашли
        }

        /// <summary>
        /// Очищает форму заказа для создания нового заказа
        /// </summary>
        private void ClearOrderForm()
        {
            cmbClient.SelectedIndex = -1;
            dataGridViewOrderProducts.Rows.Clear();
            _totalAmount = 0;
            lblTotalAmount.Text = "Итого: 0 ₽";
        }

        // ============ НАСТРОЙКА ТАБЛИЦ ============

        /// <summary>
        /// Настраивает таблицу со списком всех товаров
        /// </summary>
        private void SetupAllProductsGrid()
        {
            dataGridViewAllProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewAllProducts.MultiSelect = false;
            dataGridViewAllProducts.ReadOnly = true;
            dataGridViewAllProducts.RowHeadersVisible = false;
            dataGridViewAllProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Добавляем колонки
            dataGridViewAllProducts.Columns.Add("ProductId", "ID");
            dataGridViewAllProducts.Columns.Add("ProductName", "Название");
            dataGridViewAllProducts.Columns.Add("Price", "Цена");
            dataGridViewAllProducts.Columns.Add("Stock", "В наличии");

            // Настройка колонок
            dataGridViewAllProducts.Columns["ProductId"].Visible = false;
            dataGridViewAllProducts.Columns["Price"].DefaultCellStyle.Format = "C2";
            dataGridViewAllProducts.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewAllProducts.Columns["Stock"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Подсветка товаров с малым количеством
            dataGridViewAllProducts.CellFormatting += (s, e) =>
            {
                if (dataGridViewAllProducts.Columns[e.ColumnIndex].Name == "Stock" && e.RowIndex >= 0)
                {
                    if (e.Value != null && int.TryParse(e.Value.ToString(), out int stock))
                    {
                        if (stock < 5)
                        {
                            e.CellStyle.BackColor = Color.LightPink;      // Критически мало
                            e.CellStyle.ForeColor = Color.DarkRed;
                        }
                        else if (stock < 10)
                        {
                            e.CellStyle.BackColor = Color.LightYellow;    // Мало
                            e.CellStyle.ForeColor = Color.DarkOrange;
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Настраивает таблицу с товарами в текущем заказе
        /// </summary>
        private void SetupOrderProductsGrid()
        {
            dataGridViewOrderProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewOrderProducts.MultiSelect = false;
            dataGridViewOrderProducts.RowHeadersVisible = false;
            dataGridViewOrderProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewOrderProducts.AllowUserToAddRows = false;
            dataGridViewOrderProducts.AllowUserToDeleteRows = true;

            // Добавляем колонки
            dataGridViewOrderProducts.Columns.Add("ProductId", "ID");
            dataGridViewOrderProducts.Columns.Add("ProductName", "Название");
            dataGridViewOrderProducts.Columns.Add("Price", "Цена");
            dataGridViewOrderProducts.Columns.Add("Quantity", "Кол-во");
            dataGridViewOrderProducts.Columns.Add("Total", "Сумма");
            dataGridViewOrderProducts.Columns.Add("AvailableStock", "Доступно");

            // Настройка колонок
            dataGridViewOrderProducts.Columns["ProductId"].Visible = false;
            dataGridViewOrderProducts.Columns["AvailableStock"].Visible = false; // Скрываем, используем для проверок
            dataGridViewOrderProducts.Columns["Price"].DefaultCellStyle.Format = "C2";
            dataGridViewOrderProducts.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewOrderProducts.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewOrderProducts.Columns["Total"].DefaultCellStyle.Format = "C2";
            dataGridViewOrderProducts.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // Делаем колонку Quantity редактируемой (пользователь может изменить количество)
            dataGridViewOrderProducts.Columns["Quantity"].ReadOnly = false;
        }

        // ============ ЗАГРУЗКА ДАННЫХ ============

        /// <summary>
        /// Загружает список клиентов в комбобокс
        /// </summary>
        private void LoadClients()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"SELECT 
                        client_id, 
                        last_name,
                        first_name,
                        phone
                        FROM client 
                        WHERE isActive = 1
                        ORDER BY last_name, first_name";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Создаем таблицу для отображения в комбобоксе
                    DataTable displayDt = new DataTable();
                    displayDt.Columns.Add("client_id", typeof(int));
                    displayDt.Columns.Add("DisplayName", typeof(string)); // Для отображения в комбобоксе

                    foreach (DataRow row in dt.Rows)
                    {
                        string lastName = row["last_name"].ToString();
                        string firstName = row["first_name"].ToString();
                        string phone = row["phone"]?.ToString() ?? "";

                        // Формируем ФИО с инициалами и телефоном
                        string displayName = FormatClientName(lastName, firstName, phone);

                        displayDt.Rows.Add(
                            Convert.ToInt32(row["client_id"]),
                            displayName
                        );
                    }

                    // Устанавливаем источник данных (простое отображение, без поиска)
                    cmbClient.DataSource = displayDt;
                    cmbClient.DisplayMember = "DisplayName";
                    cmbClient.ValueMember = "client_id";
                    cmbClient.SelectedIndex = -1;

                    // Устанавливаем ширину выпадающего списка
                    cmbClient.DropDownWidth = 350;

                    // Делаем комбобокс недоступным для ввода текста (только выбор из списка)
                    cmbClient.DropDownStyle = ComboBoxStyle.DropDownList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}");
            }
        }

        /// <summary>
        /// Форматирует имя клиента для отображения в комбобоксе
        /// Формат: "Фамилия И. (телефон)"
        /// </summary>
        private string FormatClientName(string lastName, string firstName, string phone)
        {
            // Формируем инициалы (только первая буква имени)
            string initials = "";
            if (!string.IsNullOrEmpty(firstName))
            {
                initials = firstName.Substring(0, 1).ToUpper() + ".";
            }

            // Формируем основную часть
            string result = $"{lastName} {initials}";

            // Добавляем телефон, если он есть (полностью)
            if (!string.IsNullOrEmpty(phone))
            {
                result += $" ({phone})";
            }

            return result;
        }

        /// <summary>
        /// Загружает список всех доступных товаров (с остатком > 0)
        /// </summary>
        private void LoadAllProducts()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"SELECT product_id, name, price, stock_quantity 
                                    FROM product 
                                    WHERE stock_quantity > 0
                                    ORDER BY name";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    _allProductsTable = new DataTable();
                    adapter.Fill(_allProductsTable);

                    dataGridViewAllProducts.Rows.Clear();
                    foreach (DataRow row in _allProductsTable.Rows)
                    {
                        dataGridViewAllProducts.Rows.Add(
                            row["product_id"],
                            row["name"],
                            Convert.ToDecimal(row["price"]),
                            Convert.ToInt32(row["stock_quantity"])
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}");
            }
        }

        // ============ ФИЛЬТРАЦИЯ ВВОДА В ПОЛЕ ПОИСКА ============

        /// <summary>
        /// Фильтрация ввода в поле поиска - разрешаем только буквы, цифры, пробел и дефис
        /// </summary>
        private void TxtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем backspace (управляющие символы)
            if (!char.IsControl(e.KeyChar))
            {
                // Проверяем, является ли символ буквой, цифрой, пробелом или дефисом
                bool isValid = char.IsLetterOrDigit(e.KeyChar) ||
                               e.KeyChar == ' ' ||
                               e.KeyChar == '-';

                if (!isValid)
                {
                    e.Handled = true; // Блокируем ввод

                    // Показываем подсказку при попытке ввести спецсимвол
                    if (sender is TextBox textBox)
                    {
                        toolTip1.Show("Разрешены только буквы, цифры, пробел и дефис",
                            textBox, 0, -20, 1500);
                    }
                }
            }
        }

        /// <summary>
        /// Фильтрация ввода в поле поиска (дополнительная проверка при вставке из буфера)
        /// </summary>
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingSearch) return;

            _isUpdatingSearch = true;

            try
            {
                // Фильтруем текст при вставке из буфера обмена
                if (sender is TextBox textBox)
                {
                    int selectionStart = textBox.SelectionStart;
                    string filteredText = FilterSearchText(textBox.Text);

                    if (filteredText != textBox.Text)
                    {
                        textBox.Text = filteredText;
                        textBox.SelectionStart = Math.Min(selectionStart, filteredText.Length);
                    }
                }

                // Выполняем поиск с отфильтрованным текстом
                PerformSearch();
            }
            finally
            {
                _isUpdatingSearch = false;
            }
        }

        /// <summary>
        /// Фильтр для текста поиска - оставляем только буквы, цифры, пробел и дефис
        /// </summary>
        private string FilterSearchText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return new string(input.Where(c =>
                char.IsLetterOrDigit(c) ||  // Буквы и цифры
                c == ' ' ||                  // Пробел
                c == '-').ToArray());        // Дефис
        }

        /// <summary>
        /// Выполняет поиск товаров по введенному тексту
        /// </summary>
        private void PerformSearch()
        {
            string searchText = txtSearch.Text.ToLower();
            dataGridViewAllProducts.Rows.Clear();

            foreach (DataRow row in _allProductsTable.Rows)
            {
                string productName = row["name"].ToString().ToLower();
                if (string.IsNullOrEmpty(searchText) || productName.Contains(searchText))
                {
                    dataGridViewAllProducts.Rows.Add(
                        row["product_id"],
                        row["name"],
                        Convert.ToDecimal(row["price"]),
                        Convert.ToInt32(row["stock_quantity"])
                    );
                }
            }
        }

        // ============ РАБОТА С ЗАКАЗОМ ============

        /// <summary>
        /// Добавление товара в заказ по двойному клику
        /// </summary>
        private void DataGridViewAllProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dataGridViewAllProducts.Rows[e.RowIndex];
            int productId = Convert.ToInt32(row.Cells["ProductId"].Value);
            string productName = row.Cells["ProductName"].Value.ToString();
            decimal price = Convert.ToDecimal(row.Cells["Price"].Value);
            int availableStock = Convert.ToInt32(row.Cells["Stock"].Value);

            // Проверяем, есть ли уже такой товар в заказе
            bool exists = false;
            foreach (DataGridViewRow orderRow in dataGridViewOrderProducts.Rows)
            {
                if (Convert.ToInt32(orderRow.Cells["ProductId"].Value) == productId)
                {
                    // Увеличиваем количество на 1
                    int currentQuantity = Convert.ToInt32(orderRow.Cells["Quantity"].Value);
                    if (currentQuantity < availableStock)
                    {
                        orderRow.Cells["Quantity"].Value = currentQuantity + 1;
                        UpdateOrderRowTotal(orderRow);
                    }
                    else
                    {
                        MessageBox.Show($"Недостаточно товара на складе. Доступно: {availableStock}");
                    }
                    exists = true;
                    break;
                }
            }

            // Если товара нет в заказе, добавляем новую строку
            if (!exists)
            {
                int newRowIndex = dataGridViewOrderProducts.Rows.Add(
                    productId,
                    productName,
                    price,
                    1,           // Начальное количество = 1
                    price,        // Сумма = цена * 1
                    availableStock
                );
                UpdateOrderRowTotal(dataGridViewOrderProducts.Rows[newRowIndex]);
            }

            UpdateTotalAmount();
        }

        /// <summary>
        /// Удаление товара из заказа по двойному клику
        /// </summary>
        private void DataGridViewOrderProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (MessageBox.Show("Удалить товар из заказа?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                dataGridViewOrderProducts.Rows.RemoveAt(e.RowIndex);
                UpdateTotalAmount();
            }
        }

        /// <summary>
        /// Валидация количества при редактировании
        /// </summary>
        private void DataGridViewOrderProducts_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (dataGridViewOrderProducts.Columns[e.ColumnIndex].Name == "Quantity")
            {
                DataGridViewRow row = dataGridViewOrderProducts.Rows[e.RowIndex];
                int availableStock = Convert.ToInt32(row.Cells["AvailableStock"].Value);

                if (!int.TryParse(e.FormattedValue.ToString(), out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Введите корректное количество (положительное число)");
                    e.Cancel = true;
                }
                else if (quantity > availableStock)
                {
                    MessageBox.Show($"Недостаточно товара на складе. Доступно: {availableStock}");
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// После редактирования количества пересчитываем сумму
        /// </summary>
        private void DataGridViewOrderProducts_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewOrderProducts.Columns[e.ColumnIndex].Name == "Quantity")
            {
                DataGridViewRow row = dataGridViewOrderProducts.Rows[e.RowIndex];
                UpdateOrderRowTotal(row);
                UpdateTotalAmount();
            }
        }

        /// <summary>
        /// Ограничение ввода только цифр для поля количества
        /// </summary>
        private void DataGridViewOrderProducts_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dataGridViewOrderProducts.CurrentCell.ColumnIndex ==
                dataGridViewOrderProducts.Columns["Quantity"].Index)
            {
                if (e.Control is TextBox tb)
                {
                    tb.KeyPress += (s, args) =>
                    {
                        if (!char.IsControl(args.KeyChar) && !char.IsDigit(args.KeyChar))
                        {
                            args.Handled = true;
                        }
                    };
                }
            }
        }

        /// <summary>
        /// Обновляет сумму для отдельной строки заказа
        /// </summary>
        private void UpdateOrderRowTotal(DataGridViewRow row)
        {
            decimal price = Convert.ToDecimal(row.Cells["Price"].Value);
            int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
            row.Cells["Total"].Value = price * quantity;
        }

        /// <summary>
        /// Обновляет общую сумму заказа
        /// </summary>
        private void UpdateTotalAmount()
        {
            _totalAmount = 0;
            foreach (DataGridViewRow row in dataGridViewOrderProducts.Rows)
            {
                _totalAmount += Convert.ToDecimal(row.Cells["Total"].Value);
            }
            lblTotalAmount.Text = $"Итого: {_totalAmount:C2}";
        }

        // ============ ОФОРМЛЕНИЕ ЗАКАЗА ============

        /// <summary>
        /// Обработчик кнопки оформления заказа
        /// </summary>
        private void btnCreateOrder_Click(object sender, EventArgs e)
        {
            // Проверка выбора клиента
            if (cmbClient.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка наличия товаров в заказе
            if (dataGridViewOrderProducts.Rows.Count == 0)
            {
                MessageBox.Show("Добавьте товары в заказ", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Подтверждение оформления
            if (MessageBox.Show($"Оформить заказ на сумму {_totalAmount:C2}?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            int clientId = Convert.ToInt32(cmbClient.SelectedValue);

            using (var connection = new MySqlConnection(_connection))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction()) // Используем транзакцию для целостности данных
                {
                    try
                    {
                        // 1. Сначала проверяем наличие всех товаров
                        Dictionary<int, int> requestedQuantities = new Dictionary<int, int>();
                        Dictionary<int, string> productNames = new Dictionary<int, string>();

                        foreach (DataGridViewRow row in dataGridViewOrderProducts.Rows)
                        {
                            int productId = Convert.ToInt32(row.Cells["ProductId"].Value);
                            int requestedQuantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                            string productName = row.Cells["ProductName"].Value.ToString();

                            requestedQuantities[productId] = requestedQuantity;
                            productNames[productId] = productName;
                        }

                        // Проверяем наличие по одному
                        foreach (var item in requestedQuantities)
                        {
                            int productId = item.Key;
                            int requestedQuantity = item.Value;

                            string checkQuery = "SELECT stock_quantity FROM product WHERE product_id = @ProductId FOR UPDATE";
                            MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection, transaction);
                            checkCmd.Parameters.AddWithValue("@ProductId", productId);

                            int availableStock = Convert.ToInt32(checkCmd.ExecuteScalar());
                            if (availableStock < requestedQuantity)
                            {
                                throw new Exception($"Недостаточно товара '{productNames[productId]}' на складе.\n" +
                                                  $"Запрошено: {requestedQuantity}, Доступно: {availableStock}");
                            }
                        }

                        // 2. Создаем заказ
                        string orderQuery = @"INSERT INTO `order` (client_id, user_id, order_date, total_amount, status) 
                                     VALUES (@ClientId, @UserId, NOW(), @TotalAmount, 'обработка')";
                        MySqlCommand orderCmd = new MySqlCommand(orderQuery, connection, transaction);
                        orderCmd.Parameters.AddWithValue("@ClientId", clientId);
                        orderCmd.Parameters.AddWithValue("@UserId", _currentUserId);
                        orderCmd.Parameters.AddWithValue("@TotalAmount", _totalAmount);
                        orderCmd.ExecuteNonQuery();

                        int orderId = (int)orderCmd.LastInsertedId;

                        // 3. Добавляем товары в order_product и обновляем остатки
                        foreach (DataGridViewRow row in dataGridViewOrderProducts.Rows)
                        {
                            int productId = Convert.ToInt32(row.Cells["ProductId"].Value);
                            int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                            decimal price = Convert.ToDecimal(row.Cells["Price"].Value);

                            // Добавляем в order_product
                            string itemQuery = @"INSERT INTO order_product (order_id, product_id, quantity, unit_price) 
                                       VALUES (@OrderId, @ProductId, @Quantity, @Price)";
                            MySqlCommand itemCmd = new MySqlCommand(itemQuery, connection, transaction);
                            itemCmd.Parameters.AddWithValue("@OrderId", orderId);
                            itemCmd.Parameters.AddWithValue("@ProductId", productId);
                            itemCmd.Parameters.AddWithValue("@Quantity", quantity);
                            itemCmd.Parameters.AddWithValue("@Price", price);
                            itemCmd.ExecuteNonQuery();

                            // Обновляем остаток на складе
                            string updateStockQuery = "UPDATE product SET stock_quantity = stock_quantity - @Quantity WHERE product_id = @ProductId";
                            MySqlCommand updateStockCmd = new MySqlCommand(updateStockQuery, connection, transaction);
                            updateStockCmd.Parameters.AddWithValue("@Quantity", quantity);
                            updateStockCmd.Parameters.AddWithValue("@ProductId", productId);
                            updateStockCmd.ExecuteNonQuery();
                        }

                        // Подтверждаем транзакцию
                        transaction.Commit();

                        MessageBox.Show($"Заказ №{orderId} успешно оформлен!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Спрашиваем, печатать ли чек
                        if (MessageBox.Show("Создать чек?", "Печать чека",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            GenerateReceipt(orderId);
                        }

                        // Очищаем форму для нового заказа
                        ClearOrderForm();

                        // Обновляем список товаров
                        LoadAllProducts();
                    }
                    catch (Exception ex)
                    {
                        // Откатываем транзакцию в случае ошибки
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Генерирует чек для оформленного заказа
        /// </summary>
        /// <param name="orderId">Номер заказа</param>
        private void GenerateReceipt(int orderId)
        {
            List<OrderItem> items = new List<OrderItem>();
            foreach (DataGridViewRow row in dataGridViewOrderProducts.Rows)
            {
                items.Add(new OrderItem
                {
                    ProductId = Convert.ToInt32(row.Cells["ProductId"].Value),
                    ProductName = row.Cells["ProductName"].Value.ToString(),
                    Price = Convert.ToDecimal(row.Cells["Price"].Value),
                    Quantity = Convert.ToInt32(row.Cells["Quantity"].Value)
                });
            }

            string clientName = cmbClient.Text;

            ReceiptGenerator generator = new ReceiptGenerator();
            generator.GenerateReceipt(orderId, clientName, _fio, items, _totalAmount, DateTime.Now);
        }

        // ============ ОБРАБОТЧИКИ КНОПОК ============

        /// <summary>
        /// Очистка текущего заказа
        /// </summary>
        private void btnClearOrder_Click(object sender, EventArgs e)
        {
            if (dataGridViewOrderProducts.Rows.Count > 0)
            {
                if (MessageBox.Show("Очистить текущий заказ?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ClearOrderForm();
                }
            }
        }

        /// <summary>
        /// Добавление нового клиента
        /// </summary>
        private void btnAddClient_Click(object sender, EventArgs e)
        {
            AddClientForm addClientForm = new AddClientForm();
            if (addClientForm.ShowDialog() == DialogResult.OK)
            {
                LoadClients(); // Перезагружаем список клиентов
                MessageBox.Show("Клиент успешно добавлен", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Поиск клиента через отдельную форму
        /// </summary>
        private void button7_Click(object sender, EventArgs e)
        {
            using (SearchClient searchForm = new SearchClient())
            {
                if (searchForm.ShowDialog() == DialogResult.OK)
                {
                    // Ищем клиента в списке комбобокса по ID
                    foreach (DataRowView item in cmbClient.Items)
                    {
                        if (Convert.ToInt32(item["client_id"]) == searchForm.SelectedClientId)
                        {
                            cmbClient.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Добавление товара через кнопку (альтернатива двойному клику)
        /// </summary>
        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            if (dataGridViewAllProducts.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridViewAllProducts.SelectedRows[0];
                int productId = Convert.ToInt32(row.Cells["ProductId"].Value);
                string productName = row.Cells["ProductName"].Value.ToString();
                decimal price = Convert.ToDecimal(row.Cells["Price"].Value);
                int availableStock = Convert.ToInt32(row.Cells["Stock"].Value);

                // Аналогично двойному клику...
                bool exists = false;
                foreach (DataGridViewRow orderRow in dataGridViewOrderProducts.Rows)
                {
                    if (Convert.ToInt32(orderRow.Cells["ProductId"].Value) == productId)
                    {
                        int currentQuantity = Convert.ToInt32(orderRow.Cells["Quantity"].Value);
                        if (currentQuantity < availableStock)
                        {
                            orderRow.Cells["Quantity"].Value = currentQuantity + 1;
                            UpdateOrderRowTotal(orderRow);
                        }
                        else
                        {
                            MessageBox.Show($"Недостаточно товара на складе. Доступно: {availableStock}");
                        }
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    int newRowIndex = dataGridViewOrderProducts.Rows.Add(
                        productId,
                        productName,
                        price,
                        1,
                        price,
                        availableStock
                    );
                    UpdateOrderRowTotal(dataGridViewOrderProducts.Rows[newRowIndex]);
                }

                UpdateTotalAmount();
            }
        }

        /// <summary>
        /// Удаление выбранного товара из заказа
        /// </summary>
        private void btnRemoveProduct_Click(object sender, EventArgs e)
        {
            if (dataGridViewOrderProducts.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Удалить выбранный товар из заказа?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (DataGridViewRow row in dataGridViewOrderProducts.SelectedRows)
                    {
                        if (!row.IsNewRow)
                        {
                            dataGridViewOrderProducts.Rows.Remove(row);
                        }
                    }
                    UpdateTotalAmount();
                }
            }
        }

        /// <summary>
        /// Возврат в главное меню
        /// </summary>
        private void InMenu_Click(object sender, EventArgs e)
        {
            if (_roleID == 2)
            {
                MenuSellerForm menu = new MenuSellerForm(_fio);
                menu.Show();
                this.Hide();
            }
            else
            {
                this.Close();
            }
        }
    }
}