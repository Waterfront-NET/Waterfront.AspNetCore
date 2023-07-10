using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Waterfront.AspNetCore.Extensions;
using Waterfront.AspNetCore.Services.TokenRequests;
using Waterfront.Common.Authentication;
using Waterfront.Common.Authorization;
using Waterfront.Common.Contracts.Tokens.Response;
using Waterfront.Common.Tokens.Definition;
using Waterfront.Common.Tokens.Encoding;
using Waterfront.Common.Tokens.Requests;
using Waterfront.Core.Serialization.Acl;
using Waterfront.Core.Serialization.Tokens.Converters;

namespace Waterfront.AspNetCore.Middleware;

public class TokenMiddleware : IMiddleware
{
    private static readonly JsonSerializerOptions s_JsonSerializerOptions =
        new JsonSerializerOptions { Converters = { TokenResponseJsonConverter.Instance } };

    private readonly ILogger _logger;
    private readonly TokenRequestService _requestService;
    private readonly IAclAuthenticationService _authenticationService;
    private readonly IAclAuthorizationService _authorizationService;
    private readonly ITokenDefinitionService _tokenDefinitionService;
    private readonly ITokenEncoder _tokenEncoder;

    public TokenMiddleware(
        ILoggerFactory loggerFactory,
        ITokenDefinitionService tokenDefinitionService,
        ITokenEncoder tokenEncoder,
        TokenRequestService requestService,
        IAclAuthenticationService authenticationService,
        IAclAuthorizationService authorizationService
    )
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _tokenDefinitionService = tokenDefinitionService;
        _tokenEncoder = tokenEncoder;
        _requestService = requestService;
        _authenticationService = authenticationService;
        _authorizationService = authorizationService;
    }

    private bool IsRequestMethodValid(HttpContext httpContext) =>
        httpContext.Request.Method == HttpMethod.Get.Method;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        string requestId = context.TraceIdentifier;
        using IDisposable loggerScope = _logger.BeginScope(
            "Processing TokenRequest with id {RequestId}",
            requestId
        )!;

        if (!IsRequestMethodValid(context))
        {
            _logger.LogDebug(
                "Request method is now allowed ({RequestMethod}), failing with code {StatusCode}",
                context.Request.Method,
                HttpStatusCode.MethodNotAllowed.ToInt32()
            );

            await Results
                .StatusCode(HttpStatusCode.MethodNotAllowed.ToInt32())
                .ExecuteAsync(context);
            return;
        }

        TokenRequest tokenRequest;

        try
        {
            _logger.LogDebug("Creating TokenRequest...");
            tokenRequest = _requestService.CreateRequest(context);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError("Failed to create TokenRequest: {Exception}", exception.ToString());
            _logger.LogError("Failing with code {StatusCode}", HttpStatusCode.BadRequest.ToInt32());
            await Results
                .BadRequest(
                    new
                    {
                        exception.Message,
                        exception.Data,
                        exception.StackTrace,
                        exception.Source
                    }
                )
                .ExecuteAsync(context);
            return;
        }

        _logger.LogDebug("Token request created:\n{@TokenRequest}", tokenRequest);

        AclAuthenticationResult authnResult = await _authenticationService.AuthenticateAsync(
            tokenRequest
        );

        if (!authnResult.IsSuccessful)
        {
            _logger.LogWarning(
                "Request authentication failed, returning status code {StatusCode}",
                HttpStatusCode.Unauthorized.ToInt32()
            );
            await Results.Unauthorized().ExecuteAsync(context);
            return;
        }

        _logger.LogDebug(
            "Token request authenticated successfully: {@AuthenticationResult}",
            authnResult
        );

        AclAuthorizationResult authzResult = await _authorizationService.AuthorizeAsync(
            tokenRequest,
            authnResult
        );

        if (!authzResult.IsSuccessful)
        {
            _logger.LogWarning(
                "Failed to authorize request, authorization failed for the following scopes: {@Scopes}",
                authzResult.ForbiddenScopes
            );
            await Results
                .Json(
                    new
                    {
                        forbidden_scopes = authzResult.ForbiddenScopes.Select(
                            scope => scope.ToSerialized()
                        )
                    },
                    statusCode: HttpStatusCode.Unauthorized.ToInt32()
                )
                .ExecuteAsync(context);
            return;
        }

        TokenDefinition tokenDefinition = await _tokenDefinitionService.CreateDefinitionAsync(
            tokenRequest,
            authnResult,
            authzResult
        );

        _logger.LogDebug("Token definition created:\n{@TokenDefinition}", tokenDefinition);

        string jwt = await _tokenEncoder.EncodeTokenAsync(tokenDefinition);

        _logger.LogDebug("Token encoded:\n{EncodedTokenValue}", jwt);

        TokenResponse tokenResponse = TokenResponse.Create(tokenDefinition, jwt, null);

        _logger.LogDebug("Token response created:\n{@TokenResponse}", tokenResponse);

        await Results
            .Json(
                tokenResponse,
                options: s_JsonSerializerOptions,
                statusCode: HttpStatusCode.OK.ToInt32()
            )
            .ExecuteAsync(context);

        _logger.LogDebug("Response written [{RequestID}]", requestId);

        // We cancel following middleware
    }
}
