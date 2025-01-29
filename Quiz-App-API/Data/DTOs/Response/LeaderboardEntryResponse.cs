namespace Quiz_App_API.Data.DTOs.Response
{
    public class LeaderboardEntryResponse
    {
        public string UserName { get; set; }
        public int TotalScore { get; set; }  // Sum of (score × number of quizzes)
        public int QuizzesTaken { get; set; }
    }
}
