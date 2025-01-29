namespace Quiz_App_API.Data.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatorId { get; set; }
        public int SubjectId { get; set; }

        public ApplicationUser Creator { get; set; }
        public Subject Subject { get; set; }
        public List<Question> Questions { get; set; } = new();
        public List<QuizAttempt> Attempts { get; set; } = new();
    }
}

