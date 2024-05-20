using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ProductApplication.DTO;
using ProductApplication.Interfaces;
using Serilog;

namespace ProductService.Controllers
{
    /// <summary>
    /// Class used for handling the http requests regarding the product crud operations
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddProduct([FromBody] CreateProductDTO productDTO)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid model state");
                return BadRequest(ModelState);
            }

            try
            {
                bool success = await _productService.AddProductAsync(productDTO);
                if (success)
                {
                    Log.Information("Product added successfully.");
                    return Ok(new { Message = "Product added successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to add product" });
                }
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to add a product");
                return BadRequest(new { Message = "Error adding product", Error = ex.Message });
            }

        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            Log.Information("Fetched {ProductCount} products", products.Count());
            return Ok(products);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetProductById([FromRoute] string id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product != null)
                {
                    Log.Information("Product found: {ProductId}", product._id);
                    return Ok(product);
                }
                else
                {
                    return NotFound(new { Message = "Product not found" });
                }
            }
            catch (KeyNotFoundException e)
            {
                Log.Warning("Product not found with ID: {ProductId}", id);
                return NotFound(new { Message = e.Message });
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] string id, [FromBody] UpdateProductDTO updateProductDTO)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid model state for product update");
                return BadRequest(ModelState);
            }

            try
            {
                bool updated = await _productService.UpdateProductAsync(id, updateProductDTO);
                if (updated)
                {
                    Log.Information("Product updated successfully: {ProductId}", id);
                    return Ok(new { Message = "Product updated successfully" });
                }
                else
                {
                    return NotFound(new { Message = "Product not found for update" });
                }
            }
            catch (KeyNotFoundException e)
            {
                Log.Warning("Product not found for update with ID: {ProductId}", id);
                return NotFound(new { Message = e.Message });
            }
            catch (ArgumentException e)
            {
                Log.Warning("Argument error for product update: {ErrorMessage}", e.Message);
                return BadRequest(new { Message = e.Message });
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] string id)
        {
            try
            {
                bool deleted = await _productService.DeleteProductAsync(id);
                if (deleted)
                {
                    Log.Information("Product deleted successfully: {ProductId}", id);
                    return Ok(new { Message = "Product deleted successfully" });
                }
                else
                {
                    return NotFound(new { Message = "Product not found for deletion" });
                }
            }
            catch (KeyNotFoundException e)
            {
                Log.Warning("Product not found for deletion with ID: {ProductId}", id);
                return NotFound(new { Message = e.Message });
            }
        }
    }
}