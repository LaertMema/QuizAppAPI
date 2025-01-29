using System.ComponentModel.DataAnnotations;

namespace Quiz_App_API.Data.DTOs.Request
{
    public class CreateQuizRequestDTO
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public List<CreateQuestionRequestDTO> Questions { get; set; }
    }
}
