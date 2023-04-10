using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Waterfront.AspNetCore.Configuration.Endpoints;
using Waterfront.AspNetCore.Extensions;
using Waterfront.Core.Authentication;
using Waterfront.Core.Authorization;
using Waterfront.Core.Configuration.Tokens;
using Waterfront.Core.Tokens.Signing.CertificateProviders;

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
    ) where TProvider : ISigningCertificateProvider
    {
        ServiceDescriptor descriptor = ServiceDescriptor.Describe(
            typeof(ISigningCertificateProvider),
            typeof(TProvider),
            lifetime
        );
        _services.TryAdd(descriptor);
        return this;
    }

    public WaterfrontBuilder WithCertificateProvider<TProvider, TOptions>(
        Action<TOptions> configureOptions,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    ) where TProvider : SigningCertificateProviderBase<TOptions> where TOptions : class
    {
        ServiceDescriptor descriptor = ServiceDescriptor.Describe(
            typeof(ISigningCertificateProvider),
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
    ) where TService : AclAuthenticationServiceBase<TOptions> where TOptions : class
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
    ) where TService : AclAuthorizationServiceBase<TOptions> where TOptions : class
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

    public WaterfrontBuilder UseConfiguration(IConfiguration configuration)
    {
        IConfigurationSection? endpointsSection = configuration.GetSection("Endpoints");

        if ( endpointsSection.Exists() )
        {
            _services.AddSingleton<IConfigureOptions<EndpointOptions>>(
                new ConfigureOptions<EndpointOptions>(endpointsSection.Bind)
            );
        }

        IConfigurationSection? tokensSection = configuration.GetSection("Tokens");

        if ( tokensSection.Exists() )
        {
            _services.AddSingleton<IConfigureOptions<TokenOptions>>(
                new ConfigureOptions<TokenOptions>(tokensSection.Bind)
            );
        }

        IConfigurationSection? certificateProvidersSection = configuration.GetSection("CertificateProviders");

        if ( !certificateProvidersSection.Exists() )
        {
            certificateProvidersSection = configuration.GetSection("Certificate_Providers");
        }

        if ( certificateProvidersSection.Exists() )
        {
            IConfigurationSection? fileProvider = certificateProvidersSection.GetSection("File");

            if ( fileProvider.Exists() )
            {
                WithCertificateProvider<FileSigningCertificateProvider,
                    FileTokenCertificateProviderOptions>(
                    options => {
                        fileProvider.Bind(options);
                    }
                );
            }
        }
    }
}
