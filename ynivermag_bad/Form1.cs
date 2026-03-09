using MySql.Data.MySqlClient;
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
    /// <summary>
    /// Главная форма авторизации в приложении.
    /// Обеспечивает вход пользователей в систему с проверкой учетных данных.
    /// Поддерживает:
    /// - Фильтрацию ввода логина (только допустимые символы)
    /// - Проверку подключения к БД
    /// - Валидацию длины полей
    /// - Разграничение доступа по ролям (Администратор, Продавец, Товаровед)
    /// - Обработку неактивных учетных записей
    /// - Переход к форме настроек при ошибке подключения
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Строка подключения к базе данных
        /// </summary>
        private string _connection;

        /// <summary>
        /// Конструктор формы авторизации
        /// Инициализирует компоненты, настраивает фильтрацию ввода и подсказки
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;

            // Подписываемся на события для фильтрации ввода в реальном времени
            Login.TextChanged += Login_TextChanged;
            Password.TextChanged += Password_TextChanged;

            // Добавляем всплывающие подсказки для полей ввода
            toolTip1.SetToolTip(Login, "Только латинские буквы, цифры и символы _ . -");
            toolTip1.SetToolTip(Password, "Можно использовать любые символы");
        }

        #region Фильтрация ввода

        /// <summary>
        /// Фильтрация ввода в поле логина в реальном времени
        /// Разрешены только латинские буквы, цифры и символы _ . -
        /// </summary>
        private void Login_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = Login.SelectionStart;
            string filteredText = FilterToLoginChars(Login.Text);

            if (filteredText != Login.Text)
            {
                Login.Text = filteredText;
                // Корректируем позицию курсора после фильтрации
                Login.SelectionStart = Math.Min(selectionStart, Login.Text.Length);
            }
        }

        /// <summary>
        /// Фильтрует строку, оставляя только разрешенные символы для логина
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>Отфильтрованная строка, содержащая только допустимые символы</returns>
        /// <remarks>
        /// Разрешены:
        /// - Латинские буквы (a-z, A-Z)
        /// - Цифры (0-9)
        /// - Символ подчеркивания (_)
        /// - Точка (.)
        /// - Дефис (-)
        /// </remarks>
        private string FilterToLoginChars(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return new string(input.Where(c =>
                (c >= 'a' && c <= 'z') ||   // строчные латинские
                (c >= 'A' && c <= 'Z') ||   // заглавные латинские
                (c >= '0' && c <= '9') ||   // цифры
                c == '_' ||                  // подчеркивание
                c == '.' ||                  // точка
                c == '-').ToArray());        // дефис
        }

        /// <summary>
        /// Фильтрация ввода в поле пароля
        /// Пароль может содержать любые символы, ограничена только максимальная длина
        /// </summary>
        private void Password_TextChanged(object sender, EventArgs e)
        {
            // Пароль может содержать любые символы, никакой фильтрации не требуется
            // Только ограничиваем максимальную длину для безопасности
            if (Password.Text.Length > 50)
            {
                int selectionStart = Password.SelectionStart;
                Password.Text = Password.Text.Substring(0, 50);
                Password.SelectionStart = Math.Min(selectionStart, Password.Text.Length);

                MessageBox.Show("Максимальная длина пароля - 50 символов", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region Авторизация

        /// <summary>
        /// Обработчик кнопки авторизации
        /// Выполняет проверку подключения к БД, валидацию полей и вход пользователя
        /// </summary>
        private void Autorization_Click(object sender, EventArgs e)
        {
            // ===== ПРОВЕРКА ПОДКЛЮЧЕНИЯ К БАЗЕ ДАННЫХ =====
            if (Connection.TestConnection())
            {
                // ===== ВАЛИДАЦИЯ ПОЛЕЙ =====
                // Проверка на пустые поля
                if (string.IsNullOrWhiteSpace(Login.Text))
                {
                    MessageBox.Show("Введите логин!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Login.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password.Text))
                {
                    MessageBox.Show("Введите пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Password.Focus();
                    return;
                }

                // Проверка минимальной длины логина
                if (Login.Text.Length < 3)
                {
                    MessageBox.Show("Логин должен содержать минимум 3 символа", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Login.Focus();
                    return;
                }

                // Проверка максимальной длины логина
                if (Login.Text.Length > 50)
                {
                    MessageBox.Show("Логин должен содержать не более 50 символов", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Login.Focus();
                    return;
                }

                // Проверка минимальной длины пароля
                if (Password.Text.Length < 3)
                {
                    MessageBox.Show("Пароль должен содержать минимум 3 символа", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Password.Focus();
                    return;
                }

                // Проверка максимальной длины пароля
                if (Password.Text.Length > 50)
                {
                    MessageBox.Show("Пароль должен содержать не более 50 символов", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Password.Focus();
                    return;
                }

                // ===== АВТОРИЗАЦИЯ =====
                try
                {
                    using (MySqlConnection con = new MySqlConnection(_connection))
                    {
                        con.Open();

                        // Хеширование введенного пароля для сравнения с хранящимся в БД
                        string passwordHash = MySQLHelper.GetHash(Password.Text);

                        // Проверка наличия активного пользователя с указанными логином и паролем
                        string query = @"SELECT COUNT(*) FROM user 
                               WHERE username = @login 
                               AND password_hash = @passwordHash 
                               AND isActive = 1";

                        MySqlCommand cmd = new MySqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@login", Login.Text);
                        cmd.Parameters.AddWithValue("@passwordHash", passwordHash);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        if (count > 0)
                        {
                            // Пользователь найден и активен - получаем его роль и ФИО
                            var role = MySQLHelper.GetRoleName(Login.Text, passwordHash);
                            string FIO = MySQLHelper.GetLastNameWithInitials(Login.Text, passwordHash);

                            if (role != null && FIO != null)
                            {
                                // Перенаправление на соответствующую форму в зависимости от роли
                                switch (role)
                                {
                                    case "Администратор":
                                        {
                                            MenuAdminForm admin = new MenuAdminForm(FIO, Login.Text);
                                            admin.Show();
                                            this.Hide();
                                            break;
                                        }
                                    case "Продавец":
                                        {
                                            MenuSellerForm seller = new MenuSellerForm(FIO);
                                            seller.Show();
                                            this.Hide();
                                            break;
                                        }
                                    case "Товаровед":
                                        {
                                            MenuTovarovedForm menu = new MenuTovarovedForm(FIO, Login.Text);
                                            menu.Show();
                                            this.Hide();
                                            break;
                                        }
                                    default:
                                        MessageBox.Show($"Роль '{role}' не поддерживается", "Ошибка",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        break;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Ошибка получения данных пользователя", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                Login.Text = "";
                                Password.Text = "";
                            }
                        }
                        else
                        {
                            // Проверка на неактивного пользователя (учетная запись отключена)
                            string checkInactiveQuery = @"SELECT COUNT(*) FROM user 
                                                 WHERE username = @login 
                                                 AND password_hash = @passwordHash 
                                                 AND isActive = 0";

                            MySqlCommand checkCmd = new MySqlCommand(checkInactiveQuery, con);
                            checkCmd.Parameters.AddWithValue("@login", Login.Text);
                            checkCmd.Parameters.AddWithValue("@passwordHash", passwordHash);

                            int inactiveCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (inactiveCount > 0)
                            {
                                // Пользователь существует, но его учетная запись отключена
                                MessageBox.Show("Ваша учетная запись отключена. Обратитесь к администратору.",
                                              "Доступ запрещен",
                                              MessageBoxButtons.OK,
                                              MessageBoxIcon.Warning);
                            }
                            else
                            {
                                // Неверный логин или пароль
                                MessageBox.Show("Неверный логин или пароль", "Ошибка авторизации",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                            // Очистка полей ввода для повторной попытки
                            Login.Text = "";
                            Password.Text = "";
                            Login.Focus();
                        }

                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    // Обработка ошибок при авторизации
                    MessageBox.Show($"Ошибка при авторизации: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // ===== ОШИБКА ПОДКЛЮЧЕНИЯ К БАЗЕ ДАННЫХ =====
                MessageBox.Show("Ошибка подключения к базе данных. Проверьте настройки подключения.",
                    "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Открытие формы настроек для изменения параметров подключения
                SettingForm settingForm = new SettingForm();
                settingForm.ShowDialog(); // Используем ShowDialog, чтобы форма была модальной

                // После закрытия формы настроек обновляем строку подключения
                _connection = Connection.ConnectionString;

                // Пробуем подключиться снова с новыми настройками
                if (Connection.TestConnection())
                {
                    MessageBox.Show("Подключение к базе данных восстановлено!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        #endregion

        #region Выход из приложения

        /// <summary>
        /// Обработчик кнопки выхода из приложения
        /// </summary>
        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #endregion

        #region Обработчики событий формы

        /// <summary>
        /// Очистка полей при загрузке формы
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            Login.Text = "";
            Password.Text = "";
        }

        /// <summary>
        /// Обработка нажатия клавиш в поле пароля
        /// При нажатии Enter выполняется авторизация
        /// </summary>
        private void Password_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Autorization_Click(sender, e);
                e.Handled = true; // Предотвращаем дальнейшую обработку события
            }
        }

        /// <summary>
        /// Обработка нажатия клавиш в поле логина
        /// При нажатии Enter фокус переходит на поле пароля
        /// </summary>
        private void Login_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Password.Focus();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработчик закрытия формы
        /// Предотвращает случайное закрытие приложения и запрашивает подтверждение
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult result = MessageBox.Show("Вы действительно хотите выйти из приложения?",
                    "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Application.Exit(); // Завершаем приложение
                }
                else
                {
                    e.Cancel = true; // Отменяем закрытие формы
                }
            }
        }

        #endregion
    }
}