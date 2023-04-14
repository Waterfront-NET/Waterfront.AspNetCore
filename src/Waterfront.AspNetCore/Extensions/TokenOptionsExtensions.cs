using Waterfront.Core.Configuration.Tokens;

namespace Waterfront.AspNetCore.Extensions;

public static class TokenOptionsExtensions
{
    public static TokenOptions SetLifetime(this TokenOptions self, TimeSpan lifetime)
    {
        self.Lifetime = lifetime;
        return self;
    }

    public static TokenOptions SetLifetime(this TokenOptions self, int lifetimeSeconds) =>
    self.SetLifetime(TimeSpan.FromSeconds(lifetimeSeconds));

    public static TokenOptions SetIssuer(this TokenOptions self, string issuer)
    {
        self.Issuer = issuer;
        return self;
    }
}