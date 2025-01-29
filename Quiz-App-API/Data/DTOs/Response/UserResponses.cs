namespace Quiz_App_API.Data.DTOs.Response
{
    public class UserResponses
    {  /// <summary>
    /// Na duhen per login e auth
    /// </summary>
        public class UserResponse
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
        }

        public class AuthResponse
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public bool Success { get; set; }
            public string Message { get; set; }
        }
    }
}
