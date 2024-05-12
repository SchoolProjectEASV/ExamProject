using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CategoryApplication.DTO
{
    /// <summary>
    /// The DTO used to update a category
    /// </summary>
    public class UpdateCategoryDTO
    {
        /// <summary>
        /// The category name
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// The category description
        /// </summary>
        public string? Description { get; set; }
    }
}
