using static Quiz_App_API.Data.DTOs.Response.QuizResponses;

namespace Quiz_App_API.Data.DTOs.Response
{
    public class SubjectResponses
    {
        public class SubjectResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public class SubjectWithQuizzesResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public List<QuizSummaryResponse> Quizzes { get; set; }
        }
    }
}
