using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UdemyClone.Models;
using UdemyClone.Services.IServices;

namespace UdemyClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorController : ControllerBase
    {
        private readonly IInstructorService instructorService;

        public InstructorController(IInstructorService instructorService)
        {
            this.instructorService = instructorService;
        }

        [HttpPost("Upload-Course")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseModel model)
        {
            var instructorId = GetIdFromToken();

            try
            {
                var courseDto = await instructorService.CreateCourseAsync(model, instructorId);
                return CreatedAtAction(nameof(GetCourseById), new { id = courseDto.Id }, courseDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        [HttpPut("Update-Course")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateCourse(Guid CourseId, [FromBody] CourseModel model)
        {
            var instructorId = GetIdFromToken();

            try
            {
                var updatedCourse = await instructorService.UpdateCourseAsync(CourseId, model, instructorId);

                if (updatedCourse == null)
                {
                    return NotFound("Course not found or you are not authorized to update this course.");
                }

                return Ok(updatedCourse);
            }
            catch(ArgumentNullException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("Get-My-Courses-Instructor")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetCoursesByInstructor()
        {
            var instructorId = GetIdFromToken();

            try
            {
                var courses = await instructorService.GetCoursesByInstructorAsync(instructorId);

                if (courses == null || !courses.Any())
                    return NotFound("No courses found for the given instructor.");
                

                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        [HttpGet("My-Courses-Enrollments")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetInstructorCoursesEnrollments()
        {
            var instructorId = GetIdFromToken();

            try
            {
                var enrollments = await instructorService.GetInstructorCoursesEnrollmentsAsync(instructorId);

                if (enrollments == null || !enrollments.Any())
                {
                    return NotFound("No enrollments found for the given instructor.");
                }

                return Ok(enrollments);
            }
            catch (ArgumentException ex)
            {

                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {

                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        [HttpGet("Get-All-Courses")]
        public async Task<IActionResult> GetAllCourses([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (courses, totalPages) = await instructorService.GetAllCoursesAsync(pageNumber, pageSize);

                var response = new
                {
                    TotalPages = totalPages,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    Courses = courses
                };

                return Ok(response);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        [HttpGet("Get-Course-By-Id")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetCourseById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Course ID cannot be empty.");

            var course = await instructorService.GetCourseByIdAsync(id);

            if (course == null)
                return NotFound("Course not found.");

            return Ok(course);
        }

        [HttpDelete("Delete-Course")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteCourse(Guid courseId)
        {
            var instructorId = GetIdFromToken();

            try
            {
                var result = await instructorService.DeleteCourseAsync(courseId, instructorId);
                if (!result)
                {
                    return NotFound("Course not found or does not belong to the instructor.");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
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
