using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ynivermag_bad
{
    public class Connection
    {
        public static string Host { get; set; } = Properties.Settings.Default.host;
        public static string Database { get; set; } = Properties.Settings.Default.database;
        public static string UserId { get; set; } = Properties.Settings.Default.uid;
        public static string Password { get; set; } = Properties.Settings.Default.pwd;
        public static string ConnectionString => $"Server={Host};Database={Database};Uid={UserId};Pwd={Password};";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
