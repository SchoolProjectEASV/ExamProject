using Domain;
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
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int id);
        Task<int> AddOrderAsync(AddOrderDTO orderDTO);
        Task<bool> DeleteOrderAsync(int id);
        Task<bool> UpdateOrderAsync(UpdateOrderDTO updateOrderDTO);

        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
    }
}
