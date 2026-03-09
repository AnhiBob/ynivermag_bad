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
    public partial class EditUserForm : Form
    {
        private string _connection;
        public UserModel User { get; private set; }
        public bool IsEditMode { get; private set; }
        private bool _isPasswordChanged = false;
        private bool _isUpdatingFields = false;

        public EditUserForm(UserModel user)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            User = user;
            IsEditMode = true;

            // Настройка полей
            ConfigurePasswordField();

            // Загрузка данных
            LoadRoles();
            LoadTextBoxs();

            // Подписываемся на события для фильтрации ввода
            SubscribeToEvents();
        }

        #region Инициализация

        private void ConfigurePasswordField()
        {
            Password.PasswordChar = '*';
            // Можно добавить кнопку для показа/скрытия пароля
        }

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

        private void LoadTextBoxs()
        {
            _isUpdatingFields = true;

            LastName.Text = User.last_name;
            FirstName.Text = User.first_name;
            Login.Text = User.username;
            Email.Text = User.email;
            Password.Text = "";

            if (RoleCb.DataSource != null)
            {
                RoleCb.SelectedValue = User.role_id;
            }

            _isUpdatingFields = false;

            // Блокировка роли для администраторов
            if (IsAdminUser())
            {
                RoleCb.Enabled = false;
            }
            else
            {
                RoleCb.Enabled = true;
            }
        }

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

        private bool IsAdminUser()
        {
            return User.role_id == 1 ||
                   User.RoleName?.ToLower() == "администратор" ||
                   User.RoleName?.ToLower() == "administrator" ||
                   User.RoleName?.ToLower() == "admin";
        }

        #endregion

        #region Фильтрация ввода (как в примере)

        /// <summary>
        /// Фильтрация ввода в поле фамилии (только русские буквы, дефис, пробел)
        /// </summary>
        private void LastName_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingFields) return;

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
        /// Фильтрация ввода в поле логина (только латиница, цифры, подчеркивание, точка)
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
        private string FilterToLoginChars(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return new string(input.Where(c =>
                (c >= 'a' && c <= 'z') ||   // строчные латинские
                (c >= 'A' && c <= 'Z') ||   // заглавные латинские
                (c >= '0' && c <= '9') ||   // цифры
                c == '_' ||                  // подчеркивание
                c == '.').ToArray());        // точка
        }

        /// <summary>
        /// Фильтрация email (только допустимые символы и автоматический lower case)
        /// </summary>
        private void Email_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingFields) return;

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
        /// Фильтрация ввода в поле пароля (никаких ограничений, кроме длины)
        /// </summary>
        private void Password_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingFields) return;

            if (!string.IsNullOrWhiteSpace(Password.Text))
            {
                _isPasswordChanged = true;
            }

            // Ограничиваем длину пароля
            if (Password.Text.Length > 50)
            {
                int selectionStart = Password.SelectionStart;
                Password.Text = Password.Text.Substring(0, 50);
                Password.SelectionStart = Math.Min(selectionStart, Password.Text.Length);
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

            // Проверка имени
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

            // Проверка логина
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

            // Проверка email
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

            // Проверка пароля (если изменен)
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

            // Проверка роли
            if (RoleCb.SelectedValue == null || RoleCb.SelectedValue == DBNull.Value)
            {
                errors.Add("Выберите роль");
                RoleCb.BackColor = Color.LightPink;
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

            if (_isPasswordChanged && !string.IsNullOrWhiteSpace(Password.Text))
            {
                User.password_hash = MySQLHelper.GetHash(Password.Text);
            }
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
            return LastName.Text != User.last_name ||
                   FirstName.Text != User.first_name ||
                   Login.Text != User.username ||
                   Email.Text != User.email ||
                   (_isPasswordChanged && !string.IsNullOrWhiteSpace(Password.Text)) ||
                   (RoleCb.SelectedValue != null &&
                    (int)RoleCb.SelectedValue != User.role_id);
        }

        private void LastName_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LastName.Text))
            {
                LastName.Text = CapitalizeName(LastName.Text);
            }
        }

        private void FirstName_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FirstName.Text))
            {
                FirstName.Text = CapitalizeName(FirstName.Text);
            }
        }

        private void Email_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Email.Text))
            {
                Email.Text = Email.Text.Trim().ToLower();
            }
        }

        #endregion
    }
}