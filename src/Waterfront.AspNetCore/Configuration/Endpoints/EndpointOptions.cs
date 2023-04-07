using Microsoft.AspNetCore.Http;

namespace Waterfront.AspNetCore.Configuration.Endpoints;

public class EndpointOptions
{
    public PathString TokenEndpoint { get; set; }
    public PathString InfoEndpoint { get; set; }
    public PathString PublicKeyEndpoint { get; set; }
}
