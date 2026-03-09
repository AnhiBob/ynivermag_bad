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
    /// Форма для добавления нового пользователя в систему.
    /// Обеспечивает ввод и валидацию всех необходимых данных:
    /// - ФИО (только русские буквы, с авто-капитализацией)
    /// - Логин (только латиница, цифры, подчеркивание, проверка уникальности)
    /// - Пароль (минимальная длина 3 символа, хеширование)
    /// - Email (необязательное поле, проверка формата)
    /// - Роль (выбор из списка доступных ролей)
    /// </summary>
    public partial class AddUserForm : Form
    {
        /// <summary>
        /// Строка подключения к базе данных
        /// </summary>
        private string _connection;

        /// <summary>
        /// Модель данных нового пользователя
        /// </summary>
        public UserModel NewUser { get; private set; }

        /// <summary>
        /// Конструктор формы добавления пользователя
        /// Инициализирует компоненты, загружает роли и настраивает обработчики событий
        /// </summary>
        public AddUserForm()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            NewUser = new UserModel();

            // Настройка полей (отключение скрытия пароля для наглядности)
            ConfigurePasswordField();

            // Загрузка списка ролей из базы данных
            LoadRoles();

            // Подписываемся на события для фильтрации ввода
            // Фильтрация происходит в реальном времени при вводе текста
            SubscribeToEvents();
        }

        #region Инициализация

        /// <summary>
        /// Настраивает поле пароля - отключает скрытие символов
        /// (для удобства ввода, так как пароль виден только при создании)
        /// </summary>
        private void ConfigurePasswordField()
        {
            // Убираем скрытие пароля, чтобы пользователь видел, что вводит
            Password.UseSystemPasswordChar = false;
        }

        /// <summary>
        /// Подписывается на события изменения текста для всех полей ввода
        /// Это позволяет фильтровать ввод в реальном времени
        /// </summary>
        private void SubscribeToEvents()
        {
            LastName.TextChanged += LastName_TextChanged;
            FirstName.TextChanged += FirstName_TextChanged;
            Login.TextChanged += Login_TextChanged;
            Password.TextChanged += Password_TextChanged;
            Email.TextChanged += Email_TextChanged;
        }

        /// <summary>
        /// Загружает список активных ролей из базы данных в комбобокс
        /// </summary>
        private void LoadRoles()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT role_id, role_name FROM role WHERE isActive = 1 ORDER BY role_name";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    RoleCb.DataSource = dt;
                    RoleCb.DisplayMember = "role_name";
                    RoleCb.ValueMember = "role_id";

                    if (RoleCb.Items.Count > 0)
                    {
                        RoleCb.SelectedIndex = 0; // Выбираем первую роль по умолчанию
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
        /// <returns>Отфильтрованная строка, содержащая только разрешенные символы</returns>
        /// <remarks>
        /// Разрешены:
        /// - Заглавные русские буквы (А-Я)
        /// - Строчные русские буквы (а-я)
        /// - Буквы Ё и ё
        /// - Дефис (-) для двойных фамилий
        /// - Пробел ( ) для составных имен
        /// </remarks>
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
        /// Фильтрация ввода в поле логина
        /// Разрешены только латинские буквы, цифры и подчеркивание
        /// </summary>
        private void Login_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = Login.SelectionStart;
            string filteredText = FilterToLoginChars(Login.Text);

            if (filteredText != Login.Text)
            {
                Login.Text = filteredText;
                Login.SelectionStart = Math.Min(selectionStart, Login.Text.Length);
            }
        }

        /// <summary>
        /// Фильтр для логина: только латиница, цифры, подчеркивание
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>Отфильтрованная строка, содержащая только разрешенные символы</returns>
        /// <remarks>
        /// Логин должен состоять только из:
        /// - Латинских букв (a-z, A-Z)
        /// - Цифр (0-9)
        /// - Символа подчеркивания (_)
        /// </remarks>
        private string FilterToLoginChars(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return new string(input.Where(c =>
                (c >= 'a' && c <= 'z') ||   // строчные латинские
                (c >= 'A' && c <= 'Z') ||   // заглавные латинские
                (c >= '0' && c <= '9') ||   // цифры
                c == '_').ToArray());        // подчеркивание
        }

        /// <summary>
        /// Фильтрация ввода в поле пароля
        /// Пароль может содержать любые символы, ограничена только длина
        /// </summary>
        private void Password_TextChanged(object sender, EventArgs e)
        {
            // Ничего не фильтруем - пароль может содержать любые символы
            // Просто ограничиваем длину для безопасности
            if (Password.Text.Length > 50)
            {
                int selectionStart = Password.SelectionStart;
                Password.Text = Password.Text.Substring(0, 50);
                Password.SelectionStart = Math.Min(selectionStart, Password.Text.Length);
            }
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
                errors.Add("Введите фамилию пользователя");
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
                errors.Add("Введите имя пользователя");
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

            // ===== ПРОВЕРКА ЛОГИНА =====
            if (string.IsNullOrWhiteSpace(Login.Text))
            {
                errors.Add("Введите логин пользователя");
                Login.BackColor = Color.LightPink;
            }
            else if (Login.Text.Length < 3)
            {
                errors.Add("Логин должен содержать минимум 3 символа");
                Login.BackColor = Color.LightPink;
            }
            else if (Login.Text.Length > 20)
            {
                errors.Add("Логин должен содержать не более 20 символов");
                Login.BackColor = Color.LightPink;
            }
            else if (!IsLoginUnique(Login.Text))
            {
                errors.Add("Этот логин уже занят");
                Login.BackColor = Color.LightPink;
            }

            // ===== ПРОВЕРКА ПАРОЛЯ =====
            if (string.IsNullOrWhiteSpace(Password.Text))
            {
                errors.Add("Введите пароль");
                Password.BackColor = Color.LightPink;
            }
            else if (Password.Text.Length < 3)
            {
                errors.Add("Пароль должен содержать минимум 3 символа");
                Password.BackColor = Color.LightPink;
            }
            else if (Password.Text.Length > 50)
            {
                errors.Add("Пароль должен содержать не более 50 символов");
                Password.BackColor = Color.LightPink;
            }

            // ===== ПРОВЕРКА EMAIL (необязательное поле) =====
            if (!string.IsNullOrWhiteSpace(Email.Text))
            {
                if (!IsValidEmail(Email.Text))
                {
                    errors.Add("Введите корректный email адрес (например: name@domain.com)");
                    Email.BackColor = Color.LightPink;
                }
            }

            // ===== ПРОВЕРКА РОЛИ =====
            if (RoleCb.SelectedValue == null || RoleCb.SelectedValue == DBNull.Value)
            {
                errors.Add("Выберите роль");
                RoleCb.BackColor = Color.LightPink;
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
        /// Проверяет уникальность логина в базе данных
        /// </summary>
        /// <param name="login">Проверяемый логин</param>
        /// <returns>true, если логин уникален (не существует в БД)</returns>
        private bool IsLoginUnique(string login)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM user WHERE username = @Login";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Login", login);
                        long count = Convert.ToInt64(cmd.ExecuteScalar());
                        return count == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки логина: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        #region Сохранение данных

        /// <summary>
        /// Сохраняет данные из полей формы в объект NewUser
        /// Выполняет хеширование пароля и форматирование ФИО
        /// </summary>
        private void SaveUserData()
        {
            // Хешируем пароль перед сохранением
            string passwordHash = MySQLHelper.GetHash(Password.Text);

            NewUser.last_name = CapitalizeName(LastName.Text.Trim());
            NewUser.first_name = CapitalizeName(FirstName.Text.Trim());
            NewUser.username = Login.Text.Trim().ToLower();
            NewUser.email = string.IsNullOrWhiteSpace(Email.Text) ? null : Email.Text.Trim().ToLower();
            NewUser.password_hash = passwordHash;

            if (RoleCb.SelectedValue != null && RoleCb.SelectedValue != DBNull.Value)
            {
                NewUser.role_id = (int)RoleCb.SelectedValue;
            }
        }

        /// <summary>
        /// Приводит имя/фамилию к формату с заглавной первой буквой
        /// Обрабатывает составные имена с дефисом и пробелами
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
        /// Добавляет нового пользователя в базу данных
        /// </summary>
        /// <returns>true, если добавление прошло успешно</returns>
        private bool AddUserToDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query = @"INSERT INTO user 
                            (username, password_hash, email, first_name, last_name, role_id, isActive) 
                            VALUES (@Username, @PasswordHash, @Email, @FirstName, @LastName, @RoleId, 1)";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Username", NewUser.username);
                        cmd.Parameters.AddWithValue("@PasswordHash", NewUser.password_hash);
                        cmd.Parameters.AddWithValue("@Email",
                            string.IsNullOrWhiteSpace(NewUser.email) ? DBNull.Value : (object)NewUser.email);
                        cmd.Parameters.AddWithValue("@FirstName", NewUser.first_name);
                        cmd.Parameters.AddWithValue("@LastName", NewUser.last_name);
                        cmd.Parameters.AddWithValue("@RoleId", NewUser.role_id);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("✅ Пользователь успешно добавлен!", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Не удалось добавить пользователя", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                // Обработка специфических ошибок MySQL
                if (sqlEx.Number == 1062) // Ошибка дубликата (unique constraint)
                {
                    MessageBox.Show("Пользователь с таким логином или email уже существует", "Ошибка",
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
                MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        #region Обработчики событий

        /// <summary>
        /// Обработчик нажатия кнопки "Добавить"
        /// Выполняет валидацию, сохранение и закрытие формы
        /// </summary>
        private void AddUser_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveUserData();
                if (AddUserToDatabase())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
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
        /// </summary>
        /// <returns>true, если есть введенные данные</returns>
        private bool HasUnsavedChanges()
        {
            return !string.IsNullOrWhiteSpace(LastName.Text) ||
                   !string.IsNullOrWhiteSpace(FirstName.Text) ||
                   !string.IsNullOrWhiteSpace(Login.Text) ||
                   !string.IsNullOrWhiteSpace(Password.Text) ||
                   !string.IsNullOrWhiteSpace(Email.Text);
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