using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Waterfront.AspNetCore.Configuration.Endpoints;
using Waterfront.Core.Authentication;
using Waterfront.Core.Authorization;
using Waterfront.Core.Configuration.Tokens;
using Waterfront.Core.Tokens.Definition;
using Waterfront.Core.Tokens.Encoders;
using Waterfront.Core.Tokens.Signing.CertificateProviders;

namespace Waterfront.AspNetCore.Configuration;

public sealed class WaterfrontBuilder
{
    private readonly IServiceCollection _services;

    public WaterfrontBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public WaterfrontBuilder WithTokenEncoder<TEncoder>(
        ServiceLifetime lifetime = ServiceLifetime.Singleton
    ) where TEncoder : ITokenEncoder
    {
        ServiceDescriptor descriptor = ServiceDescriptor.Describe(
            typeof(ITokenEncoder),
            typeof(TEncoder),
            lifetime
        );

        _services.Replace(descriptor);

        return this;
    }

    public WaterfrontBuilder ConfigureEndpoints(Action<EndpointOptions> configureEndpoints)
    {
        return ConfigureEndpoints((_, endpoints) => configureEndpoints(endpoints));
    }

    public WaterfrontBuilder ConfigureEndpoints(
        Action<IServiceProvider, EndpointOptions> configureEndpoints
    )
    {
        ServiceDescriptor descriptor = ServiceDescriptor.Describe(
            typeof(IConfigureOptions<EndpointOptions>),
            sp => new ConfigureOptions<EndpointOptions>(opt => configureEndpoints(sp, opt)),
            ServiceLifetime.Transient
        );
        _services.Replace(descriptor);
        return this;
    }

    public WaterfrontBuilder ConfigureTokens(Action<TokenOptions> configureTokens)
    {
        return ConfigureTokens((_, tokens) => configureTokens(tokens));
    }

    public WaterfrontBuilder ConfigureTokens(Action<IServiceProvider, TokenOptions> configureTokens)
    {
        ServiceDescriptor descriptor = ServiceDescriptor.Describe(
            typeof(IConfigureOptions<TokenOptions>),
            sp => new ConfigureOptions<TokenOptions>(opt => configureTokens(sp, opt)),
            ServiceLifetime.Transient
        );
        _services.Replace(descriptor);
        return this;
    }

