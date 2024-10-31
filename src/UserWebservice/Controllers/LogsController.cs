using System.Security.Claims;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using UserWebservice.Dtos;

namespace UserWebservice.Controllers;

// This class is responsible for handling requests for logs
// Pagination has been omitted due to project scope
[ApiController]
[Route("api/[controller]")]
public class LogsController(
    LogRepository logRepository,
    UserRepository userRepository,
    MeterRepository meterRepository)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLogs()
    {
        var username = HttpContext.User.Claims.FirstOrDefault(c => c.Type is ClaimTypes.NameIdentifier)?.Value;

        // This should never occur
        // Fail fast if it does
        if (username is null)
            return Unauthorized();

        var user = await userRepository.GetByUsername(username);

        if (user is null)
            return Unauthorized();

        // If logs happen in a user context they are logged with the user id
        // If logs happen in a meter context they are logged with the meter id
        // The users should be able to see all logs for all meters and for themselves
        
        var smartMeters = await meterRepository.GetByUserId(user.Id);
        
        var meterLogs = await smartMeters
            .ToAsyncEnumerable()
            .SelectAwait(async meter => new { MeterId = meter.Id, Logs = await logRepository.GetForMeter(meter.Id) })
            .Select(l => new MeterLogResponseDto(l.MeterId, l.Logs))
            .ToListAsync();

        var userLogs = await logRepository.GetForUser(user.Id);

        var response = new LogResponseDto(userLogs, [..meterLogs]);

        return Ok(response);
    }
}