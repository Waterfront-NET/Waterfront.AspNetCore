using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Waterfront.AspNetCore.Extensions;
using Waterfront.AspNetCore.Extensions.Tokens;
using Waterfront.AspNetCore.Json.Converters;
using Waterfront.AspNetCore.Services.Authentication;
using Waterfront.AspNetCore.Services.Authorization;
using Waterfront.Common.Authentication;
using Waterfront.Common.Authorization;
using Waterfront.Common.Contracts.Tokens.Response;
using Waterfront.Common.Tokens;
using Waterfront.Core;
using Waterfront.Core.Jwt;

namespace Waterfront.AspNetCore.Middleware;

public class TokenMiddleware : IMiddleware
{
    private readonly ILogger<TokenMiddleware>          _logger;
    private readonly ITokenRequestService              _tokenRequestService;
    private readonly ITokenDefinitionService           _tokenDefinitionService;
    private readonly ITokenEncoder                     _tokenEncoder;
    private readonly TokenRequestAuthenticationService _authenticationService;
    private readonly TokenRequestAuthorizationService  _authorizationService;

    public TokenMiddleware(
        ILogger<TokenMiddleware> logger,
        ITokenRequestService tokenRequestService,
        ITokenDefinitionService tokenDefinitionService,
        ITokenEncoder tokenEncoder,
        TokenRequestAuthenticationService authenticationService,
        TokenRequestAuthorizationService authorizationService
    )
    {
        _logger                 = logger;
        _tokenRequestService    = tokenRequestService;
        _tokenDefinitionService = tokenDefinitionService;
        _tokenEncoder           = tokenEncoder;
        _authenticationService  = authenticationService;
        _authorizationService   = authorizationService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        using var scope = _logger.BeginScope(context.TraceIdentifier);
        _logger.LogDebug("Starting to handle request");

        if ( context.Request.Method != HttpMethod.Get.Method )
        {
            _logger.LogDebug("Request is not a GET request, aborting");
            await next(context);
            return;
        }

        (bool tokenRequestCreateSuccessful, TokenRequest tokenRequest) =
        await _tokenRequestService.TryCreateRequestAsync(context);

        if ( !tokenRequestCreateSuccessful )
        {
            _logger.LogWarning("Request is invalid");
            await Results.BadRequest(new { error = "Invalid request" }).ExecuteAsync(context);
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
            _logger.LogWarning("Failed to authorize request, authorization failed for the following scopes: {@Scopes}", authzResult.ForbiddenScopes);
            await Results.Unauthorized().ExecuteAsync(context);
            return;
        }

        TokenDefinition tokenDefinition =
        await _tokenDefinitionService.CreateTokenDefinitionAsync(
            tokenRequest,
            authnResult,
            authzResult
        );
        
        _logger.LogDebug("Token definition created: {@TokenDefinition}", tokenDefinition );

        string jwt = await _tokenEncoder.EncodeTokenAsync(tokenDefinition);
        
        _logger.LogDebug("Token encoded: {EncodedTokenValue}", jwt);

        TokenResponse tokenResponse = new TokenResponse {
            Token        = jwt,
            IssuedAt     = tokenDefinition.IssuedAt.ToString("O"),
            ExpiresIn    = tokenDefinition.LifetimeSeconds(),
            RefreshToken = null
        };

        _logger.LogDebug("Token response created: {@TokenResponse}", tokenResponse);

        await context.Response.WriteAsJsonAsync(
            tokenResponse,
            new JsonSerializerOptions { Converters = { TokenResponseJsonConverter.Instance } }
        );
        context.Response.StatusCode = HttpStatusCode.OK.ToInt32();
    }
}
