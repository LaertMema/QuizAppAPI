using Microsoft.AspNetCore.Identity;

namespace Quiz_App_API.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<Quiz> CreatedQuizzes { get; set; } = new();
        public List<QuizAttempt> QuizAttempts { get; set; } = new();
    }
}
