using System.Security.Cryptography;

namespace FashionWeb.Data.Storage;

public class FileStorageService
{
    public async Task<string> ComputeSha256Async(Stream stream)
    {
        using var sha256 = SHA256.Create();
        stream.Position = 0;
        var hash = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexString(hash);
    }
}
