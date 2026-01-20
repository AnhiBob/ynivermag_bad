using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace ynivermag_bad
{
    public partial class EditProductForm : Form
    {
        private string _connection;
        public ProductModel Product { get; private set; }

        public EditProductForm(ProductModel product)
        {
            InitializeComponent();
            Product = product;
            LoadProductData();
            _connection = Connection.ConnectionString;
            LoadCategories();
        }

        private void LoadProductData()
        {
            Name.Text = Product.name;
            Price.Text = Product.price.ToString();
            Count.Text = Product.stock_quantity.ToString();
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

                    // Устанавливаем текущую категорию
                    if (CategoryCb.Items.Count > 0)
                    {
                        CategoryCb.SelectedValue = Product.category_id;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void EditProduct_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveProductData();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool ValidateData()
        {
            // Проверка названия
            if (string.IsNullOrWhiteSpace(Name.Text))
            {
                MessageBox.Show("Введите название продукта", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Name.Focus();
                return false;
            }

            // Проверка цены
            if (string.IsNullOrWhiteSpace(Price.Text))
            {
                MessageBox.Show("Введите цену продукта", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Price.Focus();
                return false;
            }

            if (!decimal.TryParse(Price.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Цена должна быть положительным числом", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Price.Focus();
                Price.SelectAll();
                return false;
            }

            // Проверка количества
            if (string.IsNullOrWhiteSpace(Count.Text))
            {
                MessageBox.Show("Введите количество продукта", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Count.Focus();
                return false;
            }

            if (!int.TryParse(Count.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Количество должно быть неотрицательным целым числом", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Count.Focus();
                Count.SelectAll();
                return false;
            }

            // Проверка на уникальность названия продукта
            if (!IsProductNameUnique())
            {
                MessageBox.Show("Продукт с таким названием уже существует",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Name.Focus();
                return false;
            }

            return true;
        }

        private bool IsProductNameUnique()
        {
            using (var connection = new MySqlConnection(_connection))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT COUNT(*) FROM product 
                            WHERE name = @Name AND product_id != @ProductId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Name", Name.Text.Trim());
                    cmd.Parameters.AddWithValue("@ProductId", Product.product_id);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка проверки названия продукта: {ex.Message}");
                    return false;
                }
            }
        }

        private void SaveProductData()
        {
            decimal.TryParse(Price.Text, out decimal price);
            int.TryParse(Count.Text, out int stock);

            Product.name = Name.Text.Trim();
            Product.price = price;
            Product.stock_quantity = stock;

            if (CategoryCb.SelectedValue != null)
            {
                Product.category_id = (int)CategoryCb.SelectedValue;
            }
        }

        private void Price_TextChanged(object sender, EventArgs e)
        {
            // Убираем все нецифровые символы, кроме точки и запятой
            string text = Price.Text;
            string filteredText = new string(text.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());

            // Заменяем запятую на точку
            filteredText = filteredText.Replace(',', '.');

            // Проверяем, чтобы точка была только одна
            int dotCount = filteredText.Count(c => c == '.');
            if (dotCount > 1)
            {
                // Оставляем только первую точку
                int firstDotIndex = filteredText.IndexOf('.');
                filteredText = filteredText.Substring(0, firstDotIndex + 1) +
                              filteredText.Substring(firstDotIndex + 1).Replace(".", "");
            }

            if (filteredText != text)
            {
                int cursorPosition = Price.SelectionStart;
                Price.Text = filteredText;
                Price.SelectionStart = Math.Min(cursorPosition, filteredText.Length);
            }
        }

        private void Count_TextChanged(object sender, EventArgs e)
        {
            // Убираем все нецифровые символы
            string text = Count.Text;
            string filteredText = new string(text.Where(char.IsDigit).ToArray());

            if (filteredText != text)
            {
                int cursorPosition = Count.SelectionStart;
                Count.Text = filteredText;
                Count.SelectionStart = Math.Min(cursorPosition, filteredText.Length);
            }
        }
    }
}