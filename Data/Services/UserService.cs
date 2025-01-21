using Microsoft.AspNetCore.Identity;
using Quiz_App_API.Data.Models;
using static Quiz_App_API.Data.DTOs.Request.UserRequests;
using static Quiz_App_API.Data.DTOs.Response.UserResponses;

namespace Quiz_App_API.Data.Services
{
    public class UserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;

        public UserService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "User with this email already exists"
                };
            }

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            return new AuthResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Success = true,
                Message = "Registration successful"
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, request.Password, isPersistent: false, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            return new AuthResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Success = true,
                Message = "Login successful"
            };
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<UserResponse> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };
        }
    }
}
