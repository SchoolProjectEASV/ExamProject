using AutoMapper;
using OrderApplication.DTO;
using OrderApplication.Interfaces;
using OrderInfrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Domain.PostgressEntities;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Domain.MongoEntities;
using System.Text.Json;
using StackExchange.Redis;
using System.Threading.Tasks;
using Domain.HelperEntities;

namespace OrderApplication
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly IConnectionMultiplexer _redis;
        private readonly string _productServiceUrl;

        public OrderService(IOrderRepository orderRepository, IMapper mapper, IHttpClientFactory httpClientFactory, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient();
            _productServiceUrl = configuration["ProductService:Url"];
            _httpClient.BaseAddress = new Uri(_productServiceUrl);
            _redis = redis;
        }

        public async Task<IEnumerable<Domain.PostgressEntities.Order>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            var orderDTOs = new List<Domain.PostgressEntities.Order>();

            foreach (var order in orders)
            {
                var orderDTO = _mapper.Map<Domain.PostgressEntities.Order> (order);
                orderDTO.Products = await GetProductsForOrder(order.Id);
                orderDTOs.Add(orderDTO);
            }

            return orderDTOs;
        }

        public async Task<Domain.PostgressEntities.Order> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order with the id {id} was not found");
            }

            var orderDTO = _mapper.Map<Domain.PostgressEntities.Order>(order);
            orderDTO.Products = await GetProductsForOrder(order.Id);
            return orderDTO;
        }

        public async Task<int> AddOrderAsync(AddOrderDTO orderDTO)
        {
            var order = _mapper.Map<Domain.PostgressEntities.Order>(orderDTO);
            order.CreatedAt = DateTime.UtcNow;
            order.TotalPrice = await CalculateTotalPrice(orderDTO.Products);

            var orderId = await _orderRepository.AddOrderAsync(order);

            var productIds = orderDTO.Products.Select(p => p.ProductId).ToList();
            var db = _redis.GetDatabase();
            await db.StringSetAsync(GetRedisKeyForOrder(orderId), JsonSerializer.Serialize(productIds));

            return orderId;
        }

        public async Task<bool> AddProductToOrderAsync(AddProductToOrderDTO dto)
        {
            var order = await _orderRepository.GetOrderByIdAsync(dto.OrderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order with the id {dto.OrderId} was not found");
            }

            var productPrice = await GetProductPrice(dto.ProductId);
            order.TotalPrice += productPrice * dto.Quantity;

            var success = await _orderRepository.UpdateOrderAsync(order);

            if (success)
            {
                var productIds = await GetProductIdsForOrder(dto.OrderId);
                productIds.Add(dto.ProductId);
                var db = _redis.GetDatabase();
                await db.StringSetAsync(GetRedisKeyForOrder(dto.OrderId), JsonSerializer.Serialize(productIds));
            }

            return success;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var success = await _orderRepository.DeleteOrderAsync(id);
            if (success)
            {
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(GetRedisKeyForOrder(id));
            }
            return success;
        }

        public async Task<bool> UpdateOrderAsync(UpdateOrderDTO updateOrderDTO)
        {
            var order = _mapper.Map<Domain.PostgressEntities.Order>(updateOrderDTO);
            var success = await _orderRepository.UpdateOrderAsync(order);
            return success;
        }

        public async Task<IEnumerable<Domain.PostgressEntities.Order>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            var orderDTOs = new List<Domain.PostgressEntities.Order>();

            foreach (var order in orders)
            {
                var orderDTO = _mapper.Map<Domain.PostgressEntities.Order>(order);
                orderDTO.Products = await GetProductsForOrder(order.Id);
                orderDTOs.Add(orderDTO);
            }

            return orderDTOs;
        }

        private async Task<List<OrderProduct>> GetProductsForOrder(int orderId)
        {
            var products = new List<OrderProduct>();
            var productIds = await GetProductIdsForOrder(orderId);

            foreach (var productId in productIds)
            {
                var productResponse = await _httpClient.GetAsync($"/Product/{productId}");
                if (productResponse.IsSuccessStatusCode)
                {
                    var productContent = await productResponse.Content.ReadAsStringAsync();
                    var product = JsonSerializer.Deserialize<Product>(productContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    products.Add(new OrderProduct
                    {
                        ProductId = product._id.ToString(),
                        Name = product.Name,
                        Price = product.Price,
                        Quantity = 1
                    });
                }
            }
            return products;
        }

        private async Task<List<string>> GetProductIdsForOrder(int orderId)
        {
            var db = _redis.GetDatabase();
            var productIdsJson = await db.StringGetAsync(GetRedisKeyForOrder(orderId));
            if (!string.IsNullOrEmpty(productIdsJson))
            {
                return JsonSerializer.Deserialize<List<string>>(productIdsJson);
            }
            return new List<string>();
        }

        private async Task<float> CalculateTotalPrice(List<OrderProductDTO> products)
        {
            float totalPrice = 0;
            foreach (var product in products)
            {
                var productPrice = await GetProductPrice(product.ProductId);
                totalPrice += productPrice * product.Quantity;
            }
            return totalPrice;
        }

        private async Task<float> GetProductPrice(string productId)
        {
            var productResponse = await _httpClient.GetAsync($"/Product/{productId}");
            if (!productResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to retrieve product details for ID: {productId}");
            }

            var productContent = await productResponse.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<Product>(productContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return product.Price;
        }

        private string GetRedisKeyForOrder(int orderId)
        {
            return $"order:{orderId}:products";
        }
    }
}
