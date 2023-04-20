using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Waterfront.AspNetCore.Configuration.Endpoints;
using Waterfront.AspNetCore.Middleware;
using Waterfront.AspNetCore.Services.TokenRequests;
using Waterfront.Extensions.DependencyInjection;

namespace Waterfront.AspNetCore.Extensions;

public static class WaterfrontBuilderExtensions
{
    public static IWaterfrontBuilder AddTokenMiddleware(this IWaterfrontBuilder builder)
    {
        builder.Services.TryAddSingleton<TokenRequestCreationService>();
        builder.Services.TryAddScoped<TokenRequestAuthenticationService>();
        builder.Services.TryAddScoped<TokenRequestAuthorizationService>();
        builder.Services.TryAddScoped<TokenMiddleware>();

        return builder;
    }

    public static IWaterfrontBuilder ConfigureEndpoints(
        this IWaterfrontBuilder builder,
        Action<EndpointOptions> configureOptions
    ) => builder.ConfigureEndpoints(
        (EndpointOptions opt, IServiceProvider _) => configureOptions(opt)
    );

    public static IWaterfrontBuilder ConfigureEndpoints(
        this IWaterfrontBuilder builder,
        Action<EndpointOptions, IServiceProvider> configureOptions
    )
    {
        builder.Services.AddTransient<IConfigureOptions<EndpointOptions>>(
            sp => new ConfigureOptions<EndpointOptions>(opt => configureOptions(opt, sp))
        );

        return builder;
    }

    public static IWaterfrontBuilder ConfigureEndpoints(
        this IWaterfrontBuilder builder,
        Action<EndpointOptions, IConfiguration> configureOptions
    ) => builder.ConfigureEndpoints<IConfiguration>(configureOptions);

    public static IWaterfrontBuilder ConfigureEndpoints<TDependency>(
        this IWaterfrontBuilder builder,
        Action<EndpointOptions, TDependency> configureOptions
    ) where TDependency : notnull
    {
        return builder.ConfigureEndpoints(
            (EndpointOptions opt, IServiceProvider sp) => configureOptions(
                opt,
                sp.GetRequiredService<TDependency>()
            )
        );
    }
}
