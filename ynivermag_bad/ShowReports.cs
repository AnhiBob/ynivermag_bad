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
using TextBox = System.Windows.Forms.TextBox;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Excel = Microsoft.Office.Interop.Excel;
using MySql.Data.MySqlClient;

namespace ynivermag_bad
{
    /// <summary>
    /// Форма для просмотра и фильтрации отчетов по заказам.
    /// Предоставляет функционал для:
    /// - Фильтрации заказов по дате, продавцу, статусу
    /// - Поиска по тексту
    /// - Сортировки по различным полям
    /// - Экспорта данных в Excel с форматированием
    /// - Визуального выделения статусов цветом
    /// </summary>
    public partial class ShowReports : Form
    {
        // ============ ПОЛЯ КЛАССА ============

        /// <summary>
        /// ID роли текущего пользователя
        /// </summary>
        private int _roleID;

        /// <summary>
        /// ФИО текущего пользователя
        /// </summary>
        private string _fio;

        /// <summary>
        /// Менеджер фильтрации для работы с данными
        /// </summary>
        private FilterManager _filterManager;

        /// <summary>
        /// Минимальная дата в базе данных (для ограничения выбора)
        /// </summary>
        private DateTime _minDate;

        /// <summary>
        /// Максимальная дата в базе данных (для ограничения выбора)
        /// </summary>
        private DateTime _maxDate;

        /// <summary>
        /// Флаг для предотвращения рекурсивного обновления поля поиска
        /// </summary>
        private bool _isUpdatingSearch = false;

        // ============ КОНСТРУКТОР ============

