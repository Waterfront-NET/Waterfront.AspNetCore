﻿using Microsoft.Extensions.Logging;
using Waterfront.AspNetCore.Extensions;
using Waterfront.Common.Authentication;
using Waterfront.Common.Authorization;
using Waterfront.Common.Tokens.Requests;

namespace Waterfront.AspNetCore.Services.TokenRequests;

/// <summary>
/// Helper service which aggregates all the results from <see cref="IAclAuthorizationService"/> into final result
/// </summary>
public class TokenRequestAuthorizationService
{
    private readonly ILogger<TokenRequestAuthorizationService> _logger;
    private readonly IEnumerable<IAclAuthorizationService>     _authorizationServices;

    public TokenRequestAuthorizationService(
        ILogger<TokenRequestAuthorizationService> logger,
        IEnumerable<IAclAuthorizationService> authorizationServices
    )
    {
        _logger                = logger;
        _authorizationServices = authorizationServices;
    }

    public async ValueTask<AclAuthorizationResult> AuthorizeAsync(
        TokenRequest request,
        AclAuthenticationResult authnResult
    )
    {
        if ( request.IsAuthenticationRequest )
        {
            // Short-circuit the call to skip all the authorization part since it is not needed
            return new AclAuthorizationResult(request) {
                AuthorizedScopes = Array.Empty<TokenRequestScope>(),
                ForbiddenScopes  = Array.Empty<TokenRequestScope>()
            };
        }

        if ( !authnResult.IsSuccessful )
        {
            throw new InvalidOperationException(
                $"Cannot authorize unauthenticated request ({request.Id})"
            );
        }

        AclAuthorizationResult authzResult =
        new AclAuthorizationResult(request) { ForbiddenScopes = request.Scopes };

        foreach ( IAclAuthorizationService service in _authorizationServices )
        {
            AclAuthorizationResult currentResult =
            await service.AuthorizeAsync(request, authnResult, authzResult);

            _logger.LogInformation(
                "Current result: {@CurrentResult}\nOld result: {@OldResult}",
                currentResult,
                authzResult
            );

            authzResult = authzResult.Combine(currentResult);

            _logger.LogInformation("Mutated result: {@MutResult}", authzResult);
            if ( authzResult.IsSuccessful )
            {
                break;
            }
        }

        if ( !authzResult.IsSuccessful )
        {
            _logger.LogWarning(
                "Failed to authorize scopes {@Scopes} on request {RequestId}",
                authzResult.ForbiddenScopes,
                request.Id
            );
        }
        else
        {
            _logger.LogInformation("Request {RequestId} was successfully authorized", request.Id);
        }

        return authzResult;
    }
}
