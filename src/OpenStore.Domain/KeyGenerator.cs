using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable once MemberCanBePrivate.Global

namespace OpenStore.Domain;

public static class KeyGenerator
{
    private static readonly HashAlgorithm Md5 = new MD5CryptoServiceProvider();

    public static Guid GenerateGuid() => Guid.NewGuid();

    public static string GenerateKey() => GenerateMd5Key(GenerateGuid().ToString());
        
    public static string GenerateKey(int maxLength) => GenerateMd5Key(Guid.NewGuid().ToString(), maxLength);
        
    public static string GenerateCombinedKey(IEnumerable<object> input) => GenerateMd5Key(string.Join("|", input.Where(x => x != null)));
    public static string GenerateCombinedKey(params object[] input) => GenerateMd5Key(string.Join("|", input.Where(x => x != null)));
        
    private static string GenerateMd5Key(string input, int? maxLength = null)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var textData = Encoding.UTF8.GetBytes(input);
        var hash = Md5.ComputeHash(textData);

        var hashed =  BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();

        return maxLength.HasValue ? hashed.Substring(0, maxLength.Value - 1) : hashed;
    }
}