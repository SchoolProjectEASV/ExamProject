using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.MongoEntities
{
    /// <summary>
    /// The category entity used with mongodb
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Unique identifier 
        /// </summary>
        public ObjectId _id { get; set; }
        /// <summary>
        /// The date the category was created/added
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// The name of the category
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// The description of the category
        /// </summary>
        public string? Description { get; set; }
    }
}
