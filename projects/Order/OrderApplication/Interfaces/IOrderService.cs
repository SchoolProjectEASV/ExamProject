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
        public Task<IEnumerable<Order>> GetAllOrdersAsync();
        public Task<Order> GetOrderByIdAsync(int id);
        Task<int> AddOrderAsync(AddOrderDTO orderDTO);
        Task<bool> DeleteOrderAsync(int id);
        Task<bool> UpdateOrderAsync(UpdateOrderDTO updateOrderDTO);

        public Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
        public Task<bool> AddProductToOrderAsync(AddProductToOrderDTO dto);
    }
}
