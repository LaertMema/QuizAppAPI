

namespace Quiz_App_API.Data.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Text { get; set; }
        public List<Alternative> Alternatives { get; set; } = new();
        public Quiz Quiz { get; set; }
    }
}
