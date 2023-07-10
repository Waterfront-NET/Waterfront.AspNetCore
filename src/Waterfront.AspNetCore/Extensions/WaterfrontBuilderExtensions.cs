using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Waterfront.AspNetCore.Configuration.Endpoints;
using Waterfront.AspNetCore.Middleware;
using Waterfront.AspNetCore.Services.TokenRequests;
using Waterfront.Core.Extensions.DependencyInjection;

namespace Waterfront.AspNetCore.Extensions;

public static class WaterfrontBuilderExtensions
{
    public static IWaterfrontBuilder AddTokenMiddleware(this IWaterfrontBuilder builder)
    {
        builder.Services.TryAddSingleton<TokenRequestService>();
        builder.Services.TryAddScoped<TokenMiddleware>();

        return builder;
    }

    public static IWaterfrontBuilder ConfigureEndpoints(
        this IWaterfrontBuilder builder,
        Action<EndpointOptions> configureOptions
    )
    {
        builder.Services.Configure(configureOptions);
        return builder;
    }
}
