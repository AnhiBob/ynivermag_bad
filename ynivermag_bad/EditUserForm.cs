using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace ynivermag_bad
{
    public partial class EditUserForm : Form
    {
        private string _connection;
        public UserModel User { get; private set; }
        public bool IsEditMode { get; private set; }
        private bool _isPasswordChanged = false;

        public EditUserForm(UserModel user)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            LoadRoles();
            User = user;
            IsEditMode = true;
            LoadTextBoxs();
        }

        private void LoadTextBoxs()
        {
            LastName.Text = User.last_name;
            FirstName.Text = User.first_name;
            Login.Text = User.username;
            Email.Text = User.email;
            Password.Text = "";
            Password.PasswordChar = '*';

            // Устанавливаем текущую роль по значению, а не по тексту
            RoleCb.SelectedValue = User.role_id;
            // RoleCb.Text = User.RoleName; // Уберите эту строку - она вызывает ошибку

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
                    string query = "SELECT role_id, role_name FROM role ORDER BY role_id";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    RoleCb.DataSource = dt;
                    RoleCb.DisplayMember = "role_name";
                    RoleCb.ValueMember = "role_id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}");
            }
        }

        private bool IsAdminUser()
        {
            // Проверяем, является ли пользователь администратором
            return User.RoleName?.ToLower() == "администратор" ||
                   User.RoleName?.ToLower() == "administrator" ||
                   User.RoleName?.ToLower() == "admin" ||
                   User.role_id == 1; // ID роли администратора из вашей БД
        }

        private void EditUser_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveUserData();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(LastName.Text))
            {
                MessageBox.Show("Введите фамилию");
                LastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(FirstName.Text))
            {
                MessageBox.Show("Введите имя");
                FirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Login.Text))
            {
                MessageBox.Show("Введите логин");
                Login.Focus();
                return false;
            }

            // Проверка на уникальность логина
            if (!IsLoginUnique())
            {
                MessageBox.Show("Пользователь с таким логином уже существует");
                Login.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email.Text))
            {
                MessageBox.Show("Введите email");
                Email.Focus();
                return false;
            }

            // Проверка на уникальность email
            if (!IsEmailUnique())
            {
                MessageBox.Show("Пользователь с таким email уже существует");
                Email.Focus();
                return false;
            }

            // Проверка пароля (если введен)
            if (!string.IsNullOrWhiteSpace(Password.Text) && Password.Text.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать не менее 6 символов");
                Password.Focus();
                return false;
            }

            return true;
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

        private void SaveUserData()
        {
            User.last_name = LastName.Text.Trim();
            User.first_name = FirstName.Text.Trim();
            User.username = Login.Text.Trim();
            User.email = Email.Text.Trim();

            if (RoleCb.SelectedValue != null)
            {
                User.role_id = (int)RoleCb.SelectedValue;
            }

            // Обновляем пароль только если он был изменен
            if (_isPasswordChanged && !string.IsNullOrWhiteSpace(Password.Text))
            {
                string passwordHash = MySQLHelper.GetHash(Password.Text);
                User.password_hash = passwordHash;
            }
        }

        private void LastName_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = LastName.SelectionStart;
            string filteredText = InputValidator.FilterToRussianLetters(LastName.Text);

            if (filteredText != LastName.Text)
            {
                LastName.Text = filteredText;
                LastName.SelectionStart = Math.Min(selectionStart, LastName.Text.Length);
            }
        }

        private void FirstName_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = FirstName.SelectionStart;
            string filteredText = InputValidator.FilterToRussianLetters(FirstName.Text);

            if (filteredText != FirstName.Text)
            {
                FirstName.Text = filteredText;
                FirstName.SelectionStart = Math.Min(selectionStart, FirstName.Text.Length);
            }
        }

        private void Login_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = Login.SelectionStart;
            string filteredText = new string(Login.Text
                .Where(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                           char.IsDigit(c) || c == '_' || c == '.')
                .ToArray());

            if (filteredText != Login.Text)
            {
                Login.Text = filteredText;
                Login.SelectionStart = Math.Min(selectionStart, Login.Text.Length);
            }
        }

        private void Email_TextChanged(object sender, EventArgs e)
        {
            // Приводим email к нижнему регистру
            string text = Email.Text;
            string formattedText = text.ToLower();

            if (formattedText != text)
            {
                int cursorPosition = Email.SelectionStart;
                Email.Text = formattedText;
                Email.SelectionStart = Math.Min(cursorPosition, formattedText.Length);
            }
        }

        private void Password_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Password.Text))
            {
                _isPasswordChanged = true;
            }
        }
    }
}