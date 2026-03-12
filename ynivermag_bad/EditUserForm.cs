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
    /// Форма для редактирования существующего пользователя.
    /// Позволяет изменять все данные пользователя:
    /// - ФИО (только русские буквы, авто-капитализация)
    /// - Логин (только латиница, цифры, подчеркивание, точка, проверка уникальности)
    /// - Пароль (опционально, если нужно сменить)
    /// - Email (обязательное поле, проверка формата и уникальности)
    /// - Роль (выбор из списка, с защитой от изменения роли администратора)
    /// </summary>
    public partial class EditUserForm : Form
    {
        /// <summary>
        /// Строка подключения к базе данных
        /// </summary>
        private string _connection;

        /// <summary>
        /// Модель данных редактируемого пользователя
        /// </summary>
        public UserModel User { get; private set; }

        /// <summary>
        /// Флаг, указывающий, что форма находится в режиме редактирования
        /// </summary>
        public bool IsEditMode { get; private set; }

        /// <summary>
        /// Флаг, указывающий, был ли изменен пароль
        /// </summary>
        private bool _isPasswordChanged = false;

        /// <summary>
        /// Флаг для предотвращения рекурсивного обновления полей
        /// </summary>
        private bool _isUpdatingFields = false;

        /// <summary>
        /// Конструктор формы редактирования пользователя
        /// </summary>
        /// <param name="user">Модель пользователя с данными для редактирования</param>
        public EditUserForm(UserModel user)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            User = user;
            IsEditMode = true;

            // Настройка поля пароля (скрытие символов)
            ConfigurePasswordField();

            // Загрузка списка ролей из базы данных
            LoadRoles();

            // Загрузка данных пользователя в поля формы
            LoadTextBoxs();

            // Подписываемся на события для фильтрации ввода
            SubscribeToEvents();
        }

        #region Инициализация

        /// <summary>
        /// Настраивает поле пароля - включает скрытие символов
        /// </summary>
        private void ConfigurePasswordField()
        {
            Password.PasswordChar = '*'; // Скрываем вводимые символы
            // Можно добавить кнопку для показа/скрытия пароля
        }

        /// <summary>
        /// Подписывается на события изменения текста для всех полей ввода
        /// и события потери фокуса для форматирования
        /// </summary>
        private void SubscribeToEvents()
        {
            LastName.TextChanged += LastName_TextChanged;
            FirstName.TextChanged += FirstName_TextChanged;
            Login.TextChanged += Login_TextChanged;
            Email.TextChanged += Email_TextChanged;
            Password.TextChanged += Password_TextChanged;

            // Подписка на события потери фокуса для форматирования
            LastName.Leave += LastName_Leave;
            FirstName.Leave += FirstName_Leave;
            Email.Leave += Email_Leave;
        }

        /// <summary>
        /// Загружает данные пользователя в поля формы
        /// </summary>
        private void LoadTextBoxs()
        {
            _isUpdatingFields = true; // Блокируем обработку событий TextChanged

            LastName.Text = User.last_name;
            FirstName.Text = User.first_name;
            Login.Text = User.username;
            Email.Text = User.email;
            Password.Text = ""; // Поле пароля оставляем пустым (не показываем существующий пароль)

            if (RoleCb.DataSource != null)
            {
                RoleCb.SelectedValue = User.role_id;
            }

            _isUpdatingFields = false; // Разблокируем обработку событий

            // Блокировка изменения роли для администраторов
            // Администратор не может понизить сам себя
            if (IsAdminUser())
            {
                RoleCb.Enabled = false; // Запрещаем изменение роли
            }
            else
            {
                RoleCb.Enabled = true;
            }
        }

        /// <summary>
        /// Загружает список ролей из базы данных в комбобокс
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

                    if (User != null && User.role_id > 0)
                    {
                        RoleCb.SelectedValue = User.role_id;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Проверяет, является ли текущий пользователь администратором
        /// </summary>
        /// <returns>true, если пользователь имеет роль администратора</returns>
        private bool IsAdminUser()
        {
            return User.role_id == 1 ||
                   User.RoleName?.ToLower() == "администратор" ||
                   User.RoleName?.ToLower() == "administrator" ||
                   User.RoleName?.ToLower() == "admin";
        }

        #endregion

        #region Фильтрация ввода

        /// <summary>
        /// Фильтрация ввода в поле фамилии
        /// Разрешены только русские буквы, дефис и пробел
        /// </summary>
        private void LastName_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingFields) return; // Игнорируем при программном обновлении

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
            if (_isUpdatingFields) return;

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
        /// Фильтрация ввода в поле логина
        /// Разрешены только латинские буквы, цифры, подчеркивание и точка
        /// </summary>
        private void Login_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingFields) return;

            int selectionStart = Login.SelectionStart;
            string filteredText = FilterToLoginChars(Login.Text);

            if (filteredText != Login.Text)
            {
                Login.Text = filteredText;
                Login.SelectionStart = Math.Min(selectionStart, Login.Text.Length);
            }
        }

        /// <summary>
        /// Фильтр для логина: только латиница, цифры, подчеркивание, точка
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>Отфильтрованная строка</returns>
        private string FilterToLoginChars(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return new string(input.Where(c =>
                (c >= 'a' && c <= 'z') ||   // строчные латинские
                (c >= 'A' && c <= 'Z') ||   // заглавные латинские
                (c >= '0' && c <= '9') ||   // цифры
                c == '_' ||                  // подчеркивание
                c == '.').ToArray());        // точка (для email-подобных логинов)
        }

        /// <summary>
        /// Фильтрация email
        /// Удаляет пробелы и приводит к нижнему регистру
        /// </summary>
        private void Email_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingFields) return;

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
        /// Фильтрация ввода в поле пароля
        /// Пароль может содержать любые символы, ограничена только длина
        /// </summary>
        private void Password_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingFields) return;

            // Если в поле пароля появился текст, отмечаем, что пароль изменен
            if (!string.IsNullOrWhiteSpace(Password.Text))
            {
                _isPasswordChanged = true;
            }

            // Ограничиваем длину пароля для безопасности
            if (Password.Text.Length > 50)
            {
                int selectionStart = Password.SelectionStart;
                Password.Text = Password.Text.Substring(0, 50);
                Password.SelectionStart = Math.Min(selectionStart, Password.Text.Length);
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
            else if (!IsLoginUnique())
            {
                errors.Add("Этот логин уже занят");
                Login.BackColor = Color.LightPink;
            }

            // ===== ПРОВЕРКА EMAIL =====
            if (string.IsNullOrWhiteSpace(Email.Text))
            {
                errors.Add("Введите email пользователя");
                Email.BackColor = Color.LightPink;
            }
            else if (!IsValidEmail(Email.Text))
            {
                errors.Add("Введите корректный email адрес (например: name@domain.com)");
                Email.BackColor = Color.LightPink;
            }
            else if (!IsEmailUnique())
            {
                errors.Add("Пользователь с таким email уже существует");
                Email.BackColor = Color.LightPink;
            }

            // ===== ПРОВЕРКА ПАРОЛЯ (если изменен) =====
            if (_isPasswordChanged)
            {
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
        /// Проверяет уникальность логина (исключая текущего пользователя)
        /// </summary>
        /// <returns>true, если логин уникален</returns>
        private bool IsLoginUnique()
        {
            using (var connection = new MySqlConnection(_connection))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT COUNT(*) FROM user 
                            WHERE username = @Login AND user_id != @UserId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Login", Login.Text.Trim());
                    cmd.Parameters.AddWithValue("@UserId", User.user_id);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка проверки логина: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Проверяет уникальность email (исключая текущего пользователя)
        /// </summary>
        /// <returns>true, если email уникален</returns>
        private bool IsEmailUnique()
        {
            using (var connection = new MySqlConnection(_connection))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT COUNT(*) FROM user 
                            WHERE email = @Email AND user_id != @UserId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Email", Email.Text.Trim());
                    cmd.Parameters.AddWithValue("@UserId", User.user_id);

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
        /// Сохраняет данные из полей формы в объект User
        /// Выполняет форматирование и хеширование пароля при необходимости
        /// </summary>
        private void SaveUserData()
        {
            User.last_name = CapitalizeName(LastName.Text.Trim());
            User.first_name = CapitalizeName(FirstName.Text.Trim());
            User.username = Login.Text.Trim().ToLower();
            User.email = Email.Text.Trim().ToLower();

            if (RoleCb.SelectedValue != null && RoleCb.SelectedValue != DBNull.Value)
            {
                User.role_id = (int)RoleCb.SelectedValue;
            }

            // Если пароль был изменен, хешируем новый пароль
            if (_isPasswordChanged && !string.IsNullOrWhiteSpace(Password.Text))
            {
                User.password_hash = MySQLHelper.GetHash(Password.Text);
            }
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
        private void EditUser_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveUserData();
                MessageBox.Show("✅ Пользователь успешно обновлен!", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        /// Сравнивает текущие значения полей с исходными данными пользователя
        /// </summary>
        /// <returns>true, если есть изменения</returns>
        private bool HasUnsavedChanges()
        {
            return LastName.Text != User.last_name ||
                   FirstName.Text != User.first_name ||
                   Login.Text != User.username ||
                   Email.Text != User.email ||
                   (_isPasswordChanged && !string.IsNullOrWhiteSpace(Password.Text)) ||
                   (RoleCb.SelectedValue != null &&
                    (int)RoleCb.SelectedValue != User.role_id);
        }

        /// <summary>
        /// Обработчик потери фокуса полем фамилии
        /// Применяет форматирование с заглавной буквы
        /// </summary>
        private void LastName_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LastName.Text))
            {
                LastName.Text = CapitalizeName(LastName.Text);
            }
        }

        /// <summary>
        /// Обработчик потери фокуса полем имени
        /// Применяет форматирование с заглавной буквы
        /// </summary>
        private void FirstName_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FirstName.Text))
            {
                FirstName.Text = CapitalizeName(FirstName.Text);
            }
        }

        /// <summary>
        /// Обработчик потери фокуса полем email
        /// Приводит email к нижнему регистру
        /// </summary>
        private void Email_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Email.Text))
            {
                Email.Text = Email.Text.Trim().ToLower();
            }
        }

        #endregion

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
    }
}