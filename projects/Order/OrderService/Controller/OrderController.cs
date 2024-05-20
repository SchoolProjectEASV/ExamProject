using Domain;
using Microsoft.AspNetCore.Mvc;
using OrderApplication.DTO;
using OrderApplication.Interfaces;

namespace OrderService.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrder([FromBody] AddOrderDTO orderDTO)
        {
            var orderId = await _orderService.AddOrderAsync(orderDTO);
            return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, orderId);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDTO updateOrderDTO)
        {
            if (id != updateOrderDTO.Id)
            {
                return BadRequest();
            }

            var success = await _orderService.UpdateOrderAsync(updateOrderDTO);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var success = await _orderService.DeleteOrderAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
