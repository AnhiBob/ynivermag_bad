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
    public partial class RecordsSellerForm : Form
    {
        private string _fio;
        private int _roleID;
        private string _connection;
        private DataTable _allProductsTable;
        private DataTable _orderProductsTable;
        private int _currentUserId;
        private int _selectedClientId = -1;
        private decimal _totalAmount = 0;
        private bool _isUpdatingSearch = false;

        public RecordsSellerForm(string FIO, int roleID)
        {
            InitializeComponent();
            _fio = FIO;
            _roleID = roleID;
            _connection = Connection.ConnectionString;

            // Настройка форм
            this.Text = "Оформление заказа";
            FIOlabel.Text = $"Продавец: {_fio}";

            // Получаем ID текущего пользователя
            _currentUserId = GetCurrentUserId();

            // Настройка DataGridView
            SetupAllProductsGrid();
            SetupOrderProductsGrid();

            // Загрузка клиентов и продуктов
            LoadClients();
            LoadAllProducts();

            // Подписка на события
            dataGridViewAllProducts.CellDoubleClick += DataGridViewAllProducts_CellDoubleClick;
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

        private int GetCurrentUserId()
        {
            using (var connection = new MySqlConnection(_connection))
            {
                try
                {
                    connection.Open();
                    // Ищем пользователя по имени (FIO может быть в формате "Фамилия Имя")
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
                            e.CellStyle.BackColor = Color.LightPink;
                            e.CellStyle.ForeColor = Color.DarkRed;
                        }
                        else if (stock < 10)
                        {
                            e.CellStyle.BackColor = Color.LightYellow;
                            e.CellStyle.ForeColor = Color.DarkOrange;
                        }
                    }
                }
            };
        }

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
            dataGridViewOrderProducts.Columns["AvailableStock"].Visible = false;
            dataGridViewOrderProducts.Columns["Price"].DefaultCellStyle.Format = "C2";
            dataGridViewOrderProducts.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewOrderProducts.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewOrderProducts.Columns["Total"].DefaultCellStyle.Format = "C2";
            dataGridViewOrderProducts.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // Делаем колонку Quantity редактируемой
            dataGridViewOrderProducts.Columns["Quantity"].ReadOnly = false;
        }

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

                    // Создаем таблицу для отображения
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

                    // Делаем комбобокс недоступным для ввода текста
                    cmbClient.DropDownStyle = ComboBoxStyle.DropDownList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}");
            }
        }

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

        // Добавление товара в заказ
        private void DataGridViewAllProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dataGridViewAllProducts.Rows[e.RowIndex];
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
                    // Увеличиваем количество
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
                // Добавляем новый товар
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

        // Удаление товара из заказа
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

        // Валидация количества
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

        // После редактирования количества
        private void DataGridViewOrderProducts_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewOrderProducts.Columns[e.ColumnIndex].Name == "Quantity")
            {
                DataGridViewRow row = dataGridViewOrderProducts.Rows[e.RowIndex];
                UpdateOrderRowTotal(row);
                UpdateTotalAmount();
            }
        }

        // Ограничение ввода только цифр для количества
        private void DataGridViewOrderProducts_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dataGridViewOrderProducts.CurrentCell.ColumnIndex ==
                dataGridViewOrderProducts.Columns["Quantity"].Index)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
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

        private void UpdateOrderRowTotal(DataGridViewRow row)
        {
            decimal price = Convert.ToDecimal(row.Cells["Price"].Value);
            int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
            row.Cells["Total"].Value = price * quantity;
        }

        private void UpdateTotalAmount()
        {
            _totalAmount = 0;
            foreach (DataGridViewRow row in dataGridViewOrderProducts.Rows)
            {
                _totalAmount += Convert.ToDecimal(row.Cells["Total"].Value);
            }
            lblTotalAmount.Text = $"Итого: {_totalAmount:C2}";
        }

        // Оформление заказа
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

                        string checkQuery = "SELECT stock_quantity FROM product WHERE product_id = @ProductId";
                        MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                        checkCmd.Parameters.AddWithValue("@ProductId", productId);

                        int availableStock = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (availableStock < requestedQuantity)
                        {
                            MessageBox.Show($"Недостаточно товара '{productNames[productId]}' на складе.\n" +
                                          $"Запрошено: {requestedQuantity}, Доступно: {availableStock}",
                                          "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // 2. Создаем заказ
                    string orderQuery = @"INSERT INTO `order` (client_id, user_id, order_date, total_amount, status) 
                             VALUES (@ClientId, @UserId, NOW(), @TotalAmount, 'обработка')";
                    MySqlCommand orderCmd = new MySqlCommand(orderQuery, connection);
                    orderCmd.Parameters.AddWithValue("@ClientId", clientId);
                    orderCmd.Parameters.AddWithValue("@UserId", _currentUserId);
                    orderCmd.Parameters.AddWithValue("@TotalAmount", _totalAmount);
                    orderCmd.ExecuteNonQuery();

                    int orderId = (int)orderCmd.LastInsertedId;

                    // 3. Добавляем товары в order_product и обновляем остатки
                    bool allSuccessful = true;
                    List<string> errors = new List<string>();

                    foreach (DataGridViewRow row in dataGridViewOrderProducts.Rows)
                    {
                        try
                        {
                            int productId = Convert.ToInt32(row.Cells["ProductId"].Value);
                            int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                            decimal price = Convert.ToDecimal(row.Cells["Price"].Value);
                            string productName = row.Cells["ProductName"].Value.ToString();

                            // Добавляем в order_product
                            string itemQuery = @"INSERT INTO order_product (order_id, product_id, quantity, unit_price) 
                                   VALUES (@OrderId, @ProductId, @Quantity, @Price)";
                            MySqlCommand itemCmd = new MySqlCommand(itemQuery, connection);
                            itemCmd.Parameters.AddWithValue("@OrderId", orderId);
                            itemCmd.Parameters.AddWithValue("@ProductId", productId);
                            itemCmd.Parameters.AddWithValue("@Quantity", quantity);
                            itemCmd.Parameters.AddWithValue("@Price", price);
                            itemCmd.ExecuteNonQuery();

                            // Обновляем остаток на складе
                            string updateStockQuery = "UPDATE product SET stock_quantity = stock_quantity - @Quantity WHERE product_id = @ProductId";
                            MySqlCommand updateStockCmd = new MySqlCommand(updateStockQuery, connection);
                            updateStockCmd.Parameters.AddWithValue("@Quantity", quantity);
                            updateStockCmd.Parameters.AddWithValue("@ProductId", productId);
                            updateStockCmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            allSuccessful = false;
                            errors.Add($"Ошибка при обработке товара '{row.Cells["ProductName"].Value}': {ex.Message}");
                        }
                    }

                    if (allSuccessful)
                    {
                        MessageBox.Show($"Заказ №{orderId} успешно оформлен!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Спрашиваем, печатать ли чек
                        if (MessageBox.Show("Создать чек?", "Печать чека",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            // Собираем товары
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

                            // Создаем чек в Word
                            ReceiptGenerator generator = new ReceiptGenerator();
                            generator.GenerateReceipt(orderId, clientName, _fio, items, _totalAmount, DateTime.Now);
                        }

                        // Очищаем форму для нового заказа
                        ClearOrderForm();

                        // Обновляем список товаров
                        LoadAllProducts();
                    }
                    else
                    {
                        // Если были ошибки, показываем их все
                        string errorMessage = "При оформлении заказа произошли следующие ошибки:\n\n" +
                                             string.Join("\n", errors) +
                                             "\n\nЗаказ был частично оформлен. Проверьте данные вручную.";

                        MessageBox.Show(errorMessage, "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearOrderForm()
        {
            cmbClient.SelectedIndex = -1;
            dataGridViewOrderProducts.Rows.Clear();
            _totalAmount = 0;
            lblTotalAmount.Text = "Итого: 0 ₽";
        }

        // Очистка заказа
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

        // Фильтрация ввода в поле поиска - разрешаем только буквы, цифры, пробел и дефис
        private void TxtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем: буквы (русские и английские), цифры, пробел, дефис, backspace
            if (!char.IsControl(e.KeyChar))
            {
                // Проверяем, является ли символ буквой, цифрой, пробелом или дефисом
                bool isValid = char.IsLetterOrDigit(e.KeyChar) ||
                               e.KeyChar == ' ' ||
                               e.KeyChar == '-';

                if (!isValid)
                {
                    e.Handled = true;

                    // Показываем подсказку при попытке ввести спецсимвол
                    TextBox textBox = sender as TextBox;
                    if (textBox != null)
                    {
                        toolTip1.Show("Разрешены только буквы, цифры, пробел и дефис",
                            textBox, 0, -20, 1500);
                    }
                }
            }
        }

        // Фильтрация ввода в поле поиска (дополнительная проверка при вставке)
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingSearch) return;

            _isUpdatingSearch = true;

            try
            {
                TextBox textBox = sender as TextBox;
                if (textBox != null)
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
            finally
            {
                _isUpdatingSearch = false;
            }
        }

        // Фильтр для текста поиска - оставляем только буквы, цифры, пробел и дефис
        private string FilterSearchText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return new string(input.Where(c =>
                char.IsLetterOrDigit(c) ||  // Буквы и цифры
                c == ' ' ||                  // Пробел
                c == '-').ToArray());        // Дефис
        }

        // Кнопка добавления нового клиента
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

        // Кнопка возврата в меню
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
                // Для других ролей, если нужно
                this.Close();
            }
        }

        // Добавление товара через кнопку (альтернатива двойному клику)
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

        // Удаление товара через кнопку
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
    }
}