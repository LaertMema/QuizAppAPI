using static Quiz_App_API.Data.DTOs.Response.SubjectResponses;

namespace Quiz_App_API.Data.DTOs.Response
{
    public class QuizResponses
    {
        public class QuizResponse
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public string CreatorId { get; set; }
            public string CreatorName { get; set; }
            public SubjectResponse Subject { get; set; }
            public List<QuestionResponse> Questions { get; set; }
        }

        public class QuizSummaryResponse
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public int QuestionCount { get; set; }
            public string CreatorName { get; set; }
            public SubjectResponse Subject { get; set; }
        }

        public class QuestionResponse
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public List<AlternativeResponse> Options { get; set; }
        }

        public class AlternativeResponse
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }
    }
}
