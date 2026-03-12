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
    /// <summary>
    /// Главная форма для просмотра и управления всеми записями в системе.
    /// Предоставляет функционал для:
    /// - Просмотра списков клиентов, товаров и пользователей
    /// - Редактирования записей
    /// - Мягкого удаления (помечание как неактивные)
    /// - Добавления новых записей
    /// - Отображения изображений товаров
    /// - Разграничения доступа по ролям
    /// </summary>
    public partial class ShowAll : Form
    {
        // ============ ПОЛЯ КЛАССА ============

        /// <summary>
        /// ID роли текущего пользователя (1-админ, 2-продавец, 3-товаровед)
        /// </summary>
        private int _roleID;

        /// <summary>
        /// ФИО текущего пользователя
        /// </summary>
        private string _fio;

        /// <summary>
        /// Логин текущего пользователя
        /// </summary>
        private string _login;

        /// <summary>
        /// ID текущего пользователя в базе данных
        /// </summary>
        private int _currentUserId;

        /// <summary>
        /// Строка подключения к базе данных
        /// </summary>
        private string _connection;

        /// <summary>
        /// Класс для операций редактирования записей
        /// </summary>
        private EditClass _editClass;

        /// <summary>
        /// Сервис для работы с изображениями товаров
        /// </summary>
        private ProductImageService _productImageService;

        // ============ ВКЛАДКИ ============
        private TabPage _tabClients;
        private TabPage _tabProduct;
        private TabPage _tabUsers;

        // ============ ДАННЫЕ ============
        private DataTable _usersData;
        private DataTable _productsData;
        private DataTable _clientsData;

        /// <summary>
        /// Размер миниатюр изображений товаров (в пикселях)
        /// </summary>
        private const int THUMBNAIL_SIZE = 80;

        /// <summary>
        /// Флаг режима редактирования
        /// </summary>
        private bool _isEditing = false;

        /// <summary>
        /// ID редактируемой записи
        /// </summary>
        private int _editingId = 0;

        /// <summary>
        /// Тип редактируемой сущности ("product", "client", "user")
        /// </summary>
        private string _editingEntityType = "";

        // ============ КОНСТРУКТОР ============

        /// <summary>
        /// Конструктор формы просмотра всех записей
        /// </summary>
        /// <param name="FIO">ФИО текущего пользователя</param>
        /// <param name="roleId">ID роли пользователя</param>
        /// <param name="login">Логин пользователя (опционально)</param>
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

            // Настройка вкладок в зависимости от роли
            ConfigureTabsByRole();

            // Подписка на события
            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;

            // Настройка таблиц
            ConfigureAllGrids();

            // Настройка контекстных меню
            SetupContextMenus();

            // Настройка видимости кнопок добавления
            ConfigureAddButtons();
        }

        #region ============ ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ============

        /// <summary>
        /// Создает новое подключение к базе данных
        /// </summary>
        private MySqlConnection GetNewConnection() => new MySqlConnection(_connection);

        /// <summary>
        /// Получает ID текущего пользователя из базы данных
        /// </summary>
        /// <returns>ID пользователя или 1, если не найден</returns>
        private int GetCurrentUserId()
        {
            try
            {
                using (var conn = GetNewConnection())
                {
                    conn.Open();

                    // Пытаемся найти по логину (приоритетный способ)
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

        /// <summary>
        /// Показывает информационное сообщение
        /// </summary>
        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Показывает сообщение об ошибке
        /// </summary>
        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Показывает предупреждение
        /// </summary>
        private void ShowWarning(string message)
        {
            MessageBox.Show(message, "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Настраивает видимость кнопок добавления в зависимости от роли
        /// </summary>
        private void ConfigureAddButtons()
        {
            // По умолчанию все кнопки невидимы
            AddProduct.Visible = false;
            AddClient.Visible = false;
            AddUser.Visible = false;

            switch (_roleID)
            {
                case 1: // Админ - может добавлять всё
                    AddProduct.Visible = true;
                    AddClient.Visible = true;
                    AddUser.Visible = true;
                    break;
                case 2: // Продавец - может добавлять только клиентов
                    AddClient.Visible = true;
                    break;
                case 3: // Товаровед - может добавлять только товары
                    AddProduct.Visible = true;
                    break;
            }
        }

        /// <summary>
        /// Настраивает все таблицы на форме
        /// </summary>
        private void ConfigureAllGrids()
        {
            ConfigureDataGridView(dataGridViewClient);
            ConfigureDataGridView(dataGridViewProduct);
            ConfigureDataGridView(dataGridViewUser);
            ConfigureProductGridView(); // Специальная настройка для товаров
        }

        /// <summary>
        /// Базовая настройка DataGridView
        /// </summary>
        /// <param name="dgv">Таблица для настройки</param>
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

            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(76, 175, 80); // Зеленый
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            // При клике на ячейку выделяем всю строку
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

        /// <summary>
        /// Обработчик смены активной вкладки
        /// </summary>
        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCurrentTabData();
        }

        /// <summary>
        /// Загружает данные для текущей активной вкладки
        /// </summary>
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

        /// <summary>
        /// Возврат в главное меню
        /// </summary>
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

        /// <summary>
        /// Настраивает доступные вкладки в зависимости от роли пользователя
        /// </summary>
        private void ConfigureTabsByRole()
        {
            tabControl1.TabPages.Clear();

            switch (_roleID)
            {
                case 1: // Админ - все вкладки
                    tabControl1.TabPages.AddRange(new[] { _tabClients, _tabProduct, _tabUsers });
                    break;
                case 2: // Продавец - клиенты и товары
                    tabControl1.TabPages.AddRange(new[] { _tabClients, _tabProduct });
                    break;
                case 3: // Товаровед - только товары
                    tabControl1.TabPages.AddRange(new[] { _tabProduct });
                    break;
            }
        }

        #endregion

        #region ============ ТОВАРЫ ============

        /// <summary>
        /// Специальная настройка таблицы товаров
        /// </summary>
        private void ConfigureProductGridView()
        {
            dataGridViewProduct.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewProduct.RowTemplate.Height = THUMBNAIL_SIZE + 10;
        }

        /// <summary>
        /// Загружает данные о товарах из базы данных
        /// </summary>
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
                    HideProductColumns();

                    // Настройка отображения колонок
                    ConfigureProductColumns();

                    // Подсветка остатков
                    HighlightLowStock();
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка загрузки товаров: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Скрывает служебные колонки в таблице товаров
        /// </summary>
        private void HideProductColumns()
        {
            if (dataGridViewProduct.Columns["ID"] != null)
                dataGridViewProduct.Columns["ID"].Visible = false;
            if (dataGridViewProduct.Columns["PhotoPath"] != null)
                dataGridViewProduct.Columns["PhotoPath"].Visible = false;
            if (dataGridViewProduct.Columns["CategoryId"] != null)
                dataGridViewProduct.Columns["CategoryId"].Visible = false;
            if (dataGridViewProduct.Columns["Активен"] != null)
                dataGridViewProduct.Columns["Активен"].Visible = false;
        }

        /// <summary>
        /// Настраивает отображение колонок в таблице товаров
        /// </summary>
        private void ConfigureProductColumns()
        {
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
        }

        /// <summary>
        /// Подсвечивает ячейки с малым количеством товара
        /// Красный - менее 5, желтый - менее 10
        /// </summary>
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

        /// <summary>
        /// Загружает миниатюру изображения товара
        /// </summary>
        /// <param name="photoPath">Путь к файлу изображения</param>
        /// <returns>Изображение-миниатюра или заглушка</returns>
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

        /// <summary>
        /// Создает изображение-заглушку для товаров без фото
        /// </summary>
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

        /// <summary>
        /// Загружает данные о клиентах из базы данных
        /// </summary>
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

                    // Создаем отображаемую таблицу с объединенным ФИО
                    DataTable displayDt = CreateClientDisplayTable();
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

        /// <summary>
        /// Создает таблицу для отображения клиентов с объединенным ФИО
        /// </summary>
        private DataTable CreateClientDisplayTable()
        {
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

            return displayDt;
        }

        #endregion

        #region ============ ПОЛЬЗОВАТЕЛИ ============

        /// <summary>
        /// Загружает данные о пользователях из базы данных (только для админа)
        /// </summary>
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

                    // Создаем отображаемую таблицу с объединенным ФИО
                    DataTable displayDt = CreateUserDisplayTable();
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

        /// <summary>
        /// Создает таблицу для отображения пользователей с объединенным ФИО
        /// </summary>
        private DataTable CreateUserDisplayTable()
        {
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

            return displayDt;
        }

        #endregion

        #region ============ ПРОВЕРКА ЗАВИСИМОСТЕЙ ============

        /// <summary>
        /// Проверяет, есть ли у записи связанные данные в других таблицах
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="id">ID записи</param>
        /// <returns>true, если есть зависимости</returns>
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
                            // Проверяем, есть ли товар в заказах или истории
                            return CheckProductDependencies(connection, id);

                        case "client":
                            // Проверяем, есть ли у клиента заказы
                            return CheckClientDependencies(connection, id);

                        case "user":
                            // Проверяем, есть ли у пользователя заказы или действия в истории
                            return CheckUserDependencies(connection, id);

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

        /// <summary>
        /// Проверяет зависимости товара
        /// </summary>
        private bool CheckProductDependencies(MySqlConnection connection, int productId)
        {
            string productQuery = "SELECT COUNT(*) FROM order_product WHERE product_id = @id";
            MySqlCommand productCmd = new MySqlCommand(productQuery, connection);
            productCmd.Parameters.AddWithValue("@id", productId);
            int orderCount = Convert.ToInt32(productCmd.ExecuteScalar());

            string historyQuery = "SELECT COUNT(*) FROM inventory_history WHERE product_id = @id";
            MySqlCommand historyCmd = new MySqlCommand(historyQuery, connection);
            historyCmd.Parameters.AddWithValue("@id", productId);
            int historyCount = Convert.ToInt32(historyCmd.ExecuteScalar());

            return orderCount > 0 || historyCount > 0;
        }

        /// <summary>
        /// Проверяет зависимости клиента
        /// </summary>
        private bool CheckClientDependencies(MySqlConnection connection, int clientId)
        {
            string clientQuery = "SELECT COUNT(*) FROM `order` WHERE client_id = @id";
            MySqlCommand clientCmd = new MySqlCommand(clientQuery, connection);
            clientCmd.Parameters.AddWithValue("@id", clientId);
            return Convert.ToInt32(clientCmd.ExecuteScalar()) > 0;
        }

        /// <summary>
        /// Проверяет зависимости пользователя
        /// </summary>
        private bool CheckUserDependencies(MySqlConnection connection, int userId)
        {
            string userQuery = "SELECT COUNT(*) FROM `order` WHERE user_id = @id";
            MySqlCommand userCmd = new MySqlCommand(userQuery, connection);
            userCmd.Parameters.AddWithValue("@id", userId);
            int userOrderCount = Convert.ToInt32(userCmd.ExecuteScalar());

            string userHistoryQuery = "SELECT COUNT(*) FROM inventory_history WHERE user_id = @id";
            MySqlCommand userHistoryCmd = new MySqlCommand(userHistoryQuery, connection);
            userHistoryCmd.Parameters.AddWithValue("@id", userId);
            int userHistoryCount = Convert.ToInt32(userHistoryCmd.ExecuteScalar());

            return userOrderCount > 0 || userHistoryCount > 0;
        }

        #endregion

        #region ============ КОНТЕКСТНОЕ МЕНЮ ============

        /// <summary>
        /// Настраивает контекстные меню для таблиц
        /// </summary>
        private void SetupContextMenus()
        {
            dataGridViewClient.MouseClick += DataGridView_MouseClick;
            dataGridViewProduct.MouseClick += DataGridView_MouseClick;
            dataGridViewUser.MouseClick += DataGridView_MouseClick;
        }

        /// <summary>
        /// Обработчик правого клика мыши по таблице
        /// Показывает контекстное меню с действиями
        /// </summary>
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

        /// <summary>
        /// Открывает форму редактирования для выбранной записи
        /// </summary>
        private void EditRecord(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0) return;

            try
            {
                int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);

                if (dgv == dataGridViewProduct)
                {
                    EditProduct(id);
                }
                else if (dgv == dataGridViewClient)
                {
                    EditClient(id);
                }
                else if (dgv == dataGridViewUser && _roleID == 1)
                {
                    EditUser(id);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при редактировании: {ex.Message}");
            }
        }

        /// <summary>
        /// Редактирование товара
        /// </summary>
        private void EditProduct(int productId)
        {
            var productModel = _editClass.LoadProductById(productId);
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

        /// <summary>
        /// Редактирование клиента
        /// </summary>
        private void EditClient(int clientId)
        {
            var clientModel = _editClass.LoadClientById(clientId);
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

        /// <summary>
        /// Редактирование пользователя
        /// </summary>
        private void EditUser(int userId)
        {
            var userModel = _editClass.LoadUserById(userId);
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

        #endregion

        #region ============ УДАЛЕНИЕ (SOFT DELETE) ============

        /// <summary>
        /// Удаляет выбранную запись (soft delete - помечает как неактивную)
        /// </summary>
        private void DeleteRecord(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0) return;

            string entityType = GetEntityType(dgv);
            string entityName = "";
            int id = 0;

            // Получаем данные о записи
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

                // Специальные проверки для пользователей
                if (!ValidateUserDeletion(id, entityName)) return;
            }

            string tableName = GetTableName(dgv);

            // Проверка зависимостей
            if (HasDependencies(tableName, id))
            {
                ShowDependencyError(dgv, entityName);
                return;
            }

            // Проверка, не удален ли уже объект
            if (IsAlreadyDeleted(tableName, id, entityType)) return;

            // Подтверждение удаления
            if (!ConfirmDeletion(entityType, entityName)) return;

            // Выполнение удаления
            SoftDeleteRecord(tableName, id, entityType, dgv);
        }

        /// <summary>
        /// Возвращает тип сущности для отображения в сообщениях
        /// </summary>
        private string GetEntityType(DataGridView dgv)
        {
            return dgv == dataGridViewProduct ? "товар" :
                   dgv == dataGridViewClient ? "клиента" : "пользователя";
        }

        /// <summary>
        /// Возвращает имя таблицы в базе данных
        /// </summary>
        private string GetTableName(DataGridView dgv)
        {
            return dgv == dataGridViewProduct ? "product" :
                   dgv == dataGridViewClient ? "client" : "user";
        }

        /// <summary>
        /// Проверяет возможность удаления пользователя
        /// </summary>
        private bool ValidateUserDeletion(int userId, string userName)
        {
            // Проверка на удаление самого себя
            if (userId == _currentUserId)
            {
                MessageBox.Show(
                    "Вы не можете удалить свой собственный аккаунт!\n\n" +
                    "Для безопасности системы нельзя удалить учётную запись, под которой вы вошли.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            // Проверка на последнего администратора
            if (IsLastAdmin(userId))
            {
                MessageBox.Show(
                    $"Нельзя удалить пользователя '{userName}'.\n\n" +
                    "Это последний активный администратор в системе.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Показывает сообщение об ошибке зависимостей
        /// </summary>
        private void ShowDependencyError(DataGridView dgv, string entityName)
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
        }

        /// <summary>
        /// Проверяет, не была ли запись уже удалена
        /// </summary>
        private bool IsAlreadyDeleted(string tableName, int id, string entityType)
        {
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
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка проверки статуса: {ex.Message}");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Запрашивает подтверждение удаления
        /// </summary>
        private bool ConfirmDeletion(string entityType, string entityName)
        {
            var result = MessageBox.Show(
                $"Вы точно хотите удалить {entityType} '{entityName}'?\n\n" +
                $"{char.ToUpper(entityType[0]) + entityType.Substring(1)} будет помечен как неактивный, но останется в базе данных.",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            return result == DialogResult.Yes;
        }

        /// <summary>
        /// Выполняет мягкое удаление записи (установка isActive = 0)
        /// </summary>
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

                        // Сбрасываем кэш данных
                        ClearDataCache(dgv);
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

        /// <summary>
        /// Очищает кэш данных для соответствующей таблицы
        /// </summary>
        private void ClearDataCache(DataGridView dgv)
        {
            if (dgv == dataGridViewProduct)
                _productsData = null;
            else if (dgv == dataGridViewClient)
                _clientsData = null;
            else if (dgv == dataGridViewUser)
                _usersData = null;
        }

        /// <summary>
        /// Проверяет, является ли пользователь последним активным администратором
        /// </summary>
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

        /// <summary>
        /// Добавление нового товара
        /// </summary>
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

        /// <summary>
        /// Добавление нового клиента
        /// </summary>
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

        /// <summary>
        /// Добавление нового пользователя (только для админа)
        /// </summary>
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

        /// <summary>
        /// Загрузка формы
        /// </summary>
        private void ShowAll_Load(object sender, EventArgs e)
        {
            LoadCurrentTabData();
        }

        /// <summary>
        /// Освобождение ресурсов при закрытии формы
        /// </summary>
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