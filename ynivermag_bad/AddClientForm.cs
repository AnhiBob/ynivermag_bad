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
    /// <summary>
    /// Форма для добавления нового клиента в систему.
    /// Обеспечивает ввод и валидацию всех необходимых данных о клиенте:
    /// - ФИО (только русские буквы)
    /// - Email (только латиница, проверка уникальности)
    /// - Телефон (автоформатирование, проверка уникальности)
    /// - Адрес (свободный ввод с ограничениями)
    /// </summary>
    public partial class AddClientForm : Form
    {
        /// <summary>
        /// ID только что добавленного клиента (возвращается после успешного сохранения)
        /// </summary>
        public int NewClientId { get; private set; } = -1;

        /// <summary>
        /// Строка подключения к базе данных
        /// </summary>
        private string _connection;

        /// <summary>
        /// Модель данных нового клиента
        /// </summary>
        public ClientModel NewClient { get; private set; }

        /// <summary>
        /// ID добавленного клиента (альтернативное свойство)
        /// </summary>
        public int AddedClientId { get; private set; }

        /// <summary>
        /// Ссылка на родительскую форму ShowAll для обновления данных
        /// </summary>
        private ShowAll _showForm;

        /// <summary>
        /// Конструктор формы добавления клиента
        /// </summary>
        /// <param name="showForm">Ссылка на форму ShowAll (может быть null)</param>
        public AddClientForm(ShowAll showForm = null)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            _showForm = showForm;
            NewClient = new ClientModel();

            // Подписываемся на события для фильтрации ввода в реальном времени
            // Это позволяет сразу отсеивать недопустимые символы
            FirstNameTextBox.TextChanged += FirstNameTextBox_TextChanged;
            LastNameTextBox.TextChanged += LastNameTextBox_TextChanged;
            EmailTextBox.TextChanged += EmailTextBox_TextChanged;
            PhoneMaskedTextBox.TextChanged += PhoneMaskedTextBox_TextChanged;
            AddressTextBox.TextChanged += AddressTextBox_TextChanged;

            // Подписываемся на события валидации при потере фокуса
            // Нужно для финального форматирования (например, заглавные буквы в имени)
            PhoneMaskedTextBox.Leave += PhoneMaskedTextBox_Leave;
        }

        #region Вспомогательные методы для работы с телефоном

        /// <summary>
        /// Извлекает только цифры из строки, отбрасывая все остальные символы
        /// </summary>
        /// <param name="text">Исходный текст (может содержать форматирование)</param>
        /// <returns>Строка, содержащая только цифры</returns>
        /// <example>
        /// Вход: "+7 (123) 456-78-90" -> Выход: "71234567890"
        /// </example>
        private string GetPhoneDigits(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            return new string(text.Where(c => char.IsDigit(c)).ToArray());
        }

        /// <summary>
        /// Получает чистый 10-значный номер телефона (без кода страны)
        /// </summary>
        /// <returns>10 цифр номера или пустая строка, если номер некорректен</returns>
        /// <remarks>
        /// Если введено 11 цифр и первая 7 или 8 (код России), то первая цифра отбрасывается.
        /// Пример: "+7 (912) 345-67-89" -> "9123456789"
        /// </remarks>
        private string GetCleanPhoneNumber()
        {
            string digits = GetPhoneDigits(PhoneMaskedTextBox.Text);

            // Если цифр 11 и первая 7 или 8, убираем первую (код страны)
            if (digits.Length == 11 && (digits[0] == '7' || digits[0] == '8'))
            {
                digits = digits.Substring(1);
            }

            return digits;
        }

        /// <summary>
        /// Форматирует номер телефона для отображения в красивом виде
        /// </summary>
        /// <param name="phoneDigits">Цифры номера (10 или 11 цифр)</param>
        /// <returns>Отформатированный номер телефона</returns>
        /// <example>
        /// Вход: "9123456789" -> Выход: "+7 (912) 345-67-89"
        /// Вход: "89123456789" -> Выход: "+7 (912) 345-67-89"
        /// </example>
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
        /// Корректирует позицию курсора после автоматического форматирования телефона
        /// </summary>
        /// <param name="originalPosition">Исходная позиция курсора</param>
        /// <param name="oldText">Старый текст до форматирования</param>
        /// <param name="newText">Новый текст после форматирования</param>
        /// <returns>Скорректированная позиция курсора</returns>
        /// <remarks>
        /// Необходимо, чтобы при вводе цифр курсор не "прыгал" из-за добавленных форматирующих символов
        /// (скобок, дефисов, пробелов).
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

        #region Фильтрация ввода (только русские буквы)

        /// <summary>
        /// Обработчик изменения текста в поле имени.
        /// Фильтрует ввод, оставляя только русские буквы, дефис и пробел.
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
        /// Обработчик изменения текста в поле фамилии.
        /// Фильтрует ввод, оставляя только русские буквы, дефис и пробел.
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
        /// Фильтрует строку, оставляя только русские буквы, дефис и пробел.
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>Отфильтрованная строка, содержащая только разрешенные символы</returns>
        /// <remarks>
        /// Разрешены:
        /// - Заглавные русские буквы (А-Я)
        /// - Строчные русские буквы (а-я)
        /// - Буквы Ё и ё
        /// - Дефис (-)
        /// - Пробел ( )
        /// </remarks>
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
        /// Альтернативный метод фильтрации с использованием Unicode категорий.
        /// Более универсальный, но немного медленнее.
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
        /// Автоматическое форматирование номера телефона при вводе.
        /// Фильтрует только цифры и форматирует их в стандартный вид +7 (XXX) XXX-XX-XX.
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

        /// <summary>
        /// Форматирует последовательность цифр в номер телефона.
        /// </summary>
        /// <param name="digits">Цифры номера</param>
        /// <returns>Отформатированный номер</returns>
        /// <example>
        /// "7" -> "+7"
        /// "7912" -> "+7 (912"
        /// "7912345" -> "+7 (912) 345"
        /// "7912345678" -> "+7 (912) 345-67-8"
        /// "79123456789" -> "+7 (912) 345-67-89"
        /// </example>
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
        /// Фильтрация email: оставляет только латинские буквы, цифры и разрешенные спецсимволы.
        /// Также автоматически приводит к нижнему регистру.
        /// </summary>
        private void EmailTextBox_TextChanged(object sender, EventArgs e)
        {
            int cursorPosition = EmailTextBox.SelectionStart;
            string text = EmailTextBox.Text;

            // Фильтруем только допустимые символы для email
            string filteredText = FilterToEmailChars(text);

            // Приводим к нижнему регистру (email регистронезависим)
            filteredText = filteredText.ToLower();

            if (filteredText != text)
            {
                EmailTextBox.Text = filteredText;
                // Корректируем позицию курсора
                EmailTextBox.SelectionStart = Math.Max(0, cursorPosition - (text.Length - filteredText.Length));
            }
        }

        /// <summary>
        /// Фильтр для email: оставляет только латинские буквы, цифры и разрешенные спецсимволы.
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>Строка, содержащая только допустимые для email символы</returns>
        /// <remarks>
        /// Спецификация RFC разрешает множество символов в email.
        /// Мы используем упрощенный набор для практичности.
        /// </remarks>
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
        /// Упрощенный фильтр для email (только самые основные символы).
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
        /// Фильтрация адреса: оставляет буквы, цифры, пробелы и основные знаки препинания.
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
        /// Фильтр для адреса: оставляет буквы, цифры, пробелы и знаки препинания.
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>Отфильтрованная строка, безопасная для адреса</returns>
        private string FilterToAddressChars(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            char[] allowedPunctuation = { '.', ',', '-', '/', '\\', ' ' };

            return new string(input.Where(c =>
                char.IsLetterOrDigit(c) ||  // Любые буквы и цифры
                allowedPunctuation.Contains(c)).ToArray()); // Разрешенные знаки препинания
        }

        #endregion

        #region Проверка существующего неактивного клиента

        /// <summary>
        /// Проверяет, существует ли неактивный клиент с таким же номером телефона.
        /// Если найден, показывает подсказку и меняет цвет поля.
        /// </summary>
        /// <remarks>
        /// Это не блокирующая проверка, а информационная подсказка для пользователя.
        /// Позволяет избежать создания дубликатов, если клиент был ранее удален (помечен как неактивный).
        /// </remarks>
        private void CheckForInactiveClientHint()
        {
            try
            {
                string phoneDigits = GetPhoneDigits(PhoneMaskedTextBox.Text);

                // Для подсказки нужно минимум 10 цифр
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

                            // Добавляем ToolTip с информацией
                            toolTip1.SetToolTip(PhoneMaskedTextBox,
                                $"Найден неактивный клиент: {lastName} {firstName}. Можно восстановить его через форму управления клиентами.");
                        }
                        else
                        {
                            // Если неактивных клиентов нет, возвращаем обычный цвет
                            PhoneMaskedTextBox.BackColor = SystemColors.Window;
                            toolTip1.SetToolTip(PhoneMaskedTextBox, "");
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

        #region Обработчики событий

        /// <summary>
        /// Обработчик потери фокуса полем телефона.
        /// Очищает поле, если в нем нет цифр.
        /// </summary>
        private void PhoneMaskedTextBox_Leave(object sender, EventArgs e)
        {
            string digits = GetPhoneDigits(PhoneMaskedTextBox.Text);
            if (string.IsNullOrWhiteSpace(digits))
            {
                PhoneMaskedTextBox.Text = "";
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Добавить".
        /// Выполняет валидацию, сохранение данных и закрытие формы.
        /// </summary>
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

        /// <summary>
        /// Обработчик кнопки "Назад"/"Отмена".
        /// Проверяет наличие несохраненных изменений и предлагает их сохранить.
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
        /// Проверяет, есть ли несохраненные изменения в форме.
        /// </summary>
        /// <returns>true, если есть введенные данные</returns>
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

        /// <summary>
        /// Комплексная проверка всех полей перед сохранением.
        /// Собирает все ошибки в список и показывает их одной группой.
        /// </summary>
        /// <returns>true, если все поля заполнены корректно</returns>
        private bool ValidateData()
        {
            // Собираем все ошибки в список
            List<string> errors = new List<string>();

            // ===== ПРОВЕРКА ИМЕНИ =====
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

            // ===== ПРОВЕРКА ФАМИЛИИ =====
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

            // ===== ПРОВЕРКА EMAIL =====
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

            // ===== ПРОВЕРКА ТЕЛЕФОНА =====
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

            // ===== ПРОВЕРКА АДРЕСА (необязательное поле) =====
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

        /// <summary>
        /// Проверяет корректность email-адреса.
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
        /// Проверяет уникальность email в базе данных.
        /// </summary>
        /// <param name="email">Проверяемый email</param>
        /// <returns>true, если email уникален (не существует в БД)</returns>
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

        /// <summary>
        /// Проверяет существование активного клиента с указанным номером телефона.
        /// </summary>
        /// <param name="phoneDigits">10 цифр номера (без кода)</param>
        /// <returns>true, если клиент с таким телефоном уже существует и активен</returns>
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

        /// <summary>
        /// Сохраняет данные из полей формы в объект NewClient.
        /// Выполняет форматирование (заглавные буквы, приведение email к нижнему регистру).
        /// </summary>
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

        /// <summary>
        /// Приводит имя/фамилию к формату с заглавной первой буквой.
        /// Обрабатывает составные имена с дефисом и пробелами.
        /// </summary>
        /// <param name="name">Исходное имя</param>
        /// <returns>Отформатированное имя</returns>
        /// <example>
        /// "иван" -> "Иван"
        /// "иван петров" -> "Иван Петров"
        /// "анна-мария" -> "Анна-Мария"
        /// </example>
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

        /// <summary>
        /// Добавляет нового клиента в базу данных.
        /// </summary>
        /// <returns>true, если добавление прошло успешно</returns>
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
                // Обработка специфических ошибок MySQL
                if (sqlEx.Number == 1062) // Ошибка дубликата (unique constraint)
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

        private void FirstNameTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                // Делаем первую букву заглавной
                string name = FirstNameTextBox.Text.Trim();
                if (name.Length > 0)
                {
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    FirstNameTextBox.Text = name;
                }
            

        }
    }

        private void LastNameTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                // Делаем первую букву заглавной
                string name = LastNameTextBox.Text.Trim();
                if (name.Length > 0)
                {
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    LastNameTextBox.Text = name;
                }
            }
        }
    }
}
