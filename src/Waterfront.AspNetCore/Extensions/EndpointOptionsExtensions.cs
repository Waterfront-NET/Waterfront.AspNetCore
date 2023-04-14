using Microsoft.AspNetCore.Http;
using Waterfront.AspNetCore.Configuration.Endpoints;

namespace Waterfront.AspNetCore.Extensions;

public static class EndpointOptionsExtensions
{
    public static EndpointOptions SetTokenEndpoint(
        this EndpointOptions self,
        PathString tokenEndpoint
    )
    {
        self.TokenEndpoint = tokenEndpoint;
        return self;
    }

    public static EndpointOptions SetInfoEndpoint(
        this EndpointOptions self,
        PathString infoEndpoint
    )
    {
        self.InfoEndpoint = infoEndpoint;
        return self;
    }

    public static EndpointOptions SetPublicKeyEndpoint(
        this EndpointOptions self,
        PathString publicKeyEndpoint
    )
    {
        self.PublicKeyEndpoint = publicKeyEndpoint;
        return self;
    }
}
