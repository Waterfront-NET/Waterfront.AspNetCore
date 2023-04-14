using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Waterfront.AspNetCore.Extensions;
using Waterfront.AspNetCore.Services.TokenRequests;
using Waterfront.Common.Authentication;
using Waterfront.Common.Authorization;
using Waterfront.Common.Contracts.Tokens.Response;
using Waterfront.Common.Tokens;
using Waterfront.Core.Json.Converters;
using Waterfront.Core.Tokens.Definition;
using Waterfront.Core.Tokens.Encoders;
using Waterfront.Core.Utility.Serialization.Acl;

namespace Waterfront.AspNetCore.Middleware;

public class TokenMiddleware : IMiddleware
{
    private static readonly JsonSerializerOptions s_JsonSerializerOptions =
    new JsonSerializerOptions { Converters = { TokenResponseJsonConverter.Instance } };
    
    private readonly ILogger<TokenMiddleware>          _logger;
    private readonly ITokenDefinitionService           _tokenDefinitionService;
    private readonly ITokenEncoder                     _tokenEncoder;
    private readonly TokenRequestCreationService       _requestCreationService;
    private readonly TokenRequestAuthenticationService _authenticationService;
    private readonly TokenRequestAuthorizationService  _authorizationService;
    

    public TokenMiddleware(
        ILogger<TokenMiddleware> logger,
        ITokenDefinitionService tokenDefinitionService,
        ITokenEncoder tokenEncoder,
        TokenRequestCreationService requestCreationService,
        TokenRequestAuthenticationService authenticationService,
        TokenRequestAuthorizationService authorizationService
    )
    {
        _logger                 = logger;
        _tokenDefinitionService = tokenDefinitionService;
        _tokenEncoder           = tokenEncoder;
        _requestCreationService = requestCreationService;
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

        TokenRequest tokenRequest;

        try
        {
            tokenRequest = _requestCreationService.CreateRequest(context);
        }
        catch (InvalidOperationException exception)
        {
            await Results.BadRequest(exception).ExecuteAsync(context);
            return;
        }

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
                         options: s_JsonSerializerOptions,
                         statusCode: HttpStatusCode.OK.ToInt32()
                     )
                     .ExecuteAsync(context);

        _logger.LogDebug("Response written [{RequestID}]", requestId);
    }
}
