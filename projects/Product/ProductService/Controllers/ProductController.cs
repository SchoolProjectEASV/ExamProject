using Microsoft.AspNetCore.Mvc;
using ProductApplication.DTO;
using ProductApplication.Interfaces;
using Serilog;

namespace ProductService.Controllers
{
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

            var createdProduct = await _productService.AddProductAsync(productDTO);
            Log.Information("Product added successfully with ID: {ProductId}", createdProduct._id);
            return Ok(new { Message = "Product added successfully", Product = createdProduct });
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
                Log.Information("Product found: {ProductId}", product._id);
                return Ok(product);
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
                var updatedProduct = await _productService.UpdateProductAsync(id, updateProductDTO);
                Log.Information("Product updated successfully: {ProductId}", updatedProduct._id);

                return Ok(new { Message = "Product updated successfully", Product = updatedProduct });
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
                var deletedProduct = await _productService.DeleteProductAsync(id);
                Log.Information("Product deleted successfully: {ProductId}", deletedProduct._id);

                return Ok(new { Message = "Product deleted successfully", Product = deletedProduct });
            }
            catch (KeyNotFoundException e)
            {
                Log.Warning("Product not found for deletion with ID: {ProductId}", id);
                return NotFound(new { Message = e.Message });
            }
        }
    }
}
