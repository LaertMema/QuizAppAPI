namespace Quiz_App_API.Data.Models
{
    public class QuizAttempt
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string UserId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int Score { get; set; }
        public Quiz Quiz { get; set; }
        public ApplicationUser User { get; set; }
        //UserAnswers jane pergjigjet gjate nje attempti
        public List<UserAnswer> Answers { get; set; } = new();
    }
}
