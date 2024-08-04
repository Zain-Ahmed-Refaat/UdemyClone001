using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UdemyClone.Dto;
using UdemyClone.Models;
using UdemyClone.Services.IServices;

namespace UdemyClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService courseService;

        public CourseController(ICourseService courseService)
        {
            this.courseService = courseService;
        }

        [HttpPost("Upload-Lesson")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UploadLessonAsync([FromBody] LessonModel model)
        {
            if (model == null)         
                return BadRequest("Lesson model cannot be null.");
            
            var instructorId = GetIdFromToken();

            try
            {
                var lessonDto = await courseService.UploadLessonAsync(model, instructorId);
                return CreatedAtAction(nameof(GetLessonById), new { id = lessonDto.LessonId }, lessonDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            { 
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }


        [HttpGet("Get-All-Lessons")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetAllLessonsAsync(Guid courseId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var instructorId = GetIdFromToken();

                var lessons = await courseService.GetAllLessonsAsync(instructorId, courseId, pageNumber, pageSize);
                return Ok(lessons);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Get-Lesson-By-ID")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetLessonById(Guid id)
        {

            var instructorId = GetIdFromToken();

            try
            {
                var lesson = await courseService.GetLessonByIdAsync(id, instructorId);

                if (lesson == null)
                    return NotFound("Lesson not found.");

                return Ok(lesson);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private Guid GetIdFromToken()
        {
            var userIdClaim = User.FindFirstValue("UserID");

            var userID = string.IsNullOrEmpty(userIdClaim) ?
                         throw new UnauthorizedAccessException("User ID claim not found in the token.") :
                          Guid.Parse(userIdClaim);

            return userID;
        }
    }
}
