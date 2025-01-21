using System.ComponentModel.DataAnnotations;

namespace Quiz_App_API.Data.DTOs.Request
{
    public class QuizAttemptRequests
    {
        public class StartQuizAttemptRequest
        {
            [Required]
            public int QuizId { get; set; }
        }

        public class SubmitQuizAttemptRequest
        {
            [Required]
            public int QuizAttemptId { get; set; }

            [Required]
            public List<SubmitAnswerRequest> Answers { get; set; }
        }

        public class SubmitAnswerRequest
        {
            [Required]
            public int QuestionId { get; set; }

            [Required]
            public int SelectedOptionId { get; set; }
        }
    }
}
