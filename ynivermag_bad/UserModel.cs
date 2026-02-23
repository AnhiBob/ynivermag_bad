using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ynivermag_bad
{
    public class UserModel
    {
        public int user_id { get; set; }
        public string username { get; set; }
        public string password_hash { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int role_id { get; set; }
        public string RoleName { get; set; }
    }

    public class OrderItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int AvailableStock { get; set; }
        public decimal Total => Price * Quantity;
    }
}
