using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ynivermag_bad
{
    public class ProductModel
    {
        public int product_id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public int stock_quantity { get; set; }
        public int? category_id { get; set; } // ВАЖНО: nullable
        public string category_name { get; set; }
        public string description { get; set; }
        public string photo_path { get; set; }
        public bool isActive { get; set; }

        // Для хранения изображения в памяти
        public Image ProductImage { get; set; }
        public byte[] PhotoBytes { get; set; }
    }
}
