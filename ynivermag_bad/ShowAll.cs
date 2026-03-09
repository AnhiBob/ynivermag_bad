using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ynivermag_bad
{
    public partial class ShowAll : Form
    {
        private int _roleID;
        private string _fio;
        private string _login;
        private int _currentUserId;
        private string _connection;
        private EditClass _editClass;
        private ProductImageService _productImageService;

        // Вкладки
        private TabPage _tabClients;
        private TabPage _tabProduct;
        private TabPage _tabUsers;

        // Данные
        private DataTable _usersData;
        private DataTable _productsData;
        private DataTable _clientsData;

        // Размер миниатюр
        private const int THUMBNAIL_SIZE = 80;

        // Состояние редактирования
        private bool _isEditing = false;
        private int _editingId = 0;
        private string _editingEntityType = "";

        public ShowAll(string FIO, int roleId, string login = null)
        {
            InitializeComponent();
            _roleID = roleId;
            _fio = FIO;
            _login = login;
            _connection = Connection.ConnectionString;
            _editClass = new EditClass();
            _productImageService = new ProductImageService();

            // Получаем ID текущего пользователя
            _currentUserId = GetCurrentUserId();

            FIOlb.Text = _fio;
            _tabClients = tabPage1;
            _tabProduct = tabPage2;
            _tabUsers = tabPage3;

            // Настройка вкладок по ролям
            ConfigureTabsByRole();

            // Подписка на события
            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;

            // Настройка DataGridView
            ConfigureAllGrids();

            // Контекстные меню
            SetupContextMenus();

            // Настройка видимости кнопок добавления
            ConfigureAddButtons();
        }

        #region ============ ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ============

        private MySqlConnection GetNewConnection() => new MySqlConnection(_connection);

        private int GetCurrentUserId()
        {
            try
            {
                using (var conn = GetNewConnection())
                {
                    conn.Open();

                    // Пытаемся найти по логину
                    if (!string.IsNullOrEmpty(_login))
                    {
                        string sqlLogin = "SELECT user_id FROM user WHERE username = @login";
                        MySqlCommand cmdLogin = new MySqlCommand(sqlLogin, conn);
                        cmdLogin.Parameters.AddWithValue("@login", _login);
                        object resultLogin = cmdLogin.ExecuteScalar();
                        if (resultLogin != null)
                            return Convert.ToInt32(resultLogin);
                    }

                    // Если не нашли по логину, пробуем по ФИО
                    string sqlFio = "SELECT user_id FROM user WHERE CONCAT(last_name, ' ', first_name) = @fio";
                    MySqlCommand cmdFio = new MySqlCommand(sqlFio, conn);
                    cmdFio.Parameters.AddWithValue("@fio", _fio);
                    object resultFio = cmdFio.ExecuteScalar();
                    if (resultFio != null)
                        return Convert.ToInt32(resultFio);

                    return 1; // Запасной вариант
                }
            }
            catch
            {
                return 1;
            }
        }

        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowWarning(string message)
        {
            MessageBox.Show(message, "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ConfigureAddButtons()
        {
            // По умолчанию все кнопки невидимы
            AddProduct.Visible = false;
            AddClient.Visible = false;
            AddUser.Visible = false;

            switch (_roleID)
            {
                case 1: // Админ
                    AddProduct.Visible = true;
                    AddClient.Visible = true;
                    AddUser.Visible = true;
                    break;
                case 2: // Продавец
                    AddClient.Visible = true;
                    break;
                case 3: // Товаровед
                    AddProduct.Visible = true;
                    break;
            }
        }

        private void ConfigureAllGrids()
        {
            // Настройка каждого DataGridView
            ConfigureDataGridView(dataGridViewClient);
            ConfigureDataGridView(dataGridViewProduct);
            ConfigureDataGridView(dataGridViewUser);

            // Специальная настройка для товаров
            ConfigureProductGridView();
        }

        private void ConfigureDataGridView(DataGridView dgv)
        {
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.RowHeadersVisible = false;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToOrderColumns = false;
            dgv.AllowUserToResizeColumns = true;
            dgv.AllowUserToResizeRows = false;

            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(76, 175, 80);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            dgv.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    dgv.Rows[e.RowIndex].Selected = true;
                }
            };
        }

        #endregion

        #region ============ НАВИГАЦИЯ ============

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCurrentTabData();
        }

        private void LoadCurrentTabData()
        {
            if (tabControl1.SelectedTab == null) return;

            switch (tabControl1.SelectedTab.Name)
            {
                case "tabPage1": // Клиенты
                    LoadClientsData();
                    break;
                case "tabPage2": // Товары
                    LoadProductData();
                    break;
                case "tabPage3": // Пользователи
                    if (_roleID == 1) // Только админ видит пользователей
                        LoadUsersData();
                    break;
            }
        }

        private void InMenuClient_Click(object sender, EventArgs e)
        {
            if (_roleID == 1)
            {
                new MenuAdminForm(_fio).Show();
                this.Hide();
            }
            else if (_roleID == 2)
            {
                new MenuSellerForm(_fio).Show();
                this.Hide();
            }
            else if (_roleID == 3)
            {
                new MenuTovarovedForm(_fio).Show();
                this.Hide();
            }
        }

        private void ConfigureTabsByRole()
        {
            tabControl1.TabPages.Clear();

            switch (_roleID)
            {
                case 1: // Админ
                    tabControl1.TabPages.AddRange(new[] { _tabClients, _tabProduct, _tabUsers });
                    break;
                case 2: // Продавец
                    tabControl1.TabPages.AddRange(new[] { _tabClients, _tabProduct });
                    break;
                case 3: // Товаровед
                    tabControl1.TabPages.AddRange(new[] { _tabProduct });
                    break;
            }
        }

        #endregion

        #region ============ ТОВАРЫ ============

        private void ConfigureProductGridView()
        {
            dataGridViewProduct.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewProduct.RowTemplate.Height = THUMBNAIL_SIZE + 10;
        }

        private void LoadProductData()
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string query = @"SELECT 
                        p.product_id as ID,
                        p.name as Название,
                        p.price as Цена,
                        p.stock_quantity as Количество,
                        c.name as Категория,
                        p.photo_path as PhotoPath,
                        p.isActive as Активен,
                        p.category_id as CategoryId
                    FROM product p
                    LEFT JOIN category c ON p.category_id = c.category_id
                    WHERE p.isActive = 1
                    ORDER BY p.name";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Добавляем колонку для изображения
                    dt.Columns.Add("Фото", typeof(Image));

                    foreach (DataRow row in dt.Rows)
                    {
                        string photoPath = row["PhotoPath"]?.ToString();
                        row["Фото"] = LoadThumbnail(photoPath);
                    }

                    _productsData = dt;
                    dataGridViewProduct.DataSource = _productsData;

                    // Скрываем служебные колонки
                    if (dataGridViewProduct.Columns["ID"] != null)
                        dataGridViewProduct.Columns["ID"].Visible = false;
                    if (dataGridViewProduct.Columns["PhotoPath"] != null)
                        dataGridViewProduct.Columns["PhotoPath"].Visible = false;
                    if (dataGridViewProduct.Columns["CategoryId"] != null)
                        dataGridViewProduct.Columns["CategoryId"].Visible = false;
                    if (dataGridViewProduct.Columns["Активен"] != null)
                        dataGridViewProduct.Columns["Активен"].Visible = false;

                    // Настройка отображения колонок
                    if (dataGridViewProduct.Columns["Название"] != null)
                    {
                        dataGridViewProduct.Columns["Название"].Width = 250;
                        dataGridViewProduct.Columns["Название"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                    }
                    if (dataGridViewProduct.Columns["Цена"] != null)
                    {
                        dataGridViewProduct.Columns["Цена"].DefaultCellStyle.Format = "C2";
                        dataGridViewProduct.Columns["Цена"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                    if (dataGridViewProduct.Columns["Количество"] != null)
                    {
                        dataGridViewProduct.Columns["Количество"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                    if (dataGridViewProduct.Columns["Фото"] != null)
                    {
                        dataGridViewProduct.Columns["Фото"].Width = 80;
                    }

                    // Подсветка остатков
                    HighlightLowStock();
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка загрузки товаров: {ex.Message}");
                }
            }
        }

        private void HighlightLowStock()
        {
            foreach (DataGridViewRow row in dataGridViewProduct.Rows)
            {
                if (row.Cells["Количество"].Value != null)
                {
                    int qty = Convert.ToInt32(row.Cells["Количество"].Value);
                    if (qty < 5)
                        row.Cells["Количество"].Style.BackColor = Color.LightPink;
                    else if (qty < 10)
                        row.Cells["Количество"].Style.BackColor = Color.LightYellow;
                }
            }
        }

        private Image LoadThumbnail(string photoPath)
        {
            try
            {
                if (string.IsNullOrEmpty(photoPath))
                    return CreatePlaceholder();

                string fullPath = Path.Combine(_productImageService.GetProductsImagesPath(), photoPath);

                if (!File.Exists(fullPath))
                    return CreatePlaceholder();

                using (FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Image img = Image.FromStream(fs))
                {
                    Bitmap thumb = new Bitmap(THUMBNAIL_SIZE, THUMBNAIL_SIZE, PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(thumb))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.Clear(Color.White);

                        float ratio = Math.Min((float)THUMBNAIL_SIZE / img.Width, (float)THUMBNAIL_SIZE / img.Height);
                        int w = (int)(img.Width * ratio);
                        int h = (int)(img.Height * ratio);
                        int x = (THUMBNAIL_SIZE - w) / 2;
                        int y = (THUMBNAIL_SIZE - h) / 2;

                        g.DrawImage(img, x, y, w, h);
                    }
                    return thumb;
                }
            }
            catch
            {
                return CreatePlaceholder();
            }
        }

        private Image CreatePlaceholder()
        {
            Bitmap bmp = new Bitmap(THUMBNAIL_SIZE, THUMBNAIL_SIZE, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(245, 245, 245));
                using (Pen pen = new Pen(Color.FromArgb(200, 200, 200)))
                {
                    g.DrawRectangle(pen, 1, 1, THUMBNAIL_SIZE - 3, THUMBNAIL_SIZE - 3);
                }
                using (Font f = new Font("Arial", 8, FontStyle.Regular))
                using (Brush b = new SolidBrush(Color.FromArgb(150, 150, 150)))
                {
                    string text = "нет фото";
                    SizeF sz = g.MeasureString(text, f);
                    g.DrawString(text, f, b,
                        (THUMBNAIL_SIZE - sz.Width) / 2,
                        (THUMBNAIL_SIZE - sz.Height) / 2);
                }
            }
            return bmp;
        }

        #endregion

        #region ============ КЛИЕНТЫ ============

        private void LoadClientsData()
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string query = @"SELECT 
                        client_id as ID,
                        last_name as Фамилия,
                        first_name as Имя,
                        email as Email,
                        phone as Телефон,
                        address as Адрес,
                        isActive as Активен
                    FROM client
                    WHERE isActive = 1
                    ORDER BY last_name, first_name";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    _clientsData = new DataTable();
                    adapter.Fill(_clientsData);

                    // Создаем отображаемую таблицу с ФИО
                    DataTable displayDt = new DataTable();
                    displayDt.Columns.Add("ID", typeof(int));
                    displayDt.Columns.Add("ФИО", typeof(string));
                    displayDt.Columns.Add("Email", typeof(string));
                    displayDt.Columns.Add("Телефон", typeof(string));
                    displayDt.Columns.Add("Адрес", typeof(string));

                    foreach (DataRow row in _clientsData.Rows)
                    {
                        string fullName = $"{row["Фамилия"]} {row["Имя"]}";
                        displayDt.Rows.Add(
                            Convert.ToInt32(row["ID"]),
                            fullName,
                            row["Email"]?.ToString() ?? "",
                            row["Телефон"]?.ToString() ?? "",
                            row["Адрес"]?.ToString() ?? ""
                        );
                    }

                    dataGridViewClient.DataSource = displayDt;
                    dataGridViewClient.Columns["ID"].Visible = false;
                    dataGridViewClient.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка загрузки клиентов: {ex.Message}");
                }
            }
        }

        #endregion

        #region ============ ПОЛЬЗОВАТЕЛИ ============

        private void LoadUsersData()
        {
            if (_roleID != 1) return; // Только админ

            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string query = @"SELECT 
                        u.user_id as ID,
                        u.last_name as Фамилия,
                        u.first_name as Имя,
                        u.email as Email,
                        u.username as Логин,
                        r.role_name as Роль,
                        u.isActive as Активен
                    FROM user u
                    INNER JOIN role r ON u.role_id = r.role_id
                    WHERE u.isActive = 1
                    ORDER BY u.last_name, u.first_name";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    _usersData = new DataTable();
                    adapter.Fill(_usersData);

                    // Создаем отображаемую таблицу с ФИО
                    DataTable displayDt = new DataTable();
                    displayDt.Columns.Add("ID", typeof(int));
                    displayDt.Columns.Add("ФИО", typeof(string));
                    displayDt.Columns.Add("Email", typeof(string));
                    displayDt.Columns.Add("Логин", typeof(string));
                    displayDt.Columns.Add("Роль", typeof(string));

                    foreach (DataRow row in _usersData.Rows)
                    {
                        string fullName = $"{row["Фамилия"]} {row["Имя"]}";
                        displayDt.Rows.Add(
                            Convert.ToInt32(row["ID"]),
                            fullName,
                            row["Email"]?.ToString() ?? "",
                            row["Логин"]?.ToString() ?? "",
                            row["Роль"]?.ToString() ?? ""
                        );
                    }

                    dataGridViewUser.DataSource = displayDt;
                    dataGridViewUser.Columns["ID"].Visible = false;
                    dataGridViewUser.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка загрузки пользователей: {ex.Message}");
                }
            }
        }

        #endregion

        #region ============ ПРОВЕРКА ЗАВИСИМОСТЕЙ ============

        private bool HasDependencies(string tableName, int id)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    switch (tableName)
                    {
                        case "product":
                            // Проверяем, есть ли товар в заказах (order_product)
                            string productQuery = "SELECT COUNT(*) FROM order_product WHERE product_id = @id";
                            MySqlCommand productCmd = new MySqlCommand(productQuery, connection);
                            productCmd.Parameters.AddWithValue("@id", id);

                            // Также проверяем историю изменений (inventory_history)
                            string historyQuery = "SELECT COUNT(*) FROM inventory_history WHERE product_id = @id";
                            MySqlCommand historyCmd = new MySqlCommand(historyQuery, connection);
                            historyCmd.Parameters.AddWithValue("@id", id);

                            int orderCount = Convert.ToInt32(productCmd.ExecuteScalar());
                            int historyCount = Convert.ToInt32(historyCmd.ExecuteScalar());

                            return orderCount > 0 || historyCount > 0;

                        case "client":
                            // Проверяем, есть ли у клиента заказы (order)
                            string clientQuery = "SELECT COUNT(*) FROM `order` WHERE client_id = @id";
                            MySqlCommand clientCmd = new MySqlCommand(clientQuery, connection);
                            clientCmd.Parameters.AddWithValue("@id", id);
                            return Convert.ToInt32(clientCmd.ExecuteScalar()) > 0;

                        case "user":
                            // Проверяем, есть ли у пользователя заказы (order)
                            string userQuery = "SELECT COUNT(*) FROM `order` WHERE user_id = @id";
                            MySqlCommand userCmd = new MySqlCommand(userQuery, connection);
                            userCmd.Parameters.AddWithValue("@id", id);

                            // Также проверяем историю изменений (inventory_history)
                            string userHistoryQuery = "SELECT COUNT(*) FROM inventory_history WHERE user_id = @id";
                            MySqlCommand userHistoryCmd = new MySqlCommand(userHistoryQuery, connection);
                            userHistoryCmd.Parameters.AddWithValue("@id", id);

                            int userOrderCount = Convert.ToInt32(userCmd.ExecuteScalar());
                            int userHistoryCount = Convert.ToInt32(userHistoryCmd.ExecuteScalar());

                            return userOrderCount > 0 || userHistoryCount > 0;

                        default:
                            return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка проверки зависимостей: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true; // В случае ошибки считаем, что зависимости есть
                }
            }
        }

        #endregion

        #region ============ КОНТЕКСТНОЕ МЕНЮ ============

        private void SetupContextMenus()
        {
            dataGridViewClient.MouseClick += DataGridView_MouseClick;
            dataGridViewProduct.MouseClick += DataGridView_MouseClick;
            dataGridViewUser.MouseClick += DataGridView_MouseClick;
        }

        private void DataGridView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            DataGridView dgv = sender as DataGridView;
            var hit = dgv.HitTest(e.X, e.Y);
            if (hit.RowIndex < 0) return;

            dgv.ClearSelection();
            dgv.Rows[hit.RowIndex].Selected = true;

            // Проверяем права на редактирование/удаление
            bool canEdit = false;
            bool canDelete = false;

            if (dgv == dataGridViewProduct && (_roleID == 1 || _roleID == 3))
            {
                canEdit = true;
                canDelete = true;
            }
            else if (dgv == dataGridViewClient && (_roleID == 1 || _roleID == 2))
            {
                canEdit = true;
                canDelete = true;
            }
            else if (dgv == dataGridViewUser && _roleID == 1)
            {
                canEdit = true;
                canDelete = true;
            }

            if (!canEdit && !canDelete) return;

            var menu = new ContextMenuStrip();

            if (canEdit)
            {
                var editMenuItem = new ToolStripMenuItem("Редактировать");
                editMenuItem.Click += (s, args) => EditRecord(dgv);
                menu.Items.Add(editMenuItem);
            }

            if (canDelete)
            {
                var deleteMenuItem = new ToolStripMenuItem("Удалить");
                deleteMenuItem.Click += (s, args) => DeleteRecord(dgv);
                menu.Items.Add(deleteMenuItem);
            }

            if (menu.Items.Count > 0)
                menu.Show(dgv, e.Location);
        }

        #endregion

        #region ============ РЕДАКТИРОВАНИЕ ============

        private void EditRecord(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0) return;

            try
            {
                int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);

                if (dgv == dataGridViewProduct)
                {
                    var productModel = _editClass.LoadProductById(id);
                    if (productModel != null)
                    {
                        var editForm = new EditProductForm(productModel);
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            _editClass.UpdateProductInDatabase(editForm.Product);
                            _productsData = null;
                            LoadProductData();
                            ShowInfo("✅ Товар успешно обновлен");
                        }
                    }
                    else
                    {
                        ShowInfo("Не удалось загрузить данные товара");
                    }
                }
                else if (dgv == dataGridViewClient)
                {
                    var clientModel = _editClass.LoadClientById(id);
                    if (clientModel != null)
                    {
                        var editForm = new EditClientForm(clientModel);
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            _editClass.UpdateClientInDatabase(editForm.Client);
                            _clientsData = null;
                            LoadClientsData();
                            ShowInfo("✅ Клиент успешно обновлен");
                        }
                    }
                    else
                    {
                        ShowInfo("Не удалось загрузить данные клиента");
                    }
                }
                else if (dgv == dataGridViewUser && _roleID == 1)
                {
                    var userModel = _editClass.LoadUserById(id);
                    if (userModel != null)
                    {
                        var editForm = new EditUserForm(userModel);
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            _editClass.UpdateUserInDatabase(editForm.User);
                            _usersData = null;
                            LoadUsersData();
                            ShowInfo("✅ Пользователь успешно обновлен");
                        }
                    }
                    else
                    {
                        ShowInfo("Не удалось загрузить данные пользователя");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при редактировании: {ex.Message}");
            }
        }

        #endregion

        #region ============ УДАЛЕНИЕ (SOFT DELETE) ============

        private void DeleteRecord(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0) return;

            string entityType = dgv == dataGridViewProduct ? "товар" :
                               dgv == dataGridViewClient ? "клиента" : "пользователя";

            string entityName = "";
            int id = 0;

            if (dgv == dataGridViewProduct)
            {
                id = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);
                entityName = dgv.SelectedRows[0].Cells["Название"].Value?.ToString() ?? "";
            }
            else if (dgv == dataGridViewClient)
            {
                id = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);
                entityName = dgv.SelectedRows[0].Cells["ФИО"].Value?.ToString() ?? "";
            }
            else if (dgv == dataGridViewUser)
            {
                id = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);
                entityName = dgv.SelectedRows[0].Cells["ФИО"].Value?.ToString() ?? "";

                // Проверка на удаление самого себя
                if (id == _currentUserId)
                {
                    MessageBox.Show(
                        "Вы не можете удалить свой собственный аккаунт!\n\n" +
                        "Для безопасности системы нельзя удалить учётную запись, под которой вы вошли.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Проверка на последнего администратора
                if (IsLastAdmin(id))
                {
                    MessageBox.Show(
                        $"Нельзя удалить пользователя '{entityName}'.\n\n" +
                        "Это последний активный администратор в системе.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
            }

            // Проверка зависимостей
            string tableName = dgv == dataGridViewProduct ? "product" :
                              dgv == dataGridViewClient ? "client" : "user";

            if (HasDependencies(tableName, id))
            {
                string message = dgv == dataGridViewProduct
                    ? $"Невозможно удалить товар '{entityName}'.\n\n" +
                      "Этот товар участвует в продажах. Сначала удалите связанные записи."
                    : dgv == dataGridViewClient
                    ? $"Невозможно удалить клиента '{entityName}'.\n\n" +
                      "У клиента есть история покупок. Сначала удалите связанные записи."
                    : $"Невозможно удалить пользователя '{entityName}'.\n\n" +
                      "У пользователя есть действия в системе. Сначала удалите связанные записи.";

                MessageBox.Show(message, "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка, не удален ли уже объект
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string checkQuery = $"SELECT isActive FROM {tableName} WHERE {tableName}_id = @id";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@id", id);

                    object result = checkCmd.ExecuteScalar();

                    if (result != null)
                    {
                        bool isActive = Convert.ToBoolean(result);

                        if (!isActive)
                        {
                            ShowInfo($"{char.ToUpper(entityType[0]) + entityType.Substring(1)} уже удален");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка проверки статуса: {ex.Message}");
                    return;
                }
            }

            var dialogResult = MessageBox.Show(
                $"Вы точно хотите удалить {entityType} '{entityName}'?\n\n" +
                $"{char.ToUpper(entityType[0]) + entityType.Substring(1)} будет помечен как неактивный, но останется в базе данных.",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (dialogResult == DialogResult.Yes)
            {
                SoftDeleteRecord(tableName, id, entityType, dgv);
            }
        }

        private void SoftDeleteRecord(string tableName, int id, string entityType, DataGridView dgv)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string query = $"UPDATE {tableName} SET isActive = 0 WHERE {tableName}_id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", id);

                    int affected = cmd.ExecuteNonQuery();

                    if (affected > 0)
                    {
                        ShowInfo($"{char.ToUpper(entityType[0]) + entityType.Substring(1)} успешно удален");

                        // Обновляем данные
                        if (dgv == dataGridViewProduct)
                            _productsData = null;
                        else if (dgv == dataGridViewClient)
                            _clientsData = null;
                        else if (dgv == dataGridViewUser)
                            _usersData = null;

                        LoadCurrentTabData();
                    }
                    else
                    {
                        ShowInfo($"{char.ToUpper(entityType[0]) + entityType.Substring(1)} не найден");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool IsLastAdmin(int userId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT COUNT(*) FROM user 
                                   WHERE role_id = 1 AND isActive = 1 AND user_id != @id";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", userId);
                    int otherAdmins = Convert.ToInt32(cmd.ExecuteScalar());
                    return otherAdmins == 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion

        #region ============ ДОБАВЛЕНИЕ ============

        private void AddProduct_Click(object sender, EventArgs e)
        {
            var form = new AddProductForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                _productsData = null;
                LoadProductData();
                ShowInfo("✅ Товар успешно добавлен");
            }
        }

        private void AddClient_Click(object sender, EventArgs e)
        {
            var form = new AddClientForm(this);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _clientsData = null;
                LoadClientsData();
                ShowInfo("✅ Клиент успешно добавлен");
            }
        }

        private void AddUser_Click(object sender, EventArgs e)
        {
            if (_roleID != 1)
            {
                ShowWarning("У вас нет прав для добавления пользователей");
                return;
            }

            var form = new AddUserForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                _usersData = null;
                LoadUsersData();
                ShowInfo("✅ Пользователь успешно добавлен");
            }
        }

        #endregion

        #region ============ ЖИЗНЕННЫЙ ЦИКЛ ФОРМЫ ============

        private void ShowAll_Load(object sender, EventArgs e)
        {
            LoadCurrentTabData();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_productsData != null)
            {
                foreach (DataRow row in _productsData.Rows)
                {
                    if (row["Фото"] is Image img)
                        img.Dispose();
                }
            }
            base.OnFormClosing(e);
        }

        #endregion
    }
}