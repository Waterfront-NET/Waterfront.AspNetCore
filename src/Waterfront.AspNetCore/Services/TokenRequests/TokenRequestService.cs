using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Waterfront.AspNetCore.Extensions;
using Waterfront.Common.Authentication.Credentials;
using Waterfront.Common.Tokens.Requests;
using Waterfront.Core.Parsing.Acl;
using Waterfront.Core.Parsing.Authentication;

namespace Waterfront.AspNetCore.Services.TokenRequests;

public class TokenRequestService
{
    private const string QUERY_PARAM_ACCOUNT = "account";
    private const string QUERY_PARAM_CLIENT_ID = "client_id";
    private const string QUERY_PARAM_OFFLINE_TOKEN = "offline_token";
    private const string QUERY_PARAM_SCOPE = "scope";

    private readonly ILogger _logger;

    public TokenRequestService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public TokenRequest CreateRequest(HttpContext context)
    {
        _logger.LogDebug(
            "Started binding context for request {@RequestId}",
            context.TraceIdentifier
        );

        IQueryCollection query = context.Request.Query;

        string service = query["service"].ToString();

        if (string.IsNullOrEmpty(service))
        {
            _logger.LogError("Failed to get the 'service' value from request query");
            throw new InvalidOperationException("Failed to get value: service");
        }

        string account = query[QUERY_PARAM_ACCOUNT].ToString();
        string clientId = query[QUERY_PARAM_CLIENT_ID].ToString();
        string strOfflineToken = query[QUERY_PARAM_OFFLINE_TOKEN].ToString();
        string[] strScopes = query[QUERY_PARAM_SCOPE].ToArray()!;

        BasicCredentials basicCredentials = BasicAuthParser.IsBasicAuth(
            context.Request.Headers.Authorization
        ) switch
        {
            true => BasicAuthParser.ParseHeaderValue(context.Request.Headers.Authorization),
            false => default(BasicCredentials)
        };
        ConnectionCredentials connectionCredentials = context.GetConnectionCredentials();
        // TODO
        RefreshTokenCredentials refreshTokenCredentials = new RefreshTokenCredentials { };

        return new TokenRequest
        {
            Id = context.TraceIdentifier,
            Service = service,
            Account = account,
            ClientId = clientId,
            OfflineToken = strOfflineToken switch
            {
                { Length: not 0 } => bool.Parse(strOfflineToken),
                var _ => false
            },
            Scopes = new List<TokenRequestScope>(
                strScopes.Select(AclEntityParser.ParseTokenRequestScope)
            ),
            BasicCredentials = basicCredentials,
            ConnectionCredentials = connectionCredentials,
            RefreshTokenCredentials = refreshTokenCredentials
        };
    }
}
