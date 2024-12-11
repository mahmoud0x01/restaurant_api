using Mahmoud_Restaurant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Mahmoud_Restaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AuthService _authService;

        public AccountController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(Models.UserDto userDto, [FromQuery] string adminSecretKey = null)
        {
            try
            {
                var user = await _authService.Register(userDto, adminSecretKey);
                if (user.IsAdmin)
                {
                    return CreatedAtAction(nameof(Register), new { id = user.Id }, new { username = user.Email, admin = user.IsAdmin });
                }
                else
                {
                    return CreatedAtAction(nameof(Register), new { id = user.Id }, new { username = user.Email });
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid admin secret key." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(Models.LoginDto loginDto)
        {
            var token = await _authService.Login(loginDto);
            if (token == null) return Unauthorized();
            return Ok(new { Token = token });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Get the current user's email from the JWT claims
                var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
                var jti = User.FindFirst("jti")?.Value;
                if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(jti))
                {
                    return Unauthorized("Invalid token.");
                }

                // Fetch user details from the database
                var user = await _authService.Authorize(userEmail);
                if (user == null)
                {
                    return NotFound("Already Logged out.");
                }

                // Extract the token from the Authorization header
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest(new { message = "Token is missing." });
                }

                try
                {
                    _authService.BlacklistToken(token); // Call the BlacklistToken method
                    return Ok(new { message = "Logout successful" });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = ex.Message });
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("profile")]
        [Authorize] // Requires user to be authenticated
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // Get the current user's email from the JWT claims
                var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized("Invalid token.");
                }

                // Fetch user details from the database
                var user = await _authService.Authorize(userEmail);
                if (user == null)
                {
                    return NotFound("User not found or not Authorized.");
                }

                // Return user profile details
                var profile = new
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    Address = user.Address,
                    BirthDate = user.BirthDate,
                    Gender = user.Gender,
                    PhoneNumber = user.PhoneNumber,
                    IsAdmin = user.IsAdmin
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
