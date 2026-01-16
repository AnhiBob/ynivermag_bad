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
        }

        private void Autorization_Click(object sender, EventArgs e)
        {
            if (Connection.TestConnection())
            {
                if (Login.Text == "" || Password.Text == "")
                {
                    MessageBox.Show("Заполните все поле!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    using (MySqlConnection con = new MySqlConnection(_connection))
                    {
                        con.Open();

                        string passwordHash = MySQLHelper.GetHash(Password.Text);

                        string query = $"SELECT Count(*) FROM user Where username='{Login.Text}' and password_hash = '{passwordHash}';";

                        MySqlCommand cmd = new MySqlCommand(query, con);
                        cmd.ExecuteScalar();


                        var role = MySQLHelper.GetRoleName(Login.Text, passwordHash);
                        string FIO = MySQLHelper.GetLastNameWithInitials(Login.Text, passwordHash);

                        if (role != null && FIO != null)
                        {
                            switch (role)
                            {
                                case "Администратор":
                                    {
                                        MenuAdminForm admin = new MenuAdminForm(FIO);
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
                                        MenuTovarovedForm menu = new MenuTovarovedForm(FIO);
                                        menu.Show();
                                        this.Hide();
                                        break;
                                    }

                            }
                        }
                        else
                        {
                            MessageBox.Show("Неверный логин или пароль", "Ошибка");
                            Login.Text = "";
                            Password.Text = "";
                        }


                        con.Close();
                    }
                }
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
