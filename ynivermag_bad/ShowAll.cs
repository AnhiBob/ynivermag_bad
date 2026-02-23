using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ynivermag_bad
{
    public partial class ShowAll : Form
    {
        private int _roleID;
        private string _fio;

        private TabPage _tabClients;
        private TabPage _tabProduct;
        private TabPage _tabUsers;
        private string _connection;

        // Для хранения оригинальных DataTable
        private DataTable _usersData;
        private DataTable _productsData;
        private DataTable _clientsData;
        private EditClass _editClass;
        private ProductImageService _productImageService;

        public ShowAll(string FIO, int roleId)
        {
            InitializeComponent();
            _roleID = roleId;
            _fio = FIO;
            FIOlb.Text = _fio;
            _tabClients = tabPage1;
            _tabProduct = tabPage2;
            _tabUsers = tabPage3;
            _connection = Connection.ConnectionString;
            ConfigureTabsByRole();

            _editClass = new EditClass();
            _productImageService = new ProductImageService(); // ДОБАВИТЬ

            // Настраиваем обработчики событий
            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;
            dataGridViewProduct.Resize += dataGridViewProduct_Resize;
            dataGridViewProduct.CellFormatting += dataGridViewProduct_CellFormatting;
        }

        private MySqlConnection GetNewConnection()
        {
            return new MySqlConnection(_connection);
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Загружаем данные при переключении вкладок
            LoadCurrentTabData();
        }

        private void LoadCurrentTabData()
        {
            if (tabControl1.SelectedTab == null)
                return;

            string tabName = tabControl1.SelectedTab.Name;

            switch (tabName)
            {
                case "tabPage1": // Клиенты
                    if (_clientsData == null || dataGridViewClient.DataSource == null)
                        LoadClientsData();
                    break;
                case "tabPage2": // Товары
                    if (_productsData == null || dataGridViewProduct.DataSource == null)
                        LoadProductData();
                    break;
                case "tabPage3": // Пользователи
                    if (_usersData == null || dataGridViewUser.DataSource == null)
                        LoadUsersData();
                    break;
            }
        }

        private void InMenuClient_Click(object sender, EventArgs e)
        {
            if (_roleID == 1)
            {
                MenuAdminForm admin = new MenuAdminForm(_fio);
                admin.Show();
                this.Hide();
            }
            else if (_roleID == 2)
            {
                MenuSellerForm seller = new MenuSellerForm(_fio);
                seller.Show();
                this.Hide();
            }
            else if (_roleID == 3)
            {
                MenuTovarovedForm menu = new MenuTovarovedForm(_fio);
                menu.Show();
                this.Hide();
            }
        }

        private void LoadProductData()
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                p.product_id as 'ID',
                p.name as 'Название',
                p.price as 'Цена',
                p.stock_quantity as 'Количество на складе',
                p.category_id as 'CategoryID',
                p.photo_path as 'Фото',
                c.name as 'Категория'
            FROM product p
            LEFT JOIN category c ON p.category_id = c.category_id
            ORDER BY p.name";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Создаем DataTable для отображения
                    DataTable displayDt = new DataTable();
                    displayDt.Columns.Add("ID", typeof(int));
                    displayDt.Columns.Add("Название", typeof(string));
                    displayDt.Columns.Add("Цена", typeof(decimal));
                    displayDt.Columns.Add("Количество", typeof(int));
                    displayDt.Columns.Add("Категория", typeof(string));
                    displayDt.Columns.Add("Фото", typeof(Image));
                    displayDt.Columns.Add("Имя файла", typeof(string));

                    // Фиксированный размер для миниатюр
                    int thumbnailSize = 60;

                    foreach (DataRow row in dt.Rows)
                    {
                        string photoFileName = row["Фото"] != DBNull.Value ? row["Фото"].ToString() : null;

                        // Получаем миниатюру
                        Image thumbnail;
                        if (!string.IsNullOrEmpty(photoFileName))
                        {
                            string imagePath = Path.Combine(_productImageService.GetProductsImagesPath(), photoFileName);
                            if (File.Exists(imagePath))
                            {
                                using (var img = Image.FromFile(imagePath))
                                {
                                    thumbnail = _productImageService.ScaleImageToFit(img, thumbnailSize, thumbnailSize);
                                }
                            }
                            else
                            {
                                thumbnail = _productImageService.ScaleImageToFit(
                                    _productImageService.LoadDefaultProductImage(), thumbnailSize, thumbnailSize);
                            }
                        }
                        else
                        {
                            thumbnail = _productImageService.ScaleImageToFit(
                                _productImageService.LoadDefaultProductImage(), thumbnailSize, thumbnailSize);
                        }

                        // ПРАВИЛЬНЫЙ ПАРСИНГ ЦЕНЫ
                        decimal price = 0;
                        object priceValue = row["Цена"];

                        if (priceValue != null && priceValue != DBNull.Value)
                        {
                            try
                            {
                                // Если это уже decimal
                                if (priceValue is decimal)
                                {
                                    price = (decimal)priceValue;
                                }
                                // Если это double или float
                                else if (priceValue is double || priceValue is float)
                                {
                                    price = Convert.ToDecimal(priceValue);
                                }
                                // Если это int
                                else if (priceValue is int)
                                {
                                    price = (int)priceValue;
                                }
                                // Если это строка
                                else
                                {
                                    string priceStr = priceValue.ToString().Trim();

                                    // Заменяем запятую на точку для парсинга
                                    priceStr = priceStr.Replace(',', '.');

                                    // Убираем все пробелы
                                    priceStr = priceStr.Replace(" ", "");

                                    // Парсим с инвариантной культурой (где разделитель - точка)
                                    if (decimal.TryParse(priceStr,
                                        System.Globalization.NumberStyles.Any,
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        out decimal parsedPrice))
                                    {
                                        price = parsedPrice;
                                    }
                                    else
                                    {
                                        // Если не получилось, пробуем с текущей культурой
                                        decimal.TryParse(priceStr, out price);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Ошибка парсинга цены для товара {row["Название"]}: {ex.Message}");
                                price = 0;
                            }
                        }


                        displayDt.Rows.Add(
                            Convert.ToInt32(row["ID"]),
                            row["Название"].ToString(),
                            price,  // Используем правильно распарсенную цену
                            Convert.ToInt32(row["Количество на складе"]),
                            row["Категория"] != DBNull.Value ? row["Категория"].ToString() : "Без категории",
                            thumbnail,
                            photoFileName
                        );
                    }

                    // Сохраняем выделение если есть
                    int selectedIndex = -1;
                    if (dataGridViewProduct.SelectedRows.Count > 0)
                    {
                        selectedIndex = dataGridViewProduct.SelectedRows[0].Index;
                    }

                    // Очищаем колонки
                    dataGridViewProduct.Columns.Clear();

                    // Настраиваем базовые свойства
                    dataGridViewProduct.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dataGridViewProduct.MultiSelect = false;
                    dataGridViewProduct.RowHeadersVisible = false;
                    dataGridViewProduct.ReadOnly = true;
                    dataGridViewProduct.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridViewProduct.AllowUserToResizeRows = false;

                    // Устанавливаем источник данных
                    dataGridViewProduct.DataSource = displayDt;

                    // Настраиваем видимость колонок
                    dataGridViewProduct.Columns["ID"].Visible = false;
                    dataGridViewProduct.Columns["Имя файла"].Visible = false;

                    // Настройка колонки с изображением
                    if (dataGridViewProduct.Columns["Фото"] is DataGridViewImageColumn imageColumn)
                    {
                        imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
                        imageColumn.Width = thumbnailSize + 10;
                        imageColumn.HeaderText = "Фото";
                    }

                    // Настройка остальных колонок
                    dataGridViewProduct.Columns["Название"].Width = 200;

                    // ВАЖНО: правильное форматирование цены
                    dataGridViewProduct.Columns["Цена"].DefaultCellStyle.Format = "N2"; // Вместо C2, чтобы видеть число
                    dataGridViewProduct.Columns["Цена"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                    dataGridViewProduct.Columns["Количество"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dataGridViewProduct.Columns["Категория"].Width = 150;

                    // Устанавливаем высоту ДЛЯ КАЖДОЙ СТРОКИ
                    foreach (DataGridViewRow row in dataGridViewProduct.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            row.Height = thumbnailSize + 8;
                        }
                    }

                    // Также устанавливаем RowTemplate для новых строк
                    dataGridViewProduct.RowTemplate.Height = thumbnailSize + 8;
                    dataGridViewProduct.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                    // Восстанавливаем выделение
                    if (selectedIndex >= 0 && selectedIndex < dataGridViewProduct.Rows.Count)
                    {
                        dataGridViewProduct.Rows[selectedIndex].Selected = true;
                    }

                    // Подсветка низкого количества
                    HighlightLowStock();

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridViewProduct_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Если это колонка с изображением и значение null
            if (dataGridViewProduct.Columns[e.ColumnIndex].Name == "Фото" && e.Value == null)
            {
                // Создаем заглушку через сервис
                int size = dataGridViewProduct.RowTemplate.Height - 8;
                e.Value = _productImageService.ScaleImageToFit(
                    _productImageService.LoadDefaultProductImage(), size, size);
                e.FormattingApplied = true;
            }
        }

        private void HighlightLowStock()
        {
            foreach (DataGridViewRow row in dataGridViewProduct.Rows)
            {
                if (row.Cells["Количество"].Value != null)
                {
                    int quantity = Convert.ToInt32(row.Cells["Количество"].Value);
                    if (quantity < 10)
                    {
                        row.Cells["Количество"].Style.BackColor = Color.LightPink;
                        row.Cells["Количество"].Style.ForeColor = Color.DarkRed;
                        row.Cells["Количество"].Style.Font = new Font(dataGridViewProduct.Font, FontStyle.Bold);
                    }
                    else if (quantity < 20)
                    {
                        row.Cells["Количество"].Style.BackColor = Color.LightYellow;
                        row.Cells["Количество"].Style.ForeColor = Color.DarkOrange;
                        row.Cells["Количество"].Style.Font = new Font(dataGridViewProduct.Font, FontStyle.Bold);
                    }
                }
            }
        }
        public Size CalculateOptimalThumbnailSize(DataGridView dgv, int defaultHeight)
        {
            // Всегда возвращаем фиксированный размер
            return new Size(60, 60);
        }
        private void dataGridViewProduct_Resize(object sender, EventArgs e)
        {
            // При изменении размера обновляем высоту всех строк
            if (dataGridViewProduct.Rows.Count > 0)
            {
                // Фиксированный размер - 60px
                int thumbnailSize = 60;

                // Обновляем ширину колонки с фото
                if (dataGridViewProduct.Columns.Contains("Фото"))
                {
                    dataGridViewProduct.Columns["Фото"].Width = thumbnailSize + 10;
                }

                // Обновляем высоту каждой строки
                foreach (DataGridViewRow row in dataGridViewProduct.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        row.Height = thumbnailSize + 8;
                    }
                }
            }
        }

        private void LoadClientsData()
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                client_id as 'ID',
                email as 'Email',
                first_name as 'Имя',
                last_name as 'Фамилия',
                phone as 'Телефон',
                address as 'Адрес'
            FROM client
            ORDER BY last_name, first_name";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    _clientsData = new DataTable();
                    adapter.Fill(_clientsData);

                    // Копируем и форматируем данные
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
                            row["Email"].ToString(),
                            row["Телефон"].ToString(),
                            row["Адрес"].ToString()
                        );
                    }

                    dataGridViewClient.DataSource = displayDt;
                    ConfigureDataGridView(dataGridViewClient);
                    dataGridViewClient.Columns["ID"].Visible = false;

                    // Автоподбор ширины
                    dataGridViewClient.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ConfigureTabsByRole()
        {
            tabControl1.TabPages.Clear();

            // В зависимости от роли показываем разные вкладки
            switch (_roleID)
            {
                case 1: // Администратор - все вкладки
                    tabControl1.TabPages.AddRange(new TabPage[]
                    {
                        _tabClients,
                        _tabProduct,
                        _tabUsers
                    });
                    break;
                case 2: // Продавец - только клиенты и товары
                    tabControl1.TabPages.AddRange(new TabPage[]
                    {
                        _tabClients,
                        _tabProduct
                    });
                    break;
                case 3: // Товаровед - только товары
                    tabControl1.TabPages.AddRange(new TabPage[]
                    {
                        _tabProduct
                    });
                    break;
            }
        }

        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadUsersData()
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                u.user_id as 'ID',
                u.last_name as 'Фамилия',
                u.first_name as 'Имя',
                u.email as 'Email',
                u.username as 'Логин',
                u.role_id as 'RoleID',
                r.role_name as 'Роль'
            FROM user u
            INNER JOIN role r ON u.role_id = r.role_id
            ORDER BY u.last_name, u.first_name";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    _usersData = new DataTable();
                    adapter.Fill(_usersData);

                    // Копируем и форматируем данные
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
                            row["Email"].ToString(),
                            row["Логин"].ToString(),
                            row["Роль"].ToString()
                        );
                    }

                    dataGridViewUser.DataSource = displayDt;
                    ConfigureDataGridView(dataGridViewUser);
                    dataGridViewUser.Columns["ID"].Visible = false;

                    // Автоподбор ширины
                    dataGridViewUser.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowAll_Load(object sender, EventArgs e)
        {
            // Загружаем данные для активной вкладки при запуске
            LoadCurrentTabData();
        }

        private void ConfigureDataGridView(DataGridView dgv)
        {
            if (dgv == null) return;

            // Базовые настройки
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.RowHeadersVisible = false;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Стиль выделения
            dgv.DefaultCellStyle.SelectionBackColor = Color.YellowGreen;
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Альтернативные строки
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;

            // Шрифт заголовков
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Включаем прокрутку
            dgv.ScrollBars = ScrollBars.Both;

            // Отключаем автоматическую смену размера строк
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        }

        // Дополнительные методы для обновления данных
        public void RefreshData()
        {
            // Сбрасываем кэш данных
            _usersData = null;
            _productsData = null;
            _clientsData = null;

            // Перезагружаем текущую вкладку
            LoadCurrentTabData();
        }

        private void AddProduct_Click(object sender, EventArgs e)
        {
            AddProductForm addProductForm = new AddProductForm();

            DialogResult result = addProductForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                LoadProductData();
                MessageBox.Show("Товар успешно добавлен", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddClient_Click(object sender, EventArgs e)
        {
            AddClientForm addClientForm = new AddClientForm();

            DialogResult result = addClientForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                LoadClientsData();
                MessageBox.Show("Клиент успешно добавлен", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddUser_Click(object sender, EventArgs e)
        {
            AddUserForm addUserForm = new AddUserForm();

            DialogResult result = addUserForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                LoadUsersData();
                MessageBox.Show("Пользователь успешно добавлен", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //******************************************************************************
        private void dataGridViewClient_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridViewClient.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0 && hitTest.RowIndex < dataGridViewClient.Rows.Count)
                {
                    dataGridViewClient.ClearSelection();
                    dataGridViewClient.Rows[hitTest.RowIndex].Selected = true;

                    var contextMenu = new ContextMenuStrip();

                    var editMenuItem = new ToolStripMenuItem("Редактировать");
                    editMenuItem.Click += (s, args) => EditSelectedClient();

                    var deleteMenuItem = new ToolStripMenuItem("Удалить");
                    deleteMenuItem.Click += (s, args) => DeleteSelectedClient();

                    contextMenu.Items.Add(editMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);

                    contextMenu.Show(dataGridViewClient, e.Location);
                }
            }
        }

        private void EditSelectedClient()
        {
            if (dataGridViewClient.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите клиента для редактирования");
                return;
            }

            var selectedRow = dataGridViewClient.SelectedRows[0];
            OpenEditFormClient(selectedRow);
        }

        private void OpenEditFormClient(DataGridViewRow row)
        {
            try
            {
                // Получаем ID клиента (предполагается, что у вас есть скрытая колонка с ID)
                int clientId = Convert.ToInt32(row.Cells["ID"].Value);

                // Загружаем полные данные клиента из базы по ID
                var clientModel = _editClass.LoadClientById(clientId);

                if (clientModel != null)
                {
                    // Открываем форму редактирования
                    var editForm = new EditClientForm(clientModel);

                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        // Обновляем данные в базе
                        _editClass.UpdateClientInDatabase(editForm.Client);

                        // Перезагружаем данные
                        LoadClientsData();

                        ShowInfo("Клиент успешно обновлен");
                    }
                }
                else
                {
                    ShowInfo("Не удалось загрузить данные клиента");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии формы редактирования: {ex.Message}");
            }
        }

        private void DeleteSelectedClient()
        {
            if (dataGridViewClient.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите клиента для удаления");
                return;
            }

            var selectedRow = dataGridViewClient.SelectedRows[0];
            string clientFullName = selectedRow.Cells["ФИО"].Value?.ToString();

            var result = MessageBox.Show(
                $"Вы точно хотите удалить клиента '{clientFullName}'?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                int clientId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
                DeleteClientFromDatabase(clientId);
            }
        }

        private void DeleteClientFromDatabase(int clientId)
        {
            using (var connection = new MySqlConnection(Connection.ConnectionString))
            {
                try
                {
                    connection.Open();

                    // Проверяем, есть ли заказы у этого клиента
                    string checkOrdersQuery = "SELECT COUNT(*) FROM `order` WHERE client_id = @ClientId";
                    MySqlCommand checkCmd = new MySqlCommand(checkOrdersQuery, connection);
                    checkCmd.Parameters.AddWithValue("@ClientId", clientId);
                    int orderCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (orderCount > 0)
                    {
                        // Получаем имя клиента для сообщения
                        string clientNameQuery = "SELECT first_name, last_name FROM client WHERE client_id = @ClientId";
                        MySqlCommand nameCmd = new MySqlCommand(clientNameQuery, connection);
                        nameCmd.Parameters.AddWithValue("@ClientId", clientId);

                        string clientName = "";
                        using (var reader = nameCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string firstName = reader["first_name"]?.ToString() ?? "";
                                string lastName = reader["last_name"]?.ToString() ?? "";
                                clientName = $"{lastName} {firstName}".Trim();
                            }
                        }

                        var confirmResult = MessageBox.Show(
                            $"У клиента '{clientName}' есть {orderCount} заказ(ов).\n\n" +
                            "Удалить клиента и все его заказы?",
                            "Предупреждение",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );

                        if (confirmResult == DialogResult.No)
                        {
                            connection.Close();
                            return;
                        }

                        // Удаляем записи из order_product для заказов клиента
                        string deleteOrderProductsQuery = @"DELETE op FROM order_product op 
                                                   INNER JOIN `order` o ON op.order_id = o.order_id 
                                                   WHERE o.client_id = @ClientId";
                        MySqlCommand deleteOrderProductsCmd = new MySqlCommand(deleteOrderProductsQuery, connection);
                        deleteOrderProductsCmd.Parameters.AddWithValue("@ClientId", clientId);
                        deleteOrderProductsCmd.ExecuteNonQuery();

                        // Удаляем заказы клиента
                        string deleteOrdersQuery = "DELETE FROM `order` WHERE client_id = @ClientId";
                        MySqlCommand deleteOrdersCmd = new MySqlCommand(deleteOrdersQuery, connection);
                        deleteOrdersCmd.Parameters.AddWithValue("@ClientId", clientId);
                        deleteOrdersCmd.ExecuteNonQuery();
                    }

                    // Удаляем клиента
                    string query = "DELETE FROM client WHERE client_id = @ClientId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ClientId", clientId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Клиент успешно удален");
                        LoadClientsData(); // Перезагружаем данные
                    }
                    else
                    {
                        ShowInfo("Клиент не найден");
                    }

                    connection.Close();
                }
                catch (MySqlException ex)
                {
                    // Обработка ошибок внешнего ключа
                    if (ex.Number == 1451) // Ошибка внешнего ключа (нельзя удалить из-за зависимостей)
                    {
                        MessageBox.Show("Нельзя удалить клиента, так как есть связанные заказы.\n" +
                                      "Пожалуйста, сначала удалите все заказы клиента.",
                                      "Ошибка удаления",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка удаления клиента: {ex.Message}",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления клиента: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }
            //******************************************************************************
            //******************************************************************************


            private void dataGridViewProduct_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridViewProduct.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0 && hitTest.RowIndex < dataGridViewProduct.Rows.Count)
                {
                    dataGridViewProduct.ClearSelection();
                    dataGridViewProduct.Rows[hitTest.RowIndex].Selected = true;

                    var contextMenu = new ContextMenuStrip();

                    var editMenuItem = new ToolStripMenuItem("Редактировать");
                    editMenuItem.Click += (s, args) => EditSelectedProduct();

                    var deleteMenuItem = new ToolStripMenuItem("Удалить");
                    deleteMenuItem.Click += (s, args) => DeleteSelectedProduct();

                    contextMenu.Items.Add(editMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);

                    contextMenu.Show(dataGridViewProduct, e.Location);
                }
            }
        }

        private void EditSelectedProduct()
        {
            if (dataGridViewProduct.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите продукт для редактирования");
                return;
            }

            var selectedRow = dataGridViewProduct.SelectedRows[0];
            OpenEditFormProduct(selectedRow);
        }

        private void OpenEditFormProduct(DataGridViewRow row)
        {
            try
            {
                // Получаем ID продукта
                int productId = Convert.ToInt32(row.Cells["ID"].Value);

                // Получаем имя файла фото
                string photoFileName = null;
                if (row.Cells["Имя файла"].Value != null)
                {
                    photoFileName = row.Cells["Имя файла"].Value.ToString();
                }

                // Загружаем полные данные продукта из базы по ID
                var productModel = _editClass.LoadProductById(productId);

                if (productModel != null)
                {
                    // Загружаем изображение продукта
                    if (!string.IsNullOrEmpty(photoFileName))
                    {
                        try
                        {
                            string imagesPath = _productImageService.GetProductsImagesPath();
                            string imagePath = Path.Combine(imagesPath, photoFileName);

                            if (File.Exists(imagePath))
                            {
                                productModel.ProductImage = _productImageService.LoadImageFromFile(imagePath);
                            }
                            else
                            {
                                productModel.ProductImage = _productImageService.LoadDefaultProductImage();
                            }
                        }
                        catch (Exception ex)
                        {
                            productModel.ProductImage = _productImageService.LoadDefaultProductImage();
                        }
                    }
                    else
                    {
                        productModel.ProductImage = _productImageService.LoadDefaultProductImage();
                    }

                    productModel.photo_path = photoFileName;

                    // Открываем форму редактирования
                    var editForm = new EditProductForm(productModel);

                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        // Обновляем данные в базе
                        _editClass.UpdateProductInDatabase(editForm.Product);

                        // Перезагружаем данные
                        LoadProductData();

                        ShowInfo("Продукт успешно обновлен");
                    }
                }
                else
                {
                    ShowInfo("Не удалось загрузить данные продукта");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии формы редактирования: {ex.Message}");
            }
        }

        private void DeleteSelectedProduct()
        {
            if (dataGridViewProduct.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите продукт для удаления");
                return;
            }

            var selectedRow = dataGridViewProduct.SelectedRows[0];
            string productName = selectedRow.Cells["Название"].Value?.ToString();

            var result = MessageBox.Show(
                $"Вы точно хотите удалить продукт '{productName}'?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                int productId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
                DeleteProductFromDatabase(productId);
            }
        }

        private void DeleteProductFromDatabase(int productId)
        {
            using (var connection = new MySqlConnection(Connection.ConnectionString))
            {
                try
                {
                    connection.Open();

                    // Получаем информацию о продукте перед удалением
                    string productInfoQuery = "SELECT name, photo_path FROM product WHERE product_id = @ProductId";
                    MySqlCommand infoCmd = new MySqlCommand(productInfoQuery, connection);
                    infoCmd.Parameters.AddWithValue("@ProductId", productId);

                    string productName = "";
                    string photoPath = "";

                    using (var reader = infoCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            productName = reader["name"]?.ToString() ?? "";
                            photoPath = reader["photo_path"] != DBNull.Value ? reader["photo_path"].ToString() : null;
                        }
                    }

                    // Проверяем, есть ли заказы с этим продуктом
                    string checkOrdersQuery = "SELECT COUNT(*) FROM order_product WHERE product_id = @ProductId";
                    MySqlCommand checkCmd = new MySqlCommand(checkOrdersQuery, connection);
                    checkCmd.Parameters.AddWithValue("@ProductId", productId);
                    int orderCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (orderCount > 0)
                    {
                        var confirmResult = MessageBox.Show(
                            $"Продукт '{productName}' используется в {orderCount} заказ(ах).\n\n" +
                            "Удалить продукт и все связанные записи?",
                            "Предупреждение",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );

                        if (confirmResult == DialogResult.No)
                        {
                            connection.Close();
                            return;
                        }

                        // Удаляем записи из order_product для этого продукта
                        string deleteOrderProductsQuery = "DELETE FROM order_product WHERE product_id = @ProductId";
                        MySqlCommand deleteOrderProductsCmd = new MySqlCommand(deleteOrderProductsQuery, connection);
                        deleteOrderProductsCmd.Parameters.AddWithValue("@ProductId", productId);
                        deleteOrderProductsCmd.ExecuteNonQuery();
                    }

                    // Удаляем файл изображения, если он существует и не является заглушкой
                    if (!string.IsNullOrEmpty(photoPath))
                    {
                        string fullPath = Path.Combine(_productImageService.GetProductsImagesPath(), photoPath);
                        if (File.Exists(fullPath) && !fullPath.Contains("Default.jpg"))
                        {
                            File.Delete(fullPath);
                        }
                    }

                    // Удаляем продукт
                    string query = "DELETE FROM product WHERE product_id = @ProductId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ProductId", productId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Продукт успешно удален");
                        LoadProductData(); // Перезагружаем данные
                    }
                    else
                    {
                        ShowInfo("Продукт не найден");
                    }

                    connection.Close();
                }
                catch (MySqlException ex)
                {
                    // Обработка ошибок внешнего ключа
                    if (ex.Number == 1451)
                    {
                        MessageBox.Show("Нельзя удалить продукт, так как он используется в заказах.\n" +
                                      "Пожалуйста, сначала удалите все заказы с этим продуктом.",
                                      "Ошибка удаления",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка удаления продукта: {ex.Message}",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления продукта: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridViewUser_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridViewUser.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0 && hitTest.RowIndex < dataGridViewUser.Rows.Count)
                {
                    dataGridViewUser.ClearSelection();
                    dataGridViewUser.Rows[hitTest.RowIndex].Selected = true;

                    var contextMenu = new ContextMenuStrip();

                    var editMenuItem = new ToolStripMenuItem("Редактировать");
                    editMenuItem.Click += (s, args) => EditSelectedUser();

                    var deleteMenuItem = new ToolStripMenuItem("Удалить");
                    deleteMenuItem.Click += (s, args) => DeleteSelectedUser();

                    contextMenu.Items.Add(editMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);

                    contextMenu.Show(dataGridViewUser, e.Location);
                }
            }
        }

        private void EditSelectedUser()
        {
            if (dataGridViewUser.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите пользователя для редактирования");
                return;
            }

            var selectedRow = dataGridViewUser.SelectedRows[0];
            OpenEditFormUser(selectedRow);
        }

        private void OpenEditFormUser(DataGridViewRow row)
        {
            try
            {
                // Получаем ID пользователя
                int userId = Convert.ToInt32(row.Cells["ID"].Value);

                // Загружаем полные данные пользователя из базы по ID
                var userModel = _editClass.LoadUserById(userId);

                if (userModel != null)
                {
                    // Проверьте, что userModel не null и содержит данные
                    Console.WriteLine($"userModel loaded: ID={userModel.user_id}, Name={userModel.first_name} {userModel.last_name}");

                    // Открываем форму редактирования
                    var editForm = new EditUserForm(userModel); // userModel не должен быть null!

                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        // Обновляем данные в базе
                        _editClass.UpdateUserInDatabase(editForm.User);

                        // Перезагружаем данные
                        LoadUsersData();

                        ShowInfo("Пользователь успешно обновлен");
                    }
                }
                else
                {
                    ShowInfo("Не удалось загрузить данные пользователя");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии формы редактирования: {ex.Message}\n\nStackTrace: {ex.StackTrace}");
            }
        }

        private void DeleteSelectedUser()
        {
            if (dataGridViewUser.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите пользователя для удаления");
                return;
            }

            var selectedRow = dataGridViewUser.SelectedRows[0];
            string userName = selectedRow.Cells["ФИО"].Value?.ToString();

            var result = MessageBox.Show(
                $"Вы точно хотите удалить пользователя '{userName}'?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                int userId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
                DeleteUserFromDatabase(userId);
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Освобождаем ресурсы изображений в DataGridView
            foreach (DataGridViewRow row in dataGridViewProduct.Rows)
            {
                if (row.Cells["Фото"].Value is Image img)
                {
                    img.Dispose();
                }
            }

            base.OnFormClosing(e);
        }

        private void DeleteUserFromDatabase(int userId)
        {
            using (var connection = new MySqlConnection(Connection.ConnectionString))
            {
                try
                {
                    connection.Open();

                    // Проверяем, есть ли заказы у этого пользователя
                    string checkOrdersQuery = "SELECT COUNT(*) FROM `order` WHERE user_id = @UserId";
                    MySqlCommand checkCmd = new MySqlCommand(checkOrdersQuery, connection);
                    checkCmd.Parameters.AddWithValue("@UserId", userId);
                    int orderCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (orderCount > 0)
                    {
                        // Получаем имя пользователя для сообщения
                        string userNameQuery = "SELECT first_name, last_name FROM user WHERE user_id = @UserId";
                        MySqlCommand nameCmd = new MySqlCommand(userNameQuery, connection);
                        nameCmd.Parameters.AddWithValue("@UserId", userId);

                        string userName = "";
                        using (var reader = nameCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string firstName = reader["first_name"]?.ToString() ?? "";
                                string lastName = reader["last_name"]?.ToString() ?? "";
                                userName = $"{lastName} {firstName}".Trim();
                            }
                        }

                        var confirmResult = MessageBox.Show(
                            $"Пользователь '{userName}' создал {orderCount} заказ(ов).\n\n" +
                            "Удалить пользователя и все его заказы?",
                            "Предупреждение",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );

                        if (confirmResult == DialogResult.No)
                        {
                            connection.Close();
                            return;
                        }

                        // Удаляем записи из order_product для заказов пользователя
                        string deleteOrderProductsQuery = @"DELETE op FROM order_product op 
                                           INNER JOIN `order` o ON op.order_id = o.order_id 
                                           WHERE o.user_id = @UserId";
                        MySqlCommand deleteOrderProductsCmd = new MySqlCommand(deleteOrderProductsQuery, connection);
                        deleteOrderProductsCmd.Parameters.AddWithValue("@UserId", userId);
                        deleteOrderProductsCmd.ExecuteNonQuery();

                        // Удаляем заказы пользователя
                        string deleteOrdersQuery = "DELETE FROM `order` WHERE user_id = @UserId";
                        MySqlCommand deleteOrdersCmd = new MySqlCommand(deleteOrdersQuery, connection);
                        deleteOrdersCmd.Parameters.AddWithValue("@UserId", userId);
                        deleteOrdersCmd.ExecuteNonQuery();
                    }

                    // Удаляем пользователя
                    string query = "DELETE FROM user WHERE user_id = @UserId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Пользователь успешно удален");
                        LoadUsersData(); // Перезагружаем данные
                    }
                    else
                    {
                        ShowInfo("Пользователь не найден");
                    }

                    connection.Close();
                }
                catch (MySqlException ex)
                {
                    // Обработка ошибок внешнего ключа
                    if (ex.Number == 1451) // Ошибка внешнего ключа (нельзя удалить из-за зависимостей)
                    {
                        MessageBox.Show("Нельзя удалить пользователя, так как есть связанные заказы.\n" +
                                      "Пожалуйста, сначала удалите все заказы пользователя.",
                                      "Ошибка удаления",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }
    }
    
}