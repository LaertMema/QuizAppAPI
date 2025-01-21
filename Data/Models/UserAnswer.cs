namespace Quiz_App_API.Data.Models
{
    public class UserAnswer
    {
        public int Id { get; set; }
        public int QuizAttemptId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
        public QuizAttempt QuizAttempt { get; set; }
        public Question Question { get; set; }
    }
}
