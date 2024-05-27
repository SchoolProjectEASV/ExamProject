using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.DTO
{
    public class AddProductToOrderDTO
    {
        public int OrderId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