    public WaterfrontBuilder WithAuthentication<TAuthnService>(
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TAuthnService : IAclAuthenticationService
    {
        ValidateAuthServiceLifetime(lifetime);

        ServiceDescriptor descriptor = ServiceDescriptor.Describe(
            typeof(IAclAuthenticationService),
            typeof(TAuthnService),
            lifetime
        );
        _services.Add(descriptor);
        return this;
    }

    public WaterfrontBuilder WithAuthentication<TAuthnService, TServiceOptions>(
        Action<TServiceOptions> configureOptions,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TAuthnService : AclAuthenticationServiceBase<TServiceOptions>
        where TServiceOptions : class
    {
        ValidateAuthServiceLifetime(lifetime);

        ServiceDescriptor descriptor = ServiceDescriptor.Describe(
            typeof(IAclAuthenticationService),
            typeof(TAuthnService),
            lifetime
        );
        ServiceDescriptor optionsDescriptor = ServiceDescriptor.Describe(
            typeof(IConfigureOptions<TServiceOptions>),
            _ => new ConfigureOptions<TServiceOptions>(configureOptions),
            ServiceLifetime.Transient
        );
        _services.Add(descriptor);
        _services.Replace(optionsDescriptor);
        return this;
    }

    public WaterfrontBuilder WithAuthentication<TAuthnService, TServiceOptions>(
        Action<IServiceProvider, TServiceOptions> configureOptions,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    ) where TAuthnService : AclAuthenticationServiceBase<TServiceOptions>
      where TServiceOptions : class
    {
        ValidateAuthServiceLifetime(lifetime);

        ServiceDescriptor descriptor = ServiceDescriptor.Describe(
            typeof(IAclAuthenticationService),
            typeof(TAuthnService),
            lifetime
        );
        ServiceDescriptor optionsDescriptor = ServiceDescriptor.Describe(
            typeof(IConfigureOptions<TServiceOptions>),
            sp => new ConfigureOptions<TServiceOptions>(opt => configureOptions(sp, opt)),
            ServiceLifetime.Transient
        );
        _services.Add(descriptor);
        _services.Replace(optionsDescriptor);
        return this;
    }

    public WaterfrontBuilder WithAuthentication<TAuthnService, TServiceOptions, TDep>(
        Action<TDep, TServiceOptions> configureOptions,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    ) where TAuthnService : AclAuthenticationServiceBase<TServiceOptions>
      where TServiceOptions : class
      where TDep : notnull
    {
        return WithAuthentication<TAuthnService, TServiceOptions>(
            (sp, opt) => configureOptions(sp.GetRequiredService<TDep>(), opt)
        );
    }

    public WaterfrontBuilder WithAuthorization<TAuthzService>(
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TAuthzService : IAclAuthorizationService
    {
        ValidateAuthServiceLifetime(lifetime);

        ServiceDescriptor descriptor = ServiceDescriptor.Describe(
            typeof(IAclAuthorizationService),
            typeof(TAuthzService),
            lifetime
        );
        _services.Add(descriptor);
        return this;
    }

    public WaterfrontBuilder WithAuthorization<TAuthzService, TServiceOptions>(
        Action<TServiceOptions> configureOptions,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    ) where TAuthzService : AclAuthorizationServiceBase<TServiceOptions>
      where TServiceOptions : class
    {
        ServiceDescriptor descriptor = ServiceDescriptor.Describe(
            typeof(IAclAuthorizationService),
            typeof(TAuthzService),
            lifetime
        );
        ServiceDescriptor optionsDescriptor = ServiceDescriptor.Describe(
            typeof(IConfigureOptions<TServiceOptions>),
            _ => new ConfigureOptions<TServiceOptions>(configureOptions),
            ServiceLifetime.Transient
        );
        _services.Add(descriptor);
        _services.Replace(optionsDescriptor);

        return this;
    }

    public WaterfrontBuilder WithAuthorization<TAuthzService, TServiceOptions>(
        Action<IServiceProvider, TServiceOptions> configureOptions,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    ) where TAuthzService : AclAuthorizationServiceBase<TServiceOptions>
      where TServiceOptions : class
    {
        ValidateAuthServiceLifetime(lifetime);

        ServiceDescriptor descriptor = ServiceDescriptor.Describe(
            typeof(IAclAuthorizationService),
            typeof(TAuthzService),
            lifetime
        );
        ServiceDescriptor optionsDescriptor = ServiceDescriptor.Describe(
            typeof(IConfigureOptions<TServiceOptions>),
            sp => new ConfigureOptions<TServiceOptions>(opt => configureOptions(sp, opt)),
            ServiceLifetime.Transient
        );
        _services.Add(descriptor);
        _services.Replace(optionsDescriptor);

        return this;
    }

    public WaterfrontBuilder WithAuthorization<TAuthzService, TServiceOptions, TDep>(
        Action<TDep, TServiceOptions> configureOptions,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TAuthzService : AclAuthorizationServiceBase<TServiceOptions>
        where TServiceOptions : class
        where TDep : notnull
    {
        return WithAuthorization<TAuthzService, TServiceOptions>(
            (sp, opt) => configureOptions(sp.GetRequiredService<TDep>(), opt)
        );
    }

    public WaterfrontBuilder WithTokenDefinitionService<TDefService>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
    where TDefService : ITokenDefinitionService
    {
        ServiceDescriptor descriptor = ServiceDescriptor.Describe(
            typeof(ITokenDefinitionService),
            typeof(TDefService),
            lifetime
        );

        _services.Replace(descriptor);
        return this;
    }

    private void ValidateAuthServiceLifetime(ServiceLifetime lifetime)
    {
        if (lifetime == ServiceLifetime.Transient)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lifetime),
                "Auth service lifetime cannot be Transient - other core services are resolved as Scoped"
            );
        }
    }
}
