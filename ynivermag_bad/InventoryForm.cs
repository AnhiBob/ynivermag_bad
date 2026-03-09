using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ynivermag_bad
{
    public partial class InventoryForm : Form
    {

        private const int MAX_RECEIVE_QUANTITY = 100; // Максимум для приёмки
        private const int MAX_WRITEOFF_QUANTITY = 50; // Максимум для списания
        private const int MAX_CART_ITEMS = 20; // Максимум позиций в корзине

        private string _connection;
        private string _currentUser;
        private string _currentLogin;
        private int _currentUserId;
        private bool _isUpdatingSearchReceive = false;
        private bool _isUpdatingSearchWriteOff = false;

        // Для приёмки
        private DataTable _receiveCart;

        // Для списания
        private DataTable _writeOffCart;

        public InventoryForm(string fio, string login = null)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            _currentUser = fio;
            _currentLogin = login;

            // Загружаем ID пользователя по логину
            LoadCurrentUserId();

            // Настраиваем таблицы
            ConfigureAllGrids();

            // Заполняем причины списания
            comboReason.Items.AddRange(new string[] {
                "Брак", "Истек срок годности", "Утеря", "Порча", "Инвентаризация", "Другое"
            });
            comboReason.SelectedIndex = -1;
            // Подписываемся на событие изменения выбора
            comboReason.SelectedIndexChanged += ComboReason_SelectedIndexChanged;

            // Изначально кнопка неактивна (так как SelectedIndex = -1)
            button4.Enabled = false;

            // Подписываемся на события
            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;

            // ДОБАВЛЕНО: фильтрация ввода в полях поиска
            txtSearchReceive.TextChanged += TxtSearchReceive_TextChanged;
            txtSearchReceive.KeyPress += TxtSearch_KeyPress;
            txtSearchWriteOff.TextChanged += TxtSearchWriteOff_TextChanged;
            txtSearchWriteOff.KeyPress += TxtSearch_KeyPress;

            // ДОБАВЛЕНО: подсказки для полей поиска
            toolTip1.SetToolTip(txtSearchReceive, "Поиск по названию товара (буквы, цифры, пробел, дефис)");
            toolTip1.SetToolTip(txtSearchWriteOff, "Поиск по названию товара (буквы, цифры, пробел, дефис)");

            // Инициализируем корзины
            _receiveCart = new DataTable();
            _receiveCart.Columns.Add("ID", typeof(int));
            _receiveCart.Columns.Add("Товар", typeof(string));
            _receiveCart.Columns.Add("Количество", typeof(int));
            _receiveCart.Columns.Add("Цена", typeof(decimal));
            _receiveCart.Columns.Add("Сумма", typeof(decimal));
            _receiveCart.Columns.Add("Доступно", typeof(int)); // Для списания
            dataGridViewReceiveCart.DataSource = _receiveCart;

            _writeOffCart = new DataTable();
            _writeOffCart.Columns.Add("ID", typeof(int));
            _writeOffCart.Columns.Add("Товар", typeof(string));
            _writeOffCart.Columns.Add("Количество", typeof(int));
            _writeOffCart.Columns.Add("Цена", typeof(decimal));
            _writeOffCart.Columns.Add("Сумма", typeof(decimal));
            _writeOffCart.Columns.Add("Доступно", typeof(int)); // Для списания
            dataGridViewWriteOffCart.DataSource = _writeOffCart;

            // Загружаем товары при запуске
            LoadProducts(dataGridViewReceiveSearch);
            LoadProducts(dataGridViewWriteOffSearch);
            UpdateReceiveTotal();
            UpdateWriteOffTotal();
        }

        private MySqlConnection GetNewConnection() => new MySqlConnection(_connection);

        private void LoadCurrentUserId()
        {
            try
            {
                using (var conn = GetNewConnection())
                {
                    conn.Open();

                    // Пытаемся найти по логину (приоритет)
                    if (!string.IsNullOrEmpty(_currentLogin))
                    {
                        string sqlLogin = "SELECT user_id FROM user WHERE username = @login";
                        MySqlCommand cmdLogin = new MySqlCommand(sqlLogin, conn);
                        cmdLogin.Parameters.AddWithValue("@login", _currentLogin);

                        object resultLogin = cmdLogin.ExecuteScalar();
                        if (resultLogin != null)
                        {
                            _currentUserId = Convert.ToInt32(resultLogin);
                            return;
                        }
                    }

                    // Если не нашли по логину, пробуем по ФИО
                    string sqlFio = "SELECT user_id FROM user WHERE CONCAT(last_name, ' ', first_name) = @fio";
                    MySqlCommand cmdFio = new MySqlCommand(sqlFio, conn);
                    cmdFio.Parameters.AddWithValue("@fio", _currentUser);

                    object resultFio = cmdFio.ExecuteScalar();
                    if (resultFio != null)
                    {
                        _currentUserId = Convert.ToInt32(resultFio);
                        return;
                    }

                    // Если всё равно не нашли, используем ID=1 как запасной
                    MessageBox.Show($"Пользователь не найден!\nЛогин: {_currentLogin}\nФИО: {_currentUser}\n\nБудет использован ID=1",
                        "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _currentUserId = 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователя: {ex.Message}\n\nБудет использован ID=1",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _currentUserId = 1;
            }
        }

        private void ConfigureAllGrids()
        {
            // ========== ТАБЛИЦА ПОИСКА ДЛЯ ПРИЁМКИ ==========
            ConfigureSearchGrid(dataGridViewReceiveSearch);

            // ========== КОРЗИНА ПРИЁМКИ ==========
            ConfigureReceiveCartGrid();

            // ========== ТАБЛИЦА ПОИСКА ДЛЯ СПИСАНИЯ ==========
            ConfigureSearchGrid(dataGridViewWriteOffSearch);

            // ========== КОРЗИНА СПИСАНИЯ ==========
            ConfigureWriteOffCartGrid();

            // ========== ТАБЛИЦА ИСТОРИИ ==========
            ConfigureHistoryGrid();
        }

        private void ConfigureSearchGrid(DataGridView grid)
        {
            grid.AutoGenerateColumns = false;
            grid.Columns.Clear();
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.RowTemplate.Height = 30;

            // ID (скрытый)
            DataGridViewTextBoxColumn idCol = new DataGridViewTextBoxColumn();
            idCol.Name = "ID";
            idCol.DataPropertyName = "ID";
            idCol.Visible = false;
            grid.Columns.Add(idCol);

            // Название
            DataGridViewTextBoxColumn nameCol = new DataGridViewTextBoxColumn();
            nameCol.Name = "Название";
            nameCol.HeaderText = "Название товара";
            nameCol.DataPropertyName = "Название";
            nameCol.Width = 300;
            nameCol.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grid.Columns.Add(nameCol);

            // Цена
            DataGridViewTextBoxColumn priceCol = new DataGridViewTextBoxColumn();
            priceCol.Name = "Цена";
            priceCol.HeaderText = "Цена";
            priceCol.DataPropertyName = "Цена";
            priceCol.Width = 120;
            priceCol.DefaultCellStyle.Format = "C2";
            priceCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.Columns.Add(priceCol);

            // Количество на складе
            DataGridViewTextBoxColumn qtyCol = new DataGridViewTextBoxColumn();
            qtyCol.Name = "Количество";
            qtyCol.HeaderText = "На складе";
            qtyCol.DataPropertyName = "Количество";
            qtyCol.Width = 120;
            qtyCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.Columns.Add(qtyCol);

            // Кнопка добавления
            DataGridViewButtonColumn addBtn = new DataGridViewButtonColumn();
            addBtn.Name = "Добавить";
            addBtn.HeaderText = "";
            addBtn.Text = "➕ Добавить";
            addBtn.UseColumnTextForButtonValue = true;
            addBtn.Width = 100;
            grid.Columns.Add(addBtn);
        }

        private void ConfigureReceiveCartGrid()
        {
            dataGridViewReceiveCart.AutoGenerateColumns = false;
            dataGridViewReceiveCart.Columns.Clear();
            dataGridViewReceiveCart.ReadOnly = false;
            dataGridViewReceiveCart.RowHeadersVisible = false;
            dataGridViewReceiveCart.AllowUserToAddRows = false;
            dataGridViewReceiveCart.RowTemplate.Height = 30;
            dataGridViewReceiveCart.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridViewReceiveCart.CellEndEdit += DataGridViewReceiveCart_CellEndEdit;
            dataGridViewReceiveCart.CellValidating += DataGridViewReceiveCart_CellValidating;
            dataGridViewReceiveCart.EditingControlShowing += DataGridViewReceiveCart_EditingControlShowing;

            // ID (скрытый)
            DataGridViewTextBoxColumn idCol = new DataGridViewTextBoxColumn();
            idCol.Name = "ID";
            idCol.DataPropertyName = "ID";
            idCol.Visible = false;
            dataGridViewReceiveCart.Columns.Add(idCol);

            // Товар
            DataGridViewTextBoxColumn nameCol = new DataGridViewTextBoxColumn();
            nameCol.Name = "Товар";
            nameCol.HeaderText = "Товар";
            nameCol.DataPropertyName = "Товар";
            nameCol.Width = 280;
            nameCol.ReadOnly = true;
            nameCol.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridViewReceiveCart.Columns.Add(nameCol);

            // Количество (редактируемое)
            DataGridViewTextBoxColumn qtyCol = new DataGridViewTextBoxColumn();
            qtyCol.Name = "Количество";
            qtyCol.HeaderText = "Кол-во";
            qtyCol.DataPropertyName = "Количество";
            qtyCol.Width = 80;
            qtyCol.ReadOnly = false;
            qtyCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewReceiveCart.Columns.Add(qtyCol);

            // Цена
            DataGridViewTextBoxColumn priceCol = new DataGridViewTextBoxColumn();
            priceCol.Name = "Цена";
            priceCol.HeaderText = "Цена";
            priceCol.DataPropertyName = "Цена";
            priceCol.Width = 120;
            priceCol.ReadOnly = true;
            priceCol.DefaultCellStyle.Format = "C2";
            priceCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewReceiveCart.Columns.Add(priceCol);

            // Сумма
            DataGridViewTextBoxColumn sumCol = new DataGridViewTextBoxColumn();
            sumCol.Name = "Сумма";
            sumCol.HeaderText = "Сумма";
            sumCol.DataPropertyName = "Сумма";
            sumCol.Width = 120;
            sumCol.ReadOnly = true;
            sumCol.DefaultCellStyle.Format = "C2";
            sumCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewReceiveCart.Columns.Add(sumCol);

            // Кнопка удаления
            DataGridViewButtonColumn removeBtn = new DataGridViewButtonColumn();
            removeBtn.Name = "Удалить";
            removeBtn.HeaderText = "";
            removeBtn.Text = "❌";
            removeBtn.UseColumnTextForButtonValue = true;
            removeBtn.Width = 50;
            removeBtn.ReadOnly = true;
            dataGridViewReceiveCart.Columns.Add(removeBtn);
        }

        private void ConfigureWriteOffCartGrid()
        {
            dataGridViewWriteOffCart.AutoGenerateColumns = false;
            dataGridViewWriteOffCart.Columns.Clear();
            dataGridViewWriteOffCart.ReadOnly = false;
            dataGridViewWriteOffCart.RowHeadersVisible = false;
            dataGridViewWriteOffCart.AllowUserToAddRows = false;
            dataGridViewWriteOffCart.RowTemplate.Height = 30;
            dataGridViewWriteOffCart.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridViewWriteOffCart.CellEndEdit += DataGridViewWriteOffCart_CellEndEdit;
            dataGridViewWriteOffCart.CellValidating += DataGridViewWriteOffCart_CellValidating;
            dataGridViewWriteOffCart.EditingControlShowing += DataGridViewWriteOffCart_EditingControlShowing;

            // ID (скрытый)
            DataGridViewTextBoxColumn idCol = new DataGridViewTextBoxColumn();
            idCol.Name = "ID";
            idCol.DataPropertyName = "ID";
            idCol.Visible = false;
            dataGridViewWriteOffCart.Columns.Add(idCol);

            // Товар
            DataGridViewTextBoxColumn nameCol = new DataGridViewTextBoxColumn();
            nameCol.Name = "Товар";
            nameCol.HeaderText = "Товар";
            nameCol.DataPropertyName = "Товар";
            nameCol.Width = 280;
            nameCol.ReadOnly = true;
            nameCol.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridViewWriteOffCart.Columns.Add(nameCol);

            // Количество (редактируемое)
            DataGridViewTextBoxColumn qtyCol = new DataGridViewTextBoxColumn();
            qtyCol.Name = "Количество";
            qtyCol.HeaderText = "Кол-во";
            qtyCol.DataPropertyName = "Количество";
            qtyCol.Width = 80;
            qtyCol.ReadOnly = false;
            qtyCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewWriteOffCart.Columns.Add(qtyCol);

            // Цена
            DataGridViewTextBoxColumn priceCol = new DataGridViewTextBoxColumn();
            priceCol.Name = "Цена";
            priceCol.HeaderText = "Цена";
            priceCol.DataPropertyName = "Цена";
            priceCol.Width = 120;
            priceCol.ReadOnly = true;
            priceCol.DefaultCellStyle.Format = "C2";
            priceCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewWriteOffCart.Columns.Add(priceCol);

            // Сумма
            DataGridViewTextBoxColumn sumCol = new DataGridViewTextBoxColumn();
            sumCol.Name = "Сумма";
            sumCol.HeaderText = "Сумма";
            sumCol.DataPropertyName = "Сумма";
            sumCol.Width = 120;
            sumCol.ReadOnly = true;
            sumCol.DefaultCellStyle.Format = "C2";
            sumCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewWriteOffCart.Columns.Add(sumCol);

            // Доступно (скрытое поле для проверки)
            DataGridViewTextBoxColumn availCol = new DataGridViewTextBoxColumn();
            availCol.Name = "Доступно";
            availCol.DataPropertyName = "Доступно";
            availCol.Visible = false;
            dataGridViewWriteOffCart.Columns.Add(availCol);

            // Кнопка удаления
            DataGridViewButtonColumn removeBtn = new DataGridViewButtonColumn();
            removeBtn.Name = "Удалить";
            removeBtn.HeaderText = "";
            removeBtn.Text = "❌";
            removeBtn.UseColumnTextForButtonValue = true;
            removeBtn.Width = 50;
            removeBtn.ReadOnly = true;
            dataGridViewWriteOffCart.Columns.Add(removeBtn);
        }

        private void ConfigureHistoryGrid()
        {
            dataGridViewHistory.AutoGenerateColumns = false;
            dataGridViewHistory.Columns.Clear();
            dataGridViewHistory.ReadOnly = true;
            dataGridViewHistory.RowHeadersVisible = false;
            dataGridViewHistory.AllowUserToAddRows = false;
            dataGridViewHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Дата
            DataGridViewTextBoxColumn dateCol = new DataGridViewTextBoxColumn();
            dateCol.Name = "Дата";
            dateCol.HeaderText = "Дата";
            dateCol.DataPropertyName = "Дата";
            dateCol.Width = 140;
            dataGridViewHistory.Columns.Add(dateCol);

            // Товар
            DataGridViewTextBoxColumn productCol = new DataGridViewTextBoxColumn();
            productCol.Name = "Товар";
            productCol.HeaderText = "Товар";
            productCol.DataPropertyName = "Товар";
            productCol.Width = 250;
            dataGridViewHistory.Columns.Add(productCol);

            // Тип операции
            DataGridViewTextBoxColumn typeCol = new DataGridViewTextBoxColumn();
            typeCol.Name = "Тип";
            typeCol.HeaderText = "Тип";
            typeCol.DataPropertyName = "Тип";
            typeCol.Width = 100;
            dataGridViewHistory.Columns.Add(typeCol);

            // Количество
            DataGridViewTextBoxColumn qtyCol = new DataGridViewTextBoxColumn();
            qtyCol.Name = "Количество";
            qtyCol.HeaderText = "Кол-во";
            qtyCol.DataPropertyName = "Количество";
            qtyCol.Width = 80;
            qtyCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewHistory.Columns.Add(qtyCol);

            // Было
            DataGridViewTextBoxColumn oldCol = new DataGridViewTextBoxColumn();
            oldCol.Name = "Было";
            oldCol.HeaderText = "Было";
            oldCol.DataPropertyName = "Было";
            oldCol.Width = 70;
            oldCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewHistory.Columns.Add(oldCol);

            // Стало
            DataGridViewTextBoxColumn newCol = new DataGridViewTextBoxColumn();
            newCol.Name = "Стало";
            newCol.HeaderText = "Стало";
            newCol.DataPropertyName = "Стало";
            newCol.Width = 70;
            newCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewHistory.Columns.Add(newCol);

            // Пользователь
            DataGridViewTextBoxColumn userCol = new DataGridViewTextBoxColumn();
            userCol.Name = "Пользователь";
            userCol.HeaderText = "Пользователь";
            userCol.DataPropertyName = "Пользователь";
            userCol.Width = 180;
            dataGridViewHistory.Columns.Add(userCol);

            // Комментарий
            DataGridViewTextBoxColumn commentCol = new DataGridViewTextBoxColumn();
            commentCol.Name = "Комментарий";
            commentCol.HeaderText = "Комментарий";
            commentCol.DataPropertyName = "Комментарий";
            commentCol.Width = 200;
            dataGridViewHistory.Columns.Add(commentCol);
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPageReceive)
            {
                LoadProducts(dataGridViewReceiveSearch);
                UpdateReceiveTotal();
            }
            else if (tabControl1.SelectedTab == tabPageWriteOff)
            {
                LoadProducts(dataGridViewWriteOffSearch);
                UpdateWriteOffTotal();
            }
            else if (tabControl1.SelectedTab == tabPageHistory)
            {
                LoadHistory();
            }
        }

        private void LoadProducts(DataGridView grid, string searchText = "")
        {
            try
            {
                using (var conn = GetNewConnection())
                {
                    conn.Open();
                    string sql = @"SELECT 
                        product_id as ID,
                        name as Название,
                        stock_quantity as Количество,
                        price as Цена
                    FROM product 
                    WHERE isActive = TRUE";

                    if (!string.IsNullOrWhiteSpace(searchText))
                        sql += " AND name LIKE @search";

                    sql += " ORDER BY name LIMIT 100";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    if (!string.IsNullOrWhiteSpace(searchText))
                        cmd.Parameters.AddWithValue("@search", $"%{searchText}%");

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    grid.DataSource = dt;

                    // Автоматически подгоняем высоту строк под содержимое
                    foreach (DataGridViewRow row in grid.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            row.Height = grid.RowTemplate.Height;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        #region Фильтрация ввода в полях поиска

        /// <summary>
        /// Фильтрация ввода в поле поиска - разрешаем только буквы, цифры, пробел и дефис
        /// </summary>
        private void TxtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем backspace
            if (char.IsControl(e.KeyChar))
                return;

            // Разрешенные символы: буквы, цифры, пробел, дефис
            bool isValid = char.IsLetterOrDigit(e.KeyChar) ||
                           e.KeyChar == ' ' ||
                           e.KeyChar == '-';

            if (!isValid)
            {
                e.Handled = true;

                // Показываем подсказку при попытке ввести спецсимвол
                if (sender is System.Windows.Forms.TextBox textBox)
                {
                    toolTip1.Show("Разрешены только буквы, цифры, пробел и дефис",
                        textBox, 0, -20, 1500);
                }
            }
        }

        /// <summary>
        /// Фильтрация ввода в поле поиска приёмки (дополнительная проверка при вставке)
        /// </summary>
        private void TxtSearchReceive_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingSearchReceive) return;

            _isUpdatingSearchReceive = true;

            try
            {
                if (sender is System.Windows.Forms.TextBox textBox)
                {
                    int selectionStart = textBox.SelectionStart;
                    string filteredText = FilterSearchText(textBox.Text);

                    if (filteredText != textBox.Text)
                    {
                        textBox.Text = filteredText;
                        textBox.SelectionStart = Math.Min(selectionStart, filteredText.Length);
                    }
                }

                // Загружаем товары с отфильтрованным текстом
                LoadProducts(dataGridViewReceiveSearch, txtSearchReceive.Text);
            }
            finally
            {
                _isUpdatingSearchReceive = false;
            }
        }

        /// <summary>
        /// Фильтрация ввода в поле поиска списания (дополнительная проверка при вставке)
        /// </summary>
        private void TxtSearchWriteOff_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingSearchWriteOff) return;

            _isUpdatingSearchWriteOff = true;

            try
            {
                if (sender is System.Windows.Forms.TextBox textBox)
                {
                    int selectionStart = textBox.SelectionStart;
                    string filteredText = FilterSearchText(textBox.Text);

                    if (filteredText != textBox.Text)
                    {
                        textBox.Text = filteredText;
                        textBox.SelectionStart = Math.Min(selectionStart, filteredText.Length);
                    }
                }

                // Загружаем товары с отфильтрованным текстом
                LoadProducts(dataGridViewWriteOffSearch, txtSearchWriteOff.Text);
            }
            finally
            {
                _isUpdatingSearchWriteOff = false;
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

        #endregion

        // ========== ПРИЁМКА ==========

        private void dataGridViewReceiveSearch_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != dataGridViewReceiveSearch.Columns["Добавить"].Index)
                return;

            var row = dataGridViewReceiveSearch.Rows[e.RowIndex];
            int productId = Convert.ToInt32(row.Cells["ID"].Value);
            string productName = row.Cells["Название"].Value.ToString();
            decimal price = Convert.ToDecimal(row.Cells["Цена"].Value);
            int quantity = (int)numericReceive.Value;

            // Проверка на максимальное количество
            if (quantity <= 0)
            {
                MessageBox.Show("Количество должно быть больше 0!");
                return;
            }

            if (quantity > MAX_RECEIVE_QUANTITY)
            {
                MessageBox.Show($"Максимальное количество для приёмки: {MAX_RECEIVE_QUANTITY} шт.");
                return;
            }

            // Проверка на максимальное количество позиций в корзине
            if (_receiveCart.Rows.Count >= MAX_CART_ITEMS)
            {
                MessageBox.Show($"Максимальное количество позиций в корзине: {MAX_CART_ITEMS}");
                return;
            }

            // Проверяем, есть ли уже такой товар в корзине
            bool found = false;
            foreach (DataRow r in _receiveCart.Rows)
            {
                if (Convert.ToInt32(r["ID"]) == productId)
                {
                    int newTotal = Convert.ToInt32(r["Количество"]) + quantity;

                    // Проверяем общее количество после добавления
                    if (newTotal > MAX_RECEIVE_QUANTITY)
                    {
                        MessageBox.Show($"Общее количество товара '{productName}' не может превышать {MAX_RECEIVE_QUANTITY} шт.");
                        return;
                    }

                    r["Количество"] = newTotal;
                    r["Сумма"] = Convert.ToDecimal(r["Цена"]) * newTotal;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                DataRow newRow = _receiveCart.NewRow();
                newRow["ID"] = productId;
                newRow["Товар"] = productName;
                newRow["Количество"] = quantity;
                newRow["Цена"] = price;
                newRow["Сумма"] = price * quantity;
                _receiveCart.Rows.Add(newRow);
            }

            UpdateReceiveTotal();
        }

        private void dataGridViewReceiveCart_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Проверяем, нажата ли кнопка удаления
            if (e.ColumnIndex == dataGridViewReceiveCart.Columns["Удалить"].Index)
            {
                _receiveCart.Rows[e.RowIndex].Delete();
                UpdateReceiveTotal();
            }
        }

        // Валидация количества в корзине приёмки
        private void DataGridViewReceiveCart_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (dataGridViewReceiveCart.Columns[e.ColumnIndex].Name == "Количество")
            {
                if (!int.TryParse(e.FormattedValue.ToString(), out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Количество должно быть положительным числом!");
                    e.Cancel = true;
                }
                else if (quantity > MAX_RECEIVE_QUANTITY)
                {
                    MessageBox.Show($"Количество не может превышать {MAX_RECEIVE_QUANTITY}!");
                    e.Cancel = true;
                }
            }
        }

        // После редактирования количества в приёмке
        private void DataGridViewReceiveCart_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewReceiveCart.Columns[e.ColumnIndex].Name == "Количество")
            {
                DataGridViewRow row = dataGridViewReceiveCart.Rows[e.RowIndex];
                int quantity = Convert.ToInt32(row.Cells["Количество"].Value);
                decimal price = Convert.ToDecimal(row.Cells["Цена"].Value);
                row.Cells["Сумма"].Value = price * quantity;
                UpdateReceiveTotal();
            }
        }

        // Ограничение ввода только цифр для количества в корзине приёмки
        private void DataGridViewReceiveCart_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dataGridViewReceiveCart.CurrentCell.ColumnIndex == dataGridViewReceiveCart.Columns["Количество"].Index)
            {
                if (e.Control is System.Windows.Forms.TextBox tb)
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

        private void UpdateReceiveTotal()
        {
            int totalItems = 0; // Общее количество единиц товара
            int totalPositions = _receiveCart.Rows.Count; // Количество позиций

            foreach (DataRow row in _receiveCart.Rows)
            {
                totalItems += Convert.ToInt32(row["Количество"]);
            }

            decimal totalSum = 0;
            foreach (DataRow row in _receiveCart.Rows)
                totalSum += Convert.ToDecimal(row["Сумма"]);

            lblReceiveTotal.Text = $"Позиций: {totalPositions}, Всего единиц: {totalItems}, Сумма: {totalSum:C2}";
        }

        private void btnReceiveClear_Click(object sender, EventArgs e)
        {
            _receiveCart.Clear();
            UpdateReceiveTotal();
        }

        private void btnReceiveProcess_Click(object sender, EventArgs e)
        {
            if (_receiveCart.Rows.Count == 0)
            {
                MessageBox.Show("Корзина пуста!");
                return;
            }

            try
            {
                using (var conn = GetNewConnection())
                {
                    conn.Open();

                    foreach (DataRow row in _receiveCart.Rows)
                    {
                        int productId = Convert.ToInt32(row["ID"]);
                        int quantity = Convert.ToInt32(row["Количество"]);

                        // Получаем текущее количество
                        string sql1 = "SELECT stock_quantity FROM product WHERE product_id = @id";
                        MySqlCommand cmd1 = new MySqlCommand(sql1, conn);
                        cmd1.Parameters.AddWithValue("@id", productId);
                        int oldQty = Convert.ToInt32(cmd1.ExecuteScalar());
                        int newQty = oldQty + quantity;

                        // Обновляем количество
                        string sql2 = "UPDATE product SET stock_quantity = @newQty WHERE product_id = @id";
                        MySqlCommand cmd2 = new MySqlCommand(sql2, conn);
                        cmd2.Parameters.AddWithValue("@newQty", newQty);
                        cmd2.Parameters.AddWithValue("@id", productId);
                        cmd2.ExecuteNonQuery();

                        // Записываем в историю
                        string sql3 = @"INSERT INTO inventory_history 
                            (product_id, user_id, operation_type, quantity, old_quantity, new_quantity, comment)
                            VALUES (@pid, @uid, 'приёмка', @qty, @old, @new, 'Приёмка товара')";

                        MySqlCommand cmd3 = new MySqlCommand(sql3, conn);
                        cmd3.Parameters.AddWithValue("@pid", productId);
                        cmd3.Parameters.AddWithValue("@uid", _currentUserId);
                        cmd3.Parameters.AddWithValue("@qty", quantity);
                        cmd3.Parameters.AddWithValue("@old", oldQty);
                        cmd3.Parameters.AddWithValue("@new", newQty);
                        cmd3.ExecuteNonQuery();
                    }

                    decimal total = 0;
                    foreach (DataRow row in _receiveCart.Rows)
                        total += Convert.ToDecimal(row["Сумма"]);

                    MessageBox.Show($"✅ Приёмка выполнена!\n\n" +
                        $"Сумма: {total:C2}",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    _receiveCart.Clear();
                    LoadProducts(dataGridViewReceiveSearch, txtSearchReceive.Text);
                    UpdateReceiveTotal();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}");
            }
        }

        // ========== СПИСАНИЕ ==========

        private void dataGridViewWriteOffSearch_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != dataGridViewWriteOffSearch.Columns["Добавить"].Index)
                return;

            var row = dataGridViewWriteOffSearch.Rows[e.RowIndex];
            int productId = Convert.ToInt32(row.Cells["ID"].Value);
            string productName = row.Cells["Название"].Value.ToString();
            int availableQty = Convert.ToInt32(row.Cells["Количество"].Value);
            decimal price = Convert.ToDecimal(row.Cells["Цена"].Value);
            int quantity = (int)numericWriteOff.Value;

            if (quantity <= 0)
            {
                MessageBox.Show("Количество должно быть больше 0!");
                return;
            }

            if (availableQty < quantity)
            {
                MessageBox.Show($"Недостаточно товара! На складе: {availableQty} шт.");
                return;
            }

            // Проверяем, есть ли уже такой товар в корзине
            bool found = false;
            foreach (DataRow r in _writeOffCart.Rows)
            {
                if (Convert.ToInt32(r["ID"]) == productId)
                {
                    int newQty = Convert.ToInt32(r["Количество"]) + quantity;
                    if (newQty > availableQty)
                    {
                        MessageBox.Show($"Всего можно списать максимум {availableQty} шт.");
                        return;
                    }
                    r["Количество"] = newQty;
                    r["Сумма"] = Convert.ToDecimal(r["Цена"]) * newQty;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                DataRow newRow = _writeOffCart.NewRow();
                newRow["ID"] = productId;
                newRow["Товар"] = productName;
                newRow["Количество"] = quantity;
                newRow["Цена"] = price;
                newRow["Сумма"] = price * quantity;
                newRow["Доступно"] = availableQty;
                _writeOffCart.Rows.Add(newRow);
            }

            UpdateWriteOffTotal();
        }

        private void dataGridViewWriteOffCart_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Проверяем, нажата ли кнопка удаления
            if (e.ColumnIndex == dataGridViewWriteOffCart.Columns["Удалить"].Index)
            {
                _writeOffCart.Rows[e.RowIndex].Delete();
                UpdateWriteOffTotal();
            }
        }

        // Валидация количества в корзине списания
        private void DataGridViewWriteOffCart_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (dataGridViewWriteOffCart.Columns[e.ColumnIndex].Name == "Количество")
            {
                DataGridViewRow row = dataGridViewWriteOffCart.Rows[e.RowIndex];
                int availableQty = Convert.ToInt32(row.Cells["Доступно"].Value);

                if (!int.TryParse(e.FormattedValue.ToString(), out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Количество должно быть положительным числом!");
                    e.Cancel = true;
                }
                else if (quantity > availableQty)
                {
                    MessageBox.Show($"Недостаточно товара! Доступно: {availableQty} шт.");
                    e.Cancel = true;
                }
                else if (quantity > MAX_WRITEOFF_QUANTITY)
                {
                    MessageBox.Show($"Количество не может превышать {MAX_WRITEOFF_QUANTITY}!");
                    e.Cancel = true;
                }
            }
        }

        // После редактирования количества в списании
        private void DataGridViewWriteOffCart_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewWriteOffCart.Columns[e.ColumnIndex].Name == "Количество")
            {
                DataGridViewRow row = dataGridViewWriteOffCart.Rows[e.RowIndex];
                int quantity = Convert.ToInt32(row.Cells["Количество"].Value);
                decimal price = Convert.ToDecimal(row.Cells["Цена"].Value);
                row.Cells["Сумма"].Value = price * quantity;
                UpdateWriteOffTotal();
            }
        }

        // Ограничение ввода только цифр для количества в корзине списания
        private void DataGridViewWriteOffCart_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dataGridViewWriteOffCart.CurrentCell.ColumnIndex == dataGridViewWriteOffCart.Columns["Количество"].Index)
            {
                if (e.Control is System.Windows.Forms.TextBox tb)
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


        private void UpdateWriteOffTotal()
        {
            int totalItems = 0; // Общее количество единиц товара
            int totalPositions = _writeOffCart.Rows.Count; // Количество позиций

            foreach (DataRow row in _writeOffCart.Rows)
            {
                totalItems += Convert.ToInt32(row["Количество"]);
            }

            decimal totalSum = 0;
            foreach (DataRow row in _writeOffCart.Rows)
                totalSum += Convert.ToDecimal(row["Сумма"]);

            lblWriteOffTotal.Text = $"Позиций: {totalPositions}, Всего единиц: {totalItems}, Сумма: {totalSum:C2}";
        }

        private void btnWriteOffClear_Click(object sender, EventArgs e)
        {

            _writeOffCart.Clear();
            UpdateWriteOffTotal();
            comboReason.SelectedIndex = -1; // Сбрасываем выбор причины
                                            // Кнопка автоматически станет неактивной через обработчик SelectedIndexChanged
        }

        private void btnWriteOffProcess_Click(object sender, EventArgs e)
        {
            if (_writeOffCart.Rows.Count == 0)
            {
                MessageBox.Show("Корзина пуста!");
                return;
            }

            if (comboReason.SelectedItem == null)
            {
                MessageBox.Show("Выберите причину списания!");
                return;
            }

            string reason = comboReason.SelectedItem.ToString();

            // Счетчики для отчета
            int successCount = 0;
            int errorCount = 0;
            decimal totalSum = 0;
            List<string> errors = new List<string>();

            try
            {
                using (var conn = GetNewConnection())
                {
                    conn.Open();

                    foreach (DataRow row in _writeOffCart.Rows)
                    {
                        try
                        {
                            int productId = Convert.ToInt32(row["ID"]);
                            string productName = row["Товар"].ToString();
                            int quantity = Convert.ToInt32(row["Количество"]);
                            decimal price = Convert.ToDecimal(row["Цена"]);

                            // Получаем текущее количество
                            string sql1 = "SELECT stock_quantity FROM product WHERE product_id = @id";
                            MySqlCommand cmd1 = new MySqlCommand(sql1, conn);
                            cmd1.Parameters.AddWithValue("@id", productId);

                            object result = cmd1.ExecuteScalar();
                            if (result == null || result == DBNull.Value)
                            {
                                errors.Add($"❌ Товар '{productName}' не найден в базе");
                                errorCount++;
                                continue;
                            }

                            int oldQty = Convert.ToInt32(result);

                            if (oldQty < quantity)
                            {
                                errors.Add($"❌ Недостаточно товара '{productName}'. На складе: {oldQty}, запрошено: {quantity}");
                                errorCount++;
                                continue;
                            }

                            int newQty = oldQty - quantity;

                            // Обновляем количество
                            string sql2 = "UPDATE product SET stock_quantity = @newQty WHERE product_id = @id";
                            MySqlCommand cmd2 = new MySqlCommand(sql2, conn);
                            cmd2.Parameters.AddWithValue("@newQty", newQty);
                            cmd2.Parameters.AddWithValue("@id", productId);
                            cmd2.ExecuteNonQuery();

                            // Записываем в историю
                            string sql3 = @"INSERT INTO inventory_history 
                        (product_id, user_id, operation_type, quantity, old_quantity, new_quantity, comment)
                        VALUES (@pid, @uid, 'списание', @qty, @old, @new, @comm)";

                            MySqlCommand cmd3 = new MySqlCommand(sql3, conn);
                            cmd3.Parameters.AddWithValue("@pid", productId);
                            cmd3.Parameters.AddWithValue("@uid", _currentUserId);
                            cmd3.Parameters.AddWithValue("@qty", quantity);
                            cmd3.Parameters.AddWithValue("@old", oldQty);
                            cmd3.Parameters.AddWithValue("@new", newQty);
                            cmd3.Parameters.AddWithValue("@comm", reason);
                            cmd3.ExecuteNonQuery();

                            // Успешно обработано
                            successCount++;
                            totalSum += price * quantity;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"❌ Ошибка при обработке товара '{row["Товар"]}': {ex.Message}");
                            errorCount++;
                        }
                    }
                }

                // Формируем сообщение о результате
                string message = "";

                if (successCount > 0)
                {
                    message += $"✅ Успешно списано: {successCount} товаров\n";
                    message += $"💰 Сумма: {totalSum:C2}\n";
                    message += $"📝 Причина: {reason}\n";
                }

                if (errorCount > 0)
                {
                    message += $"\n❌ Ошибок: {errorCount}\n";
                    message += string.Join("\n", errors);
                }

                if (successCount > 0)
                {
                    MessageBox.Show(message, "Результат списания",
                        MessageBoxButtons.OK,
                        successCount > 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                    // Очищаем только успешно обработанные товары
                    DataTable newCart = new DataTable();
                    newCart.Columns.Add("ID", typeof(int));
                    newCart.Columns.Add("Товар", typeof(string));
                    newCart.Columns.Add("Количество", typeof(int));
                    newCart.Columns.Add("Цена", typeof(decimal));
                    newCart.Columns.Add("Сумма", typeof(decimal));
                    newCart.Columns.Add("Доступно", typeof(int));

                    // Копируем только те товары, которые не удалось обработать
                    foreach (DataRow row in _writeOffCart.Rows)
                    {
                        bool wasError = false;
                        foreach (string error in errors)
                        {
                            if (error.Contains(row["Товар"].ToString()))
                            {
                                wasError = true;
                                break;
                            }
                        }

                        if (wasError)
                        {
                            newCart.ImportRow(row);
                        }
                    }

                    _writeOffCart.Clear();
                    foreach (DataRow row in newCart.Rows)
                    {
                        _writeOffCart.ImportRow(row);
                    }

                    LoadProducts(dataGridViewWriteOffSearch, txtSearchWriteOff.Text);
                    UpdateWriteOffTotal();

                    if (_writeOffCart.Rows.Count == 0)
                    {
                        comboReason.SelectedIndex = -1;
                        button4.Enabled = false;
                    }
                }
                else
                {
                    MessageBox.Show(message, "Ошибка списания",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Критическая ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ComboReason_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Активируем кнопку только если выбран какой-то элемент
            button4.Enabled = comboReason.SelectedItem != null;
        }

        // ========== ИСТОРИЯ ==========

        private void LoadHistory()
        {
            try
            {
                using (var conn = GetNewConnection())
                {
                    conn.Open();
                    string sql = @"SELECT 
                        DATE_FORMAT(h.operation_date, '%d.%m.%Y %H:%i') as Дата,
                        p.name as Товар,
                        CASE h.operation_type 
                            WHEN 'приёмка' THEN '📦 ПРИЁМКА'
                            ELSE '🗑️ СПИСАНИЕ'
                        END as Тип,
                        h.quantity as Количество,
                        h.old_quantity as Было,
                        h.new_quantity as Стало,
                        CONCAT(u.last_name, ' ', u.first_name) as Пользователь,
                        h.comment as Комментарий
                    FROM inventory_history h
                    INNER JOIN product p ON h.product_id = p.product_id
                    INNER JOIN user u ON h.user_id = u.user_id
                    ORDER BY h.operation_date DESC
                    LIMIT 500";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridViewHistory.DataSource = dt;

                    foreach (DataGridViewRow row in dataGridViewHistory.Rows)
                    {
                        string type = row.Cells["Тип"].Value?.ToString() ?? "";
                        if (type.Contains("ПРИЁМКА"))
                            row.DefaultCellStyle.BackColor = Color.FromArgb(230, 255, 230);
                        else if (type.Contains("СПИСАНИЕ"))
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
                    }

                    lblTotalRecords.Text = $"Всего записей: {dataGridViewHistory.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}");
            }
        }

        private void btnRefreshHistory_Click(object sender, EventArgs e)
        {
            LoadHistory();
        }

        // ========== НАВИГАЦИЯ ==========

        private void addProduct_Click(object sender, EventArgs e)
        {
            // Создаем форму добавления товара
            AddProductForm addForm = new AddProductForm();

            // Показываем форму и ждем результат
            DialogResult result = addForm.ShowDialog();

            // Если товар успешно добавлен
            if (result == DialogResult.OK)
            {
                // Обновляем список товаров в текущей форме
                if (tabControl1.SelectedTab == tabPageReceive)
                {
                    LoadProducts(dataGridViewReceiveSearch, txtSearchReceive.Text);
                }
                else if (tabControl1.SelectedTab == tabPageWriteOff)
                {
                    LoadProducts(dataGridViewWriteOffSearch, txtSearchWriteOff.Text);
                }

                // Показываем сообщение об успехе
                MessageBox.Show("✅ Товар успешно добавлен! Теперь его можно выбрать в списке.",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InMenu_Click(object sender, EventArgs e)
        {
            MenuTovarovedForm menu = new MenuTovarovedForm(_currentUser);
            menu.Show();
            this.Close();
        }
    }
}