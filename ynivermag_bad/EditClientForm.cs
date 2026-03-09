using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ynivermag_bad
{
    /// <summary>
    /// Форма для редактирования существующего клиента.
    /// Позволяет изменять все данные клиента с валидацией:
    /// - ФИО (только русские буквы, авто-капитализация)
    /// - Телефон (автоформатирование, проверка уникальности)
    /// - Email (проверка формата и уникальности)
    /// - Адрес (свободный ввод с ограничениями)
    /// </summary>
    public partial class EditClientForm : Form
    {
        /// <summary>
        /// Строка подключения к базе данных
        /// </summary>
        private string _connection;

        /// <summary>
        /// Модель данных редактируемого клиента
        /// </summary>
        public ClientModel Client { get; private set; }

        /// <summary>
        /// Конструктор формы редактирования клиента
        /// </summary>
        /// <param name="client">Модель клиента с данными для редактирования</param>
        public EditClientForm(ClientModel client)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            Client = client;

            // Загрузка данных клиента в поля формы
            LoadClientData();

            // Подписываемся на события для фильтрации ввода
            // Фильтрация происходит в реальном времени при вводе текста
            SubscribeToEvents();
        }

        #region Инициализация

        /// <summary>
        /// Подписывается на события изменения текста для всех полей ввода
        /// и события валидации при потере фокуса
        /// </summary>
        private void SubscribeToEvents()
        {
            LastName.TextChanged += LastName_TextChanged;
            FirstName.TextChanged += FirstName_TextChanged;
            Phone.TextChanged += Phone_TextChanged;
            Email.TextChanged += Email_TextChanged;
            Address.TextChanged += Address_TextChanged;

            // Подписка на события валидации при потере фокуса
            Phone.Leave += Phone_Leave;
            LastName.Validating += LastName_Validating;
            FirstName.Validating += FirstName_Validating;
        }

        /// <summary>
        /// Загружает данные клиента в поля формы
        /// Выполняет форматирование телефона для удобного отображения
        /// </summary>
        private void LoadClientData()
        {
            LastName.Text = Client.last_name;
            FirstName.Text = Client.first_name;

            // Форматируем телефон для отображения в человекочитаемом виде
            if (!string.IsNullOrEmpty(Client.phone))
            {
                string phoneDigits = GetPhoneDigits(Client.phone);
                // Если номер содержит код страны (11 цифр), убираем первую цифру
                if (phoneDigits.Length == 11 && (phoneDigits[0] == '7' || phoneDigits[0] == '8'))
                {
                    phoneDigits = phoneDigits.Substring(1);
                }

                // Применяем форматирование +7 (XXX) XXX-XX-XX
                string formattedPhone = FormatPhoneNumber(phoneDigits);
                Phone.Text = formattedPhone;
            }

            Email.Text = Client.email;
            Address.Text = Client.address;
        }

        #endregion

        #region Вспомогательные методы для работы с телефоном

        /// <summary>
        /// Извлекает только цифры из строки, отбрасывая все форматирующие символы
        /// </summary>
        /// <param name="text">Исходный текст с возможным форматированием</param>
        /// <returns>Строка, содержащая только цифры</returns>
        private string GetPhoneDigits(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            return new string(text.Where(char.IsDigit).ToArray());
        }

        /// <summary>
        /// Получает чистый 10-значный номер телефона без кода страны
        /// </summary>
        /// <returns>10 цифр номера или пустая строка</returns>
        /// <remarks>
        /// Если введен номер с кодом страны (11 цифр, начинающийся с 7 или 8),
        /// код страны отбрасывается, оставляя 10 цифр
        /// </remarks>
        private string GetCleanPhoneNumber()
        {
            string digits = GetPhoneDigits(Phone.Text);

            // Если цифр 11 и первая 7 или 8, убираем первую (код страны)
            if (digits.Length == 11 && (digits[0] == '7' || digits[0] == '8'))
            {
                digits = digits.Substring(1);
            }

            return digits;
        }

        /// <summary>
        /// Проверяет, полностью ли заполнен номер телефона (10 цифр)
        /// </summary>
        private bool IsPhoneMaskCompleted()
        {
            if (string.IsNullOrWhiteSpace(Phone.Text))
                return false;

            string digits = GetPhoneDigits(Phone.Text);
            return digits.Length == 10;
        }

        /// <summary>
        /// Корректирует позицию курсора после автоматического форматирования телефона
        /// </summary>
        /// <param name="originalPosition">Исходная позиция курсора</param>
        /// <param name="oldText">Старый текст до форматирования</param>
        /// <param name="newText">Новый текст после форматирования</param>
        /// <returns>Скорректированная позиция курсора</returns>
        /// <remarks>
        /// Необходимо, чтобы при вводе цифр курсор не "прыгал" из-за добавленных 
        /// форматирующих символов (скобок, дефисов, пробелов)
        /// </remarks>
        private int GetAdjustedCursorPosition(int originalPosition, string oldText, string newText)
        {
            if (originalPosition >= oldText.Length)
                return newText.Length;

            int formatCharsBeforeCursor = 0;
            char[] formatChars = { '(', ')', ' ', '-', '+' };

            for (int i = 0; i < originalPosition && i < newText.Length; i++)
            {
                if (formatChars.Contains(newText[i]))
                {
                    formatCharsBeforeCursor++;
                }
            }

            return originalPosition + formatCharsBeforeCursor;
        }

        #endregion

        #region Фильтрация ввода

        /// <summary>
        /// Фильтрация ввода в поле фамилии
        /// Разрешены только русские буквы, дефис и пробел
        /// </summary>
        private void LastName_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = LastName.SelectionStart;
            string filteredText = FilterToRussianLetters(LastName.Text);

            if (filteredText != LastName.Text)
            {
                LastName.Text = filteredText;
                // Корректируем позицию курсора после фильтрации
                LastName.SelectionStart = Math.Min(selectionStart, LastName.Text.Length);
            }
        }

        /// <summary>
        /// Фильтрация ввода в поле имени
        /// Разрешены только русские буквы, дефис и пробел
        /// </summary>
        private void FirstName_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = FirstName.SelectionStart;
            string filteredText = FilterToRussianLetters(FirstName.Text);

            if (filteredText != FirstName.Text)
            {
                FirstName.Text = filteredText;
                FirstName.SelectionStart = Math.Min(selectionStart, FirstName.Text.Length);
            }
        }

        /// <summary>
        /// Фильтрует строку, оставляя только русские буквы, дефис и пробел
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>Отфильтрованная строка</returns>
        private string FilterToRussianLetters(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return new string(input.Where(c =>
                (c >= 'А' && c <= 'Я') ||   // Заглавные русские
                (c >= 'а' && c <= 'я') ||   // Строчные русские
                c == 'Ё' || c == 'ё' ||     // Буква Ё
                c == '-' ||                  // Дефис для двойных фамилий
                c == ' ').ToArray());        // Пробел для составных имен
        }

        /// <summary>
        /// Автоматическое форматирование номера телефона при вводе
        /// Фильтрует только цифры и форматирует их в стандартный вид +7 (XXX) XXX-XX-XX
        /// </summary>
        private void Phone_TextChanged(object sender, EventArgs e)
        {
            int originalSelectionStart = Phone.SelectionStart;
            string originalText = Phone.Text;

            // Фильтруем только цифры
            string filteredText = new string(originalText.Where(c => char.IsDigit(c)).ToArray());

            // Ограничиваем до 11 цифр (макс для российского номера)
            if (filteredText.Length > 11)
            {
                filteredText = filteredText.Substring(0, 11);
            }

            // Форматируем
            string formattedText = FormatPhoneNumber(filteredText);

            if (formattedText != originalText)
            {
                Phone.Text = formattedText;
                int adjustedPosition = GetAdjustedCursorPosition(originalSelectionStart, originalText, formattedText);
                Phone.SelectionStart = Math.Min(adjustedPosition, formattedText.Length);
            }

            // Проверка наличия неактивного клиента с таким телефоном
            if (!string.IsNullOrWhiteSpace(Phone.Text))
            {
                CheckForInactiveClientHint();
            }
        }

        /// <summary>
        /// Форматирует последовательность цифр в номер телефона
        /// </summary>
        /// <param name="digits">Цифры номера (10 или 11)</param>
        /// <returns>Отформатированный номер в виде +7 (XXX) XXX-XX-XX</returns>
        private string FormatPhoneNumber(string digits)
        {
            if (string.IsNullOrEmpty(digits))
                return "";

            // Если начинается с 7 или 8 (11 цифр)
            if (digits.Length >= 1)
            {
                if (digits[0] == '7' || digits[0] == '8')
                {
                    if (digits.Length == 1)
                        return $"+7";
                    else if (digits.Length <= 4)
                        return $"+7 ({digits.Substring(1)}";
                    else if (digits.Length <= 7)
                        return $"+7 ({digits.Substring(1, 3)}) {digits.Substring(4)}";
                    else if (digits.Length <= 9)
                        return $"+7 ({digits.Substring(1, 3)}) {digits.Substring(4, 3)}-{digits.Substring(7)}";
                    else
                        return $"+7 ({digits.Substring(1, 3)}) {digits.Substring(4, 3)}-{digits.Substring(7, 2)}-{digits.Substring(9)}";
                }
                else // Обычный 10-значный номер (без кода страны)
                {
                    if (digits.Length <= 3)
                        return $"+7 ({digits}";
                    else if (digits.Length <= 6)
                        return $"+7 ({digits.Substring(0, 3)}) {digits.Substring(3)}";
                    else if (digits.Length <= 8)
                        return $"+7 ({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6)}";
                    else
                        return $"+7 ({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6, 2)}-{digits.Substring(8)}";
                }
            }

            return digits;
        }

        /// <summary>
        /// Фильтрация email
        /// Удаляет пробелы и приводит к нижнему регистру
        /// </summary>
        private void Email_TextChanged(object sender, EventArgs e)
        {
            int cursorPosition = Email.SelectionStart;
            string text = Email.Text;

            // Убираем пробелы (email не может содержать пробелы)
            string filteredText = text.Replace(" ", "");

            // Приводим к нижнему регистру (email регистронезависим)
            filteredText = filteredText.ToLower();

            if (filteredText != text)
            {
                Email.Text = filteredText;
                // Корректируем позицию курсора после изменения текста
                Email.SelectionStart = Math.Max(0, cursorPosition - (text.Length - filteredText.Length));
            }
        }

        /// <summary>
        /// Фильтрация адреса
        /// Оставляет буквы, цифры, пробелы и основные знаки препинания
        /// </summary>
        private void Address_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = Address.SelectionStart;
            string filteredText = FilterToAddressChars(Address.Text);

            if (filteredText != Address.Text)
            {
                Address.Text = filteredText;
                Address.SelectionStart = Math.Min(selectionStart, Address.Text.Length);
            }
        }

        /// <summary>
        /// Фильтр для адреса: буквы, цифры, пробелы и разрешенные знаки препинания
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>Отфильтрованная строка</returns>
        private string FilterToAddressChars(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            char[] allowedPunctuation = { '.', ',', '-', '/', '\\', ' ' };

            return new string(input.Where(c =>
                char.IsLetterOrDigit(c) ||        // Любые буквы и цифры
                allowedPunctuation.Contains(c)).ToArray()); // Разрешенные знаки препинания
        }

        #endregion

        #region Проверка существующего неактивного клиента

        /// <summary>
        /// Проверяет, существует ли неактивный клиент с таким же номером телефона
        /// (исключая текущего редактируемого клиента)
        /// Показывает подсказку и меняет цвет поля при обнаружении
        /// </summary>
        private void CheckForInactiveClientHint()
        {
            try
            {
                string phoneDigits = GetPhoneDigits(Phone.Text);

                // Для подсказки нужно минимум 10 цифр
                if (string.IsNullOrWhiteSpace(phoneDigits) || phoneDigits.Length < 10)
                    return;

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"SELECT client_id, last_name, first_name, isActive
                                    FROM client 
                                    WHERE phone LIKE @Phone AND isActive = 0 AND client_id != @ClientId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Phone", $"%{phoneDigits}");
                    cmd.Parameters.AddWithValue("@ClientId", Client.client_id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Найден неактивный клиент - показываем подсказку
                            Phone.BackColor = Color.LightYellow;

                            toolTip1.SetToolTip(Phone,
                                $"Найден неактивный клиент с таким телефоном. Можно восстановить его через форму управления клиентами.");
                        }
                        else
                        {
                            // Неактивных клиентов нет - возвращаем обычный цвет
                            Phone.BackColor = SystemColors.Window;
                            toolTip1.SetToolTip(Phone, "");
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки при проверке подсказки - это не критично
            }
        }

        #endregion

        #region Валидация перед сохранением

        /// <summary>
        /// Комплексная проверка всех полей перед сохранением
        /// Собирает все ошибки в список и показывает их одной группой
        /// </summary>
        /// <returns>true, если все поля заполнены корректно</returns>
        private bool ValidateData()
        {
            List<string> errors = new List<string>();

            // ===== ПРОВЕРКА ФАМИЛИИ =====
            if (string.IsNullOrWhiteSpace(LastName.Text))
            {
                errors.Add("Введите фамилию клиента");
                LastName.BackColor = Color.LightPink;
            }
            else if (LastName.Text.Length < 2)
            {
                errors.Add("Фамилия должна содержать минимум 2 символа");
                LastName.BackColor = Color.LightPink;
            }
            else if (LastName.Text.Length > 50)
            {
                errors.Add("Фамилия должна содержать не более 50 символов");
                LastName.BackColor = Color.LightPink;
            }

            // ===== ПРОВЕРКА ИМЕНИ =====
            if (string.IsNullOrWhiteSpace(FirstName.Text))
            {
                errors.Add("Введите имя клиента");
                FirstName.BackColor = Color.LightPink;
            }
            else if (FirstName.Text.Length < 2)
            {
                errors.Add("Имя должно содержать минимум 2 символа");
                FirstName.BackColor = Color.LightPink;
            }
            else if (FirstName.Text.Length > 50)
            {
                errors.Add("Имя должно содержать не более 50 символов");
                FirstName.BackColor = Color.LightPink;
            }

            // ===== ПРОВЕРКА ТЕЛЕФОНА =====
            if (string.IsNullOrWhiteSpace(Phone.Text))
            {
                errors.Add("Введите телефон клиента");
                Phone.BackColor = Color.LightPink;
            }
            else
            {
                string digits = GetPhoneDigits(Phone.Text);

                if (digits.Length < 10)
                {
                    errors.Add("Номер телефона должен содержать минимум 10 цифр");
                    Phone.BackColor = Color.LightPink;
                }
                else
                {
                    string cleanNumber = GetCleanPhoneNumber();
                    if (!string.IsNullOrWhiteSpace(cleanNumber) && cleanNumber.Length == 10 && !IsPhoneUnique(cleanNumber))
                    {
                        errors.Add("Клиент с таким номером телефона уже существует");
                        Phone.BackColor = Color.LightPink;
                    }
                }
            }

            // ===== ПРОВЕРКА EMAIL (необязательное поле) =====
            if (!string.IsNullOrWhiteSpace(Email.Text))
            {
                if (!IsValidEmail(Email.Text))
                {
                    errors.Add("Введите корректный email адрес (например: name@domain.com)");
                    Email.BackColor = Color.LightPink;
                }
                else if (!IsEmailUnique())
                {
                    errors.Add("Клиент с таким email уже существует");
                    Email.BackColor = Color.LightPink;
                }
            }

            // ===== ПРОВЕРКА АДРЕСА (необязательное поле) =====
            if (!string.IsNullOrWhiteSpace(Address.Text) && Address.Text.Length < 5)
            {
                errors.Add("Адрес должен содержать минимум 5 символов");
                Address.BackColor = Color.LightPink;
            }

            // Если есть ошибки, показываем их все
            if (errors.Count > 0)
            {
                string errorMessage = "Пожалуйста, исправьте следующие ошибки:\n\n• " +
                                     string.Join("\n• ", errors);
                MessageBox.Show(errorMessage, "Ошибки валидации",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет корректность email-адреса
        /// </summary>
        /// <param name="email">Проверяемый email</param>
        /// <returns>true, если email корректен</returns>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            if (email.Length > 100) return false;

            try
            {
                // Базовая проверка наличия @ и точки
                if (!email.Contains('@') || !email.Contains('.')) return false;

                // Используем встроенный класс MailAddress для полной проверки
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверяет уникальность номера телефона (исключая текущего клиента)
        /// </summary>
        /// <param name="phoneDigits">10 цифр номера</param>
        /// <returns>true, если телефон уникален</returns>
        private bool IsPhoneUnique(string phoneDigits)
        {
            if (string.IsNullOrWhiteSpace(phoneDigits))
                return false;

            using (var connection = new MySqlConnection(_connection))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT COUNT(*) FROM client 
                            WHERE phone = @Phone AND client_id != @ClientId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Phone", $"+7{phoneDigits}");
                    cmd.Parameters.AddWithValue("@ClientId", Client.client_id);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка проверки телефона: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Проверяет уникальность email (исключая текущего клиента)
        /// </summary>
        /// <returns>true, если email уникален</returns>
        private bool IsEmailUnique()
        {
            if (string.IsNullOrWhiteSpace(Email.Text))
                return true;

            using (var connection = new MySqlConnection(_connection))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT COUNT(*) FROM client 
                            WHERE email = @Email AND client_id != @ClientId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Email", Email.Text.Trim());
                    cmd.Parameters.AddWithValue("@ClientId", Client.client_id);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка проверки email: {ex.Message}");
                    return false;
                }
            }
        }

        #endregion

        #region Сохранение данных

        /// <summary>
        /// Сохраняет данные из полей формы в объект Client
        /// Выполняет форматирование (заглавные буквы, приведение email к нижнему регистру)
        /// </summary>
        private void SaveClientData()
        {
            Client.last_name = CapitalizeName(LastName.Text.Trim());
            Client.first_name = CapitalizeName(FirstName.Text.Trim());
            Client.email = string.IsNullOrWhiteSpace(Email.Text) ? null : Email.Text.Trim().ToLower();

            // Сохраняем телефон в формате +7XXXXXXXXXX
            string phoneDigits = GetCleanPhoneNumber();
            Client.phone = string.IsNullOrWhiteSpace(phoneDigits) ? null : $"+7{phoneDigits}";

            Client.address = string.IsNullOrWhiteSpace(Address.Text) ? null : Address.Text.Trim();
        }

        /// <summary>
        /// Приводит имя/фамилию к формату с заглавной первой буквой
        /// Обрабатывает составные имена с дефисом и пробелами
        /// </summary>
        /// <param name="name">Исходное имя</param>
        /// <returns>Отформатированное имя</returns>
        private string CapitalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return name;

            string[] parts = name.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1).ToLower();
                }
            }

            string result = string.Join(" ", parts);
            if (name.Contains('-'))
            {
                result = result.Replace(" ", "-");
            }

            return result;
        }

        #endregion

        #region Обработчики событий

        /// <summary>
        /// Обработчик нажатия кнопки "Сохранить"
        /// Выполняет валидацию, сохранение и закрытие формы
        /// </summary>
        private void EditClient_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveClientData();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <summary>
        /// Обработчик кнопки "Назад"/"Отмена"
        /// Проверяет наличие несохраненных изменений
        /// </summary>
        private void Back_Click(object sender, EventArgs e)
        {
            if (HasUnsavedChanges())
            {
                var result = MessageBox.Show("У вас есть несохраненные изменения. Выйти?",
                                            "Подтверждение",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;
            }

            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Проверяет наличие несохраненных изменений в форме
        /// Сравнивает текущие значения полей с исходными данными клиента
        /// </summary>
        /// <returns>true, если есть изменения</returns>
        private bool HasUnsavedChanges()
        {
            return LastName.Text != Client.last_name ||
                   FirstName.Text != Client.first_name ||
                   Phone.Text != Client.phone ||
                   Email.Text != Client.email ||
                   Address.Text != Client.address;
        }

        /// <summary>
        /// Обработчик потери фокуса полем телефона
        /// Очищает поле, если в нем нет цифр
        /// </summary>
        private void Phone_Leave(object sender, EventArgs e)
        {
            string digits = GetPhoneDigits(Phone.Text);
            if (string.IsNullOrWhiteSpace(digits))
            {
                Phone.Text = "";
            }
        }

        /// <summary>
        /// Обработчик валидации поля фамилии
        /// Применяет форматирование с заглавной буквы
        /// </summary>
        private void LastName_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LastName.Text))
            {
                string name = LastName.Text.Trim();
                if (name.Length > 0)
                {
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    LastName.Text = name;
                }
            }
        }

        /// <summary>
        /// Обработчик валидации поля имени
        /// Применяет форматирование с заглавной буквы
        /// </summary>
        private void FirstName_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FirstName.Text))
            {
                string name = FirstName.Text.Trim();
                if (name.Length > 0)
                {
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    FirstName.Text = name;
                }
            }
        }

        #endregion
    }
}