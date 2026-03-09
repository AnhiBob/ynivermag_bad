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
    public partial class SearchClient : Form
    {
        private string _connection;
        private DataTable _clientsTable;
        private bool _isUpdatingSearch = false;

        // Свойство для возврата выбранного клиента
        public int SelectedClientId { get; private set; } = -1;
        public string SelectedClientName { get; private set; } = "";

        public SearchClient()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;

            // Настройка формы
            this.Text = "Поиск клиента";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimizeBox = false;
            this.MaximizeBox = false;

            // Настройка DataGridView
            SetupDataGridView();

            // Загрузка клиентов
            LoadClients();

            // Подписка на события
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearch.KeyPress += TxtSearch_KeyPress; // ДОБАВЛЕНО: фильтрация ввода
            dataGridViewClients.CellDoubleClick += DataGridViewClients_CellDoubleClick;
            dataGridViewClients.KeyDown += DataGridViewClients_KeyDown;
            btnSelect.Click += BtnSelect_Click;
            btnCancel.Click += BtnCancel_Click;

            // Подсказка для поля поиска
            toolTip1.SetToolTip(txtSearch, "Поиск по фамилии, имени, телефону или email (буквы, цифры, пробел, дефис)");
        }

        private void SetupDataGridView()
        {
            dataGridViewClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewClients.MultiSelect = false;
            dataGridViewClients.ReadOnly = true;
            dataGridViewClients.RowHeadersVisible = false;
            dataGridViewClients.AllowUserToAddRows = false;
            dataGridViewClients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Добавляем колонки
            dataGridViewClients.Columns.Add("client_id", "ID");
            dataGridViewClients.Columns.Add("last_name", "Фамилия");
            dataGridViewClients.Columns.Add("first_name", "Имя");
            dataGridViewClients.Columns.Add("phone", "Телефон");
            dataGridViewClients.Columns.Add("email", "Email");

            // Настройка колонок
            dataGridViewClients.Columns["client_id"].Visible = false;
            dataGridViewClients.Columns["last_name"].Width = 150;
            dataGridViewClients.Columns["first_name"].Width = 150;
            dataGridViewClients.Columns["phone"].Width = 120;
            dataGridViewClients.Columns["email"].Width = 180;

            // Подсветка строк при наведении
            dataGridViewClients.RowsDefaultCellStyle.BackColor = Color.White;
            dataGridViewClients.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dataGridViewClients.DefaultCellStyle.SelectionBackColor = Color.FromArgb(76, 175, 80);
            dataGridViewClients.DefaultCellStyle.SelectionForeColor = Color.White;
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
                        phone,
                        email
                        FROM client 
                        WHERE isActive = 1
                        ORDER BY last_name, first_name";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    _clientsTable = new DataTable();
                    adapter.Fill(_clientsTable);

                    DisplayClients(_clientsTable);

                    lblTotalCount.Text = $"Всего клиентов: {_clientsTable.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayClients(DataTable clients)
        {
            dataGridViewClients.Rows.Clear();

            foreach (DataRow row in clients.Rows)
            {
                dataGridViewClients.Rows.Add(
                    row["client_id"],
                    row["last_name"],
                    row["first_name"],
                    FormatPhone(row["phone"]?.ToString() ?? ""),
                    row["email"]?.ToString() ?? ""
                );
            }

            if (dataGridViewClients.Rows.Count > 0)
            {
                dataGridViewClients.Rows[0].Selected = true;
            }
        }

        private string FormatPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return "";

            // Простое форматирование для отображения
            string digitsOnly = new string(phone.Where(char.IsDigit).ToArray());

            if (digitsOnly.Length == 11 && digitsOnly.StartsWith("7"))
            {
                return $"+7 ({digitsOnly.Substring(1, 3)}) {digitsOnly.Substring(4, 3)}-{digitsOnly.Substring(7, 2)}-{digitsOnly.Substring(9, 2)}";
            }
            else if (digitsOnly.Length == 11 && digitsOnly.StartsWith("8"))
            {
                return $"8 ({digitsOnly.Substring(1, 3)}) {digitsOnly.Substring(4, 3)}-{digitsOnly.Substring(7, 2)}-{digitsOnly.Substring(9, 2)}";
            }

            return phone;
        }

        #region Фильтрация ввода в поле поиска

        /// <summary>
        /// Фильтрация ввода в поле поиска - разрешаем только буквы, цифры, пробел, дефис, @ и . (для email)
        /// </summary>
        private void TxtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем backspace
            if (char.IsControl(e.KeyChar))
                return;

            // Разрешенные символы: буквы, цифры, пробел, дефис, @, точка
            bool isValid = char.IsLetterOrDigit(e.KeyChar) ||
                           e.KeyChar == ' ' ||
                           e.KeyChar == '-' ||
                           e.KeyChar == '@' ||
                           e.KeyChar == '.';

            if (!isValid)
            {
                e.Handled = true;

                // Показываем подсказку при попытке ввести спецсимвол
                TextBox textBox = sender as TextBox;
                if (textBox != null)
                {
                    toolTip1.Show("Разрешены только буквы, цифры, пробел, дефис, @ и .",
                        textBox, 0, -20, 1500);
                }
            }
        }

        /// <summary>
        /// Фильтрация ввода в поле поиска (дополнительная проверка при вставке)
        /// </summary>
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
                PerformSearch();
            }
            finally
            {
                _isUpdatingSearch = false;
            }
        }

        /// <summary>
        /// Фильтр для текста поиска - оставляем только буквы, цифры, пробел, дефис, @ и .
        /// </summary>
        private string FilterSearchText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return new string(input.Where(c =>
                char.IsLetterOrDigit(c) ||  // Буквы и цифры
                c == ' ' ||                  // Пробел
                c == '-' ||                  // Дефис
                c == '@' ||                  // @ для email
                c == '.').ToArray());        // Точка для email
        }

        /// <summary>
        /// Выполнение поиска по отфильтрованному тексту
        /// </summary>
        private void PerformSearch()
        {
            string searchText = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Если строка поиска пуста, показываем всех клиентов
                DisplayClients(_clientsTable);
                lblFoundCount.Text = "";
                return;
            }

            // Фильтруем клиентов
            DataTable filteredTable = _clientsTable.Clone();

            foreach (DataRow row in _clientsTable.Rows)
            {
                string lastName = row["last_name"].ToString().ToLower();
                string firstName = row["first_name"].ToString().ToLower();
                string phone = row["phone"]?.ToString().ToLower() ?? "";
                string email = row["email"]?.ToString().ToLower() ?? "";

                // Проверяем совпадение в разных полях
                bool match = lastName.Contains(searchText) ||
                            firstName.Contains(searchText) ||
                            phone.Contains(searchText) ||
                            email.Contains(searchText) ||
                            $"{lastName} {firstName}".Contains(searchText);

                if (match)
                {
                    filteredTable.ImportRow(row);
                }
            }

            DisplayClients(filteredTable);
            lblFoundCount.Text = $"Найдено: {filteredTable.Rows.Count}";
        }

        #endregion

        private void DataGridViewClients_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                SelectCurrentClient();
            }
        }

        private void DataGridViewClients_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectCurrentClient();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            SelectCurrentClient();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SelectCurrentClient()
        {
            if (dataGridViewClients.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridViewClients.SelectedRows[0];
                SelectedClientId = Convert.ToInt32(selectedRow.Cells["client_id"].Value);

                // Формируем ФИО для отображения
                string lastName = selectedRow.Cells["last_name"].Value.ToString();
                string firstName = selectedRow.Cells["first_name"].Value.ToString();
                SelectedClientName = $"{lastName} {firstName}";

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Выберите клиента из списка", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void FindAndSelectClient(int clientId)
        {
            foreach (DataGridViewRow row in dataGridViewClients.Rows)
            {
                if (Convert.ToInt32(row.Cells["client_id"].Value) == clientId)
                {
                    row.Selected = true;
                    dataGridViewClients.FirstDisplayedScrollingRowIndex = row.Index;
                    break;
                }
            }
        }

        // Дополнительный метод для очистки поиска
        private void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            txtSearch.Focus();
        }
    }
}