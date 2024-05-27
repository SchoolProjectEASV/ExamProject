using Domain.PostgressEntities;
using OrderApplication.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.Interfaces
{
    public interface IOrderService
    {
        /// <summary>
        /// Gets all orders.
        /// </summary>
        /// <returns>Returns a collection of orders</returns>
        public Task<IEnumerable<Order>> GetAllOrders();

        /// <summary>
        /// Gets an order by it's ID
        /// </summary>
        /// <param name="id">The ID of the order</param>
        /// <returns>Returns the order being requested</returns>
        public Task<Order> GetOrderById(int id);

        /// <summary>
        /// Adds an order to the table
        /// </summary>
        /// <param name="orderDTO">The DTO with all the required parameters for making an order</param>
        /// <returns></returns>
        Task<int> AddOrder(AddOrderDTO orderDTO);

        /// <summary>
        /// Deletes an order from the table
        /// </summary>
        /// <param name="id">The ID of the order being deleted</param>
        /// <returns>Returns the id of the deleted order, with a message that specifies if an order has been deleted or not succesfully</returns>
        Task<bool> DeleteOrder(int id);

        /// <summary>
        /// Updates an order in the table
        /// </summary>
        /// <param name="updateOrderDTO">DTO with all the required parameters for updating an order</param>
        /// <returns></returns>
        
        Task<bool> UpdateOrder(UpdateOrderDTO updateOrderDTO);

        /// <summary>
        /// Gets an order by the user id
        /// </summary>
        /// <param name="userId">The user ID for whom one needs the orders from</param>
        /// <returns></returns>

        public Task<IEnumerable<Order>> GetOrdersByUserId(int userId);

        /// <summary>
        /// Adds a product to an order
        /// </summary>
        /// <param name="dto">A DTO, that specifies the required parameters for adding a product to an order</param>
        /// <returns>Returns a id of the product id and the order id, and specifies if a product has been added to an order or not</returns>
        public Task<bool> AddProductToOrder(AddProductToOrderDTO dto);
    }
}
