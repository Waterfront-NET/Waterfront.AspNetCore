using Waterfront.Common.Authorization;
using Waterfront.Common.Tokens.Requests;

namespace Waterfront.AspNetCore.Extensions;

public static class AclAuthorizationResultExtensions
{
    public static AclAuthorizationResult Combine(
        this AclAuthorizationResult self,
        AclAuthorizationResult other
    )
    {
        List<TokenRequestScope> authorizedScopes =
        self.AuthorizedScopes.Union(other.AuthorizedScopes).ToList();
        List<TokenRequestScope> forbiddenScopes =
        self.ForbiddenScopes.Except(authorizedScopes).ToList();

        return new AclAuthorizationResult {
            Id               = self.Id,
            AuthorizedScopes = authorizedScopes,
            ForbiddenScopes  = forbiddenScopes
        };
    }
}
