using Microsoft.Extensions.DependencyInjection;
using Waterfront.Core;
using Waterfront.Core.Configuration;

namespace Waterfront.AspNetCore.Extensions;

public static class WaterfrontExtensions
{
    public static WaterfrontBuilder AddWaterfront(this IServiceCollection services)
    {
        
    }

    public static IServiceCollection AddWaterfrontCore(this IServiceCollection services)
    {
        services.AddScoped<ITokenRequestCreationService, TokenRequestCreationService>();
    }
}

public class WaterfrontEndpointOptions
{
    public string TokenEndpoint { get; set; }
    public string InfoEndpoint { get; set; }
    public string PublicKeyEndpoint { get; set; }
}

public class WaterfrontBuilder
{
    private TokenOptions _tokenOptions;
    
    public WaterfrontBuilder()
    {
        
    }
    
    
}