using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LRSCard.CurrencyService.API.Options;
using LRSCard.CurrencyService.API.DTOs.Requests;
using Microsoft.AspNetCore.RateLimiting;

namespace LRSCard.CurrencyService.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[EnableRateLimiting("anonymous")]
public class AuthController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;

    public AuthController(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    /// <summary>
    /// Simulates authentication and returns a JWT token.
    /// You need first a JWT token to be able to test the other operations
    /// Use can use any login and password, and will get a valid token with admin role
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public IActionResult GetToken([FromBody] AuthRequestDTO request)
    {
        //User provides credentals, Identity provider validates and generate the token with user claims
        //User login is sucessfull! Lets fake the token
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, request.Login),
            new Claim(ClaimTypes.Role, "admin")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
            signingCredentials: creds
        );

        // Returning the token
        return Ok(new { access_token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}