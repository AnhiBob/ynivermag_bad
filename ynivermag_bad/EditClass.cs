using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    connection.Close();
                    return null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки клиента: {ex.Message}");
                    return null;
                }
            }
        }

        public void UpdateClientInDatabase(ClientModel client)
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
                    cmd.Parameters.AddWithValue("@Email", client.email);
                    cmd.Parameters.AddWithValue("@FirstName", client.first_name);
                    cmd.Parameters.AddWithValue("@LastName", client.last_name);
                    cmd.Parameters.AddWithValue("@Phone", client.phone);
                    cmd.Parameters.AddWithValue("@Address", client.address);
                    cmd.Parameters.AddWithValue("@ClientId", client.client_id);

                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления клиента: {ex.Message}");
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
                product_id,
                name,
                price,
                stock_quantity,
                category_id
            FROM product
            WHERE product_id = @ProductId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ProductId", productId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ProductModel
                            {
                                product_id = reader.GetInt32("product_id"),
                                name = reader["name"]?.ToString() ?? "",
                                price = reader.GetDecimal("price"),
                                stock_quantity = reader.GetInt32("stock_quantity"),
                                category_id = reader.GetInt32("category_id")
                            };
                        }
                    }
                    connection.Close();
                    return null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки продукта: {ex.Message}");
                    return null;
                }
            }
        }

        public void UpdateProductInDatabase(ProductModel product)
        {
            using (var connection = new MySqlConnection(Connection.ConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"UPDATE product 
                SET name = @Name,
                    price = @Price,
                    stock_quantity = @StockQuantity,
                    category_id = @CategoryId
                WHERE product_id = @ProductId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Name", product.name);
                    cmd.Parameters.AddWithValue("@Price", product.price);
                    cmd.Parameters.AddWithValue("@StockQuantity", product.stock_quantity);
                    cmd.Parameters.AddWithValue("@CategoryId", product.category_id);
                    cmd.Parameters.AddWithValue("@ProductId", product.product_id);

                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления продукта: {ex.Message}");
                }
            }
        }

        public UserModel LoadUserById(int userId)
        {
            Console.WriteLine($"Loading user with ID: {userId}"); // Отладочный вывод

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
                            var user = new UserModel
                            {
                                user_id = reader.GetInt32("user_id"),
                                username = reader["username"]?.ToString() ?? "",
                                password_hash = reader["password_hash"]?.ToString() ?? "",
                                email = reader["email"]?.ToString() ?? "",
                                first_name = reader["first_name"]?.ToString() ?? "",
                                last_name = reader["last_name"]?.ToString() ?? "",
                                role_id = reader.GetInt32("role_id")
                            };

                            Console.WriteLine($"User loaded successfully: {user.first_name} {user.last_name}");
                            return user;
                        }
                        else
                        {
                            Console.WriteLine($"No user found with ID: {userId}");
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading user: {ex.Message}");
                    MessageBox.Show($"Ошибка загрузки пользователя: {ex.Message}");
                    return null;
                }
            }
        }

        public void UpdateUserInDatabase(UserModel user)
        {
            using (var connection = new MySqlConnection(Connection.ConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"UPDATE user 
                SET username = @Username,
                    password_hash = @PasswordHash,
                    email = @Email,
                    first_name = @FirstName,
                    last_name = @LastName,
                    role_id = @RoleId
                WHERE user_id = @UserId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Username", user.username);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.password_hash);
                    cmd.Parameters.AddWithValue("@Email", user.email);
                    cmd.Parameters.AddWithValue("@FirstName", user.first_name);
                    cmd.Parameters.AddWithValue("@LastName", user.last_name);
                    cmd.Parameters.AddWithValue("@RoleId", user.role_id);
                    cmd.Parameters.AddWithValue("@UserId", user.user_id);

                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления пользователя: {ex.Message}");
                }
            }
        }
    }
}
