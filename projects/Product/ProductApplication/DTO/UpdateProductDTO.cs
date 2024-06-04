using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApplication.DTO
{
    /// <summary>
    /// The DTO used to update a product
    /// </summary>
    public class UpdateProductDTO
    {
        /// <summary>
        /// The product name
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// The product description
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// The product price
        /// </summary>
        public float Price { get; set; }
        /// <summary>
        /// The quantity of the product
        /// </summary>
        public int Quantity { get; set; }
    }
}
