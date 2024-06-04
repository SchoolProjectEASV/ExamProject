using MongoDB.Bson;

namespace Domain.MongoEntities
{
    /// <summary>
    /// The product entity used with mongodb
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public ObjectId _id { get; set; }
        /// <summary>
        /// The date the product was created / added
        /// </summary>
        public DateTime CreatedAt { get; set; }
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
        /// The product quantity
        /// </summary>
        public int Quantity { get; set; }

    }
}
