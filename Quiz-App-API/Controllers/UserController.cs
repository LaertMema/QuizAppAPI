using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quiz_App_API.Data.Services;
using static Quiz_App_API.Data.DTOs.Response.UserResponses;
using System.Security.Claims;
using static Quiz_App_API.Data.DTOs.Request.UserRequests;

namespace Quiz_App_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            try
            {
                var result = await _userService.RegisterAsync(request);
                if (!result.Success)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new AuthResponse { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            try
            {
                var result = await _userService.LoginAsync(request);
                if (!result.Success)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new AuthResponse { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            await _userService.LogoutAsync();
            return Ok();
        }

        [HttpGet("current")]
        [Authorize]
        public async Task<ActionResult<UserResponse>> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //Kujdes vi nje handler ketu'
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(user);
        }
    }
}
