using Microsoft.AspNetCore.Http;
using Waterfront.Common.Tokens;
using Waterfront.Core;
using Waterfront.Core.Utility.Parsing;

namespace Waterfront.AspNetCore.Extensions.Tokens;

public static class TokenRequestServiceExtensions
{
    public static async ValueTask<TokenRequest> CreateRequestAsync(
        this ITokenRequestService self,
        HttpContext context
    )
    {
        var result = await self.TryCreateRequestAsync(context);

        if ( !result.success )
        {
            throw new Exception("Failed to parse query string");
        }

        return result.request;
    }

    public static async  ValueTask<(bool success, TokenRequest request)> TryCreateRequestAsync(
        this ITokenRequestService self,
        HttpContext context
    )
    {
        if ( !QueryParamResolver.TryGetQueryParams(
                 context.Request.Query,
                 out string service,
                 out string? account,
                 out string? clientId,
                 out string? offlineToken,
                 out IEnumerable<string> scopes
             ) )
        {
            return default;
        }

        var request = await self.CreateRequestAsync(
            service,
            context.Connection.RemoteIpAddress!,
            context.Connection.RemotePort,
            account,
            clientId,
            PrimitiveParser.ParseBoolean(offlineToken),
            scopes
        );

        return (true, request);
    }
}
