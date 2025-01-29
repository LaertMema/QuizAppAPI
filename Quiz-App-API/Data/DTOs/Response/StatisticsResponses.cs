using static Quiz_App_API.Data.DTOs.Response.QuizAttemptResponses;

namespace Quiz_App_API.Data.DTOs.Response
{
    public class StatisticsResponses
    {
        public class UserStatisticsResponse
        {
            public int TotalQuizzesTaken { get; set; }
            public int TotalQuizzesCreated { get; set; }
            public double AverageScore { get; set; }
            public List<QuizAttemptSummaryResponse> RecentAttempts { get; set; }
        }

        public class QuizStatisticsResponse
        {
            public int TotalAttempts { get; set; }
            public double AverageScore { get; set; }
            public int HighestScore { get; set; }
            public int LowestScore { get; set; }
            public List<QuizAttemptSummaryResponse> TopScores { get; set; }
        }
    }
}
