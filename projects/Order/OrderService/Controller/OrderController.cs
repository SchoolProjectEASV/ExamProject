using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderApplication.DTO;
using OrderApplication.Interfaces;
using Serilog;

namespace OrderService.Controllers
{
    /// <summary>
    /// Used for handling the http requests regarding the Order crud operations
    /// </summary>
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
            try
            {
                var orders = await _orderService.GetAllOrders();
                Log.Information("Fetched orders", orders);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                Log.Error("Error fetching orders: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { Message = "Error fetching orders", Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderById(id);
                if (order == null)
                {
                    Log.Warning("Order not found with ID: {OrderId}", id);
                    return NotFound(new { Message = "Order not found" });
                }

                Log.Information("Order found: {OrderId}", id);
                return Ok(order);
            }
            catch (Exception ex)
            {
                Log.Error("Error fetching order by ID: {OrderId}, Error: {ErrorMessage}", id, ex.Message);
                return StatusCode(500, new { Message = "Error fetching order", Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddOrder([FromBody] AddOrderDTO orderDTO)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid model state for adding order");
                return BadRequest(ModelState);
            }

            try
            {
                var orderId = await _orderService.AddOrder(orderDTO);
                Log.Information("Order added successfully: {OrderId}", orderId);
                return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, orderId);
            }
            catch (Exception ex)
            {
                Log.Error("Error adding order: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { Message = "Error adding order", Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDTO updateOrderDTO)
        {
            if (id != updateOrderDTO.Id)
            {
                Log.Warning("Order ID mismatch for update. Provided ID: {OrderId}, Update DTO ID: {UpdateOrderId}", id, updateOrderDTO.Id);
                return BadRequest(new { Message = "Order ID mismatch" });
            }

            try
            {
                var success = await _orderService.UpdateOrder(updateOrderDTO);
                if (!success)
                {
                    Log.Warning("Order not found for update with ID: {OrderId}", id);
                    return NotFound(new { Message = "Order not found" });
                }

                Log.Information("Order updated successfully: {OrderId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                Log.Error("Error updating order: {OrderId}, Error: {ErrorMessage}", id, ex.Message);
                return StatusCode(500, new { Message = "Error updating order", Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var success = await _orderService.DeleteOrder(id);
                if (!success)
                {
                    Log.Warning("Order not found for deletion with ID: {OrderId}", id);
                    return NotFound(new { Message = "Order not found" });
                }

                Log.Information("Order deleted successfully: {OrderId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                Log.Error("Error deleting order: {OrderId}, Error: {ErrorMessage}", id, ex.Message);
                return StatusCode(500, new { Message = "Error deleting order", Error = ex.Message });
            }
        }

        [HttpPost("addProduct")]
        public async Task<IActionResult> AddProductToOrder([FromBody] AddProductToOrderDTO dto)
        {
            try
            {
                var success = await _orderService.AddProductToOrder(dto);
                if (!success)
                {
                    return BadRequest("Failed to add product to order");
                }

                return Ok("Product added to order successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOrdersByUserId(int userId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByUserId(userId);
                Log.Information("Fetched orders for user: {UserId}", userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                Log.Error("Error fetching orders for user: {UserId}, Error: {ErrorMessage}", userId, ex.Message);
                return StatusCode(500, new { Message = "Error fetching orders", Error = ex.Message });
            }
        }
    }
}
