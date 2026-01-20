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
    public partial class AddProductForm : Form
    {
        private string _connection;
        public ProductModel NewProduct { get; private set; }

        public AddProductForm()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            NewProduct = new ProductModel();
            LoadCategories();
        }

        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void LoadCategories()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT category_id, name FROM category ORDER BY name";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    CategoryCb.DataSource = dt;
                    CategoryCb.DisplayMember = "name";
                    CategoryCb.ValueMember = "category_id";

                    // Устанавливаем значение по умолчанию
                    if (CategoryCb.Items.Count > 0)
                    {
                        CategoryCb.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateData()
        {
            // Проверка названия
            if (string.IsNullOrWhiteSpace(Name.Text))
            {
                MessageBox.Show("Введите название продукта", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Name.Focus();
                return false;
            }

            // Проверка цены
            if (string.IsNullOrWhiteSpace(Price.Text))
            {
                MessageBox.Show("Введите цену продукта", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Price.Focus();
                return false;
            }

            if (!decimal.TryParse(Price.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Цена должна быть положительным числом", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Price.Focus();
                Price.SelectAll();
                return false;
            }

            // Проверка количества
            if (string.IsNullOrWhiteSpace(Count.Text))
            {
                MessageBox.Show("Введите количество продукта", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Count.Focus();
                return false;
            }

            if (!int.TryParse(Count.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Количество должно быть неотрицательным целым числом", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Count.Focus();
                Count.SelectAll();
                return false;
            }

            return true;
        }

        private void SaveProductData()
        {
            // Исправлено: парсим количество из Count.Text, а не из Price.Text
            decimal.TryParse(Price.Text, out decimal price);
            int.TryParse(Count.Text, out int stock); // Изменено здесь!

            NewProduct.name = Name.Text.Trim();
            NewProduct.price = price;
            NewProduct.stock_quantity = stock;

            if (CategoryCb.SelectedValue != null)
            {
                NewProduct.category_id = (int)CategoryCb.SelectedValue;
            }
        }

        private bool AddProductToDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    // Убрал description из запроса, так как его нет в таблице
                    string query = @"INSERT INTO product 
                            (name, price, stock_quantity, category_id) 
                            VALUES (@Name, @Price, @StockQuantity, @CategoryId)";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", NewProduct.name);
                        cmd.Parameters.AddWithValue("@Price", NewProduct.price);
                        cmd.Parameters.AddWithValue("@StockQuantity", NewProduct.stock_quantity);
                        cmd.Parameters.AddWithValue("@CategoryId", NewProduct.category_id);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                          
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Не удалось добавить продукт", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                // Обработка специфичных ошибок MySQL
                if (sqlEx.Number == 1452) // Ошибка внешнего ключа
                {
                    MessageBox.Show("Выбранная категория не существует", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (sqlEx.Number == 1062) // Ошибка дублирования
                {
                    MessageBox.Show("Продукт с таким названием уже существует", "Ошибка",
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
                MessageBox.Show($"Ошибка при добавлении продукта: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void AddProduct_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveProductData();
                if (AddProductToDatabase())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }
    }
}