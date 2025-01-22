using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quiz_App_API.Data.Services;
using static Quiz_App_API.Data.DTOs.Request.QuizAttemptRequests;
using static Quiz_App_API.Data.DTOs.Response.QuizAttemptResponses;
using System.Security.Claims;
using static Quiz_App_API.Data.DTOs.Response.StatisticsResponses;
using Quiz_App_API.Data.DTOs.Response;

namespace Quiz_App_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class QuizAttemptController : ControllerBase
    {
        private readonly QuizAttemptService _quizAttemptService;

        public QuizAttemptController(QuizAttemptService quizAttemptService)
        {
            _quizAttemptService = quizAttemptService;
        }

        [HttpPost("start")]
        public async Task<ActionResult<QuizAttemptResponse>> StartQuizAttempt(StartQuizAttemptRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var attempt = await _quizAttemptService.StartQuizAttemptAsync(request, userId);
                return Ok(attempt);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("submit")]
        public async Task<ActionResult<QuizAttemptResponse>> SubmitQuizAttempt(SubmitQuizAttemptRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _quizAttemptService.SubmitQuizAttemptAsync(request, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<QuizAttemptResponse>> GetAttempt(int id)
        {
            var attempt = await _quizAttemptService.GetAttemptByIdAsync(id);
            if (attempt == null) return NotFound();
            return Ok(attempt);
        }

        [HttpGet("my-attempts")]
        public async Task<ActionResult<List<QuizAttemptSummaryResponse>>> GetMyAttempts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var attempts = await _quizAttemptService.GetUserAttemptsAsync(userId);
            return Ok(attempts);
        }
        [HttpGet("user-statistics")]
        public async Task<ActionResult<UserStatisticsResponse>> GetUserStatistics()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var statistics = await _quizAttemptService.GetUserStatisticsAsync(userId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("quiz-statistics/{quizId}")]
        public async Task<ActionResult<QuizStatisticsResponse>> GetQuizStatistics(int quizId)
        {
            try
            {
                var statistics = await _quizAttemptService.GetQuizStatisticsAsync(quizId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("leaderboard/{subjectId}")]
        public async Task<ActionResult<List<LeaderboardEntryResponse>>> GetLeaderboard(int subjectId)
        {
            try
            {
                var leaderboard = await _quizAttemptService.GetLeaderboardBySubjectAsync(subjectId);
                return Ok(leaderboard);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
