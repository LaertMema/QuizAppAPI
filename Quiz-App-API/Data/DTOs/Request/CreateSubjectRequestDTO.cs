using System.ComponentModel.DataAnnotations;

namespace Quiz_App_API.Data.DTOs.Request
{
    public class CreateSubjectRequestDTO
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
