using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quiz_App_API.Data.Services;
using static Quiz_App_API.Data.DTOs.Response.QuizResponses;
using System.Security.Claims;
using Quiz_App_API.Data.DTOs.Request;

namespace Quiz_App_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly QuizService _quizService;

        public QuizController(QuizService quizService)
        {
            _quizService = quizService;
        }

        [HttpGet]
        public async Task<ActionResult<List<QuizSummaryResponse>>> GetAllQuizzes()
        {
            var quizzes = await _quizService.GetAllQuizzesAsync();
            return Ok(quizzes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QuizResponse>> GetQuiz(int id)
        {
            var quiz = await _quizService.GetQuizByIdAsync(id);
            if (quiz == null) return NotFound();
            return Ok(quiz);
        }

        [HttpPost]
        //[Authorize] Po i heq ngaqe skemi role :/
        public async Task<ActionResult<QuizResponse>> CreateQuiz(CreateQuizRequestDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var quiz = await _quizService.CreateQuizAsync(request, userId);
            return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Id }, quiz);
        }

        [HttpPut("{id}")]
        //[Authorize]
        public async Task<ActionResult<QuizResponse>> UpdateQuiz(int id, UpdateQuizRequestDTO request)
        {
            var quiz = await _quizService.UpdateQuizAsync(id, request);
            if (quiz == null) return NotFound();
            return Ok(quiz);
        }

        [HttpDelete("{id}")]
        //[Authorize]
        public async Task<ActionResult> DeleteQuiz(int id)
        {
            var result = await _quizService.DeleteQuizAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("subject/{subjectId}")]
        public async Task<ActionResult<List<QuizSummaryResponse>>> GetQuizzesBySubject(int subjectId)
        {
            var quizzes = await _quizService.GetQuizzesBySubjectAsync(subjectId);
            return Ok(quizzes);
        }

        [HttpGet("my-quizzes")]
        //[Authorize]
        public async Task<ActionResult<List<QuizSummaryResponse>>> GetMyQuizzes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var quizzes = await _quizService.GetUserQuizzesAsync(userId);
            return Ok(quizzes);
        }
    }
}
