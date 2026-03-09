using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ynivermag_bad
{
    public partial class AddClientForm : Form
    {
        public int NewClientId { get; private set; } = -1;
        private string _connection;
        public ClientModel NewClient { get; private set; }
        public int AddedClientId { get; private set; }
        private ShowAll _showForm; // Ссылка на форму ShowAll для обновления и проверок

        public AddClientForm(ShowAll showForm = null)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            _showForm = showForm;
            NewClient = new ClientModel();

            // Подписываемся на события для фильтрации ввода
            FirstNameTextBox.TextChanged += FirstNameTextBox_TextChanged;
            LastNameTextBox.TextChanged += LastNameTextBox_TextChanged;
            EmailTextBox.TextChanged += EmailTextBox_TextChanged;
            PhoneMaskedTextBox.TextChanged += PhoneMaskedTextBox_TextChanged;
            AddressTextBox.TextChanged += AddressTextBox_TextChanged;

            // Подписываемся на события валидации при потере фокуса
            PhoneMaskedTextBox.Leave += PhoneMaskedTextBox_Leave;
        }

        #region Вспомогательные методы для работы с телефоном

        /// <summary>
        /// Получает только цифры из текста
        /// </summary>
        private string GetPhoneDigits(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            return new string(text.Where(c => char.IsDigit(c)).ToArray());
        }

        /// <summary>
        /// Получает чистый номер телефона (только 10 цифр)
        /// </summary>
        private string GetCleanPhoneNumber()
        {
            string digits = GetPhoneDigits(PhoneMaskedTextBox.Text);

            // Если цифр 11 и первая 7 или 8, убираем первую
            if (digits.Length == 11 && (digits[0] == '7' || digits[0] == '8'))
            {
                digits = digits.Substring(1);
            }

            return digits;
        }

        /// <summary>
        /// Форматирование номера телефона для отображения
        /// </summary>
        private string FormatPhoneForDisplay(string phoneDigits)
        {
            if (phoneDigits.Length == 11 && (phoneDigits.StartsWith("7") || phoneDigits.StartsWith("8")))
            {
                return $"+7 ({phoneDigits.Substring(1, 3)}) {phoneDigits.Substring(4, 3)}-{phoneDigits.Substring(7, 2)}-{phoneDigits.Substring(9, 2)}";
            }
            else if (phoneDigits.Length == 10)
            {
                return $"+7 ({phoneDigits.Substring(0, 3)}) {phoneDigits.Substring(3, 3)}-{phoneDigits.Substring(6, 2)}-{phoneDigits.Substring(8, 2)}";
            }

            return phoneDigits;
        }

        /// <summary>
        /// Корректировка позиции курсора после форматирования телефона
        /// </summary>
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

        #region Фильтрация ввода (только русские буквы)

        /// <summary>
        /// Фильтрация ввода в поле имени (только русские буквы, дефис, пробел)
        /// </summary>
        private void FirstNameTextBox_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = FirstNameTextBox.SelectionStart;
            string filteredText = FilterToRussianLetters(FirstNameTextBox.Text);

            if (filteredText != FirstNameTextBox.Text)
            {
                FirstNameTextBox.Text = filteredText;
                FirstNameTextBox.SelectionStart = Math.Min(selectionStart, FirstNameTextBox.Text.Length);
            }
        }

        /// <summary>
        /// Фильтрация ввода в поле фамилии (только русские буквы, дефис, пробел)
        /// </summary>
        private void LastNameTextBox_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = LastNameTextBox.SelectionStart;
            string filteredText = FilterToRussianLetters(LastNameTextBox.Text);

            if (filteredText != LastNameTextBox.Text)
            {
                LastNameTextBox.Text = filteredText;
                LastNameTextBox.SelectionStart = Math.Min(selectionStart, LastNameTextBox.Text.Length);
            }
        }

        /// <summary>
        /// Фильтр только для русских букв, дефиса и пробела
        /// </summary>
        private string FilterToRussianLetters(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Диапазоны русских букв в Unicode:
            // А-Я: 0x0410-0x042F
            // а-я: 0x0430-0x044F
            // Ё: 0x0401
            // ё: 0x0451

            return new string(input.Where(c =>
                (c >= 'А' && c <= 'Я') ||   // Заглавные русские
                (c >= 'а' && c <= 'я') ||   // Строчные русские
                c == 'Ё' || c == 'ё' ||     // Буква Ё
                c == '-' ||                  // Дефис
                c == ' ').ToArray());        // Пробел
        }

        /// <summary>
        /// Альтернативный вариант с использованием char.IsLetter и проверкой диапазона
        /// </summary>
        private string FilterToRussianLettersAlt(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return new string(input.Where(c =>
            {
                // Проверяем, что это буква
                if (!char.IsLetter(c))
                    return c == '-' || c == ' '; // Разрешаем дефис и пробел

                // Получаем категорию Unicode
                var category = char.GetUnicodeCategory(c);

                // Проверяем, что это кириллица
                return category == System.Globalization.UnicodeCategory.UppercaseLetter ||
                       category == System.Globalization.UnicodeCategory.LowercaseLetter;
            }).ToArray());
        }

        /// <summary>
        /// Автоматическое форматирование номера телефона при вводе
        /// </summary>
        private void PhoneMaskedTextBox_TextChanged(object sender, EventArgs e)
        {
            int originalSelectionStart = PhoneMaskedTextBox.SelectionStart;
            string originalText = PhoneMaskedTextBox.Text;

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
                PhoneMaskedTextBox.Text = formattedText;
                int adjustedPosition = GetAdjustedCursorPosition(originalSelectionStart, originalText, formattedText);
                PhoneMaskedTextBox.SelectionStart = Math.Min(adjustedPosition, formattedText.Length);
            }

            // Проверка наличия неактивного клиента с таким телефоном
            if (!string.IsNullOrWhiteSpace(PhoneMaskedTextBox.Text))
            {
                CheckForInactiveClientHint();
            }
        }
        //

        /// <summary>
        /// Форматирование номера телефона
        /// </summary>
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
                else // Обычный 10-значный номер
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
        /// Фильтрация email (только латинские буквы, цифры и разрешенные символы)
        /// </summary>
        private void EmailTextBox_TextChanged(object sender, EventArgs e)
        {
            int cursorPosition = EmailTextBox.SelectionStart;
            string text = EmailTextBox.Text;

            // Фильтруем только допустимые символы для email
            string filteredText = FilterToEmailChars(text);

            // Приводим к нижнему регистру
            filteredText = filteredText.ToLower();

            if (filteredText != text)
            {
                EmailTextBox.Text = filteredText;
                // Корректируем позицию курсора
                EmailTextBox.SelectionStart = Math.Max(0, cursorPosition - (text.Length - filteredText.Length));
            }
        }

        /// <summary>
        /// Фильтр для email: ТОЛЬКО латинские буквы, цифры и разрешенные спецсимволы
        /// </summary>
        private string FilterToEmailChars(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Разрешенные символы для email (по RFC)
            char[] allowedSpecialChars = { '@', '.', '-', '_', '+', '!', '#', '$', '%', '&', '\'', '*', '/', '=', '?', '^', '`', '{', '|', '}', '~' };

            return new string(input.Where(c =>
            {
                // Латинские буквы (проверяем по ASCII диапазону)
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    return true;

                // Цифры
                if (c >= '0' && c <= '9')
                    return true;

                // Разрешенные спецсимволы
                if (allowedSpecialChars.Contains(c))
                    return true;

                // ВСЁ ОСТАЛЬНОЕ (включая русские буквы) - ЗАПРЕЩЕНО
                return false;
            }).ToArray());
        }

        /// <summary>
        /// Альтернативный простой вариант - если нужны только самые основные символы
        /// </summary>
        private string FilterToEmailCharsSimple(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return new string(input.Where(c =>
                (c >= 'a' && c <= 'z') ||   // строчные латинские
                (c >= 'A' && c <= 'Z') ||   // заглавные латинские
                (c >= '0' && c <= '9') ||   // цифры
                c == '@' ||                  // собачка
                c == '.' ||                  // точка
                c == '-' ||                  // дефис
                c == '_').ToArray());        // подчеркивание
        }


        /// <summary>
        /// Фильтрация адреса (буквы, цифры, пробелы, знаки препинания)
        /// </summary>
        private void AddressTextBox_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = AddressTextBox.SelectionStart;
            string filteredText = FilterToAddressChars(AddressTextBox.Text);

            if (filteredText != AddressTextBox.Text)
            {
                AddressTextBox.Text = filteredText;
                AddressTextBox.SelectionStart = Math.Min(selectionStart, AddressTextBox.Text.Length);
            }
        }

        /// <summary>
        /// Фильтр для адреса: буквы, цифры, пробелы, знаки препинания
        /// </summary>
        private string FilterToAddressChars(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            char[] allowedPunctuation = { '.', ',', '-', '/', '\\', ' ' };

            return new string(input.Where(c =>
                char.IsLetterOrDigit(c) ||
                allowedPunctuation.Contains(c)).ToArray());
        }

        #endregion

        #region Проверка существующего неактивного клиента

        /// <summary>
        /// Проверка наличия неактивного клиента с введенным номером телефона
        /// </summary>
        private void CheckForInactiveClientHint()
        {
            try
            {
                string phoneDigits = GetPhoneDigits(PhoneMaskedTextBox.Text);

                if (string.IsNullOrWhiteSpace(phoneDigits) || phoneDigits.Length < 10)
                    return;

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"SELECT client_id, last_name, first_name, isActive
                                    FROM client 
                                    WHERE phone LIKE @Phone AND isActive = 0";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Phone", $"%{phoneDigits}");

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Показываем подсказку
                            string lastName = reader["last_name"].ToString();
                            string firstName = reader["first_name"].ToString();

                            // Изменяем цвет фона для подсказки
                            PhoneMaskedTextBox.BackColor = Color.LightYellow;

                            // Можно добавить ToolTip
                            toolTip1.SetToolTip(PhoneMaskedTextBox,
                                $"Найден неактивный клиент: {lastName} {firstName}. Можно восстановить его через форму управления клиентами.");
                        }
                        else
                        {
                            PhoneMaskedTextBox.BackColor = SystemColors.Window;
                            toolTip1.SetToolTip(PhoneMaskedTextBox, "");
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки при проверке подсказки
            }
        }

        #endregion

        #region Обработчики событий

        private void PhoneMaskedTextBox_Leave(object sender, EventArgs e)
        {
            // Если поле пустое или содержит только форматирование, очищаем его
            string digits = GetPhoneDigits(PhoneMaskedTextBox.Text);
            if (string.IsNullOrWhiteSpace(digits))
            {
                PhoneMaskedTextBox.Text = "";
            }
        }

        private void AddClient_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveClientData();
                if (AddClientToDatabase())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

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

        private bool HasUnsavedChanges()
        {
            return !string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                   !string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                   !string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                   !string.IsNullOrWhiteSpace(PhoneMaskedTextBox.Text) ||
                   !string.IsNullOrWhiteSpace(AddressTextBox.Text);
        }

        #endregion

        #region Валидация перед сохранением

        private bool ValidateData()
        {
            // Собираем все ошибки в список
            List<string> errors = new List<string>();

            // Проверка имени
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                errors.Add("Введите имя клиента");
                FirstNameTextBox.BackColor = Color.LightPink;
            }
            else if (FirstNameTextBox.Text.Length < 2)
            {
                errors.Add("Имя должно содержать минимум 2 символа");
                FirstNameTextBox.BackColor = Color.LightPink;
            }

            // Проверка фамилии
            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                errors.Add("Введите фамилию клиента");
                LastNameTextBox.BackColor = Color.LightPink;
            }
            else if (LastNameTextBox.Text.Length < 2)
            {
                errors.Add("Фамилия должна содержать минимум 2 символа");
                LastNameTextBox.BackColor = Color.LightPink;
            }

            // Проверка email
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                errors.Add("Введите email клиента");
                EmailTextBox.BackColor = Color.LightPink;
            }
            else if (!IsValidEmail(EmailTextBox.Text.Trim()))
            {
                errors.Add("Введите корректный email адрес (например: name@domain.com)");
                EmailTextBox.BackColor = Color.LightPink;
            }
            else if (!IsEmailUnique(EmailTextBox.Text.Trim()))
            {
                errors.Add("Клиент с таким email уже существует");
                EmailTextBox.BackColor = Color.LightPink;
            }

            // Проверка телефона
            if (string.IsNullOrWhiteSpace(PhoneMaskedTextBox.Text))
            {
                errors.Add("Введите телефон клиента");
                PhoneMaskedTextBox.BackColor = Color.LightPink;
            }
            else
            {
                // Получаем только цифры
                string digits = GetPhoneDigits(PhoneMaskedTextBox.Text);

                // Проверяем, есть ли цифры
                if (digits.Length > 0)
                {
                    // Проверяем количество цифр (должно быть 10 или 11)
                    if (digits.Length < 10)
                    {
                        errors.Add("Номер телефона должен содержать минимум 10 цифр");
                        PhoneMaskedTextBox.BackColor = Color.LightPink;
                    }
                    else
                    {
                        // Проверка уникальности телефона среди активных клиентов
                        string cleanNumber = GetCleanPhoneNumber();
                        if (!string.IsNullOrWhiteSpace(cleanNumber) && cleanNumber.Length == 10 && IsActiveClientExists(cleanNumber))
                        {
                            errors.Add("Клиент с таким номером телефона уже существует и активен");
                            PhoneMaskedTextBox.BackColor = Color.LightPink;
                        }
                        else
                        {
                            PhoneMaskedTextBox.BackColor = Color.LightGreen;
                        }
                    }
                }
                else
                {
                    errors.Add("Введите номер телефона");
                    PhoneMaskedTextBox.BackColor = Color.LightPink;
                }
            }

            // Проверка адреса (если заполнен)
            if (!string.IsNullOrWhiteSpace(AddressTextBox.Text) && AddressTextBox.Text.Length < 5)
            {
                errors.Add("Адрес должен содержать минимум 5 символов");
                AddressTextBox.BackColor = Color.LightPink;
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

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            if (email.Length > 100) return false;

            try
            {
                // Базовая проверка наличия @ и точки
                if (!email.Contains('@') || !email.Contains('.')) return false;

                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsEmailUnique(string email)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM client WHERE email = @Email";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        long count = Convert.ToInt64(cmd.ExecuteScalar());
                        return count == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки email: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool IsActiveClientExists(string phoneDigits)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM client WHERE phone LIKE @Phone AND isActive = 1";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Phone", $"%{phoneDigits}");

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки телефона: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true; // При ошибке блокируем добавление для безопасности
            }
        }

        #endregion

        #region Сохранение данных

        private void SaveClientData()
        {
            NewClient.first_name = CapitalizeName(FirstNameTextBox.Text.Trim());
            NewClient.last_name = CapitalizeName(LastNameTextBox.Text.Trim());
            NewClient.email = EmailTextBox.Text.Trim().ToLower();

            // Обработка телефона
            if (!string.IsNullOrWhiteSpace(PhoneMaskedTextBox.Text))
            {
                string cleanNumber = GetCleanPhoneNumber();
                if (!string.IsNullOrWhiteSpace(cleanNumber) && cleanNumber.Length == 10)
                {
                    NewClient.phone = $"+7{cleanNumber}";
                }
                else
                {
                    NewClient.phone = null;
                }
            }
            else
            {
                NewClient.phone = null;
            }

            NewClient.address = AddressTextBox.Text.Trim();
        }

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

        private bool AddClientToDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query = @"INSERT INTO client 
                    (email, first_name, last_name, phone, address, isActive) 
                    VALUES (@Email, @FirstName, @LastName, @Phone, @Address, 1);
                    SELECT LAST_INSERT_ID();";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", NewClient.email);
                        cmd.Parameters.AddWithValue("@FirstName", NewClient.first_name);
                        cmd.Parameters.AddWithValue("@LastName", NewClient.last_name);
                        cmd.Parameters.AddWithValue("@Phone",
                            string.IsNullOrWhiteSpace(NewClient.phone) ?
                            DBNull.Value : (object)NewClient.phone);
                        cmd.Parameters.AddWithValue("@Address",
                            string.IsNullOrWhiteSpace(NewClient.address) ?
                            DBNull.Value : (object)NewClient.address);

                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            AddedClientId = Convert.ToInt32(result);
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                if (sqlEx.Number == 1062)
                {
                    MessageBox.Show("Клиент с таким email уже существует", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка базы данных: {sqlEx.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении клиента: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion
    }
}