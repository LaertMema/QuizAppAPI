using Microsoft.EntityFrameworkCore;
using Quiz_App_API.Data.DTOs.Request;
using Quiz_App_API.Data.Models;
using static Quiz_App_API.Data.DTOs.Response.QuizResponses;
using static Quiz_App_API.Data.DTOs.Response.SubjectResponses;

namespace Quiz_App_API.Data.Services
{
    public class SubjectService
    {
        private readonly AppDbContext _context;

        public SubjectService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SubjectResponse>> GetAllSubjectsAsync()
        {
            return await _context.Subjects
                .Select(s => new SubjectResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description
                })
                .ToListAsync();
        }

        public async Task<SubjectWithQuizzesResponse> GetSubjectWithQuizzesAsync(int id)
        {
            return await _context.Subjects
                .Where(s => s.Id == id)
                .Select(s => new SubjectWithQuizzesResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Quizzes = s.Quizzes.Select(q => new QuizSummaryResponse
                    {
                        Id = q.Id,
                        Title = q.Title,
                        Description = q.Description,
                        CreatedAt = q.CreatedAt,
                        QuestionCount = q.Questions.Count,
                        CreatorName = q.Creator.UserName,
                        Subject = new SubjectResponse
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Description = s.Description
                        }
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<SubjectResponse> CreateSubjectAsync(CreateSubjectRequestDTO request)
        {
            var subject = new Subject
            {
                Name = request.Name,
                Description = request.Description
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return new SubjectResponse
            {
                Id = subject.Id,
                Name = subject.Name,
                Description = subject.Description
            };
        }

        public async Task<SubjectResponse> UpdateSubjectAsync(int id, UpdateSubjectRequestDTO request)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return null;

            subject.Name = request.Name;
            subject.Description = request.Description;

            await _context.SaveChangesAsync();

            return new SubjectResponse
            {
                Id = subject.Id,
                Name = subject.Name,
                Description = subject.Description
            };
        }

        public async Task<bool> DeleteSubjectAsync(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return false;

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
