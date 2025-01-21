using System.ComponentModel.DataAnnotations;

namespace Quiz_App_API.Data.DTOs.Request
{
    public class CreateQuestionRequestDTO
    {
        
            [Required]
            public string Text { get; set; }

            [Required]
            [MinLength(2)]
            public List<CreateAlternativeRequestDTO> Alternatives { get; set; }
        
    }
}
