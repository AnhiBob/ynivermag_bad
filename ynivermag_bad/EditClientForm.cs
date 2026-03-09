using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ynivermag_bad
{
    public partial class EditClientForm : Form
    {
        private string _connection;
        public ClientModel Client { get; private set; }

        public EditClientForm(ClientModel client)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            Client = client;

            // Загрузка данных клиента
            LoadClientData();

            // Подписываемся на события для фильтрации ввода
            SubscribeToEvents();
        }

        #region Инициализация

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

        private void LoadClientData()
        {
            LastName.Text = Client.last_name;
            FirstName.Text = Client.first_name;

            // Форматируем телефон для отображения
            if (!string.IsNullOrEmpty(Client.phone))
            {
                string phoneDigits = GetPhoneDigits(Client.phone);
                if (phoneDigits.Length == 11 && (phoneDigits[0] == '7' || phoneDigits[0] == '8'))
                {
                    phoneDigits = phoneDigits.Substring(1);
                }

                // Применяем форматирование
                string formattedPhone = FormatPhoneNumber(phoneDigits);
                Phone.Text = formattedPhone;
            }

            Email.Text = Client.email;
            Address.Text = Client.address;
        }

        #endregion

        #region Вспомогательные методы для работы с телефоном

        private string GetPhoneDigits(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            return new string(text.Where(char.IsDigit).ToArray());
        }

        private string GetCleanPhoneNumber()
        {
            string digits = GetPhoneDigits(Phone.Text);

            // Если цифр 11 и первая 7 или 8, убираем первую
            if (digits.Length == 11 && (digits[0] == '7' || digits[0] == '8'))
            {
                digits = digits.Substring(1);
            }

            return digits;
        }

        private bool IsPhoneMaskCompleted()
        {
            if (string.IsNullOrWhiteSpace(Phone.Text))
                return false;

            string digits = GetPhoneDigits(Phone.Text);
            return digits.Length == 10;
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

        #region Фильтрация ввода (как в примере)

        /// <summary>
        /// Фильтрация ввода в поле фамилии (только русские буквы, дефис, пробел)
        /// </summary>
        private void LastName_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = LastName.SelectionStart;
            string filteredText = FilterToRussianLetters(LastName.Text);

            if (filteredText != LastName.Text)
            {
                LastName.Text = filteredText;
                LastName.SelectionStart = Math.Min(selectionStart, LastName.Text.Length);
            }
        }

        /// <summary>
        /// Фильтрация ввода в поле имени (только русские буквы, дефис, пробел)
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
        /// Фильтр только для русских букв, дефиса и пробела
        /// </summary>
        private string FilterToRussianLetters(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return new string(input.Where(c =>
                (c >= 'А' && c <= 'Я') ||   // Заглавные русские
                (c >= 'а' && c <= 'я') ||   // Строчные русские
                c == 'Ё' || c == 'ё' ||     // Буква Ё
                c == '-' ||                  // Дефис
                c == ' ').ToArray());        // Пробел
        }

        /// <summary>
        /// Автоматическое форматирование номера телефона при вводе
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
        /// Фильтрация email (только допустимые символы и автоматический lower case)
        /// </summary>
        private void Email_TextChanged(object sender, EventArgs e)
        {
            int cursorPosition = Email.SelectionStart;
            string text = Email.Text;

            // Убираем пробелы
            string filteredText = text.Replace(" ", "");

            // Приводим к нижнему регистру
            filteredText = filteredText.ToLower();

            if (filteredText != text)
            {
                Email.Text = filteredText;
                Email.SelectionStart = Math.Max(0, cursorPosition - (text.Length - filteredText.Length));
            }
        }

        /// <summary>
        /// Фильтрация адреса (буквы, цифры, пробелы, знаки препинания)
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
                string phoneDigits = GetPhoneDigits(Phone.Text);

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
                            // Изменяем цвет фона для подсказки
                            Phone.BackColor = Color.LightYellow;

                            toolTip1.SetToolTip(Phone,
                                $"Найден неактивный клиент с таким телефоном. Можно восстановить его через форму управления клиентами.");
                        }
                        else
                        {
                            Phone.BackColor = SystemColors.Window;
                            toolTip1.SetToolTip(Phone, "");
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

        #region Валидация перед сохранением

        private bool ValidateData()
        {
            List<string> errors = new List<string>();

            // Проверка фамилии
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

            // Проверка имени
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

            // Проверка телефона
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

            // Проверка email (необязательное поле)
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

            // Проверка адреса (необязательное поле)
            if (!string.IsNullOrWhiteSpace(Address.Text) && Address.Text.Length < 5)
            {
                errors.Add("Адрес должен содержать минимум 5 символов");
                Address.BackColor = Color.LightPink;
            }

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

        private void EditClient_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveClientData();
                DialogResult = DialogResult.OK;
                Close();
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
            return LastName.Text != Client.last_name ||
                   FirstName.Text != Client.first_name ||
                   Phone.Text != Client.phone ||
                   Email.Text != Client.email ||
                   Address.Text != Client.address;
        }

        private void Phone_Leave(object sender, EventArgs e)
        {
            // Если поле пустое или содержит только форматирование, очищаем его
            string digits = GetPhoneDigits(Phone.Text);
            if (string.IsNullOrWhiteSpace(digits))
            {
                Phone.Text = "";
            }
        }

        private void LastName_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LastName.Text))
            {
                LastName.Text = CapitalizeName(LastName.Text);
            }
        }

        private void FirstName_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FirstName.Text))
            {
                FirstName.Text = CapitalizeName(FirstName.Text);
            }
        }

        #endregion
    }
}