        /// <summary>
        /// Конструктор формы отчетов
        /// </summary>
        /// <param name="FIO">ФИО текущего пользователя</param>
        /// <param name="roleID">ID роли пользователя</param>
        public ShowReports(string FIO, int roleID)
        {
            try
            {
                InitializeComponent();
                SetupBasicControls();
                InitializeData();
                _roleID = roleID;
                _fio = FIO;

                // Подписка на события фильтрации ввода
                txtSearch.TextChanged += TxtSearch_TextChanged;
                txtSearch.KeyPress += TxtSearch_KeyPress;

                // Подсказка для поля поиска
                toolTip1.SetToolTip(txtSearch, "Поиск по номеру заказа, клиенту, продавцу или товарам (буквы, цифры, пробел)");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации формы: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============ ИНИЦИАЛИЗАЦИЯ ============

        /// <summary>
        /// Настраивает базовые элементы управления
        /// </summary>
        private void SetupBasicControls()
        {
            // Настройка выпадающего списка сортировки
            cmbSort.Items.AddRange(new string[] {
                "Дата (по возрастанию)",
                "Дата (по убыванию)",
                "Сумма (по возрастанию)",
                "Сумма (по убыванию)"
            });
            cmbSort.SelectedIndex = 0;

            // Настройка элементов выбора даты
            SetupDateTimePickers();

            // Настройка таблицы
            SetupDataGridView();
        }

        /// <summary>
        /// Инициализирует данные: загружает фильтры и диапазон дат
        /// </summary>
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
                    // Получаем минимальную и максимальную даты из базы
                    var dateRange = _filterManager.GetDateRange();
                    _minDate = dateRange.MinDate;
                    _maxDate = dateRange.MaxDate;

                    // Заполняем выпадающие списки фильтров
                    _filterManager.PopulateUsersComboBox(cmbUserFilter);
                    _filterManager.PopulateStatusComboBox(cmbStatusFilter);

                    // Устанавливаем даты после получения диапазона
                    SetDefaultDates();

                    // Загружаем данные
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Устанавливаем значения по умолчанию при ошибке
                cmbUserFilter.Items.Add("Все продавцы");
                cmbUserFilter.SelectedIndex = 0;

                cmbStatusFilter.Items.AddRange(new[] { "Все статусы", "обработка", "отправлен", "доставлен" });
                cmbStatusFilter.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Настраивает элементы выбора даты
        /// </summary>
        private void SetupDateTimePickers()
        {
            try
            {
                // Устанавливаем формат отображения даты
                dtpFromDate.Format = DateTimePickerFormat.Custom;
                dtpFromDate.CustomFormat = "dd.MM.yyyy";
                dtpFromDate.ShowUpDown = false;
                dtpFromDate.ShowCheckBox = false;

                dtpToDate.Format = DateTimePickerFormat.Custom;
                dtpToDate.CustomFormat = "dd.MM.yyyy";
                dtpToDate.ShowUpDown = false;
                dtpToDate.ShowCheckBox = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка настройки дат: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Устанавливает значения дат по умолчанию (последний месяц)
        /// </summary>
        private void SetDefaultDates()
        {
            try
            {
                // Устанавливаем ограничения для выбора дат
                dtpFromDate.MinDate = _minDate;
                dtpFromDate.MaxDate = _maxDate;
                dtpToDate.MinDate = _minDate;
                dtpToDate.MaxDate = _maxDate;

                // По умолчанию показываем последний месяц
                DateTime defaultFrom = _maxDate.AddMonths(-1);
                if (defaultFrom < _minDate)
                {
                    defaultFrom = _minDate;
                }

                dtpFromDate.Value = defaultFrom;
                dtpToDate.Value = _maxDate;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка установки дат: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============ НАСТРОЙКА ТАБЛИЦЫ ============

        /// <summary>
        /// Настраивает внешний вид и колонки DataGridView
        /// </summary>
        private void SetupDataGridView()
        {
            try
            {
                dataGridViewOrders.Columns.Clear();
                dataGridViewOrders.Rows.Clear();
                dataGridViewOrders.DataSource = null;

                // Создаем колонки для отображения заказов
                AddDataGridViewColumns();

                // Подписываемся на событие форматирования для раскрашивания статусов
                dataGridViewOrders.CellFormatting += DataGridViewOrders_CellFormatting;

                // Настройка режима заполнения колонок
                ConfigureColumnFillMode();

                // Разрешаем пользователю менять ширину колонок
                dataGridViewOrders.AllowUserToResizeColumns = true;

                // Устанавливаем минимальную ширину для всех колонок
                SetMinimumColumnWidths();

                // Настройка высоты строк и переноса текста
                ConfigureRowHeight();

                // Настройка выравнивания содержимого
                ConfigureColumnAlignment();

                // Подписываемся на изменение размеров формы
                this.Resize += ShowReports_Resize;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка настройки таблицы: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Добавляет колонки в DataGridView
        /// </summary>
        private void AddDataGridViewColumns()
        {
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
        }

        /// <summary>
        /// Настраивает режим заполнения колонок с весовыми коэффициентами
        /// </summary>
        private void ConfigureColumnFillMode()
        {
            dataGridViewOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // FillWeight определяет пропорции распределения свободного места
            dataGridViewOrders.Columns["OrderId"].FillWeight = 5;
            dataGridViewOrders.Columns["ClientName"].FillWeight = 15;
            dataGridViewOrders.Columns["UserName"].FillWeight = 12;
            dataGridViewOrders.Columns["OrderDate"].FillWeight = 12;
            dataGridViewOrders.Columns["TotalAmount"].FillWeight = 10;
            dataGridViewOrders.Columns["Status"].FillWeight = 10;
            dataGridViewOrders.Columns["Products"].FillWeight = 36;
        }

        /// <summary>
        /// Устанавливает минимальную ширину для всех колонок
        /// </summary>
        private void SetMinimumColumnWidths()
        {
            foreach (DataGridViewColumn col in dataGridViewOrders.Columns)
            {
                col.MinimumWidth = 50;
            }
        }

        /// <summary>
        /// Настраивает высоту строк и перенос текста
        /// </summary>
        private void ConfigureRowHeight()
        {
            // Автоподбор высоты строк по содержимому
            dataGridViewOrders.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // Минимальная высота строк
            dataGridViewOrders.RowTemplate.MinimumHeight = 30;

            // Разрешаем перенос текста
            dataGridViewOrders.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            // Особенно для колонки с товарами
            dataGridViewOrders.Columns["Products"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        }

        /// <summary>
        /// Настраивает выравнивание содержимого в колонках
        /// </summary>
        private void ConfigureColumnAlignment()
        {
            // По левому краю для всех по умолчанию
            foreach (DataGridViewColumn column in dataGridViewOrders.Columns)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }

            // Специальное выравнивание для отдельных колонок
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
        }

        /// <summary>
        /// Обработчик изменения размера формы - пересчитывает ширину колонок
        /// </summary>
        private void ShowReports_Resize(object sender, EventArgs e)
        {
            AdjustColumnWidths();
        }

        /// <summary>
        /// Подгоняет ширину колонок под доступное пространство
        /// </summary>
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
                    SetMinimumWidths(minOrderId, minClient, minUser, minDate, minAmount, minStatus, minProducts);
                }
                else
                {
                    // Хватает места - распределяем пропорционально
                    DistributeRemainingWidth(totalWidth, requiredMinWidth,
                        minOrderId, minClient, minUser, minDate, minAmount, minStatus, minProducts);

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

        /// <summary>
        /// Устанавливает минимальную ширину для всех колонок
        /// </summary>
        private void SetMinimumWidths(int minOrderId, int minClient, int minUser, int minDate,
                                      int minAmount, int minStatus, int minProducts)
        {
            dataGridViewOrders.Columns["OrderId"].Width = minOrderId;
            dataGridViewOrders.Columns["ClientName"].Width = minClient;
            dataGridViewOrders.Columns["UserName"].Width = minUser;
            dataGridViewOrders.Columns["OrderDate"].Width = minDate;
            dataGridViewOrders.Columns["TotalAmount"].Width = minAmount;
            dataGridViewOrders.Columns["Status"].Width = minStatus;
            dataGridViewOrders.Columns["Products"].Width = minProducts;
        }

        /// <summary>
        /// Распределяет оставшуюся ширину пропорционально между колонками
        /// </summary>
        private void DistributeRemainingWidth(int totalWidth, int requiredMinWidth,
                                              int minOrderId, int minClient, int minUser, int minDate,
                                              int minAmount, int minStatus, int minProducts)
        {
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
        }

        // ============ ФОРМАТИРОВАНИЕ ЯЧЕЕК ============

        /// <summary>
        /// Форматирование ячеек при отображении
        /// Раскрашивает статусы и форматирует суммы
        /// </summary>
        private void DataGridViewOrders_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Форматирование колонки статуса
            if (dataGridViewOrders.Columns[e.ColumnIndex].Name == "Status" && e.RowIndex >= 0)
            {
                FormatStatusCell(e);
            }
            // Форматирование колонки суммы
            else if (dataGridViewOrders.Columns[e.ColumnIndex].Name == "TotalAmount" && e.RowIndex >= 0)
            {
                FormatAmountCell(e);
            }
        }

        /// <summary>
        /// Форматирует ячейку статуса: устанавливает цвет фона и текста
        /// </summary>
        private void FormatStatusCell(DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewOrders.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
            {
                string status = dataGridViewOrders.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                e.CellStyle.BackColor = GetStatusColor(status);
                e.CellStyle.Font = new Font(dataGridViewOrders.Font, FontStyle.Bold);
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                e.CellStyle.SelectionBackColor = GetStatusColor(status); // Чтобы при выделении цвет не терялся
                e.CellStyle.SelectionForeColor = GetStatusTextColor(status);
            }
        }

        /// <summary>
        /// Форматирует ячейку суммы: выделяет фон и форматирует число
        /// </summary>
        private void FormatAmountCell(DataGridViewCellFormattingEventArgs e)
        {
            e.CellStyle.BackColor = Color.FromArgb(240, 255, 240); // Очень светло-зеленый
            e.CellStyle.Font = new Font(dataGridViewOrders.Font, FontStyle.Bold);
            e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

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

        /// <summary>
        /// Возвращает цвет фона для статуса
        /// </summary>
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

        /// <summary>
        /// Возвращает цвет текста для статуса
        /// </summary>
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

        #region Фильтрация ввода в поле поиска

        /// <summary>
        /// Фильтрация ввода в поле поиска - разрешаем только буквы, цифры, пробел и дефис
        /// </summary>
        private void TxtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем backspace (управляющие символы)
            if (char.IsControl(e.KeyChar))
                return;

            // Разрешенные символы: буквы, цифры, пробел, дефис
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

                // Загружаем данные с отфильтрованным текстом
                LoadData();
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

        #endregion

        // ============ ЗАГРУЗКА И ФИЛЬТРАЦИЯ ДАННЫХ ============

        /// <summary>
        /// Загружает данные с применением текущих фильтров
        /// </summary>
        private void LoadData()
        {
            try
            {
                if (_filterManager == null || dataGridViewOrders == null)
                    return;

                // Получаем значения фильтров
                string searchText = txtSearch?.Text ?? "";
                string userFilter = cmbUserFilter.SelectedItem?.ToString() ?? "Все продавцы";
                string statusFilter = cmbStatusFilter.SelectedItem?.ToString() ?? "Все статусы";
                DateTime fromDate = dtpFromDate.Value;
                DateTime toDate = dtpToDate.Value;

                // Автоматическая коррекция дат (если "от" больше "до")
                if (fromDate > toDate)
                {
                    DateTime temp = fromDate;
                    fromDate = toDate;
                    toDate = temp;
                    dtpFromDate.Value = fromDate;
                    dtpToDate.Value = toDate;
                }

                // Определение параметров сортировки
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

                // Отображение данных в таблице
                DisplayOrders(orders);

                // Обновление статистики
                UpdateStatistics(orders);

                // Подгонка ширины колонок после загрузки
                AdjustColumnWidths();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка загрузки", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Отображает список заказов в таблице
        /// </summary>
        private void DisplayOrders(List<OrderData> orders)
        {
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
        }

        /// <summary>
        /// Обновляет статистику (количество записей и общую сумму)
        /// </summary>
        private void UpdateStatistics(List<OrderData> orders)
        {
            if (lblRecordCount != null)
                lblRecordCount.Text = $"Найдено заказов: {orders.Count}";

            if (lblTotalSum != null)
            {
                decimal totalSum = orders.Sum(o => o.TotalAmount);
                lblTotalSum.Text = $"Общая сумма: {totalSum:N2} ₽";
            }
        }

        // ============ ОБРАБОТЧИКИ ИЗМЕНЕНИЯ ФИЛЬТРОВ ============

        private void dtpFromDate_ValueChanged(object sender, EventArgs e)
        {
            if (dtpFromDate.Value > dtpToDate.Value)
            {
                dtpToDate.Value = dtpFromDate.Value;
            }
            LoadData();
        }

        private void dtpToDate_ValueChanged(object sender, EventArgs e)
        {
            if (dtpToDate.Value < dtpFromDate.Value)
            {
                dtpFromDate.Value = dtpToDate.Value;
            }
            LoadData();
        }

        private void cmbUserFilter_SelectedIndexChanged(object sender, EventArgs e) => LoadData();
        private void cmbStatusFilter_SelectedIndexChanged(object sender, EventArgs e) => LoadData();
        private void cmbSort_SelectedIndexChanged(object sender, EventArgs e) => LoadData();

        /// <summary>
        /// Сбрасывает все фильтры к значениям по умолчанию
        /// </summary>
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
                    DateTime defaultFrom = dateRange.MaxDate.AddMonths(-1);
                    if (defaultFrom < dateRange.MinDate)
                    {
                        defaultFrom = dateRange.MinDate;
                    }

                    dtpFromDate.Value = defaultFrom;
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

        // ============ ЭКСПОРТ В EXCEL ============

        /// <summary>
        /// Экспортирует текущие данные в Excel с форматированием
        /// </summary>
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

                // Диалог сохранения файла
                string filePath = GetExcelSavePath();
                if (string.IsNullOrEmpty(filePath))
                    return;

                // Создаем Excel приложение
                excelApp = new Excel.Application();
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                workbook = excelApp.Workbooks.Add();
                worksheet = workbook.ActiveSheet;

                if (worksheet == null)
                    throw new Exception("Не удалось создать лист Excel");

                // Настройка страницы
                ConfigureExcelPage(worksheet, excelApp);

                // Цветовая гамма для оформления
                Color accentColor = Color.LimeGreen;
                Color lightGreen = Color.GreenYellow;
                Color veryLightGreen = Color.FromArgb(240, 255, 240);

                // Создание отчета
                CreateExcelHeader(worksheet, lightGreen);
                int currentRow = CreateExcelFilterInfo(worksheet, 3, lightGreen);
                currentRow = CreateExcelColumnHeaders(worksheet, currentRow + 2, accentColor);
                decimal totalSum = CreateExcelData(worksheet, currentRow, veryLightGreen);
                CreateExcelFooter(worksheet, currentRow + dataGridViewOrders.Rows.Count, totalSum, lightGreen);

                // Форматирование и сохранение
                FormatExcelSheet(worksheet);
                SaveExcelFile(workbook, filePath, totalSum);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Освобождение ресурсов Excel
                ReleaseExcelResources(excelApp, workbook, worksheet, range);
            }
        }

        /// <summary>
        /// Получает путь для сохранения файла Excel
        /// </summary>
        private string GetExcelSavePath()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Сохранить отчет по заказам",
                FileName = $"Отчет_по_заказам_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                DefaultExt = "xlsx"
            };

            return saveFileDialog.ShowDialog() == DialogResult.OK ? saveFileDialog.FileName : null;
        }

        /// <summary>
        /// Настраивает параметры страницы Excel
        /// </summary>
        private void ConfigureExcelPage(Excel.Worksheet worksheet, Excel.Application excelApp)
        {
            worksheet.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
            worksheet.PageSetup.LeftMargin = excelApp.CentimetersToPoints(1);
            worksheet.PageSetup.RightMargin = excelApp.CentimetersToPoints(1);
            worksheet.PageSetup.TopMargin = excelApp.CentimetersToPoints(1.5);
            worksheet.PageSetup.BottomMargin = excelApp.CentimetersToPoints(1);
        }

        /// <summary>
        /// Создает заголовок отчета в Excel
        /// </summary>
        private void CreateExcelHeader(Excel.Worksheet worksheet, Color lightGreen)
        {
            Excel.Range range = worksheet.Range["A1", "G1"];
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
        }

        /// <summary>
        /// Создает информацию о примененных фильтрах в Excel
        /// </summary>
        private int CreateExcelFilterInfo(Excel.Worksheet worksheet, int startRow, Color lightGreen)
        {
            int currentRow = startRow;

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
            Excel.Range range = worksheet.Cells[currentRow, 2];
            range.Font.Bold = true;
            range.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.LimeGreen);
            Marshal.ReleaseComObject(range);

            return currentRow;
        }

        /// <summary>
        /// Создает заголовки колонок таблицы в Excel
        /// </summary>
        private int CreateExcelColumnHeaders(Excel.Worksheet worksheet, int startRow, Color accentColor)
        {
            string[] headers = { "№ Заказа", "Клиент", "Продавец", "Дата заказа", "Сумма", "Статус", "Товары" };
            int currentRow = startRow;

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[currentRow, i + 1] = headers[i];
                Excel.Range range = worksheet.Cells[currentRow, i + 1];
                range.Font.Bold = true;
                range.Font.Name = "Segoe UI";
                range.Font.Size = 11;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(accentColor);
                range.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.White);
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                Marshal.ReleaseComObject(range);
            }

            return currentRow;
        }

        /// <summary>
        /// Заполняет данными таблицу в Excel
        /// </summary>
        private decimal CreateExcelData(Excel.Worksheet worksheet, int startRow, Color veryLightGreen)
        {
            decimal totalSum = 0;
            int currentRow = startRow;

            for (int i = 0; i < dataGridViewOrders.Rows.Count; i++)
            {
                DataGridViewRow row = dataGridViewOrders.Rows[i];
                if (row.IsNewRow) continue;

                // Получаем значения из строки
                string orderId = row.Cells["OrderId"].Value?.ToString() ?? "";
                string clientName = row.Cells["ClientName"].Value?.ToString() ?? "";
                string userName = row.Cells["UserName"].Value?.ToString() ?? "";
                string orderDate = row.Cells["OrderDate"].Value?.ToString() ?? "";
                string status = row.Cells["Status"].Value?.ToString() ?? "";
                string products = row.Cells["Products"].Value?.ToString() ?? "";

                // Парсим сумму
                decimal amount = ParseAmount(row.Cells["TotalAmount"].Value?.ToString() ?? "0");
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
                FormatExcelDataRow(worksheet, currentRow, i, veryLightGreen, status, amount);

                currentRow++;
            }

            return totalSum;
        }

        /// <summary>
        /// Парсит сумму из строки
        /// </summary>
        private decimal ParseAmount(string amountStr)
        {
            amountStr = amountStr.Replace(" ₽", "").Replace(" ", "").Trim();
            decimal.TryParse(amountStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.CurrentCulture, out decimal amount);
            return amount;
        }

        /// <summary>
        /// Форматирует строку данных в Excel
        /// </summary>
        private void FormatExcelDataRow(Excel.Worksheet worksheet, int row, int dataIndex,
                                        Color veryLightGreen, string status, decimal amount)
        {
            int columnCount = 7;
            Excel.Range range;

            for (int j = 1; j <= columnCount; j++)
            {
                range = worksheet.Cells[row, j];

                // Выравнивание
                if (j == 1 || j == 4)
                    range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                else if (j == 5)
                    range.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                // Границы
                range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                range.Borders.Color = System.Drawing.ColorTranslator.ToOle(Color.LightGray);

                // Чередование фона
                if (dataIndex % 2 == 1)
                {
                    range.Interior.Color = System.Drawing.ColorTranslator.ToOle(veryLightGreen);
                }

                Marshal.ReleaseComObject(range);
            }

            // Специальное форматирование для статуса
            range = worksheet.Cells[row, 6];
            range.Font.Bold = true;
            range.Interior.Color = System.Drawing.ColorTranslator.ToOle(GetExcelStatusColor(status));
            range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            Marshal.ReleaseComObject(range);

            // Форматирование суммы
            range = worksheet.Cells[row, 5];
            range.NumberFormat = "#,##0.00";
            range.Font.Bold = true;
            Marshal.ReleaseComObject(range);

            // Форматирование даты
            range = worksheet.Cells[row, 4];
            range.NumberFormat = "dd.MM.yyyy HH:mm";
            Marshal.ReleaseComObject(range);
        }

        /// <summary>
        /// Создает итоговую строку в Excel
        /// </summary>
        private void CreateExcelFooter(Excel.Worksheet worksheet, int row, decimal totalSum, Color lightGreen)
        {
            Excel.Range range;

            // Объединяем ячейки для текста "ИТОГО"
            range = worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 4]];
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
            range = worksheet.Cells[row, 5];
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
            for (int j = 6; j <= 7; j++)
            {
                range = worksheet.Cells[row, j];
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightGreen);
                range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                Marshal.ReleaseComObject(range);
            }
        }

