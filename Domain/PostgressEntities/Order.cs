using Domain.HelperEntities;

namespace Domain.PostgressEntities
{
    /// <summary>
    /// The Order entity used with PostgreSQL
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The time the order was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Id of the user
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// The totalprice of the Order
        /// </summary>
        public float TotalPrice { get; set; }
        /// <summary>
        /// The shipping address of the Order
        /// </summary>
        public string ShippingAddress { get; set; }
        /// <summary>
        /// List of products in the order
        /// </summary>
        public List<OrderProduct> Products { get; set; } = new List<OrderProduct>();

    }
}
