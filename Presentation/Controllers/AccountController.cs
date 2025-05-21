using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using Presentation.Services;

namespace Presentation.Controllers
{

    [Route("api/auth")]
    [ApiController]
    public class AccountController(AccountService accountService, IConfiguration configuration) : ControllerBase
    {
        private readonly AccountService _accountService = accountService;
        private readonly IConfiguration _configuration = configuration;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto user)
        {
            if (user == null)
                return BadRequest("User data is null");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _accountService.RegisterUserAsync(user);
            if (result == null)
                return BadRequest("User registration failed");
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));


            return Ok("User registered successfully");
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] UserSigninDto user)
        {
            var validUser = await _accountService.SignInAsync(user.Email, user.Password);
            if (validUser == null)
                return Unauthorized("Invalid credentials"); 

            var token = _accountService.GenerateJwtToken(validUser, _configuration);
            var userId = validUser.Id;
            var fullName = $"{validUser.FirstName} {validUser.LastName}";
            return Ok(new { token, userId, fullName});
        }

        [HttpGet("exists/{email}")]
        public async Task<IActionResult> Exists(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is null or empty");

            var exists = await _accountService.ExistsAsync(email);
            if (exists)
                return BadRequest("User already exists");

            await _accountService.SendVerificationCodeAsync(email);
            return Ok(exists);
        }

        [HttpPost("verify")]
        public IActionResult Verify([FromBody] UserVerificationDto user)
        {
            if (user == null)
                return BadRequest("User data is null");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = _accountService.VerifyCode(user);
            if (!result)
                return BadRequest("Invalid or expired code");

            return Ok("User verified successfully");
        }

    }
}
