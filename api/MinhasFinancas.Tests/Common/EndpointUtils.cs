namespace MinhasFinancas.Tests.Common;

public static class EndpointUtils
{
   public static string BuildUrl(string baseUrl, string? endpoint = default, string queryParams = "")
    {
        endpoint ??= string.Empty;

        if (!endpoint.StartsWith('/') && !string.IsNullOrEmpty(endpoint))
            endpoint = $"/{endpoint}";

        if (!string.IsNullOrEmpty(queryParams) && !queryParams.StartsWith('?'))
            queryParams = $"?{queryParams}";
        
        return $"{baseUrl}{endpoint}{queryParams}";
    }
}
