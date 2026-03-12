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
    }
}