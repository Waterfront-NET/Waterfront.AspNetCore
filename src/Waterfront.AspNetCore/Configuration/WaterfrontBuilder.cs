using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Waterfront.AspNetCore.Configuration.Endpoints;
using Waterfront.AspNetCore.Extensions;
using Waterfront.Core;
using Waterfront.Core.Authentication;
using Waterfront.Core.Authorization;
using Waterfront.Core.Configuration;
using Waterfront.Core.Configuration.Tokens;
using Waterfront.Core.Security.Cryptography;

namespace Waterfront.AspNetCore.Configuration;

public class WaterfrontBuilder
{
    private readonly IServiceCollection _services;

    public WaterfrontBuilder(IServiceCollection services)
    {
        _services = services;
        services.AddWaterfrontCore();
    }

    public WaterfrontBuilder WithCertificateProvider<TProvider>(
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    ) where TProvider : ITokenCertificateProvider
    {
        ServiceDescriptor descriptor = ServiceDescriptor.Describe(
            typeof(ITokenCertificateProvider),
            typeof(TProvider),
            lifetime
        );
        _services.TryAdd(descriptor);
        return this;
    }

    public WaterfrontBuilder WithCertificateProvider<TProvider, TOptions>(
        Action<TOptions> configureOptions,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    ) where TProvider : TokenCertificateProvider<TOptions> where TOptions : class
    {
        ServiceDescriptor descriptor = ServiceDescriptor.Describe(
            typeof(ITokenCertificateProvider),
            typeof(TProvider),
            lifetime
        );
        _services.TryAdd(descriptor);
        _services.AddSingleton<IConfigureOptions<TOptions>>(
            new ConfigureNamedOptions<TOptions>(Options.DefaultName, configureOptions)
        );
        return this;
    }

    public WaterfrontBuilder WithAuthentication<TService>()
    where TService : class, IAclAuthenticationService
    {
        _services.AddScoped<IAclAuthenticationService, TService>();
        return this;
    }

    public WaterfrontBuilder WithAuthentication<TService, TOptions>(
        Action<TOptions> configureOptions
    ) where TService : AclAuthenticationService<TOptions> where TOptions : class
    {
        _services.AddScoped<IAclAuthenticationService, TService>();
        _services.AddSingleton<IConfigureOptions<TOptions>>(
            new ConfigureNamedOptions<TOptions>(Options.DefaultName, configureOptions)
        );
        return this;
    }

    public WaterfrontBuilder WithAuthorization<TService>()
    where TService : class, IAclAuthorizationService
    {
        _services.AddScoped<IAclAuthorizationService, TService>();
        return this;
    }

    public WaterfrontBuilder WithAuthorization<TService, TOptions>(
        Action<TOptions> configureOptions
    ) where TService : AclAuthorizationService<TOptions> where TOptions : class
    {
        _services.AddScoped<IAclAuthorizationService, TService>();
        _services.AddSingleton<IConfigureOptions<TOptions>>(
            new ConfigureNamedOptions<TOptions>(Options.DefaultName, configureOptions)
        );
        return this;
    }

    public WaterfrontBuilder ConfigureTokenOptions(Action<TokenOptions> configureOptions)
    {
        _services.AddSingleton<IConfigureOptions<TokenOptions>>(
            new ConfigureNamedOptions<TokenOptions>(Options.DefaultName, configureOptions)
        );
        return this;
    }

    public WaterfrontBuilder ConfigureEndPoints(Action<EndpointOptions> configureOptions)
    {
        _services.AddSingleton<IConfigureOptions<EndpointOptions>>(
            new ConfigureOptions<EndpointOptions>(configureOptions)
        );
        return this;
    }
}
