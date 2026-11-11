namespace A11yReport.Core.Input;

public static class TargetUrl
{
    /// <summary>
    /// Parses a URL typed by a person. A missing scheme defaults to https.
    /// Only absolute http and https URLs are accepted.
    /// </summary>
    public static bool TryParse(string? text, out Uri url)
    {
        url = null!;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var candidate = text.Contains("://", StringComparison.Ordinal) ? text.Trim() : $"https://{text.Trim()}";
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var parsed) || !IsWebScheme(parsed))
        {
            return false;
        }

        url = parsed;
        return true;
    }

    private static bool IsWebScheme(Uri url) =>
        url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps;
}
