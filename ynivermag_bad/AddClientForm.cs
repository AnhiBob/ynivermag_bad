using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ynivermag_bad
{
    public partial class AddClientForm : Form
    {
        private string _connection;
        public ClientModel NewClient { get; private set; }
        public AddClientForm()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            NewClient = new ClientModel();
        }

        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
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

        private bool ValidateData()
        {
            // Проверка имени
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                MessageBox.Show("Введите имя клиента", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                FirstNameTextBox.Focus();
                return false;
            }

            // Проверка фамилии
            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                MessageBox.Show("Введите фамилию клиента", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LastNameTextBox.Focus();
                return false;
            }

            // Проверка email
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Введите email клиента", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                EmailTextBox.Focus();
                return false;
            }

            if (!IsValidEmail(EmailTextBox.Text.Trim()))
            {
                MessageBox.Show("Введите корректный email адрес", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                EmailTextBox.Focus();
                EmailTextBox.SelectAll();
                return false;
            }

            // Проверка уникальности email
            if (!IsEmailUnique(EmailTextBox.Text.Trim()))
            {
                MessageBox.Show("Клиент с таким email уже существует", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                EmailTextBox.Focus();
                EmailTextBox.SelectAll();
                return false;
            }

            // Проверка телефона (не обязательное поле, но если заполнено - проверяем)
            if (!string.IsNullOrWhiteSpace(PhoneMaskedTextBox.Text) && PhoneMaskedTextBox.Text.Contains("_"))
            {
                MessageBox.Show("Введите корректный номер телефона", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PhoneMaskedTextBox.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
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
                // В случае ошибки при проверке считаем email не уникальным для безопасности
                return false;
            }
        }

        private void SaveClientData()
        {
            NewClient.first_name = FirstNameTextBox.Text.Trim();
            NewClient.last_name = LastNameTextBox.Text.Trim();
            NewClient.email = EmailTextBox.Text.Trim();

            // Очищаем телефон от лишних символов, если он введен
            if (!string.IsNullOrWhiteSpace(PhoneMaskedTextBox.Text) && !PhoneMaskedTextBox.Text.Contains("_"))
            {
                // Убираем все нецифровые символы, кроме +
                string phone = PhoneMaskedTextBox.Text;
                phone = Regex.Replace(phone, @"[^\d+]", "");
                NewClient.phone = phone;
            }
            else
            {
                NewClient.phone = null;
            }

            NewClient.address = AddressTextBox.Text.Trim();
        }

        private bool AddClientToDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query = @"INSERT INTO client 
                            (email, first_name, last_name, phone, address) 
                            VALUES (@Email, @FirstName, @LastName, @Phone, @Address)";

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

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Не удалось добавить клиента", "Ошибка",
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

        private void PhoneMaskedTextBox_TextChanged(object sender, EventArgs e)
        {
            int originalSelectionStart = PhoneMaskedTextBox.SelectionStart;
            string originalText = PhoneMaskedTextBox.Text;

            // 1. Фильтруем текст
            string filteredText = InputValidator.FilterToPhone(originalText);

            // 2. Форматируем номер
            string formattedText = InputValidator.FormatPhoneNumber(filteredText);

            // Если текст изменился
            if (formattedText != originalText)
            {
                // Сохраняем текст
                PhoneMaskedTextBox.Text = formattedText;

                // Корректируем позицию курсора с учетом добавленных символов форматирования
                int adjustedPosition = GetAdjustedCursorPosition(originalSelectionStart, originalText, formattedText);
                PhoneMaskedTextBox.SelectionStart = Math.Min(adjustedPosition, formattedText.Length);
            }
        }

        private int GetAdjustedCursorPosition(int originalPosition, string oldText, string newText)
        {
            if (originalPosition >= oldText.Length)
                return newText.Length;

            // Считаем, сколько форматирующих символов было добавлено ДО позиции курсора
            int formatCharsBeforeCursor = 0;

            // Форматирующие символы в телефонном номере
            char[] formatChars = { '(', ')', ' ', '-', '+' };

            for (int i = 0; i < originalPosition && i < newText.Length; i++)
            {
                if (formatChars.Contains(newText[i]))
                {
                    formatCharsBeforeCursor++;
                }
            }

            // Корректируем позицию с учетом форматирующих символов
            return originalPosition + formatCharsBeforeCursor;
        }

        private void EmailTextBox_TextChanged(object sender, EventArgs e)
        {
            int originalSelectionStart = EmailTextBox.SelectionStart;
            string originalText = EmailTextBox.Text;

            // Удаляем все пробелы и приводим к нижнему регистру
            string newText = originalText.Replace(" ", "").ToLower();

            if (newText != originalText)
            {
                EmailTextBox.Text = newText;
                // Корректируем позицию курсора
                int spacesRemoved = originalText.Length - newText.Length;
                EmailTextBox.SelectionStart = Math.Max(0, originalSelectionStart - spacesRemoved);
            }

            // Теперь проверяем валидность
            bool isValid = IsValidEmail(newText);
            if (isValid)
            {
                EmailTextBox.BackColor = SystemColors.Window;
            }
            else
            {
                EmailTextBox.BackColor = Color.LightPink;
            }
        }
    }
}
