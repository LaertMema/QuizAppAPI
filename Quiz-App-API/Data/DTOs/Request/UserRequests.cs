using System.ComponentModel.DataAnnotations;

namespace Quiz_App_API.Data.DTOs.Request
{
    public class UserRequests
    {
        public class RegisterRequest
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string UserName { get; set; }

            [Required]
            [MinLength(6)]
            public string Password { get; set; }
        }

        public class LoginRequest
        {
            [Required]
            public string Email { get; set; }

            [Required]
            public string Password { get; set; }
        }
    }
}
