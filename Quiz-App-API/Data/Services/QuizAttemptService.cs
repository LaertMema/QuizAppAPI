using Microsoft.EntityFrameworkCore;
using Quiz_App_API.Data.DTOs.Response;
using Quiz_App_API.Data.Models;
using static Quiz_App_API.Data.DTOs.Request.QuizAttemptRequests;
using static Quiz_App_API.Data.DTOs.Response.QuizAttemptResponses;
using static Quiz_App_API.Data.DTOs.Response.StatisticsResponses;

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
                QuizId = request.QuizId, //Mappers perdoren ketu psh qe te mos rrish vete ti besh set
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

            // Debug log
            Console.WriteLine($"Processing submission for attempt ID: {attempt.Id}");
            Console.WriteLine($"Number of answers being submitted: {request.Answers.Count}");

            // Process answers
            foreach (var answerRequest in request.Answers)
            {
                var userAnswer = new UserAnswer
                {
                    QuizAttemptId = attempt.Id,
                    QuestionId = answerRequest.QuestionId,
                    SelectedOptionId = answerRequest.SelectedOptionId
                };

                _context.UserAnswers.Add(userAnswer);
            }

            attempt.CompletedAt = DateTime.UtcNow;

            // Save changes before calculating score
            await _context.SaveChangesAsync();

            // Now calculate score after answers are saved
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
            return await _context.UserAnswers  //Merr answers convert  to Responses
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
            // Get total questions count directly from database
            var totalQuestions = await _context.Questions
                .Where(q => q.QuizId == attempt.QuizId)
                .CountAsync();

            if (totalQuestions == 0)
            {
                return 0;
            }

            // Get user answers with their correctness
            var userAnswers = await _context.UserAnswers
                .Where(ua => ua.QuizAttemptId == attempt.Id)
                .Include(ua => ua.Question)
                    .ThenInclude(q => q.Alternatives)
                .ToListAsync();

            var correctAnswers = userAnswers.Count(answer =>
                answer.Question.Alternatives
                    .Any(alt => alt.Id == answer.SelectedOptionId && alt.IsCorrect));

            var score = (int)Math.Round((double)correctAnswers / totalQuestions * 100);

            // For debugging
            var debugInfo = new
            {
                AttemptId = attempt.Id,
                QuizId = attempt.QuizId,
                TotalQuestions = totalQuestions,
                UserAnswersCount = userAnswers.Count,
                CorrectAnswers = correctAnswers,
                FinalScore = score
            };

            // Log the debug info
            var debugJson = System.Text.Json.JsonSerializer.Serialize(debugInfo);
            System.Diagnostics.Debug.WriteLine($"Score Calculation Debug: {debugJson}");

            return score;
        }
        //The below methods are for leaderboard
        public async Task<UserStatisticsResponse> GetUserStatisticsAsync(string userId)
        {
            var attempts = await _context.QuizAttempts
                .Where(qa => qa.UserId == userId && qa.CompletedAt.HasValue)
                .Include(qa => qa.Quiz)
                .ToListAsync();

            var createdQuizzes = await _context.Quizzes
                .CountAsync(q => q.CreatorId == userId);

            var recentAttempts = attempts
                .OrderByDescending(a => a.CompletedAt)
                .Take(5)
                .Select(a => new QuizAttemptSummaryResponse
                {
                    Id = a.Id,
                    QuizTitle = a.Quiz.Title,
                    CompletedAt = a.CompletedAt.Value,
                    Score = a.Score,
                    TotalQuestions = a.Quiz.Questions.Count
                })
                .ToList();

            return new UserStatisticsResponse
            {
                TotalQuizzesTaken = attempts.Count,
                TotalQuizzesCreated = createdQuizzes,
                AverageScore = attempts.Any() ? Math.Round(attempts.Average(a => a.Score), 2) : 0,
                RecentAttempts = recentAttempts
            };
        }

        public async Task<QuizStatisticsResponse> GetQuizStatisticsAsync(int quizId)
        {
            var attempts = await _context.QuizAttempts
                .Where(qa => qa.QuizId == quizId && qa.CompletedAt.HasValue)
                .Include(qa => qa.Quiz)
                .ToListAsync();

            var topScores = attempts
                .OrderByDescending(a => a.Score)
                .Take(5)
                .Select(a => new QuizAttemptSummaryResponse
                {
                    Id = a.Id,
                    QuizTitle = a.Quiz.Title,
                    CompletedAt = a.CompletedAt.Value,
                    Score = a.Score,
                    TotalQuestions = a.Quiz.Questions.Count
                })
                .ToList();

            return new QuizStatisticsResponse
            {
                TotalAttempts = attempts.Count,
                AverageScore = attempts.Any() ? Math.Round(attempts.Average(a => a.Score), 2) : 0,
                HighestScore = attempts.Any() ? attempts.Max(a => a.Score) : 0,
                LowestScore = attempts.Any() ? attempts.Min(a => a.Score) : 0,
                TopScores = topScores
            };
        }
        public async Task<List<LeaderboardEntryResponse>> GetLeaderboardBySubjectAsync(int subjectId)
        {
            return await _context.QuizAttempts
                .Where(qa => qa.Quiz.SubjectId == subjectId && qa.CompletedAt.HasValue)
                .GroupBy(qa => qa.UserId)
                .Select(g => new LeaderboardEntryResponse
                {
                    UserName = _context.Users
                        .Where(u => u.Id == g.Key)
                        .Select(u => u.UserName)
                        .FirstOrDefault(),
                    QuizzesTaken = g.Count(),
                    TotalScore = g.Sum(qa => qa.Score + (qa.Score >= 90 ? 50 :
                                                        qa.Score >= 80 ? 30 :
                                                        qa.Score >= 70 ? 20 : 0))
                })
                .OrderByDescending(l => l.TotalScore)
                .Take(10)
                .ToListAsync();
        }
    }
}
