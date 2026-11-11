using System.Security.Cryptography;

namespace A11yReport.Core;

public static class FileHash
{
    public static string Sha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexStringLower(SHA256.HashData(stream));
    }
}
