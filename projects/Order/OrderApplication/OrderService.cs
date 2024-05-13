using AutoMapper;
using Domain;
using OrderApplication.DTO;
using OrderApplication.Interfaces;

namespace OrderApplication
{
    public class OrderService : IOrderService
    {

        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public OrderService(IOrderService orderService, IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return orders;
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return order;
        }
        public async Task<int> AddOrderAsync(AddOrderDTO orderDTO)
        {
            var user = _mapper.Map<Order>(orderDTO);
            var orderId = await _orderService.AddOrderAsync(orderDTO);
            return orderId;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var success = await _orderService.DeleteOrderAsync(id);
            return success;
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            var success = await _orderService.UpdateOrderAsync(order);
            return success;
        }
    }
}
