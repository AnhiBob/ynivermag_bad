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
    public partial class Form1 : Form
    {
        private string _connection;

        public Form1()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;

            // Подписываемся на события для фильтрации ввода
            Login.TextChanged += Login_TextChanged;
            Password.TextChanged += Password_TextChanged;

            // Добавляем подсказки
            toolTip1.SetToolTip(Login, "Только латинские буквы, цифры и символы _ . -");
            toolTip1.SetToolTip(Password, "Можно использовать любые символы");
        }

        /// <summary>
        /// Фильтрация ввода в поле логина (только латиница, цифры, _ . -)
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
        /// Фильтр для логина: только латиница, цифры, _ . -
        /// </summary>
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
        /// Фильтрация ввода в поле пароля (никаких ограничений, кроме длины)
        /// </summary>
        private void Password_TextChanged(object sender, EventArgs e)
        {
            // Пароль может содержать любые символы
            // Только ограничиваем длину для безопасности
            if (Password.Text.Length > 50)
            {
                int selectionStart = Password.SelectionStart;
                Password.Text = Password.Text.Substring(0, 50);
                Password.SelectionStart = Math.Min(selectionStart, Password.Text.Length);

                MessageBox.Show("Максимальная длина пароля - 50 символов", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Autorization_Click(object sender, EventArgs e)
        {
            // Проверка подключения к базе данных
            if (Connection.TestConnection())
            {
                // Валидация полей перед отправкой
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

                // Проверка длины логина
                if (Login.Text.Length < 3)
                {
                    MessageBox.Show("Логин должен содержать минимум 3 символа", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Login.Focus();
                    return;
                }

                if (Login.Text.Length > 50)
                {
                    MessageBox.Show("Логин должен содержать не более 50 символов", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Login.Focus();
                    return;
                }

                // Проверка длины пароля
                if (Password.Text.Length < 3)
                {
                    MessageBox.Show("Пароль должен содержать минимум 3 символа", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Password.Focus();
                    return;
                }

                if (Password.Text.Length > 50)
                {
                    MessageBox.Show("Пароль должен содержать не более 50 символов", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Password.Focus();
                    return;
                }

                try
                {
                    using (MySqlConnection con = new MySqlConnection(_connection))
                    {
                        con.Open();

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
                            var role = MySQLHelper.GetRoleName(Login.Text, passwordHash);
                            string FIO = MySQLHelper.GetLastNameWithInitials(Login.Text, passwordHash);

                            if (role != null && FIO != null)
                            {
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
                            // Проверка на неактивного пользователя
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
                                MessageBox.Show("Ваша учетная запись отключена. Обратитесь к администратору.",
                                              "Доступ запрещен",
                                              MessageBoxButtons.OK,
                                              MessageBoxIcon.Warning);
                            }
                            else
                            {
                                MessageBox.Show("Неверный логин или пароль", "Ошибка авторизации",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                            // Очистка полей ввода
                            Login.Text = "";
                            Password.Text = "";
                            Login.Focus();
                        }

                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при авторизации: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Ошибка подключения к базе данных
                MessageBox.Show("Ошибка подключения к базе данных. Проверьте настройки подключения.",
                    "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Открытие формы настроек
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

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Очистка полей при загрузке формы
        private void Form1_Load(object sender, EventArgs e)
        {
            Login.Text = "";
            Password.Text = "";
        }

        // Обработка нажатия Enter для быстрого входа
        private void Password_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Autorization_Click(sender, e);
                e.Handled = true;
            }
        }

        private void Login_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Password.Focus();
                e.Handled = true;
            }
        }

        // Предотвращение закрытия формы через крестик
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult result = MessageBox.Show("Вы действительно хотите выйти из приложения?",
                    "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Application.Exit();
                }
                else
                {
                    e.Cancel = true; // Отменяем закрытие
                }
            }
        }
    }
}