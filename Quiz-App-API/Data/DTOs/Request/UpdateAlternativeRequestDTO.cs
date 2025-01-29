namespace Quiz_App_API.Data.DTOs.Request
{
    public class UpdateAlternativeRequestDTO
    {
        public int? Id { get; set; }  // For existing alternatives
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}
