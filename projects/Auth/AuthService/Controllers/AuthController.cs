using AuthApplication;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("token")]
    public async Task<IActionResult> GenerateToken([FromBody] UserCredential userCredential)
    {
        var user = await _authService.AuthenticateAsync(userCredential.Username, userCredential.Password);
        if (user == null)
        {
            return Unauthorized();
        }

        var token = _authService.GenerateToken(user);
        return Ok(new { token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDTO registrationDto)
    {
        try
        {
            var userId = await _authService.RegisterUser(registrationDto.Username, registrationDto.Password);
            return Ok(new { UserId = userId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}

public class UserCredential
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class UserRegistrationDTO
{
    public string Username { get; set; }
    public string Password { get; set; }
}
