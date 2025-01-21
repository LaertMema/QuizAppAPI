using Microsoft.EntityFrameworkCore;
using Quiz_App_API.Data.Models;
using static Quiz_App_API.Data.DTOs.Request.QuizAttemptRequests;
using static Quiz_App_API.Data.DTOs.Response.QuizAttemptResponses;

namespace Quiz_App_API.Data.Services
{
    public class QuizAttemptService
    {
        private readonly AppDbContext _context;

        public QuizAttemptService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<QuizAttemptResponse> StartQuizAttemptAsync(StartQuizAttemptRequest request, string userId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == request.QuizId);

            if (quiz == null) throw new Exception("Quiz not found");

            var attempt = new QuizAttempt
            {
                QuizId = request.QuizId,
                UserId = userId,
                StartedAt = DateTime.UtcNow
            };

            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            return new QuizAttemptResponse
            {
                Id = attempt.Id,
                QuizId = attempt.QuizId,
                QuizTitle = quiz.Title,
                StartedAt = attempt.StartedAt,
                CompletedAt = null,
                Score = 0,
                Answers = new List<AttemptAnswerResponse>()
            };
        }

        public async Task<QuizAttemptResponse> SubmitQuizAttemptAsync(SubmitQuizAttemptRequest request, string userId)
        {
            var attempt = await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.Alternatives)
                .FirstOrDefaultAsync(qa => qa.Id == request.QuizAttemptId && qa.UserId == userId);

            if (attempt == null) throw new Exception("Quiz attempt not found");
            if (attempt.CompletedAt.HasValue) throw new Exception("Quiz attempt already submitted");

            // Process answers
            var answers = new List<UserAnswer>();
            foreach (var answerRequest in request.Answers)
            {
                var question = attempt.Quiz.Questions.FirstOrDefault(q => q.Id == answerRequest.QuestionId);
                if (question == null) continue;

                var selectedOption = question.Alternatives.FirstOrDefault(o => o.Id == answerRequest.SelectedOptionId);
                if (selectedOption == null) continue;

                var answer = new UserAnswer
                {
                    QuizAttemptId = attempt.Id,
                    QuestionId = question.Id,
                    SelectedOptionId = selectedOption.Id,
                    Question = question
                };
                answers.Add(answer);
            }

            _context.UserAnswers.AddRange(answers);
            attempt.Answers = answers;
            attempt.CompletedAt = DateTime.UtcNow;
            attempt.Score = await CalculateScore(attempt);

            await _context.SaveChangesAsync();

            return await GetAttemptByIdAsync(attempt.Id);
        }

        public async Task<QuizAttemptResponse> GetAttemptByIdAsync(int attemptId)
        {
            var attempt = await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.Answers)
                .FirstOrDefaultAsync(qa => qa.Id == attemptId);

            if (attempt == null) return null;

            return new QuizAttemptResponse
            {
                Id = attempt.Id,
                QuizId = attempt.QuizId,
                QuizTitle = attempt.Quiz.Title,
                StartedAt = attempt.StartedAt,
                CompletedAt = attempt.CompletedAt,
                Score = attempt.Score,
                Answers = await GetAttemptAnswersAsync(attempt.Id)
            };
        }

        public async Task<List<QuizAttemptSummaryResponse>> GetUserAttemptsAsync(string userId)
        {
            return await _context.QuizAttempts
                .Where(qa => qa.UserId == userId && qa.CompletedAt.HasValue)
                .Include(qa => qa.Quiz)
                .OrderByDescending(qa => qa.CompletedAt)
                .Select(qa => new QuizAttemptSummaryResponse
                {
                    Id = qa.Id,
                    QuizTitle = qa.Quiz.Title,
                    CompletedAt = qa.CompletedAt.Value,
                    Score = qa.Score,
                    TotalQuestions = qa.Quiz.Questions.Count
                })
                .ToListAsync();
        }

        private async Task<List<AttemptAnswerResponse>> GetAttemptAnswersAsync(int attemptId)
        {
            return await _context.UserAnswers
                .Where(aa => aa.QuizAttemptId == attemptId)
                .Include(aa => aa.Question)
                    .ThenInclude(q => q.Alternatives)
                .Select(aa => new AttemptAnswerResponse
                {
                    QuestionId = aa.QuestionId,
                    QuestionText = aa.Question.Text,
                    SelectedOptionId = aa.SelectedOptionId,
                    SelectedOptionText = aa.Question.Alternatives
                        .First(o => o.Id == aa.SelectedOptionId).Text,
                    IsCorrect = aa.Question.Alternatives
                        .First(o => o.Id == aa.SelectedOptionId).IsCorrect
                })
                .ToListAsync();
        }

        private async Task<int> CalculateScore(QuizAttempt attempt)
        {
            var correctAnswers = await _context.UserAnswers
                .Where(aa => aa.QuizAttemptId == attempt.Id)
                .Include(aa => aa.Question)
                    .ThenInclude(q => q.Alternatives)
                .CountAsync(aa => aa.Question.Alternatives
                    .First(o => o.Id == aa.SelectedOptionId).IsCorrect);

            var totalQuestions = attempt.Quiz.Questions.Count;
            return (int)Math.Round((double)correctAnswers / totalQuestions * 100);
        }
    }
}
