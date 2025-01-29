namespace Quiz_App_API.Data.Models
{
    public class Alternative
    {
       
            public int Id { get; set; }
            public int QuestionId { get; set; }
            public string Text { get; set; }
            public bool IsCorrect { get; set; }
            public Question Question { get; set; }
        
    }
}
