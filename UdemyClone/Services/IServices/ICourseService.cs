using UdemyClone.Dto;
using UdemyClone.Entities;
using UdemyClone.Models;

namespace UdemyClone.Services.IServices
{
    public interface ICourseService
    {
        Task<IEnumerable<LessonDto>> GetAllLessonsAsync(Guid instructorId, Guid courseId, int pageNumber, int pageSize);
        Task<LessonDto> UploadLessonAsync(LessonModel model, Guid instructorId);
        Task<Lesson> GetLessonByIdAsync(Guid id, Guid instructorId);
    }
}
