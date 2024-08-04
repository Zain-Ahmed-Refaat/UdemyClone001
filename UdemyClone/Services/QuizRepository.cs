using Microsoft.EntityFrameworkCore;
using UdemyClone.Data;
using UdemyClone.Entities;
using UdemyClone.Services.IServices;

namespace UdemyClone.Services
{
    public class QuizRepository : IQuizRepository
    {
         private readonly ApplicationDbContext _context;

         public QuizRepository(ApplicationDbContext context)
         {
             _context = context;
         }

        public async Task<StudentQuiz> GetLatestStudentQuizAttemptAsync(Guid studentId, Guid quizId)
        {
            var attempt = await _context.StudentQuizzes
                .Include(sq => sq.Student)
                .Include(sq => sq.Quiz)
                .Where(sq => sq.StudentId == studentId && sq.QuizId == quizId)
                .OrderByDescending(sq => sq.DateTaken)
                .FirstOrDefaultAsync();

            Console.WriteLine($"Latest Attempt: {attempt?.Id}, Passed: {attempt?.Passed}");

            return attempt;
        }

        public async Task AddAsync(Quiz quiz)
        {
            await _context.Quizzes.AddAsync(quiz);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Quiz quiz)
        {
            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();
        }

        public async Task<Quiz> GetByIdAsync(Guid id)
        {
            return await _context.Quizzes
                .Include(q => q.Lesson)
                .ThenInclude(q => q.Course)
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task AddStudentQuizAsync(StudentQuiz studentQuiz)
        {
            await _context.StudentQuizzes.AddAsync(studentQuiz);
            await _context.SaveChangesAsync();
        }

        public async Task AddStudentAnswerAsync(StudentAnswer studentAnswer)
        {
            await _context.StudentAnswers.AddAsync(studentAnswer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStudentQuizAsync(StudentQuiz studentQuiz)
        {
            _context.StudentQuizzes.Update(studentQuiz);
            await _context.SaveChangesAsync();
        }

        public async Task<Quiz> GetQuizzesByLessonIdAsync(Guid lessonId)
        {
            if (lessonId == Guid.Empty)
                throw new ArgumentException("Lesson ID cannot be empty.", nameof(lessonId));

            return await _context.Quizzes
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.LessonId == lessonId);
        }

        public async Task<StudentQuiz> GetStudentQuizAsync(Guid studentId, Guid quizId)
        {
             return await _context.StudentQuizzes
                .Include(sq => sq.Student)
                 .Include(sq => sq.StudentAnswers)
                 .FirstOrDefaultAsync(sq => sq.StudentId == studentId && sq.QuizId == quizId);
        }

        public async Task DeleteQuizAsync(Guid quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null)
            {
                throw new KeyNotFoundException("Quiz not found");
            }

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }

} 
