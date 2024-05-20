using CategoryApplication.DTO;
using CategoryApplication.Interfaces;
using CategoryInfrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace CategoryService.Controllers
{
    /// <summary>
    /// Class used for handling the http requests regarding the category crud operations
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService) 
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddCategory([FromBody] CreateCategoryDTO categoryDTO)
        {
            if (!ModelState.IsValid)
            {
                Log.Error("Invalid model state");
                return BadRequest(ModelState);
            }

            try
            {
                bool success = await _categoryService.AddCategoryAsync(categoryDTO);
                if (success)
                {
                    Log.Information("Category added successfully.");
                    return Ok(new { Message = "Category added successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to add category" });
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to add a category");
                return BadRequest(new { Message = "Error adding category", Error = ex.Message });
            }

        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            Log.Information("Fetched {CategoryCount} categories", categories.Count());
            return Ok(categories);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetCategoryById([FromRoute] string id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category != null)
                {
                    Log.Information("Category found: {CategoryId}", category._id);
                    return Ok(category);
                }
                else
                {
                    return NotFound(new { Message = "Category not found" });
                }
            }
            catch (KeyNotFoundException e)
            {
                Log.Error("Category not found with ID: {CategoryId}", id);
                return NotFound(new { Message = e.Message });
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateCategory([FromRoute] string id, [FromBody] UpdateCategoryDTO updateCategoryDTO)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid model state for category update");
                return BadRequest(ModelState);
            }

            try
            {
                bool updated = await _categoryService.UpdateCategoryAsync(id, updateCategoryDTO);
                if (updated)
                {
                    Log.Information("Category updated successfully: {CategoryId}", id);
                    return Ok(new { Message = "Category updated successfully" });
                }
                else
                {
                    return NotFound(new { Message = "Category not found for update" });
                }
            }
            catch (KeyNotFoundException e)
            {
                Log.Error("Category not found for update with ID: {CategoryId}", id);
                return NotFound(new { Message = e.Message });
            }
            catch (ArgumentException e)
            {
                Log.Error("Argument error for category update: {ErrorMessage}", e.Message);
                return BadRequest(new { Message = e.Message });
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] string id)
        {
            try
            {
                bool deleted = await _categoryService.DeleteCategoryAsync(id);
                if (deleted)
                {
                    Log.Information("Category deleted successfully: {CategoryId}", id);
                    return Ok(new { Message = "Category deleted successfully" });
                }
                else
                {
                    return NotFound(new { Message = "Category not found for deletion" });
                }
            }
            catch (KeyNotFoundException e)
            {
                Log.Error("Category not found for deletion with ID: {CategoryId}", id);
                return NotFound(new { Message = e.Message });
            }
        }

        [HttpPost]
        [Route("{categoryId}/add/{productId}")]
        public async Task<IActionResult> AddProductToCategory(string categoryId, string productId)
        {
            try
            {
                await _categoryService.AddProductToCategory(categoryId, productId);
                Log.Information("Product {ProductId} added to category with id {CategoryId}", productId, categoryId);
                return Ok(new { Message = "Product added to category successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                Log.Error("Failed to add product with {ProductId} to category with {CategoryId}", productId, categoryId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error("Failed to add product with id {ProductId} to category with id {CategoryId}", productId, categoryId);
                return BadRequest(new { Message = "Failed to add product to category.", Error = ex.Message });
            }
        }

        [HttpDelete]
        [Route("{categoryId}/remove/{productId}")]
        public async Task<IActionResult> RemoveProductFromCategory(string categoryId, string productId)
        {
            try
            {
                await _categoryService.RemoveProductFromCategory(categoryId, productId);
                Log.Information("Product with id {ProductId} removed from category with id {CategoryId}", productId, categoryId);
                return Ok(new { Message = "Product removed from category successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                Log.Error("Failed to remove product with id {ProductId} from category with id {CategoryId}", productId, categoryId);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error("Failed to remove product with id {ProductId} from category with id {CategoryId}", productId, categoryId);
                return BadRequest(new { Message = "Failed to remove product from category.", Error = ex.Message });
            }
        }

        [HttpDelete]
        [Route("removeProduct/{productId}")]
        public async Task<IActionResult> RemoveProductFromAllCategories(string productId)
        {
            try
            {
                await _categoryService.RemoveProductFromAllCategories(productId);
                Log.Information("Product with id {ProductId} removed from all categories", productId);
                return Ok(new { Message = "Product removed from all categories successfully." });
            }
            catch (Exception ex)
            {
                Log.Error("Failed to remove product with id {ProductId} from all categories", productId);
                return BadRequest(new { Message = "Failed to remove product from all categories.", Error = ex.Message });
            }
        }
    }
}
