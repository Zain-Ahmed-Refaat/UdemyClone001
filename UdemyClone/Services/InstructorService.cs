using Microsoft.EntityFrameworkCore;
using UdemyClone.Data;
using UdemyClone.Dto;
using UdemyClone.Entities;
using UdemyClone.Models;
using UdemyClone.Services.IServices;

namespace UdemyClone.Services
{
    public class InstructorService : IInstructorService
    {
        private readonly ApplicationDbContext context;

        public InstructorService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<CourseDto> CreateCourseAsync(CourseModel model, Guid instructorId)
        {

            if (instructorId == Guid.Empty)
                throw new ArgumentException("Instructor ID cannot be empty.", nameof(instructorId));

            _ = model switch
            {
                null => throw new ArgumentNullException(nameof(model), "Course model cannot be null."),

                { Name: var name } when string.IsNullOrWhiteSpace(name) =>
                    throw new ArgumentException("Course name cannot be empty.", nameof(model.Name)),

                { Description: var description } when string.IsNullOrWhiteSpace(description) =>
                    throw new ArgumentException("Course description cannot be empty.", nameof(model.Description)),

                _ => model 
            };

            var existingTopic = await context.Topics.FindAsync(model.TopicId);

            if (existingTopic == null)
                throw new KeyNotFoundException("Topic not found.");
           
            var course = new Course
            {
                Name = model.Name,
                Description = model.Description,
                InstructorId = instructorId,
                TopicId = model.TopicId,
            };

            context.Courses.Add(course);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error saving the course to the database. Please try again later.", ex);
            }

            var courseDto = new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                InstructorId = course.InstructorId,
                Topic = existingTopic.Name
            };

            return courseDto;
        }

        public async Task<CourseDto> UpdateCourseAsync(Guid courseId, CourseModel model, Guid instructorId)
        {

            switch ((courseId, instructorId, model))
            {
                case var (cid, iid, _) when cid == Guid.Empty:
                    throw new ArgumentException("Course ID cannot be empty.", nameof(courseId));

                case var (_, iid, _) when iid == Guid.Empty:
                    throw new ArgumentException("Instructor ID cannot be empty.", nameof(instructorId));

                case var (_, _, mdl) when mdl == null:
                    throw new ArgumentNullException(nameof(model), "Course model cannot be null.");

                case var (_, _, mdl) when string.IsNullOrWhiteSpace(mdl.Name):
                    throw new ArgumentException("Course name cannot be empty.", nameof(model.Name));

                case var (_, _, mdl) when string.IsNullOrWhiteSpace(mdl.Description):
                    throw new ArgumentException("Course description cannot be empty.", nameof(model.Description));
            }

            var existingTopic = await context.Topics.FindAsync(model.TopicId);

            if (existingTopic == null)
                throw new KeyNotFoundException("Topic not found.");
        

            var course = await context.Courses
                .Where(c => c.Id == courseId && c.InstructorId == instructorId)
                .FirstOrDefaultAsync();

            if (course == null)
                throw new ArgumentNullException("Course Not Found");
            

            course.Name = model.Name;
            course.Description = model.Description;
            course.TopicId = model.TopicId;


            context.Courses.Update(course);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error updating the course in the database. Please try again later.", ex);
            }

            return new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                InstructorId = course.InstructorId,
                Topic = existingTopic.Name
            };
        }

        public async Task<IEnumerable<CourseEnrollmentsDto>> GetInstructorCoursesEnrollmentsAsync(Guid instructorId)
        {

            if (instructorId == Guid.Empty)
            {
                throw new ArgumentException("Instructor ID cannot be empty.", nameof(instructorId));
            }


            var courses = await context.Courses
                .Where(c => c.InstructorId == instructorId)
                .Include(c => c.StudentCourses)
                .ThenInclude(sc => sc.Student)
                .ToListAsync();

            var courseEnrollments = courses.Select(course => new CourseEnrollmentsDto
            {
                CourseId = course.Id,
                CourseName = course.Name,
                Enrollments = course.StudentCourses.Select(sc => new StudentDto
                {
                    Id = sc.Student.Id,
                    UserName = sc.Student.UserName,
                    Email = sc.Student.Email
                }).ToList()
            }).ToList();

            return courseEnrollments;
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesByInstructorAsync(Guid instructorId)
        {

            return await context.Courses
                .Where(c => c.InstructorId == instructorId)
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    InstructorId = instructorId,
                    Topic = c.Topic.Name
                })
                .ToListAsync();
        }

        public async Task<(IEnumerable<CourseDto> Courses, int TotalPages)> GetAllCoursesAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
               throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
            
            if (pageSize <= 0)            
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            
            var totalCourses = await context.Courses.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCourses / (double)pageSize);

            var courses = await context.Courses
                .Include(c => c.Topic)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    InstructorId = c.InstructorId,
                    Topic = c.Topic.Name
                })
                .ToListAsync();

            return (courses, totalPages);
        }

        public async Task<bool> DeleteCourseAsync(Guid courseId, Guid instructorId)
        {

            if (courseId == Guid.Empty)
                throw new ArgumentException("Course ID cannot be empty.", nameof(courseId));

            if (instructorId == Guid.Empty)
                throw new ArgumentException("Instructor ID cannot be empty.", nameof(instructorId));

            var course = await context.Courses
                .Where(c => c.Id == courseId && c.InstructorId == instructorId)
                .FirstOrDefaultAsync();

            if (course == null)
                return false; 

            context.Courses.Remove(course);

            try
            { 
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error occurred while deleting the course. Please try again later.", ex);
            }
        }

        public async Task<CourseDto> GetCourseByIdAsync(Guid courseId)
        {
            var course = await context.Courses
                .Where(c => c.Id == courseId)
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .FirstOrDefaultAsync();

            return course;
        }
    }
}
