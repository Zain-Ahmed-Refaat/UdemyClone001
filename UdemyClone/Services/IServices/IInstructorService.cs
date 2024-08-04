using UdemyClone.Dto;
using UdemyClone.Models;

namespace UdemyClone.Services.IServices
{
    public interface IInstructorService
    {
        Task<(IEnumerable<CourseDto> Courses, int TotalPages)> GetAllCoursesAsync(int pageNumber, int pageSize);
        Task<IEnumerable<CourseEnrollmentsDto>> GetInstructorCoursesEnrollmentsAsync(Guid instructorId);
        Task<CourseDto> UpdateCourseAsync(Guid courseId, CourseModel model, Guid instructorId);
        Task<IEnumerable<CourseDto>> GetCoursesByInstructorAsync(Guid instructorId);
        Task<CourseDto> CreateCourseAsync(CourseModel model, Guid instructorId);
        Task<bool> DeleteCourseAsync(Guid courseId, Guid instructorId);
        Task<CourseDto> GetCourseByIdAsync(Guid courseId);
    }
}
