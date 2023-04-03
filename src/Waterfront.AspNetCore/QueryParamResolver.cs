using Microsoft.AspNetCore.Http;

namespace Waterfront.AspNetCore;

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
        scopes  = Array.Empty<string>();

        if (!query.ContainsKey("service"))
        {
            return false;
        }

        service = string.Empty;

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
            var list = new List<string>();
            foreach (string value in query["scope"])
            {
                list.Add(value);
            }

            scopes = list;
        }

        return true;
    }
}
