using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace ynivermag_bad
{
    public class EditClass
    {
        private string _connection = Connection.ConnectionString;

        private MySqlConnection GetNewConnection()
        {
            return new MySqlConnection(_connection);
        }

        #region Загрузка данных

        public ClientModel LoadClientById(int clientId)
        {
            using (var connection = new MySqlConnection(Connection.ConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                        client_id,
                        email,
                        first_name,
                        last_name,
                        phone,
                        address
                    FROM client
                    WHERE client_id = @ClientId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ClientId", clientId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ClientModel
                            {
                                client_id = reader.GetInt32("client_id"),
                                email = reader["email"]?.ToString() ?? "",
                                first_name = reader["first_name"]?.ToString() ?? "",
                                last_name = reader["last_name"]?.ToString() ?? "",
                                phone = reader["phone"]?.ToString() ?? "",
                                address = reader["address"]?.ToString() ?? ""
                            };
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки клиента: {ex.Message}");
                    return null;
                }
            }
        }

        public ProductModel LoadProductById(int productId)
        {
            using (var connection = new MySqlConnection(Connection.ConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                        p.product_id, 
                        p.name, 
                        p.price, 
                        p.stock_quantity, 
                        p.category_id, 
                        p.photo_path,
                        c.name as category_name,
                        p.isActive
                    FROM product p
                    LEFT JOIN category c ON p.category_id = c.category_id
                    WHERE p.product_id = @ProductId";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", productId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var product = new ProductModel
                                {
                                    product_id = reader.GetInt32("product_id"),
                                    name = reader.GetString("name"),
                                    price = reader.GetDecimal("price"),
                                    stock_quantity = reader.GetInt32("stock_quantity"),
                                    photo_path = reader.IsDBNull(reader.GetOrdinal("photo_path")) ?
                                        null : reader.GetString("photo_path"),
                                    isActive = reader.GetBoolean("isActive")
                                };

                                // ВАЖНО: правильная обработка NULL для category_id
                                if (!reader.IsDBNull(reader.GetOrdinal("category_id")))
                                {
                                    product.category_id = reader.GetInt32("category_id");
                                    product.category_name = reader["category_name"]?.ToString();
                                }
                                else
                                {
                                    product.category_id = null;
                                    product.category_name = null;
                                }

                                return product;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки продукта: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return null;
        }

        public UserModel LoadUserById(int userId)
        {
            using (var connection = new MySqlConnection(Connection.ConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                        user_id,
                        username,
                        password_hash,
                        email,
                        first_name,
                        last_name,
                        role_id
                    FROM user
                    WHERE user_id = @UserId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserModel
                            {
                                user_id = reader.GetInt32("user_id"),
                                username = reader["username"]?.ToString() ?? "",
                                password_hash = reader["password_hash"]?.ToString() ?? "",
                                email = reader["email"]?.ToString() ?? "",
                                first_name = reader["first_name"]?.ToString() ?? "",
                                last_name = reader["last_name"]?.ToString() ?? "",
                                role_id = reader.GetInt32("role_id")
                            };
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки пользователя: {ex.Message}");
                    return null;
                }
            }
        }

        #endregion

        #region Обновление данных

        public bool UpdateClientInDatabase(ClientModel client)
        {
            using (var connection = new MySqlConnection(Connection.ConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"UPDATE client 
                        SET email = @Email,
                            first_name = @FirstName,
                            last_name = @LastName,
                            phone = @Phone,
                            address = @Address
                        WHERE client_id = @ClientId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Email", client.email ?? "");
                    cmd.Parameters.AddWithValue("@FirstName", client.first_name ?? "");
                    cmd.Parameters.AddWithValue("@LastName", client.last_name ?? "");
                    cmd.Parameters.AddWithValue("@Phone", client.phone ?? "");
                    cmd.Parameters.AddWithValue("@Address", client.address ?? "");
                    cmd.Parameters.AddWithValue("@ClientId", client.client_id);

                    int affected = cmd.ExecuteNonQuery();
                    return affected > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления клиента: {ex.Message}");
                    return false;
                }
            }
        }

        public bool UpdateProductInDatabase(ProductModel product)
        {
            using (var connection = new MySqlConnection(Connection.ConnectionString))
            {
                try
                {
                    connection.Open();

                    string query;
                    MySqlCommand cmd;

                    // Проверяем, нужно ли обновлять photo_path
                    if (!string.IsNullOrEmpty(product.photo_path))
                    {
                        query = @"UPDATE product 
                               SET name = @Name,
                                   price = @Price,
                                   stock_quantity = @StockQuantity,
                                   category_id = @CategoryId,
                                   photo_path = @PhotoPath
                               WHERE product_id = @ProductId";

                        cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@PhotoPath", product.photo_path);
                    }
                    else
                    {
                        query = @"UPDATE product 
                               SET name = @Name,
                                   price = @Price,
                                   stock_quantity = @StockQuantity,
                                   category_id = @CategoryId,
                                   photo_path = NULL
                               WHERE product_id = @ProductId";

                        cmd = new MySqlCommand(query, connection);
                    }

                    cmd.Parameters.AddWithValue("@ProductId", product.product_id);
                    cmd.Parameters.AddWithValue("@Name", product.name ?? "");
                    cmd.Parameters.AddWithValue("@Price", product.price);
                    cmd.Parameters.AddWithValue("@StockQuantity", product.stock_quantity);

                    // ВАЖНО: правильная обработка NULL для category_id
                    if (product.category_id.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@CategoryId", product.category_id.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@CategoryId", DBNull.Value);
                    }

                    int affected = cmd.ExecuteNonQuery();
                    return affected > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления продукта: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        public bool UpdateUserInDatabase(UserModel user)
        {
            using (var connection = new MySqlConnection(Connection.ConnectionString))
            {
                try
                {
                    connection.Open();

                    string query;
                    MySqlCommand cmd;

                    // Если пароль изменен
                    if (!string.IsNullOrEmpty(user.password_hash))
                    {
                        query = @"UPDATE user 
                                SET username = @Username,
                                    password_hash = @PasswordHash,
                                    email = @Email,
                                    first_name = @FirstName,
                                    last_name = @LastName,
                                    role_id = @RoleId
                                WHERE user_id = @UserId";

                        cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@PasswordHash", user.password_hash);
                    }
                    else
                    {
                        query = @"UPDATE user 
                                SET username = @Username,
                                    email = @Email,
                                    first_name = @FirstName,
                                    last_name = @LastName,
                                    role_id = @RoleId
                                WHERE user_id = @UserId";

                        cmd = new MySqlCommand(query, connection);
                    }

                    cmd.Parameters.AddWithValue("@Username", user.username ?? "");
                    cmd.Parameters.AddWithValue("@Email", user.email ?? "");
                    cmd.Parameters.AddWithValue("@FirstName", user.first_name ?? "");
                    cmd.Parameters.AddWithValue("@LastName", user.last_name ?? "");
                    cmd.Parameters.AddWithValue("@RoleId", user.role_id);
                    cmd.Parameters.AddWithValue("@UserId", user.user_id);

                    int affected = cmd.ExecuteNonQuery();
                    return affected > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления пользователя: {ex.Message}");
                    return false;
                }
            }
        }

        #endregion

        #region Вспомогательные методы

        public DataTable LoadCategories()
        {
            DataTable dt = new DataTable();
            using (var connection = new MySqlConnection(Connection.ConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT category_id, name FROM category ORDER BY name";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    adapter.Fill(dt);

                    // Добавляем пустую строку для возможности не выбирать категорию
                    DataRow emptyRow = dt.NewRow();
                    emptyRow["category_id"] = DBNull.Value;
                    emptyRow["name"] = "— Без категории —";
                    dt.Rows.InsertAt(emptyRow, 0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
                }
            }
            return dt;
        }

        public DataTable LoadRoles()
        {
            DataTable dt = new DataTable();
            using (var connection = new MySqlConnection(Connection.ConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT role_id, role_name FROM role ORDER BY role_name";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    adapter.Fill(dt);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}");
                }
            }
            return dt;
        }

        #endregion
    }
}