        /// <summary>
        /// Выполняет финальное форматирование листа Excel
        /// </summary>
        private void FormatExcelSheet(Excel.Worksheet worksheet)
        {
            Excel.Range range = worksheet.UsedRange;
            range.Columns.AutoFit();

            // Минимальная ширина для колонок
            if (worksheet.Columns[1].ColumnWidth < 10) worksheet.Columns[1].ColumnWidth = 10;
            if (worksheet.Columns[2].ColumnWidth < 15) worksheet.Columns[2].ColumnWidth = 15;
            if (worksheet.Columns[3].ColumnWidth < 12) worksheet.Columns[3].ColumnWidth = 12;
            if (worksheet.Columns[4].ColumnWidth < 16) worksheet.Columns[4].ColumnWidth = 16;
            if (worksheet.Columns[5].ColumnWidth < 12) worksheet.Columns[5].ColumnWidth = 12;
            if (worksheet.Columns[6].ColumnWidth < 12) worksheet.Columns[6].ColumnWidth = 12;
            if (worksheet.Columns[7].ColumnWidth < 40) worksheet.Columns[7].ColumnWidth = 40;
            worksheet.Columns[7].WrapText = true;

            // Включаем автофильтр
            int headerRow = 7;
            range = worksheet.Range[worksheet.Cells[headerRow, 1], worksheet.Cells[headerRow + dataGridViewOrders.Rows.Count, 7]];
            range.AutoFilter(1, Type.Missing, Excel.XlAutoFilterOperator.xlAnd, Type.Missing, true);

            Marshal.ReleaseComObject(range);
        }

