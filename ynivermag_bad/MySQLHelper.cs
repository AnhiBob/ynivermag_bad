using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ynivermag_bad
{
    public class MySQLHelper
    {
        private static string _connection = Connection.ConnectionString;
        static public string GetHash(string str)
        {
            var sha2 = SHA256.Create();
            var hbyte = sha2.ComputeHash(Encoding.UTF8.GetBytes(str));

            return BitConverter.ToString(hbyte).Replace("-", "").ToLower();
        }
        static public string GetRoleName(string login, string password)
        {
            string roleName = null;

            using (MySqlConnection con = new MySqlConnection(_connection))
            {
                con.Open();
                string query = @"SELECT 
                    CASE r.role_id 
                        WHEN 1 THEN 'Администратор'
                        WHEN 2 THEN 'Продавец' 
                        WHEN 3 THEN 'Товаровед'
                        ELSE 'Неизвестно'
                    END as role
                 FROM user u
                 JOIN role r ON u.role_id = r.role_id
                 WHERE u.username = @login AND u.password_hash = @password;";

                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);

                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    roleName = result.ToString();
                }
                // Не нужно явно закрывать con.Close(), т.к. using сделает это автоматически
            }

            return roleName;
        }



        // Получение фамилии и инициалов
        static public string GetLastNameWithInitials(string login, string password)
        {
            using (MySqlConnection con = new MySqlConnection(_connection))
            {
                con.Open();
                string query = @"SELECT first_name, last_name 
                           FROM user
                           WHERE username = @login AND password_hash = @password;";

                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string lastName = reader["last_name"]?.ToString() ?? "";
                        string firstName = reader["first_name"]?.ToString() ?? "";

                        return FormatLastNameWithInitials(lastName, firstName);
                    }
                }
            }

            return null;
        }

        // Форматирование фамилии с инициалами
        static private string FormatLastNameWithInitials(string lastName, string firstName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                return "Не указано";

            var initials = new List<string>();

            if (!string.IsNullOrWhiteSpace(firstName) && firstName.Length > 0)
                initials.Add(firstName[0] + ".");


            return lastName + (initials.Count > 0 ? " " + string.Join(" ", initials) : "");
        }
    }
}

