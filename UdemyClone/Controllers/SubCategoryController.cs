using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UdemyClone.Dto;
using UdemyClone.Entities;
using UdemyClone.Services.IServices;

namespace UdemyClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubCategoryController : ControllerBase
    {
        private readonly IBaseRepository<SubCategory> subCategoryService;

        public SubCategoryController(IBaseRepository<SubCategory> subCategoryService)
        {
            this.subCategoryService = subCategoryService;
        }

        [HttpPost("Create-SubCategory")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSubCategory([FromBody] CreateSubCategoryDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name) || dto.CategoryId == Guid.Empty)
                return BadRequest("Invalid data.");

            try
            {
                var subCategory = new SubCategory
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    CategoryId = dto.CategoryId
                };

                await subCategoryService.CreateAsync(subCategory);
                return CreatedAtAction(nameof(GetSubCategoryById), new { id = subCategory.Id }, subCategory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Update-SubCategory")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSubCategory([FromBody] SubCategoryUpdateDto updateDto)
        {
            if (updateDto == null || updateDto.Id == Guid.Empty || string.IsNullOrWhiteSpace(updateDto.Name))
                return BadRequest("Invalid data.");

            try
            {
                var subCategory = await subCategoryService.GetByIdAsync(updateDto.Id);
                if (subCategory == null)
                {
                    return NotFound("SubCategory not found.");
                }

                subCategory.Name = updateDto.Name;
                subCategory.CategoryId = updateDto.CategoryId;

                await subCategoryService.UpdateAsync(subCategory);
                return Ok(subCategory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, dbEx.InnerException?.Message ?? dbEx.Message);
            }
        }

        [HttpGet("Get-SubCategory-By-Id")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSubCategoryById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Invalid SubCategory ID.");

            try
            {
                var subCategory = await subCategoryService.GetByIdAsync(id);
                if (subCategory == null)
                    return NotFound("SubCategory not found.");

                return Ok(subCategory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Get-All-SubCategories")]
        public async Task<IActionResult> GetAllSubCategories(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Page number and page size must be greater than zero.");

            try
            {
                var subCategories = await subCategoryService.GetAllAsync();
                var pagedResult = subCategories
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(pagedResult);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Search-SubCategories")]
        public async Task<IActionResult> SearchSubCategories([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term cannot be null or empty.");

            try
            {
                var subCategories = await subCategoryService.SearchAsync(searchTerm);
                return Ok(subCategories);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("Delete-SubCategory")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSubCategory(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Invalid SubCategory ID.");

            try
            {
                var subCategory = await subCategoryService.GetByIdAsync(id);
                if (subCategory == null)
                    return NotFound("SubCategory not found.");

                var result = await subCategoryService.DeleteAsync(subCategory);
                if (result)
                    return NoContent();
                else
                    return BadRequest("Failed to delete SubCategory.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
