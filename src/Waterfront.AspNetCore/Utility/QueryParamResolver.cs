using Microsoft.AspNetCore.Http;

namespace Waterfront.AspNetCore.Utility;

public static class QueryParamResolver
{
    public static bool TryGetQueryParams(
        IQueryCollection query,
        out string service,
        out string? account,
        out string? clientId,
        out string? offlineToken,
        out IEnumerable<string> scopes
    )
    {
        service = string.Empty;
        account = clientId = offlineToken = null;
        scopes  = Enumerable.Empty<string>();

        if (!query.ContainsKey("service"))
        {
            // Service is the minimal required parameter set for a valid request
            return false;
        }

        service = query["service"].First();

        if (query.ContainsKey("account"))
        {
            account = query["account"].First();
        }

        if (query.ContainsKey("client_id"))
        {
            clientId = query["client_id"].First();
        }

        if (query.ContainsKey("offline_token"))
        {
            offlineToken = query["offline_token"].First();
        }

        if (query.ContainsKey("scope"))
        {
            scopes = query["scope"];
            // var list = new List<string>();
            // foreach (string value in query["scope"])
            // {
            //     list.Add(value);
            // }
            //
            // scopes = list;
        }

        return true;
    }
}
