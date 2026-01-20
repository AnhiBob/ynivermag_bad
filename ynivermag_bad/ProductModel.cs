using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ynivermag_bad
{
    public class ProductModel
    {
        public int product_id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public int stock_quantity { get; set; }
        public int category_id { get; set; }
        public string description { get; set; }
        public string CategoryName { get; set; }
    }
}
