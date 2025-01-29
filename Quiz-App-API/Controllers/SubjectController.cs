using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quiz_App_API.Data.DTOs.Request;
using Quiz_App_API.Data.Services;
using static Quiz_App_API.Data.DTOs.Response.SubjectResponses;

namespace Quiz_App_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectController : ControllerBase
    {
        private readonly SubjectService _subjectService;

        public SubjectController(SubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        [HttpGet]
        public async Task<ActionResult<List<SubjectResponse>>> GetAllSubjects()
        {
            var subjects = await _subjectService.GetAllSubjectsAsync();
            return Ok(subjects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubjectWithQuizzesResponse>> GetSubject(int id)
        {
            var subject = await _subjectService.GetSubjectWithQuizzesAsync(id);
            if (subject == null) return NotFound();
            return Ok(subject);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")] 
        public async Task<ActionResult<SubjectResponse>> CreateSubject(CreateSubjectRequestDTO request)
        {
            var subject = await _subjectService.CreateSubjectAsync(request);
            return CreatedAtAction(nameof(GetSubject), new { id = subject.Id }, subject);
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<SubjectResponse>> UpdateSubject(int id, UpdateSubjectRequestDTO request)
        {
            var subject = await _subjectService.UpdateSubjectAsync(id, request);
            if (subject == null) return NotFound();
            return Ok(subject);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteSubject(int id)
        {
            var result = await _subjectService.DeleteSubjectAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
