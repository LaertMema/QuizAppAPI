using System.ComponentModel.DataAnnotations;

namespace Quiz_App_API.Data.DTOs.Request
{
    public class CreateAlternativeRequestDTO
    {
        [Required]
        public string Text { get; set; }
        [Required]
        public bool IsCorrect { get; set; }
    }
}
