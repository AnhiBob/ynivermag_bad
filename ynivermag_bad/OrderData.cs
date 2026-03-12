using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ynivermag_bad
{
    public class OrderData
    {
        public int OrderId { get; set; }
        public string ClientName { get; set; }
        public string UserName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string Products { get; set; }
    }

    public class DateRange
    {
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
    }
}
