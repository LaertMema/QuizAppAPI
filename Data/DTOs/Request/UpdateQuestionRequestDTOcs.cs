namespace Quiz_App_API.Data.DTOs.Request
{
    public class UpdateQuestionRequestDTOcs
    {
        public int? Id { get; set; }  // Added to handle existing questions
        public string Text { get; set; }
        public List<UpdateAlternativeRequestDTO> Alternatives { get; set; }  // Changed from CreateAlternativeRequestDTO
    }
}
