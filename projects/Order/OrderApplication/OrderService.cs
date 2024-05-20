using AutoMapper;
using Domain;
using OrderApplication.DTO;
using OrderApplication.Interfaces;
using OrderInfrastructure;
using OrderInfrastructure.Interfaces;

namespace OrderApplication
{
    public class OrderService : IOrderService
    {

        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            return orders;
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            return order;
        }
        public async Task<int> AddOrderAsync(AddOrderDTO orderDTO)
        {
            var order = _mapper.Map<Order>(orderDTO);
            order.CreatedAt = DateTime.Now;
            var orderId = await _orderRepository.AddOrderAsync(order);
            return orderId;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var success = await _orderRepository.DeleteOrderAsync(id);
            return success;
        }

        public async Task<bool> UpdateOrderAsync(UpdateOrderDTO updateOrderDTO)
        {
            var order = _mapper.Map<Order>(updateOrderDTO);

            var success = await _orderRepository.UpdateOrderAsync(order);
            return success;
        }
    }
}