        /// <summary>
        /// Сохраняет файл Excel и предлагает открыть его
        /// </summary>
        private void SaveExcelFile(Excel.Workbook workbook, string filePath, decimal totalSum)
        {
            workbook.SaveAs(filePath);
            workbook.Close(false);

            MessageBox.Show($"Отчет успешно создан!\n\n" +
                           $"Всего заказов: {dataGridViewOrders.Rows.Count}\n" +
                           $"Общая сумма: {totalSum:N2} ₽\n" +
                           $"Файл: {filePath}",
                           "Экспорт завершен",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Information);

            // Предлагаем открыть файл
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

        /// <summary>
        /// Освобождает ресурсы Excel
        /// </summary>
        private void ReleaseExcelResources(Excel.Application excelApp, Excel.Workbook workbook,
                                          Excel.Worksheet worksheet, Excel.Range range)
        {
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
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        // ============ ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ EXCEL ============

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
                if (DateTime.TryParse(dateString, out DateTime date)) return date;
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
                case "доставлен": return Color.FromArgb(198, 239, 206);
                case "отправлен": return Color.FromArgb(255, 235, 156);
                case "обработка": return Color.FromArgb(255, 199, 206);
                default: return Color.White;
            }
        }

        // ============ НАВИГАЦИЯ ============

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