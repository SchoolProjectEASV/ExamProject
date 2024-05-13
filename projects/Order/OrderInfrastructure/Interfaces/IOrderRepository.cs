using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace OrderInfrastructure.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int id);
        Task<int> AddOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int id);
        Task<bool> UpdateOrderAsync(Order order);

    }
}
