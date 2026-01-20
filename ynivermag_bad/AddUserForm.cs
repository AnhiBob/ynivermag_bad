using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ynivermag_bad
{
    public partial class AddUserForm : Form
    {
        private string _connection;

        public UserModel NewUser { get; private set; }
        public AddUserForm()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            NewUser = new UserModel();
            LoadRoles();
        }

        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void LoadRoles()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT role_id, role_name FROM Role";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    RoleCb.DataSource = dt;
                    RoleCb.DisplayMember = "role_name";
                    RoleCb.ValueMember = "role_id";

                    // Устанавливаем значение по умолчанию (например, первую роль)
                    if (RoleCb.Items.Count > 0)
                    {
                        RoleCb.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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

        private bool ValidateData()
        {
            // Проверка фамилии
            if (string.IsNullOrWhiteSpace(LastName.Text))
            {
                MessageBox.Show("Введите фамилию пользователя", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LastName.Focus();
                return false;
            }

            // Проверка имени
            if (string.IsNullOrWhiteSpace(FirstName.Text))
            {
                MessageBox.Show("Введите имя пользователя", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                FirstName.Focus();
                return false;
            }

            // Проверка логина
            if (string.IsNullOrWhiteSpace(Login.Text))
            {
                MessageBox.Show("Введите логин пользователя", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Login.Focus();
                return false;
            }

            // Проверка уникальности логина
            if (!IsLoginUnique(Login.Text.Trim()))
            {
                MessageBox.Show("Этот логин уже занят, выберите другой", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Login.Focus();
                Login.SelectAll();
                return false;
            }

            // Проверка пароля
            if (string.IsNullOrWhiteSpace(Password.Text))
            {
                MessageBox.Show("Введите пароль пользователя", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Password.Focus();
                return false;
            }


            return true;
        }

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
                        // Исправлено: имя параметра должно совпадать с @Login в запросе
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
                // В случае ошибки при проверке считаем логин не уникальным для безопасности
                return false;
            }
        }

        private void SaveUserData()
        {
            string passwordHash = MySQLHelper.GetHash(Password.Text);

            NewUser.last_name = LastName.Text.Trim();
            NewUser.first_name = FirstName.Text.Trim();
            NewUser.username = Login.Text.Trim();
            NewUser.email = Email.Text.Trim();
            NewUser.password_hash = passwordHash;
            NewUser.role_id = (int)RoleCb.SelectedValue;
        }

        private bool AddUserToDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query = @"INSERT INTO user 
                            (username, password_hash, email, first_name, last_name, role_id) 
                            VALUES (@Username, @PasswordHash, @Email, @FirstName, @LastName, @RoleId)";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Username", NewUser.username);
                        cmd.Parameters.AddWithValue("@PasswordHash", NewUser.password_hash); // Хешируем пароль
                        cmd.Parameters.AddWithValue("@Email", NewUser.email);
                        cmd.Parameters.AddWithValue("@FirstName", NewUser.first_name);
                        cmd.Parameters.AddWithValue("@LastName", NewUser.last_name);
                        cmd.Parameters.AddWithValue("@RoleId", NewUser.role_id);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {

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
                // Обработка специфичных ошибок MySQL
                if (sqlEx.Number == 1062) // Ошибка дублирования уникального ключа
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
    }
}
