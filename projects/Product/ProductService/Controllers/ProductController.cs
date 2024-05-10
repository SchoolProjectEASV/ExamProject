using Microsoft.AspNetCore.Mvc;
using ProductApplication.DTO;
using ProductApplication.Interfaces;

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
                return BadRequest(ModelState);
            }

            var createdProduct = await _productService.AddProductAsync(productDTO);
            return Ok(new { Message = "Product added successfully", Product = createdProduct });
        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetProductById([FromRoute] string id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                return Ok(product);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(new { Message = e.Message });
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] string id, [FromBody] UpdateProductDTO updateProductDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedProduct = await _productService.UpdateProductAsync(id, updateProductDTO);
                return Ok(new { Message = "Product updated successfully", Product = updatedProduct });
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(new { Message = e.Message });
            }
            catch (ArgumentException e)
            {
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
                return Ok(new { Message = "Product deleted successfully", Product = deletedProduct });
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(new { Message = e.Message });
            }
        }
    }
}
