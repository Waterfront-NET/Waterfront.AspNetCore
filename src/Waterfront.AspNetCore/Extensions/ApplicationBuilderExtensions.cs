using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Waterfront.AspNetCore.Configuration.Endpoints;
using Waterfront.AspNetCore.Middleware;

namespace Waterfront.AspNetCore.Extensions;

/// <summary>
/// Extension methods for configuring Waterfront middleware in request pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseWaterfront(this IApplicationBuilder builder) =>
    builder.UseWaterfront(_ => { });

    public static IApplicationBuilder UseWaterfront(this IApplicationBuilder builder, Action<EndpointOptions> configureEndpoints)
    {
        ILogger<IApplicationBuilder> logger = builder.ApplicationServices
                                                     .GetRequiredService<
                                                         ILogger<IApplicationBuilder>>();

        IOptions<EndpointOptions> endpointOptions = builder.ApplicationServices
                                                           .GetRequiredService<
                                                               IOptions<EndpointOptions>>();
        
        configureEndpoints(endpointOptions.Value);
        
        PathString tokenEndpoint = endpointOptions.Value.TokenEndpoint;

        if ( !tokenEndpoint.HasValue )
        {
            throw new InvalidOperationException(
                "Cannot use Waterfront without token endpoint configured"
            );
        }

        builder.Map(tokenEndpoint, app => app.UseMiddleware<TokenMiddleware>());
        logger.LogInformation("Token endpoint configured at {TokenEndpointPath}", tokenEndpoint);

        PathString infoEndpoint = endpointOptions.Value.InfoEndpoint; /*TODO*/

        if ( infoEndpoint.HasValue )
        {
            /*Register info endpoint*/
            logger.LogWarning("Info endpoint is not implemented yet");
        }
        else
        {
            logger.LogWarning("No info endpoint configured");
        }

        PathString publicKeyEndpoint = endpointOptions.Value.PublicKeyEndpoint;

        if ( publicKeyEndpoint.HasValue )
        {
            /*Register public key endpoint*/
            logger.LogWarning("Public key endpoint is not implemented yet");
        }
        else
        {
            logger.LogWarning("No public key endpoint configured");
        }

        return builder;
    }

}
