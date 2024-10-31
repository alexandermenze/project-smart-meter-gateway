using System.Threading.RateLimiting;
using UserWebservice.Configurations;

namespace UserWebservice.Extensions;

public static class RateLimitExtensions
{
    public static IServiceCollection ConfigureRateLimitingPerIp(this IServiceCollection services)
    {
        // This adds a new rate limiting policy without actually applying it to any endpoint.
        services.AddRateLimiter(opts =>
        {
            opts.AddPolicy(PolicyConstants.RateLimitPerIp, context =>
                // In case the IP address is not available, we use "unknown" as a common key.
                // This fallback ensures that the rate limiter works globally in the case of a missing IP address.
                // This is not ideal for the user but ensures stability and security.
                RateLimitPartition.Get(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ =>
                    new FixedWindowRateLimiter(
                        new FixedWindowRateLimiterOptions
                        {
                            // This policy allows 6 requests per minute from a single IP address.
                            // If the limit is exceeded, two more requests are allowed to be queued until the limit is reset.
                            PermitLimit = 6,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 2,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        })));
        });

        return services;
    }
}