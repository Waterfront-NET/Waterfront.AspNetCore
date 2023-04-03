using System.Web.Http.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Waterfront.AspNetCore.Extensions;

namespace Waterfront.AspNetCore.Middleware;

public class WaterfrontMiddleware : IMiddleware
{
    private readonly ILogger<WaterfrontMiddleware>       _logger;
    private readonly IOptions<WaterfrontEndpointOptions> _endpointOptions;

    public WaterfrontMiddleware(
        ILogger<WaterfrontMiddleware> logger,
        IOptions<WaterfrontEndpointOptions> endpointOptions
    )
    {
        _logger          = logger;
        _endpointOptions = endpointOptions;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path == _endpointOptions.Value.TokenEndpoint)
        {
            await InvokeTokenEndpoint(context);
        }
        else if (context.Request.Path == _endpointOptions.Value.InfoEndpoint)
        {
            await InvokeInfoEndpoint(context);
        }
        else if (context.Request.Path == _endpointOptions.Value.PublicKeyEndpoint)
        {
            await InvokePublicKeyEndpoint(context);
        }

        await next(context);
    }

    private async Task InvokeTokenEndpoint(HttpContext context)
    {
        IQueryCollection query = context.Request.Query;

        if (!QueryParamResolver.TryGetQueryParams(
                query,
                out var service,
                out var account,
                out var clientId,
                out var offlineToken,
                out var scopes
            ))
        {
            
        }
    }

    private Task InvokeInfoEndpoint(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private Task InvokePublicKeyEndpoint(HttpContext context)
    {
        throw new NotImplementedException();
    }
}
