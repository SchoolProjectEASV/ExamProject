using MongoDB.Bson;

namespace Domain.MongoEntities
{
    public class Product
    {
        public ObjectId _id { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public float Price { get; set; }

        public int Quantity { get; set; }

    }
}
