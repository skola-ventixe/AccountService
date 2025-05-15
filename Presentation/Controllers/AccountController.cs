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

    }
}
