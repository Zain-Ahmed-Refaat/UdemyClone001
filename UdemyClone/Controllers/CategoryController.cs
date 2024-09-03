using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UdemyClone.Entities;
using UdemyClone.Services.IServices;

namespace UdemyClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IBaseRepository<Category> categoryService;

        public CategoryController(IBaseRepository<Category> categoryService)
        {
            this.categoryService = categoryService;
        }

        [HttpPost("Create-Category")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory(string categoryName)
        {
            try
            {
                var category = new Category { Name = categoryName };
                var createdCategory = await categoryService.CreateAsync(category);
                return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Update-Category")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(Guid categoryId, string newCategoryName)
        {
            try
            {
                var category = await categoryService.GetByIdAsync(categoryId);
                if (category == null)
                    return NotFound("Category not found.");

                category.Name = newCategoryName;
                var updatedCategory = await categoryService.UpdateAsync(category);
                return Ok(updatedCategory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Get-Category-By-Id")]
        [Authorize]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var category = await categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        [HttpGet("Get-All-Categories")]
        public async Task<IActionResult> GetAllCategories(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var categories = await categoryService.GetAllAsync();
                var pagedCategories = categories.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                return Ok(pagedCategories);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Search-Categories")]
        public async Task<IActionResult> SearchCategories([FromQuery] string searchTerm)
        {
            try
            {
                var categories = await categoryService.SearchAsync(searchTerm);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("Delete-Category")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(Guid categoryId)
        {
            try
            {
                var category = await categoryService.GetByIdAsync(categoryId);
                if (category == null)
                    return NotFound("Category not found.");

                var result = await categoryService.DeleteAsync(category);
                return result ? Ok("Category successfully deleted.") : BadRequest("Failed to delete category.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
