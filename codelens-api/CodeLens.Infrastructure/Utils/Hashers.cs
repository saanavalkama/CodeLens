using System.Security.Cryptography;
using System.Text;
using CodeLens.Application.Interfaces.Utils;
using CodeLens.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CodeLens.Application.Utils;

public class HashingService : IHashingService 
{
    private readonly EncryptionOptions _options;
    public HashingService(
        IOptions<EncryptionOptions> options
    )
    {
        _options = options.Value;
    }
    public string AES_Encrypt(string text)
    {
        var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(_options.Key));

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(text);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes,0,plainBytes.Length);

        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        aes.IV.CopyTo(result,0);
        encryptedBytes.CopyTo(result, aes.IV.Length);

        return Convert.ToBase64String(result);
    }

    public string AES_Decrypt(string cipher)
    {
       var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(_options.Key));
       var fullBytes = Convert.FromBase64String(cipher);

       using var aes = Aes.Create();
       aes.Key = keyBytes;

       var iv = fullBytes[..16];
       var encryptedBytes = fullBytes[16..];
       aes.IV = iv;

       using var decryptor = aes.CreateDecryptor();
       var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

       return Encoding.UTF8.GetString(decryptedBytes);

    }

    public string SHA256_Hasher(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToBase64String(bytes);
    }
}