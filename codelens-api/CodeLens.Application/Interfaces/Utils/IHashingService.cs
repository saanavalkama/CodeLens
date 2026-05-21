using System.Runtime.CompilerServices;

namespace CodeLens.Application.Interfaces.Utils;

public interface IHashingService
{
    string AES_Encrypt(string plaintext);
    string AES_Decrypt(string chipertext);
    string SHA256_Hasher(string token);
}