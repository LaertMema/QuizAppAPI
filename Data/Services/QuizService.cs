using Microsoft.EntityFrameworkCore;
using Quiz_App_API.Data.DTOs.Request;
using Quiz_App_API.Data.Models;
using static Quiz_App_API.Data.DTOs.Response.QuizResponses;
using static Quiz_App_API.Data.DTOs.Response.SubjectResponses;

namespace Quiz_App_API.Data.Services
{
    public class QuizService
    {
        private readonly AppDbContext _context;

        public QuizService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<QuizSummaryResponse>> GetAllQuizzesAsync()
        {
            return await _context.Quizzes
                .Include(q => q.Creator)
                .Include(q => q.Subject)
                .Include(q => q.Questions)
                .Select(q => new QuizSummaryResponse
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    CreatedAt = q.CreatedAt,
                    QuestionCount = q.Questions.Count,
                    CreatorName = q.Creator.UserName,
                    Subject = new SubjectResponse
                    {
                        Id = q.Subject.Id,
                        Name = q.Subject.Name,
                        Description = q.Subject.Description
                    }
                })
                .ToListAsync();
        }

        public async Task<QuizResponse> GetQuizByIdAsync(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Creator)
                .Include(q => q.Subject)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Alternatives)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return null;

            return new QuizResponse
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                CreatedAt = quiz.CreatedAt,
                CreatorId = quiz.CreatorId,
                CreatorName = quiz.Creator.UserName,
                Subject = new SubjectResponse
                {
                    Id = quiz.Subject.Id,
                    Name = quiz.Subject.Name,
                    Description = quiz.Subject.Description
                },
                Questions = quiz.Questions.Select(q => new QuestionResponse
                {
                    Id = q.Id,
                    Text = q.Text,
                    Options = q.Alternatives.Select(o => new AlternativeResponse
                    {
                        Id = o.Id,
                        Text = o.Text,
                        IsCorrect= o.IsCorrect
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<QuizResponse> CreateQuizAsync(CreateQuizRequestDTO request, string userId)
        {
            var quiz = new Quiz
            {
                Title = request.Title,
                Description = request.Description,
                CreatorId = userId,
                SubjectId = request.SubjectId,
                CreatedAt = DateTime.UtcNow,
                Questions = request.Questions.Select(q => new Question
                {
                    Text = q.Text,
                    Alternatives = q.Alternatives.Select(o => new Alternative
                    {
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList()
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return await GetQuizByIdAsync(quiz.Id);
        }

        //public async Task<QuizResponse> UpdateQuizAsync(int id, UpdateQuizRequestDTO request)
        //{
        //    var quiz = await _context.Quizzes
        //        .Include(q => q.Questions)
        //            .ThenInclude(q => q.Alternatives)
        //        .FirstOrDefaultAsync(q => q.Id == id);

        //    if (quiz == null) return null;

        //    quiz.Title = request.Title;
        //    quiz.Description = request.Description;
        //    quiz.SubjectId = request.SubjectId;

        //    // Remove existing questions and options
        //    _context.Alternatives.RemoveRange(quiz.Questions.SelectMany(q => q.Alternatives));
        //    _context.Questions.RemoveRange(quiz.Questions);

        //    // Add new questions and options
        //    quiz.Questions = request.Questions.Select(q => new Question
        //    {
        //        Text = q.Text,
        //        Alternatives = q.Alternatives.Select(o => new Alternative
        //        {
        //            Text = o.Text,
        //            IsCorrect = o.IsCorrect
        //        }).ToList()
        //    }).ToList();

        //    await _context.SaveChangesAsync();

        //    return await GetQuizByIdAsync(quiz.Id);
        //}

        //public async Task<bool> DeleteQuizAsync(int id)
        //{
        //    var quiz = await _context.Quizzes.FindAsync(id);
        //    if (quiz == null) return false;

        //    _context.Quizzes.Remove(quiz);
        //    await _context.SaveChangesAsync();
        //    return true;
        //}
        //E sakta
        public async Task<QuizResponse> UpdateQuizAsync(int id, UpdateQuizRequestDTO request)
        {
            // First verify the subject exists
            var subjectExists = await _context.Subjects.AnyAsync(s => s.Id == request.SubjectId);
            if (!subjectExists)
            {
                throw new InvalidOperationException("Subject not found");
            }

            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Alternatives)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return null;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update basic quiz properties
                quiz.Title = request.Title;
                quiz.Description = request.Description;
                quiz.SubjectId = request.SubjectId;

                // Get all question IDs that will be deleted
                var questionIds = quiz.Questions.Select(q => q.Id).ToList();

                // First remove UserAnswers for these questions
                var userAnswersToDelete = await _context.UserAnswers
                    .Where(ua => questionIds.Contains(ua.QuestionId))
                    .ToListAsync();
                _context.UserAnswers.RemoveRange(userAnswersToDelete);

                // Then remove alternatives and questions
                foreach (var question in quiz.Questions)
                {
                    _context.Alternatives.RemoveRange(question.Alternatives);
                }
                _context.Questions.RemoveRange(quiz.Questions);

                // Add new questions and alternatives
                quiz.Questions = request.Questions.Select(q => new Question
                {
                    Text = q.Text,
                    QuizId = quiz.Id,
                    Alternatives = q.Alternatives.Select(a => new Alternative
                    {
                        Text = a.Text,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                }).ToList();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetQuizByIdAsync(quiz.Id);
            }
            catch (Exception)
            {  //Koment per grupin
                await transaction.RollbackAsync(); //Po deshtoi e kthen ne gjendje te meparshme
                throw;
            }
        }

        public async Task<bool> DeleteQuizAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Alternatives)
                    .Include(q => q.Attempts)
                        .ThenInclude(a => a.Answers)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quiz == null) return false;

                // Remove all related quiz attempts and answers
                foreach (var attempt in quiz.Attempts)
                {
                    _context.UserAnswers.RemoveRange(attempt.Answers);
                }
                _context.QuizAttempts.RemoveRange(quiz.Attempts);

                // Remove all questions and their alternatives
                foreach (var question in quiz.Questions)
                {
                    _context.Alternatives.RemoveRange(question.Alternatives);
                }
                _context.Questions.RemoveRange(quiz.Questions);

                // Finally remove the quiz
                _context.Quizzes.Remove(quiz);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<List<QuizSummaryResponse>> GetQuizzesBySubjectAsync(int subjectId)
        {
            return await _context.Quizzes
                .Where(q => q.SubjectId == subjectId)
                .Include(q => q.Creator)
                .Include(q => q.Subject)
                .Include(q => q.Questions)
                .Select(q => new QuizSummaryResponse
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    CreatedAt = q.CreatedAt,
                    QuestionCount = q.Questions.Count,
                    CreatorName = q.Creator.UserName,
                    Subject = new SubjectResponse
                    {
                        Id = q.Subject.Id,
                        Name = q.Subject.Name,
                        Description = q.Subject.Description
                    }
                })
                .ToListAsync();
        }

        public async Task<List<QuizSummaryResponse>> GetUserQuizzesAsync(string userId)
        {
            return await _context.Quizzes
                .Where(q => q.CreatorId == userId)
                .Include(q => q.Creator)
                .Include(q => q.Subject)
                .Include(q => q.Questions)
                .Select(q => new QuizSummaryResponse
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    CreatedAt = q.CreatedAt,
                    QuestionCount = q.Questions.Count,
                    CreatorName = q.Creator.UserName,
                    Subject = new SubjectResponse
                    {
                        Id = q.Subject.Id,
                        Name = q.Subject.Name,
                        Description = q.Subject.Description
                    }
                })
                .ToListAsync();
        }
    }
}
