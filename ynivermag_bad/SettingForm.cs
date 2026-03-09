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
    public partial class SettingForm : Form
    {
        public SettingForm()
        {
            InitializeComponent();
            LoadCurrentSettings();

            // Настройка формы
            this.Text = "Настройки подключения";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            // Подсказки для полей
            toolTip1.SetToolTip(txtServer, "Адрес сервера базы данных (например: localhost или 127.0.0.1)");
            toolTip1.SetToolTip(txtUser, "Имя пользователя базы данных");
            toolTip1.SetToolTip(txtPassword, "Пароль пользователя базы данных");
            toolTip1.SetToolTip(txtDatabase, "Название базы данных");
        }

        /// <summary>
        /// Загрузка текущих настроек из файла конфигурации
        /// </summary>
        private void LoadCurrentSettings()
        {
            try
            {
                txtServer.Text = Properties.Settings.Default.host;
                txtUser.Text = Properties.Settings.Default.uid;
                txtPassword.Text = Properties.Settings.Default.pwd;
                txtDatabase.Text = Properties.Settings.Default.database;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Сохранение настроек и перезапуск приложения
        /// </summary>
        private void btnReconnect_Click(object sender, EventArgs e)
        {
            // Проверка заполнения полей
            if (string.IsNullOrWhiteSpace(txtServer.Text))
            {
                MessageBox.Show("Введите адрес сервера", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServer.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUser.Text))
            {
                MessageBox.Show("Введите имя пользователя", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUser.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                MessageBox.Show("Введите название базы данных", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDatabase.Focus();
                return;
            }

            try
            {
                // Сохраняем настройки
                Properties.Settings.Default["host"] = txtServer.Text.Trim();
                Properties.Settings.Default["uid"] = txtUser.Text.Trim();
                Properties.Settings.Default["pwd"] = txtPassword.Text;
                Properties.Settings.Default["database"] = txtDatabase.Text.Trim();

                Properties.Settings.Default.Save();

                // Спрашиваем подтверждение на перезапуск
                DialogResult result = MessageBox.Show(
                    "Настройки успешно сохранены!\n\nДля применения изменений необходимо перезапустить приложение.\n\nПерезапустить сейчас?",
                    "Перезапуск приложения",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Application.Restart();
                }
                else
                {
                    // Если пользователь отказался от перезапуска, просто закрываем форму
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Кнопка отмены - просто закрывает форму
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Обработчик нажатия клавиш в полях ввода
        /// </summary>
        private void txt_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Запрещаем ввод некоторых символов (опционально)
            if (sender == txtServer || sender == txtUser || sender == txtDatabase)
            {
                // Для сервера, пользователя и базы данных разрешаем буквы, цифры, точку, дефис
                if (!char.IsControl(e.KeyChar) && !char.IsLetterOrDigit(e.KeyChar) &&
                    e.KeyChar != '.' && e.KeyChar != '-' && e.KeyChar != '_')
                {
                    e.Handled = true;
                }
            }
            // Для пароля разрешаем любые символы
        }

        /// <summary>
        /// Тестирование подключения с текущими параметрами
        /// </summary>
        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            // Проверка заполнения полей
            if (string.IsNullOrWhiteSpace(txtServer.Text) ||
                string.IsNullOrWhiteSpace(txtUser.Text) ||
                string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                MessageBox.Show("Заполните все обязательные поля (сервер, пользователь, база данных)",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Формируем временную строку подключения
                string connString = $"Server={txtServer.Text.Trim()};Database={txtDatabase.Text.Trim()};Uid={txtUser.Text.Trim()};Pwd={txtPassword.Text};";

                using (var connection = new MySql.Data.MySqlClient.MySqlConnection(connString))
                {
                    connection.Open();
                    MessageBox.Show("✅ Подключение к базе данных успешно установлено!",
                        "Тест подключения", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка подключения:\n{ex.Message}",
                    "Тест подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Предотвращение закрытия формы через крестик
        /// </summary>
        private void SettingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Отменяем закрытие
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }
    }
}