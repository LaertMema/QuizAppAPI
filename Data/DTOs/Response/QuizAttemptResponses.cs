namespace Quiz_App_API.Data.DTOs.Response
{
    public class QuizAttemptResponses
    {
        public class QuizAttemptResponse
        {
            public int Id { get; set; }
            public int QuizId { get; set; }
            public string QuizTitle { get; set; }
            public DateTime StartedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public int Score { get; set; }
            public List<AttemptAnswerResponse> Answers { get; set; }
        }

        public class AttemptAnswerResponse
        {
            public int QuestionId { get; set; }
            public string QuestionText { get; set; }
            public int SelectedOptionId { get; set; }
            public string SelectedOptionText { get; set; }
            public bool IsCorrect { get; set; }
        }

        public class QuizAttemptSummaryResponse
        {
            public int Id { get; set; }
            public string QuizTitle { get; set; }
            public DateTime CompletedAt { get; set; }
            public int Score { get; set; }
            public int TotalQuestions { get; set; }
        }
    }
}
