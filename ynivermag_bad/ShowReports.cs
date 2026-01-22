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
    public partial class ShowReports : Form
    {
        private int _roleID;
        private string _fio;
        private FilterManager _filterManager;

        public ShowReports(string FIO, int roleID)
        {
            try
            {
                InitializeComponent();
                SetupBasicControls();
                InitializeData();
                _roleID = roleID;
                _fio = FIO;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации формы: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupBasicControls()
        {
            // Настройка сортировки
            cmbSort.Items.AddRange(new string[] {
                "Дата (по возрастанию)",
                "Дата (по убыванию)",
                "Сумма (по возрастанию)",
                "Сумма (по убыванию)"
            });
            cmbSort.SelectedIndex = 0;

            // Настройка DateTimePicker
            SetupDateTimePickers();

            // Настройка DataGridView
            SetupDataGridView();
        }

        private void InitializeData()
        {
            try
            {
                // Проверка строки подключения
                if (string.IsNullOrEmpty(Connection.ConnectionString))
                {
                    MessageBox.Show("Не указана строка подключения к БД", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _filterManager = new FilterManager(Connection.ConnectionString);

                if (_filterManager != null)
                {
                    _filterManager.PopulateUsersComboBox(cmbUserFilter);
                    _filterManager.PopulateStatusComboBox(cmbStatusFilter);
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Устанавливаем значения по умолчанию
                cmbUserFilter.Items.Add("Все продавцы");
                cmbUserFilter.SelectedIndex = 0;

                cmbStatusFilter.Items.AddRange(new[] { "Все статусы", "обработка", "отправлен", "доставлен" });
                cmbStatusFilter.SelectedIndex = 0;
            }
        }

        private void SetupDateTimePickers()
        {
            try
            {
                // Устанавливаем безопасные значения
                DateTime today = DateTime.Today;

                dtpFromDate.Value = today.AddMonths(-1);
                dtpToDate.Value = today;

                dtpFromDate.Format = DateTimePickerFormat.Custom;
                dtpFromDate.CustomFormat = "dd.MM.yyyy";
                dtpFromDate.ShowUpDown = false;

                dtpToDate.Format = DateTimePickerFormat.Custom;
                dtpToDate.CustomFormat = "dd.MM.yyyy";
                dtpToDate.ShowUpDown = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка настройки дат: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDataGridView()
        {
            try
            {
                dataGridViewOrders.Columns.Clear();
                dataGridViewOrders.Rows.Clear();
                dataGridViewOrders.DataSource = null;

                // Создаем колонки для отображения заказов
                dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "OrderId",
                    HeaderText = "№ Заказа",
                    Width = 80
                });
                dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "ClientName",
                    HeaderText = "Клиент",
                    Width = 150
                });
                dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "UserName",
                    HeaderText = "Продавец",
                    Width = 120
                });
                dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "OrderDate",
                    HeaderText = "Дата заказа",
                    Width = 120
                });
                dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "TotalAmount",
                    HeaderText = "Сумма",
                    Width = 100
                });
                dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    HeaderText = "Статус",
                    Width = 100
                });
                dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Products",
                    HeaderText = "Товары",
                    Width = 200
                });

                // ============ НАСТРОЙКИ ДЛЯ ТЕКСТА ПО ВСЕЙ ШИРИНЕ ============

                // 1. Разрешаем перенос текста
                dataGridViewOrders.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                // 2. Устанавливаем выравнивание по левому краю для всех колонок
                foreach (DataGridViewColumn column in dataGridViewOrders.Columns)
                {
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                }

                // 3. Устанавливаем автоподбор высоты строк
                dataGridViewOrders.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                // 4. Увеличиваем высоту строк для лучшего отображения
                dataGridViewOrders.RowTemplate.Height = 50; // или AutoSize для автоматической высоты

                // 5. Разрешаем изменение высоты строк пользователем
                //dataGridViewRows.AllowUserToResizeRows = true;

                // 6. Устанавливаем минимальную высоту строк
                dataGridViewOrders.RowTemplate.MinimumHeight = 40;

                // 7. Настраиваем отображение текста для колонки с товарами
                dataGridViewOrders.Columns["Products"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                // 8. Отключаем автоподбор ширины колонок по содержимому (чтобы они растягивались)
                dataGridViewOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // 9. Настраиваем пропорциональное распределение ширины
                // Можно установить разные FillWeight для колонок
                dataGridViewOrders.Columns["OrderId"].FillWeight = 5;
                dataGridViewOrders.Columns["ClientName"].FillWeight = 15;
                dataGridViewOrders.Columns["UserName"].FillWeight = 12;
                dataGridViewOrders.Columns["OrderDate"].FillWeight = 12;
                dataGridViewOrders.Columns["TotalAmount"].FillWeight = 10;
                dataGridViewOrders.Columns["Status"].FillWeight = 10;
                dataGridViewOrders.Columns["Products"].FillWeight = 36; // Самая широкая колонка

                // 10. Устанавливаем выравнивание для числовых полей
                dataGridViewOrders.Columns["TotalAmount"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
                dataGridViewOrders.Columns["TotalAmount"].HeaderCell.Style.Alignment =
                    DataGridViewContentAlignment.MiddleRight;

                dataGridViewOrders.Columns["OrderId"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
                dataGridViewOrders.Columns["OrderDate"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка настройки таблицы: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadData()
        {
            try
            {
                if (_filterManager == null || dataGridViewOrders == null)
                    return;

                string searchText = txtSearch?.Text ?? "";
                string userFilter = cmbUserFilter.SelectedItem?.ToString() ?? "Все продавцы";
                string statusFilter = cmbStatusFilter.SelectedItem?.ToString() ?? "Все статусы";
                DateTime fromDate = dtpFromDate.Value;
                DateTime toDate = dtpToDate.Value;

                if (fromDate > toDate)
                {
                    MessageBox.Show("Дата 'С' не может быть больше даты 'По'", "Ошибка дат",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Определение сортировки
                string sortBy = "OrderDate";
                bool ascending = false;

                if (cmbSort.SelectedItem != null)
                {
                    switch (cmbSort.SelectedItem.ToString())
                    {
                        case "Дата (по возрастанию)":
                            sortBy = "OrderDate"; ascending = true; break;
                        case "Дата (по убыванию)":
                            sortBy = "OrderDate"; ascending = false; break;
                        case "Сумма (по возрастанию)":
                            sortBy = "TotalAmount"; ascending = true; break;
                        case "Сумма (по убыванию)":
                            sortBy = "TotalAmount"; ascending = false; break;
                    }
                }

                // Получение отфильтрованных данных
                var orders = _filterManager.GetFilteredOrders(
                    searchText, userFilter, statusFilter, fromDate, toDate, sortBy, ascending);

                // Очистка и заполнение таблицы
                dataGridViewOrders.Rows.Clear();

                foreach (var order in orders)
                {
                    int rowIndex = dataGridViewOrders.Rows.Add(
                        order.OrderId,
                        order.ClientName,
                        order.UserName,
                        order.OrderDate.ToString("dd.MM.yyyy HH:mm"),
                        order.TotalAmount.ToString("N2") + " ₽",
                        order.Status,
                        order.Products
                    );

                    // Раскраска строк по статусу
                    if (rowIndex >= 0 && rowIndex < dataGridViewOrders.Rows.Count)
                    {
                        dataGridViewOrders.Rows[rowIndex].DefaultCellStyle.BackColor = GetStatusColor(order.Status);
                    }
                }

                // Обновление статистики
                if (lblRecordCount != null)
                    lblRecordCount.Text = $"Найдено заказов: {orders.Count}";

                if (lblTotalSum != null)
                {
                    decimal totalSum = orders.Sum(o => o.TotalAmount);
                    lblTotalSum.Text = $"Общая сумма: {totalSum:N2} ₽";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка загрузки", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Color GetStatusColor(string status)
        {
            if (string.IsNullOrEmpty(status))
                return Color.White;

            switch (status.ToLower())
            {
                case "доставлен": return Color.LightGreen;
                case "отправлен": return Color.LightYellow;
                case "обработка": return Color.LightCoral;
                default: return Color.White;
            }
        }

        // Обработчики событий
        private void txtSearch_TextChanged(object sender, EventArgs e) => LoadData();
        private void cmbUserFilter_SelectedIndexChanged(object sender, EventArgs e) => LoadData();
        private void cmbStatusFilter_SelectedIndexChanged(object sender, EventArgs e) => LoadData();
        private void cmbSort_SelectedIndexChanged(object sender, EventArgs e) => LoadData();
        private void dtpFromDate_ValueChanged(object sender, EventArgs e) => LoadData();
        private void dtpToDate_ValueChanged(object sender, EventArgs e) => LoadData();

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            try
            {
                txtSearch.Text = "";
                cmbUserFilter.SelectedIndex = 0;
                cmbStatusFilter.SelectedIndex = 0;
                cmbSort.SelectedIndex = 0;

                if (_filterManager != null)
                {
                    var dateRange = _filterManager.GetDateRange();
                    dtpFromDate.Value = dateRange.MaxDate.AddMonths(-1);
                    dtpToDate.Value = dateRange.MaxDate;
                }

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка очистки фильтров: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
                    Title = "Экспорт заказов",
                    FileName = $"Заказы_{DateTime.Now:yyyy-MM-dd}.csv"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("№ Заказа;Клиент;Продавец;Дата заказа;Сумма;Статус;Товары");

                    foreach (DataGridViewRow row in dataGridViewOrders.Rows)
                    {
                        if (row.IsNewRow) continue;

                        sb.AppendLine(
                            $"\"{row.Cells["OrderId"].Value}\";" +
                            $"\"{row.Cells["ClientName"].Value}\";" +
                            $"\"{row.Cells["UserName"].Value}\";" +
                            $"\"{row.Cells["OrderDate"].Value}\";" +
                            $"\"{row.Cells["TotalAmount"].Value}\";" +
                            $"\"{row.Cells["Status"].Value}\";" +
                            $"\"{row.Cells["Products"].Value}\""
                        );
                    }

                    System.IO.File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Данные успешно экспортированы!", "Экспорт",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InMenu_Click(object sender, EventArgs e)
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
    }
}