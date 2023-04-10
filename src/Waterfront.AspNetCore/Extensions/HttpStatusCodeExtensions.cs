using System.Net;

namespace Waterfront.AspNetCore.Extensions;

internal static class HttpStatusCodeExtensions
{
    public static int ToInt32(this HttpStatusCode self) => (int)self;
}
