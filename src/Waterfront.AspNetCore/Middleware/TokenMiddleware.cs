using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Waterfront.AspNetCore.Extensions;
using Waterfront.AspNetCore.Json.Converters;
using Waterfront.AspNetCore.Services.Authentication;
using Waterfront.AspNetCore.Services.Authorization;
using Waterfront.AspNetCore.Utility;
using Waterfront.Common.Authentication;
using Waterfront.Common.Authentication.Credentials;
using Waterfront.Common.Authorization;
using Waterfront.Common.Contracts.Tokens.Response;
using Waterfront.Common.Tokens;
using Waterfront.Core;
using Waterfront.Core.Tokens.Encoders;
using Waterfront.Core.Utility.Parsing;
using Waterfront.Core.Utility.Parsing.Acl;
using Waterfront.Core.Utility.Serialization.Acl;

namespace Waterfront.AspNetCore.Middleware;

public class TokenMiddleware : IMiddleware
{
    private static readonly JsonSerializerOptions s_SerializerOptions =
    new JsonSerializerOptions { Converters = { TokenResponseJsonConverter.Instance } };

    private readonly ILogger<TokenMiddleware>          _logger;
    private readonly ITokenDefinitionService           _tokenDefinitionService;
    private readonly ITokenEncoder                     _tokenEncoder;
    private readonly TokenRequestAuthenticationService _authenticationService;
    private readonly TokenRequestAuthorizationService  _authorizationService;

    public TokenMiddleware(
        ILogger<TokenMiddleware> logger,
        ITokenDefinitionService tokenDefinitionService,
        ITokenEncoder tokenEncoder,
        TokenRequestAuthenticationService authenticationService,
        TokenRequestAuthorizationService authorizationService
    )
    {
        _logger                 = logger;
        _tokenDefinitionService = tokenDefinitionService;
        _tokenEncoder           = tokenEncoder;
        _authenticationService  = authenticationService;
        _authorizationService   = authorizationService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // using var scope = _logger.BeginScope(context.TraceIdentifier);
        string requestId = context.TraceIdentifier;

        _logger.LogDebug("Starting to handle request {RequestId}", requestId);

        if ( context.Request.Method != HttpMethod.Get.Method )
        {
            _logger.LogDebug("Request is not a GET request, aborting");
            await next(context);
            return;
        }

        if ( !QueryParamResolver.TryGetQueryParams(
                 context.Request.Query,
                 out string service,
                 out string? account,
                 out string? clientId,
                 out string? offlineToken,
                 out IEnumerable<string> scopes
             ) )
        {
            _logger.LogError("Failed to resolve query params for request {RequestId}", requestId);
            await Results.BadRequest("Invalid request query").ExecuteAsync(context);
            return;
        }

        var (basicUsername, basicPassword) =
        BasicAuthParser.ParseHeaderValue(context.Request.Headers.Authorization);

        TokenRequest tokenRequest = new TokenRequest {
            Id      = requestId,
            Service = service,
            Account = account,
            Client  = clientId,
            OfflineToken = offlineToken switch {
                               null or "" => false,
                               _          => bool.Parse(offlineToken)
                           },
            BasicCredentials = new BasicCredentials(basicUsername, basicPassword),
            ConnectionCredentials = context.GetConnectionCredentials(),
            RefreshTokenCredentials = RefreshTokenCredentials.Empty, /*TODO: Unused now*/
            Scopes = scopes.Select(AclEntityParser.ParseTokenRequestScope).ToArray()
        };

        _logger.LogDebug("Token request created: {@TokenRequest}", tokenRequest);

        AclAuthenticationResult authnResult =
        await _authenticationService.AuthenticateAsync(tokenRequest);

        if ( !authnResult.IsSuccessful )
        {
            _logger.LogWarning("Failed to authenticate request");
            await Results.Unauthorized().ExecuteAsync(context);
            return;
        }

        _logger.LogDebug("Request authenticated: {@AuthenticationResult}", authnResult);

        AclAuthorizationResult authzResult =
        await _authorizationService.AuthorizeAsync(tokenRequest, authnResult);

        if ( !authzResult.IsSuccessful )
        {
            _logger.LogWarning(
                "Failed to authorize request, authorization failed for the following scopes: {@Scopes}",
                authzResult.ForbiddenScopes
            );
            await Results.Json(
                             new {
                                 forbidden_scopes =
                                 authzResult.ForbiddenScopes.Select(scope => scope.ToSerialized())
                             },
                             statusCode: HttpStatusCode.Unauthorized.ToInt32()
                         )
                         .ExecuteAsync(context);
            return;
        }

        TokenDefinition tokenDefinition =
        await _tokenDefinitionService.CreateDefinitionAsync(tokenRequest, authnResult, authzResult);

        _logger.LogDebug("Token definition created: {@TokenDefinition}", tokenDefinition);

        string jwt = await _tokenEncoder.EncodeTokenAsync(tokenDefinition);

        _logger.LogDebug("Token encoded: {EncodedTokenValue}", jwt);

        TokenResponse tokenResponse = TokenResponse.Create(tokenDefinition, jwt, null);

        _logger.LogDebug("Token response created: {@TokenResponse}", tokenResponse);

        await Results.Json(
                         tokenResponse,
                         options: s_SerializerOptions,
                         statusCode: HttpStatusCode.OK.ToInt32()
                     )
                     .ExecuteAsync(context);

        _logger.LogDebug("Response written [{RequestID}]", requestId);
    }
}
