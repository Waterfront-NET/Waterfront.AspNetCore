using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Waterfront.AspNetCore.Extensions;
using Waterfront.Common.Authentication.Credentials;
using Waterfront.Common.Tokens;
using Waterfront.Core.Utility.Parsing;
using Waterfront.Core.Utility.Parsing.Acl;

namespace Waterfront.AspNetCore.Services.TokenRequests;

public class TokenRequestCreationService
{
    private readonly ILogger<TokenRequestCreationService> _logger;

    public TokenRequestCreationService(ILogger<TokenRequestCreationService> logger)
    {
        _logger = logger;
    }

    public TokenRequest CreateRequest(HttpContext context)
    {
        _logger.LogDebug(
            "Started binding context for request {@RequestId}",
            context.TraceIdentifier
        );

        IQueryCollection query = context.Request.Query;

        if ( !TryGetService(query, out string service) )
        {
            _logger.LogError("Failed to get the 'service' value from request query");
            throw new InvalidOperationException("Failed to get value: service");
        }

        string   account         = query["account"].ToString();
        string   clientId        = query["client_id"].ToString();
        string   strOfflineToken = query["offline_token"].ToString();
        string[] strScopes       = query["scope"].ToArray();

        BasicCredentials basicCredentials =
        BasicAuthParser.IsBasicAuth(context.Request.Headers.Authorization) switch {
            true => BasicCredentials.FromTuple(
                BasicAuthParser.ParseHeaderValue(context.Request.Headers.Authorization)
            ),
            false => BasicCredentials.Empty
        };
        ConnectionCredentials   connectionCredentials   = context.GetConnectionCredentials();
        RefreshTokenCredentials refreshTokenCredentials = RefreshTokenCredentials.Empty;

        return new TokenRequest {
            Id      = context.TraceIdentifier,
            Service = service,
            Account = account,
            Client  = clientId,
            OfflineToken = strOfflineToken switch {
                               { Length: not 0 } => bool.Parse(strOfflineToken),
                               _                 => false
                           },
            Scopes = new List<TokenRequestScope>(
                strScopes.Select(AclEntityParser.ParseTokenRequestScope)
            ),
            BasicCredentials        = basicCredentials,
            ConnectionCredentials   = connectionCredentials,
            RefreshTokenCredentials = refreshTokenCredentials
        };
    }

    private bool TryGetService(IQueryCollection query, out string service)
    {
        service = query["service"].ToString();

        return !string.IsNullOrEmpty(service);
    }
}
