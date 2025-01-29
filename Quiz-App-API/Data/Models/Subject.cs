namespace Quiz_App_API.Data.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Quiz> Quizzes { get; set; } = new();
    }
}
