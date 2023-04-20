using Microsoft.AspNetCore.Http;
using Waterfront.Common.Authentication.Credentials;

namespace Waterfront.AspNetCore.Extensions;

public static class HttpContextExtensions
{
    public static ConnectionCredentials GetConnectionCredentials(this HttpContext self)
    {
        if ( self.Connection.RemoteIpAddress == null )
        {
            throw new InvalidOperationException(
                "Could not get ConnectionCredentials from current HttpContext: HttpContext.Connection.RemoteIpAddress is null"
            );
        }

        return new ConnectionCredentials {
            IPAddress = self.Connection.RemoteIpAddress,
            Port      = self.Connection.RemotePort
        };
    }
}
