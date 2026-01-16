using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ynivermag_bad
{
    public partial class ShowAll : Form
    {
        private int _roleID;
        private string _FIO;

        private TabPage _tabClients;
        private TabPage _tabProduct;
        private TabPage _tabUsers;
        private string _connection;

        // Для хранения оригинальных DataTable
        private DataTable _usersData;
        private DataTable _productsData;
        private DataTable _clientsData;

        public ShowAll(string FIO, int roleId)
        {
            InitializeComponent();
            _roleID = roleId;
            _FIO = FIO;
            _tabClients = tabPage1;
            _tabProduct = tabPage2;
            _tabUsers = tabPage3;
            _connection = Connection.ConnectionString;
            ConfigureTabsByRole();

            // Настраиваем обработчики событий
            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;
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
                    if (_usersData == null || dataGridView1.DataSource == null)
                        LoadUsersData();
                    break;
            }
        }

        private void InMenuClient_Click(object sender, EventArgs e)
        {
            if (_roleID == 1)
            {
                MenuAdminForm admin = new MenuAdminForm(_FIO);
                admin.Show();
                this.Hide();
            }
            else if (_roleID == 2)
            {
                MenuSellerForm seller = new MenuSellerForm(_FIO);
                seller.Show();
                this.Hide();
            }
            else if (_roleID == 3)
            {
                MenuTovarovedForm menu = new MenuTovarovedForm(_FIO);
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
                c.name as 'Категория'
            FROM product p
            LEFT JOIN category c ON p.category_id = c.category_id
            ORDER BY p.name";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    _productsData = new DataTable();
                    adapter.Fill(_productsData);

                    // Копируем данные для отображения
                    DataTable displayDt = new DataTable();
                    displayDt.Columns.Add("ID", typeof(int));
                    displayDt.Columns.Add("Название", typeof(string));
                    displayDt.Columns.Add("Цена", typeof(decimal));
                    displayDt.Columns.Add("Количество", typeof(int));
                    displayDt.Columns.Add("Категория", typeof(string));

                    foreach (DataRow row in _productsData.Rows)
                    {
                        displayDt.Rows.Add(
                            Convert.ToInt32(row["ID"]),
                            row["Название"].ToString(),
                            Convert.ToDecimal(row["Цена"]),
                            Convert.ToInt32(row["Количество на складе"]),
                            row["Категория"] != DBNull.Value ? row["Категория"].ToString() : "Без категории"
                        );
                    }

                    // Настройка DataGridView
                    dataGridViewProduct.DataSource = displayDt;

                    // Настраиваем отображение
                    ConfigureDataGridView(dataGridViewProduct);

                    // Дополнительные настройки для товаров
                    dataGridViewProduct.Columns["ID"].Visible = false;
                    dataGridViewProduct.Columns["Цена"].DefaultCellStyle.Format = "C2";
                    dataGridViewProduct.Columns["Цена"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dataGridViewProduct.Columns["Количество"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                    // Автоподбор ширины колонок
                    dataGridViewProduct.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

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
                    }
                    else if (quantity < 20)
                    {
                        row.Cells["Количество"].Style.BackColor = Color.LightYellow;
                        row.Cells["Количество"].Style.ForeColor = Color.DarkOrange;
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

                    dataGridView1.DataSource = displayDt;
                    ConfigureDataGridView(dataGridView1);
                    dataGridView1.Columns["ID"].Visible = false;

                    // Автоподбор ширины
                    dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

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
    }
}