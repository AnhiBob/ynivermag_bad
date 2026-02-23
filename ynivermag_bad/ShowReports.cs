using Excel = Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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
                    Width = 80,
                    MinimumWidth = 60
                });
                dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "ClientName",
                    HeaderText = "Клиент",
                    Width = 150,
                    MinimumWidth = 100
                });
                dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "UserName",
                    HeaderText = "Продавец",
                    Width = 120,
                    MinimumWidth = 100
                });
                dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "OrderDate",
                    HeaderText = "Дата заказа",
                    Width = 120,
                    MinimumWidth = 100
                });
                dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "TotalAmount",
                    HeaderText = "Сумма",
                    Width = 100,
                    MinimumWidth = 80
                });
                dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    HeaderText = "Статус",
                    Width = 100,
                    MinimumWidth = 80
                });
                dataGridViewOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Products",
                    HeaderText = "Товары",
                    Width = 200,
                    MinimumWidth = 150
                });

                // Подписываемся на событие форматирования
                dataGridViewOrders.CellFormatting += DataGridViewOrders_CellFormatting;

                // ============ НАСТРОЙКИ ДЛЯ АВТОПОДБОРА ШИРИНЫ ============

                // Режим автоподбора колонок - Fill (заполнение)
                dataGridViewOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Но с FillWeight для пропорций
                dataGridViewOrders.Columns["OrderId"].FillWeight = 5;
                dataGridViewOrders.Columns["ClientName"].FillWeight = 15;
                dataGridViewOrders.Columns["UserName"].FillWeight = 12;
                dataGridViewOrders.Columns["OrderDate"].FillWeight = 12;
                dataGridViewOrders.Columns["TotalAmount"].FillWeight = 10;
                dataGridViewOrders.Columns["Status"].FillWeight = 10;
                dataGridViewOrders.Columns["Products"].FillWeight = 36;

                // Разрешаем пользователю менять ширину колонок
                dataGridViewOrders.AllowUserToResizeColumns = true;

                // Устанавливаем минимальную ширину для всех колонок
                foreach (DataGridViewColumn col in dataGridViewOrders.Columns)
                {
                    col.MinimumWidth = 50;
                }

                // ============ НАСТРОЙКИ ДЛЯ ВЫСОТЫ СТРОК ============

                // Автоподбор высоты строк по содержимому
                dataGridViewOrders.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                // Минимальная высота строк
                dataGridViewOrders.RowTemplate.MinimumHeight = 30;

                // Разрешаем перенос текста
                dataGridViewOrders.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                // Особенно для колонки с товарами
                dataGridViewOrders.Columns["Products"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                // ============ ВЫРАВНИВАНИЕ ============

                // По левому краю для всех
                foreach (DataGridViewColumn column in dataGridViewOrders.Columns)
                {
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                }

                // Специальное выравнивание
                dataGridViewOrders.Columns["TotalAmount"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
                dataGridViewOrders.Columns["TotalAmount"].HeaderCell.Style.Alignment =
                    DataGridViewContentAlignment.MiddleRight;

                dataGridViewOrders.Columns["OrderId"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
                dataGridViewOrders.Columns["OrderDate"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
                dataGridViewOrders.Columns["Status"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;

                // Подписываемся на изменение размеров формы
                this.Resize += ShowReports_Resize;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка настройки таблицы: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ShowReports_Resize(object sender, EventArgs e)
        {
            // При изменении размера формы пересчитываем ширину колонок
            AdjustColumnWidths();
        }

        // Метод для подгонки ширины колонок
        private void AdjustColumnWidths()
        {
            if (dataGridViewOrders.Columns.Count == 0) return;

            try
            {
                // Временно отключаем автоподбор
                dataGridViewOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

                // Рассчитываем доступную ширину
                int totalWidth = dataGridViewOrders.ClientSize.Width;

                // Вычитаем ширину вертикального скролла (если есть)
                if (dataGridViewOrders.Rows.Count > 0 &&
                    dataGridViewOrders.DisplayedRowCount(false) < dataGridViewOrders.Rows.Count)
                {
                    totalWidth -= SystemInformation.VerticalScrollBarWidth;
                }

                // Устанавливаем минимальную ширину для каждой колонки
                int minOrderId = 60;
                int minClient = 100;
                int minUser = 100;
                int minDate = 100;
                int minAmount = 80;
                int minStatus = 80;
                int minProducts = 150;

                // Если общей ширины не хватает даже на минимум
                int requiredMinWidth = minOrderId + minClient + minUser + minDate +
                                       minAmount + minStatus + minProducts;

                if (totalWidth < requiredMinWidth)
                {
                    // Включаем горизонтальный скролл
                    dataGridViewOrders.ScrollBars = ScrollBars.Both;

                    // Устанавливаем минимальные ширины
                    dataGridViewOrders.Columns["OrderId"].Width = minOrderId;
                    dataGridViewOrders.Columns["ClientName"].Width = minClient;
                    dataGridViewOrders.Columns["UserName"].Width = minUser;
                    dataGridViewOrders.Columns["OrderDate"].Width = minDate;
                    dataGridViewOrders.Columns["TotalAmount"].Width = minAmount;
                    dataGridViewOrders.Columns["Status"].Width = minStatus;
                    dataGridViewOrders.Columns["Products"].Width = minProducts;
                }
                else
                {
                    // Хватает места - распределяем пропорционально
                    int remainingWidth = totalWidth - requiredMinWidth;

                    // Коэффициенты распределения (кто сколько получит от остатка)
                    double[] weights = { 0.1, 0.2, 0.15, 0.1, 0.1, 0.1, 0.25 };

                    dataGridViewOrders.Columns["OrderId"].Width = minOrderId +
                        (int)(remainingWidth * weights[0]);
                    dataGridViewOrders.Columns["ClientName"].Width = minClient +
                        (int)(remainingWidth * weights[1]);
                    dataGridViewOrders.Columns["UserName"].Width = minUser +
                        (int)(remainingWidth * weights[2]);
                    dataGridViewOrders.Columns["OrderDate"].Width = minDate +
                        (int)(remainingWidth * weights[3]);
                    dataGridViewOrders.Columns["TotalAmount"].Width = minAmount +
                        (int)(remainingWidth * weights[4]);
                    dataGridViewOrders.Columns["Status"].Width = minStatus +
                        (int)(remainingWidth * weights[5]);
                    dataGridViewOrders.Columns["Products"].Width = minProducts +
                        (int)(remainingWidth * weights[6]);

                    // Отключаем горизонтальный скролл
                    dataGridViewOrders.ScrollBars = ScrollBars.Vertical;
                }
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки при подгонке
                Console.WriteLine($"Ошибка подгонки колонок: {ex.Message}");
            }
        }

        private void DataGridViewOrders_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Проверяем, что это колонка статуса и есть данные
            if (dataGridViewOrders.Columns[e.ColumnIndex].Name == "Status" && e.RowIndex >= 0)
            {
                if (dataGridViewOrders.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    string status = dataGridViewOrders.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                    // Устанавливаем цвет ТОЛЬКО для этой ячейки
                    e.CellStyle.BackColor = GetStatusColor(status);
                   // e.CellStyle.ForeColor = GetStatusTextColor(status);
                    e.CellStyle.Font = new Font(dataGridViewOrders.Font, FontStyle.Bold);
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    e.CellStyle.SelectionBackColor = GetStatusColor(status); // Чтобы при выделении цвет не терялся
                    e.CellStyle.SelectionForeColor = GetStatusTextColor(status);
                }
            }
            // Можно также выделить ячейку с суммой
            else if (dataGridViewOrders.Columns[e.ColumnIndex].Name == "TotalAmount" && e.RowIndex >= 0)
            {
                e.CellStyle.BackColor = Color.FromArgb(240, 255, 240); // Очень светло-зеленый
                e.CellStyle.Font = new Font(dataGridViewOrders.Font, FontStyle.Bold);
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                // Форматируем сумму
                if (e.Value != null)
                {
                    string amountStr = e.Value.ToString().Replace(" ₽", "").Trim();
                    if (decimal.TryParse(amountStr, out decimal amount))
                    {
                        e.Value = amount.ToString("N2") + " ₽";
                        e.FormattingApplied = true;
                    }
                }
            }
        }

        private Color GetStatusColor(string status)
        {
            if (string.IsNullOrEmpty(status))
                return Color.White;

            switch (status.ToLower())
            {
                case "доставлен": return Color.FromArgb(198, 239, 206); // Светло-зеленый
                case "отправлен": return Color.FromArgb(255, 235, 156); // Светло-желтый
                case "обработка": return Color.FromArgb(255, 199, 206); // Светло-розовый
                default: return Color.White;
            }
        }

        private Color GetStatusTextColor(string status)
        {
            if (string.IsNullOrEmpty(status))
                return Color.Black;

            switch (status.ToLower())
            {
                case "доставлен": return Color.DarkGreen;
                case "отправлен": return Color.DarkGoldenrod;
                case "обработка": return Color.DarkRed;
                default: return Color.Black;
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
                // В методе LoadData() заменяем медленный код на быстрый:

                // Быстрое заполнение данными (без стилей в цикле!)
                dataGridViewOrders.Rows.Clear();

                foreach (var order in orders)
                {
                    dataGridViewOrders.Rows.Add(
                        order.OrderId,
                        order.ClientName,
                        order.UserName,
                        order.OrderDate.ToString("dd.MM.yyyy HH:mm"),
                        order.TotalAmount.ToString("N2") + " ₽",
                        order.Status,
                        order.Products
                    );
                }

                // Обновление статистики
                if (lblRecordCount != null)
                    lblRecordCount.Text = $"Найдено заказов: {orders.Count}";

                if (lblTotalSum != null)
                {
                    decimal totalSum = orders.Sum(o => o.TotalAmount);
                    lblTotalSum.Text = $"Общая сумма: {totalSum:N2} ₽";
                }

                // ПОДГОНЯЕМ ШИРИНУ КОЛОНОК ПОСЛЕ ЗАГРУЗКИ
                AdjustColumnWidths();


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка загрузки", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void ExportToExcel()
        {
            Excel.Application excelApp = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;
            Excel.Range range = null;

            try
            {
                // Проверяем наличие данных
                if (dataGridViewOrders.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта!", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Диалог сохранения
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Сохранить отчет по заказам",
                    FileName = $"Отчет_по_заказам_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    DefaultExt = "xlsx"
                };

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                string filePath = saveFileDialog.FileName;

                // Создаем Excel приложение
                excelApp = new Excel.Application();
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                workbook = excelApp.Workbooks.Add();
                worksheet = workbook.ActiveSheet;

                if (worksheet == null)
                    throw new Exception("Не удалось создать лист Excel");

                // Настройка страницы
                worksheet.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
                worksheet.PageSetup.LeftMargin = excelApp.CentimetersToPoints(1);
                worksheet.PageSetup.RightMargin = excelApp.CentimetersToPoints(1);
                worksheet.PageSetup.TopMargin = excelApp.CentimetersToPoints(1.5);
                worksheet.PageSetup.BottomMargin = excelApp.CentimetersToPoints(1);

                // Цветовая гамма (LimeGreen и GreenYellow)
                Color accentColor = Color.LimeGreen;
                Color lightGreen = Color.GreenYellow;
                Color veryLightGreen = Color.FromArgb(240, 255, 240);
                Color lightGray = Color.FromArgb(245, 245, 245);

                // ============ ЗАГОЛОВОК ОТЧЕТА ============
                range = worksheet.Range["A1", "G1"];
                range.Merge();
                range.Value = "ОТЧЕТ ПО ЗАКАЗАМ";
                range.Font.Size = 18;
                range.Font.Bold = true;
                range.Font.Name = "Segoe UI";
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightGreen);
                range.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.Black);
                range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                Marshal.ReleaseComObject(range);

                // ============ ИНФОРМАЦИЯ О ФИЛЬТРАХ ============
                int currentRow = 3;

                // Период
                worksheet.Cells[currentRow, 1] = "Период:";
                worksheet.Cells[currentRow, 2] = $"{dtpFromDate.Value:dd.MM.yyyy} - {dtpToDate.Value:dd.MM.yyyy}";
                FormatInfoCell(worksheet, currentRow, 1, true, lightGreen);
                FormatInfoCell(worksheet, currentRow, 2, false);
                currentRow++;

                // Продавец
                if (cmbUserFilter.SelectedIndex > 0)
                {
                    worksheet.Cells[currentRow, 1] = "Продавец:";
                    worksheet.Cells[currentRow, 2] = cmbUserFilter.SelectedItem?.ToString();
                    FormatInfoCell(worksheet, currentRow, 1, true, lightGreen);
                    FormatInfoCell(worksheet, currentRow, 2, false);
                    currentRow++;
                }

                // Статус
                if (cmbStatusFilter.SelectedIndex > 0)
                {
                    worksheet.Cells[currentRow, 1] = "Статус:";
                    worksheet.Cells[currentRow, 2] = cmbStatusFilter.SelectedItem?.ToString();
                    FormatInfoCell(worksheet, currentRow, 1, true, lightGreen);
                    FormatInfoCell(worksheet, currentRow, 2, false);
                    currentRow++;
                }

                // Поиск
                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    worksheet.Cells[currentRow, 1] = "Поиск:";
                    worksheet.Cells[currentRow, 2] = txtSearch.Text;
                    FormatInfoCell(worksheet, currentRow, 1, true, lightGreen);
                    FormatInfoCell(worksheet, currentRow, 2, false);
                    currentRow++;
                }

                // Сортировка
                worksheet.Cells[currentRow, 1] = "⬆Сортировка:";
                worksheet.Cells[currentRow, 2] = cmbSort.SelectedItem?.ToString();
                FormatInfoCell(worksheet, currentRow, 1, true, lightGreen);
                FormatInfoCell(worksheet, currentRow, 2, false);
                currentRow++;

                // Дата формирования
                worksheet.Cells[currentRow, 1] = "⏱Дата формирования:";
                worksheet.Cells[currentRow, 2] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                FormatInfoCell(worksheet, currentRow, 1, true, lightGreen);
                FormatInfoCell(worksheet, currentRow, 2, false);
                currentRow++;

                // Количество записей
                worksheet.Cells[currentRow, 1] = "Количество заказов:";
                worksheet.Cells[currentRow, 2] = dataGridViewOrders.Rows.Count.ToString();
                FormatInfoCell(worksheet, currentRow, 1, true, lightGreen);

                // Выделяем количество
                range = worksheet.Cells[currentRow, 2];
                range.Font.Bold = true;
                range.Font.Color = System.Drawing.ColorTranslator.ToOle(accentColor);
                Marshal.ReleaseComObject(range);

                currentRow += 2;

                // ============ ЗАГОЛОВКИ ТАБЛИЦЫ ============
                string[] headers = { "№ Заказа", "Клиент", "Продавец", "Дата заказа", "Сумма", "Статус", "Товары" };
                int columnCount = headers.Length;

                for (int i = 0; i < columnCount; i++)
                {
                    worksheet.Cells[currentRow, i + 1] = headers[i];
                    range = worksheet.Cells[currentRow, i + 1];
                    range.Font.Bold = true;
                    range.Font.Name = "Segoe UI";
                    range.Font.Size = 11;
                    range.Interior.Color = System.Drawing.ColorTranslator.ToOle(accentColor);
                    range.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.White);
                    range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    Marshal.ReleaseComObject(range);
                }

                currentRow++;

                // ============ ДАННЫЕ ============
                decimal totalSum = 0;

                for (int i = 0; i < dataGridViewOrders.Rows.Count; i++)
                {
                    DataGridViewRow row = dataGridViewOrders.Rows[i];
                    if (row.IsNewRow) continue;

                    // Получаем значения
                    string orderId = row.Cells["OrderId"].Value?.ToString() ?? "";
                    string clientName = row.Cells["ClientName"].Value?.ToString() ?? "";
                    string userName = row.Cells["UserName"].Value?.ToString() ?? "";
                    string orderDate = row.Cells["OrderDate"].Value?.ToString() ?? "";
                    string status = row.Cells["Status"].Value?.ToString() ?? "";
                    string products = row.Cells["Products"].Value?.ToString() ?? "";

                    // Парсим сумму
                    string amountStr = row.Cells["TotalAmount"].Value?.ToString() ?? "0";
                    amountStr = amountStr.Replace(" ₽", "").Replace(" ", "").Trim();

                    decimal amount = 0;
                    decimal.TryParse(amountStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.CurrentCulture, out amount);

                    totalSum += amount;

                    // Заполняем ячейки
                    worksheet.Cells[currentRow, 1] = orderId;
                    worksheet.Cells[currentRow, 2] = clientName;
                    worksheet.Cells[currentRow, 3] = userName;
                    worksheet.Cells[currentRow, 4] = ParseExcelDate(orderDate);
                    worksheet.Cells[currentRow, 5] = amount;
                    worksheet.Cells[currentRow, 6] = status;
                    worksheet.Cells[currentRow, 7] = products;

                    // Форматирование ячеек
                    for (int j = 1; j <= columnCount; j++)
                    {
                        range = worksheet.Cells[currentRow, j];

                        // Выравнивание
                        if (j == 1 || j == 4) // № заказа и дата по центру
                            range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        else if (j == 5) // сумма по правому краю
                            range.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                        // Границы
                        range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                        range.Borders.Color = System.Drawing.ColorTranslator.ToOle(Color.LightGray);

                        // Чередование фона
                        if (i % 2 == 1)
                        {
                            range.Interior.Color = System.Drawing.ColorTranslator.ToOle(veryLightGreen);
                        }

                        Marshal.ReleaseComObject(range);
                    }

                    // Специальное форматирование для статуса
                    range = worksheet.Cells[currentRow, 6];
                    range.Font.Bold = true;
                    range.Interior.Color = System.Drawing.ColorTranslator.ToOle(GetExcelStatusColor(status));
                    range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    Marshal.ReleaseComObject(range);

                    // Форматирование суммы
                    range = worksheet.Cells[currentRow, 5];
                    range.NumberFormat = "#,##0.00";
                    range.Font.Bold = true;
                    Marshal.ReleaseComObject(range);

                    // Форматирование даты
                    range = worksheet.Cells[currentRow, 4];
                    range.NumberFormat = "dd.MM.yyyy HH:mm";
                    Marshal.ReleaseComObject(range);

                    currentRow++;
                }

                // ============ ИТОГОВАЯ СТРОКА ============
                // Объединяем ячейки для текста "ИТОГО"
                range = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 4]];
                range.Merge();
                range.Value = "ИТОГО ПО ВСЕМ ЗАКАЗАМ:";
                range.Font.Bold = true;
                range.Font.Size = 11;
                range.Font.Name = "Segoe UI";
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightGreen);
                range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                Marshal.ReleaseComObject(range);

                // Ячейка с итоговой суммой
                range = worksheet.Cells[currentRow, 5];
                range.Value = totalSum;
                range.Font.Bold = true;
                range.Font.Size = 11;
                range.Font.Name = "Segoe UI";
                range.NumberFormat = "#,##0.00";
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightGreen);
                range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                Marshal.ReleaseComObject(range);

                // Пустые ячейки справа
                for (int j = 6; j <= columnCount; j++)
                {
                    range = worksheet.Cells[currentRow, j];
                    range.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightGreen);
                    range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    Marshal.ReleaseComObject(range);
                }

                // ============ ФОРМАТИРОВАНИЕ ============
                // Автоподбор ширины
                range = worksheet.UsedRange;
                range.Columns.AutoFit();

                // Минимальная ширина для колонок
                if (worksheet.Columns[1].ColumnWidth < 10) worksheet.Columns[1].ColumnWidth = 10; // № заказа
                if (worksheet.Columns[2].ColumnWidth < 15) worksheet.Columns[2].ColumnWidth = 15; // Клиент
                if (worksheet.Columns[3].ColumnWidth < 12) worksheet.Columns[3].ColumnWidth = 12; // Продавец
                if (worksheet.Columns[4].ColumnWidth < 16) worksheet.Columns[4].ColumnWidth = 16; // Дата
                if (worksheet.Columns[5].ColumnWidth < 12) worksheet.Columns[5].ColumnWidth = 12; // Сумма
                if (worksheet.Columns[6].ColumnWidth < 12) worksheet.Columns[6].ColumnWidth = 12; // Статус
                if (worksheet.Columns[7].ColumnWidth < 40) worksheet.Columns[7].ColumnWidth = 40; // Товары
                worksheet.Columns[7].WrapText = true;

                Marshal.ReleaseComObject(range);

                // Автофильтр
                int headerRow = 7; // Строка с заголовками таблицы
                range = worksheet.Range[worksheet.Cells[headerRow, 1], worksheet.Cells[currentRow - 1, columnCount]];
                range.AutoFilter(1, Type.Missing, Excel.XlAutoFilterOperator.xlAnd, Type.Missing, true);
                Marshal.ReleaseComObject(range);

                // Сохраняем
                workbook.SaveAs(filePath);
                workbook.Close(false);

                MessageBox.Show($"Отчет успешно создан!\n\n" +
                               $"Всего заказов: {dataGridViewOrders.Rows.Count}\n" +
                               $"Общая сумма: {totalSum:N2} ₽\n" +
                               $"Файл: {filePath}",
                               "Экспорт завершен",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Information);

                // Открываем файл
                DialogResult result = MessageBox.Show("Открыть созданный отчет?", "Вопрос",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Освобождаем ресурсы
                try
                {
                    if (range != null) Marshal.ReleaseComObject(range);
                    if (worksheet != null) Marshal.ReleaseComObject(worksheet);
                    if (workbook != null)
                    {
                        workbook.Close(false);
                        Marshal.ReleaseComObject(workbook);
                    }
                    if (excelApp != null)
                    {
                        excelApp.Quit();
                        Marshal.ReleaseComObject(excelApp);
                    }
                }
                catch { }
                finally
                {
                    range = null;
                    worksheet = null;
                    workbook = null;
                    excelApp = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }

        // Вспомогательные методы
        private void FormatInfoCell(Excel.Worksheet worksheet, int row, int col, bool isLabel, Color? bgColor = null)
        {
            Excel.Range cell = worksheet.Cells[row, col];

            if (isLabel)
            {
                cell.Font.Bold = true;
                cell.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.Black);
                if (bgColor.HasValue)
                    cell.Interior.Color = System.Drawing.ColorTranslator.ToOle(bgColor.Value);
            }

            cell.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            cell.Borders.Color = System.Drawing.ColorTranslator.ToOle(Color.LightGray);

            Marshal.ReleaseComObject(cell);
        }

        private object ParseExcelDate(string dateString)
        {
            try
            {
                if (string.IsNullOrEmpty(dateString)) return "";
                DateTime date;
                if (DateTime.TryParse(dateString, out date)) return date;
                return dateString;
            }
            catch
            {
                return dateString;
            }
        }

        private Color GetExcelStatusColor(string status)
        {
            if (string.IsNullOrEmpty(status)) return Color.White;

            switch (status.ToLower())
            {
                case "доставлен": return Color.FromArgb(198, 239, 206); // Светло-зеленый
                case "отправлен": return Color.FromArgb(255, 235, 156); // Светло-желтый
                case "обработка": return Color.FromArgb(255, 199, 206); // Светло-розовый
                default: return Color.White;
            }
        }

        private Color GetStatusCellColor(string status)
        {
            if (string.IsNullOrEmpty(status)) return Color.White;

            switch (status.ToLower())
            {
                case "доставлен": return Color.FromArgb(198, 239, 206); // Салатовый
                case "отправлен": return Color.FromArgb(255, 235, 156); // Желтый
                case "обработка": return Color.FromArgb(255, 199, 206); // Розовый
                default: return Color.White;
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

        private void Report_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }
    }
}