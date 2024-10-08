﻿using Microsoft.EntityFrameworkCore;
using UdemyClone.Data;
using UdemyClone.Dto;
using UdemyClone.Entities;
using UdemyClone.Models;
using UdemyClone.Services.IServices;

namespace UdemyClone.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext context;

        public CourseService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<LessonDto> UploadLessonAsync(LessonModel model, Guid instructorId)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Lesson model cannot be null.");
            }

            if (model.CourseId == Guid.Empty)
            {
                throw new ArgumentException("Course ID is required.", nameof(model.CourseId));
            }

            var course = await context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == model.CourseId);

            if (course == null)           
                throw new ArgumentException("Course not found.", nameof(model.CourseId));
            

            if (course.InstructorId != instructorId)           
                throw new UnauthorizedAccessException("You do not have permission to add lessons to this course.");
            
            var existingLesson = await context.Lessons
                .FirstOrDefaultAsync(l => l.Name == model.Name && l.CourseId == model.CourseId);

            if (existingLesson != null)
            {
                throw new InvalidOperationException($"A lesson with the name '{model.Name}' already exists in this course.");
            }

            var lesson = new Lesson
            {
                Name = model.Name,
                Description = model.Description,
                CourseId = model.CourseId,
            };

            context.Lessons.Add(lesson);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error saving the lesson to the database. Please try again later.", ex);
            }

            return new LessonDto
            {
                LessonId = lesson.Id,
                Name = lesson.Name,
                Description = lesson.Description,
                CourseId = lesson.CourseId
            };
        }

        public async Task<IEnumerable<LessonDto>> GetAllLessonsAsync(Guid instructorId, Guid courseId, int pageNumber, int pageSize)
        {

            if (pageNumber <= 0)           
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
            

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            

            if (courseId == Guid.Empty)            
                throw new ArgumentException("Course ID cannot be empty.", nameof(courseId));
            

            var isValidCourse = await context.Courses
                .AnyAsync(c => c.InstructorId == instructorId && c.Id == courseId);

            if (!isValidCourse)
            {
                throw new KeyNotFoundException("Course not found or does not belong to the instructor.");
            }

            var lessons = await context.Lessons
                .Where(l => l.CourseId == courseId)
                .Include(l => l.Course)
                .OrderBy(l => l.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LessonDto
                {
                    LessonId = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    CourseId = l.CourseId,
                })
                .ToListAsync();

            return lessons;
        }

        public async Task<Lesson> GetLessonByIdAsync(Guid id, Guid instructorId)
        {
            var lesson = await context.Lessons
                .Include(l => l.Course)
                .Include(l => l.Quizzes)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
                return null;

            if (lesson.Course.InstructorId != instructorId)
                throw new UnauthorizedAccessException("You Do Not Have Permission to Access this Lesson.");

            return lesson;
        }

    }
}
