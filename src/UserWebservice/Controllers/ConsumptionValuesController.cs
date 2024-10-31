using System.Collections.Immutable;
using System.Security.Claims;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace UserWebservice.Controllers;

// This class is responsible for handling requests for consumption values
// Pagination has been omitted due to project scope
[ApiController]
[Route("api/[controller]")]
public class ConsumptionValuesController(
    ConsumptionValueRepository consumptionValueRepository,
    UserRepository userRepository,
    MeterRepository meterRepository)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetConsumptionValues()
    {
        var username = HttpContext.User.Claims.FirstOrDefault(c => c.Type is ClaimTypes.NameIdentifier)?.Value;

        // This should never occur 
        // Fail fast if it does
        if (username is null)
            return Unauthorized();

        var user = await userRepository.GetByUsername(username);

        // If the user from the token does not exist, it must have been deleted.
        if (user is null)
            return Unauthorized();

        var smartMeters = await meterRepository.GetByUserId(user.Id);
        
        // One user can potentially have multiple smart meters
        // All consumption values for all smart meters are returned
        var meterConsumptionValues = await smartMeters
            .ToAsyncEnumerable()
            .SelectAwait(async meter => await consumptionValueRepository.GetForMeter(meter.Id))
            .ToListAsync();

        var consumptionValues = meterConsumptionValues
            .SelectMany(v => v)
            .ToImmutableArray();

        return Ok(consumptionValues);
    }
}