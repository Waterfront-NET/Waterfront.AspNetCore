using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Waterfront.AspNetCore.Configuration;
using Waterfront.AspNetCore.Configuration.Endpoints;
using Waterfront.AspNetCore.Middleware;
using Waterfront.AspNetCore.Services.Authentication;
using Waterfront.AspNetCore.Services.Authorization;
using Waterfront.Core.Tokens.Definition;
using Waterfront.Core.Tokens.Encoders;

namespace Waterfront.AspNetCore.Extensions;

public static class WaterfrontExtensions
{
    public static WaterfrontBuilder AddWaterfront(this IServiceCollection services)
    {
        services.AddOptions();

        services.TryAddSingleton<ITokenDefinitionService, TokenDefinitionService>();
        services.TryAddScoped<TokenRequestAuthenticationService>();
        services.TryAddScoped<TokenRequestAuthorizationService>();

        services.TryAddScoped<TokenMiddleware>();
        
        var builder = new WaterfrontBuilder(services);
        builder.UseDefaultTokenEncoder();

        return builder;
    }

    public static IApplicationBuilder UseWaterfront(
        this IApplicationBuilder builder,
        Action<EndpointOptions>? configureEndpoints = null
    )
    {
        ILogger<IApplicationBuilder> logger = builder.ApplicationServices
                                                     .GetRequiredService<
                                                         ILogger<IApplicationBuilder>>();

        IOptions<EndpointOptions> endpointOptions = builder.ApplicationServices
                                                           .GetRequiredService<
                                                               IOptions<EndpointOptions>>();

        if ( configureEndpoints != null )
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
