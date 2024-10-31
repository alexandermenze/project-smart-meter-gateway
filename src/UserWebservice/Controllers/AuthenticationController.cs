using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Database.Repositories;
using Domain.Models;
using Domain.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UserWebservice.Configurations;
using UserWebservice.Models;

namespace UserWebservice.Controllers;

// This controller is responsible for handling the authentication of users.
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(
    UserRepository userRepository,
    LogRepository logRepository,
    IPasswordHasher<string> passwordHasher,
    JwtOptions jwtOptions)
    : ControllerBase
{
    // This is the only endpoint allowing access without authentication as marked by the explicit AllowAnonymous 
    [AllowAnonymous]
    // Enable rate limiting for the authentication endpoint to further reduce the attack surface for brute force attacks
    [EnableRateLimiting(PolicyConstants.RateLimitPerIp)]
    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] LoginCredential credential)
    {
        // Was added by SolarLint
        // Make sure that the username and passwords are not empty or too long. The maximum length is specified in
        // the model class as attributes.
        // This also makes sure that all properties exist and were mapped successfully.
        if (ModelState.IsValid is false)
            return BadRequest();

        // Since the username is unique, we can directly query for it.
        var user = await userRepository.GetByUsername(credential.Username);

        if (user is null)
            return Unauthorized();

        if (await HandlePasswordHashVerification(user, credential.Password) is false)
        {
            // To inform the user about the failed login attempt, we log the event.
            await logRepository.Insert(new LogEntry(0, user.Id, MeterId: null, DateTime.Now, LogLevel.Warning,
                $"Invalid login attempt from {HttpContext.Connection.RemoteIpAddress?.ToString()}.",
                nameof(UserWebservice)));

            // By returning Unauthorized instead of BadRequest, we do not leak information about the existence of the user.
            return Unauthorized();
        }

        // We also log successful logins to keep track of the user's activity and allow users to detect possible successful attacks.
        await logRepository.Insert(new LogEntry(0, user.Id, MeterId: null, DateTime.Now, LogLevel.Warning,
            "Successful login.", nameof(UserWebservice)));

        var token = GenerateJwtToken(user);

        // The token is returned to the user in the HTTP response body.
        return Ok(token);
    }

    private async Task<bool> HandlePasswordHashVerification(User user, string password)
    {
        var passwordHashResult = passwordHasher.VerifyHashedPassword(user.Username, user.PasswordHash, password);

        // By explicitly returning in every case, we make sure that the method is not vulnerable to new enum values.
        switch (passwordHashResult)
        {
            case PasswordVerificationResult.Success:
                // We do nothing in this case and continue with the login
                return true;

            case PasswordVerificationResult.SuccessRehashNeeded:
            {
                // The used password hash algorithm is outdated and should be updated.
                // This automatically applies the new best practises for password hashing. This is only possible on
                // login as the user is required to provide the password since it's never stored in plain text.
                var newHash = passwordHasher.HashPassword(user.Username, password);
                await userRepository.UpdatePasswordHash(user, newHash);

                return true;
            }

            case PasswordVerificationResult.Failed:
                return false;

            default:
                // As all enum values are covered by the switch, this case should never be reached. If the rare case
                // happens that a new value is added to the enum, the default case will catch it and abort the login.
                // It's better to make sure the application is not vulnerable to new enum values by explicitly handling
                // this case.
                return false;
        }
    }

    private string GenerateJwtToken(User user)
    {
        // The only information stored in the token is the user's name.
        // Possession of the token proves that the user is authenticated and authorized to access the service.
        // Since JWTs are not encrypted it is important that they are transmitted securely. In addition, the token should not contain sensitive information.
        // A token is still better than sending the username and password with every request as it expires.
        ImmutableArray<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Username)
        ];

        var token = new JwtSecurityToken(
            claims: claims,
            issuer: TokenConstants.Issuer,
            audience: TokenConstants.Audience,
            expires: DateTime.Now.Add(jwtOptions.TokenLifetime),
            signingCredentials: jwtOptions.TokenSigningCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}