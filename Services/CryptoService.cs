using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SteganographyApp.Services;

public static class CryptoService
{
    private static readonly byte[] Salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76, 0x32, 0x30, 0x32 };

    // Chiffre un texte en utilisant l'algorithme AES et un mot de passe, puis retourne le résultat en Base64.
    public static string Encrypt(string plainText, string password)
    {
        using var aes = Aes.Create();
        
        // Génération de 48 octets en une seule passe, puis découpage : 32 pour la clé, 16 pour l'IV
        byte[] keyMaterial = Rfc2898DeriveBytes.Pbkdf2(password, Salt, 10000, HashAlgorithmName.SHA256, 48);
        aes.Key = keyMaterial[..32];
        aes.IV = keyMaterial[32..];

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            cs.Write(plainBytes, 0, plainBytes.Length);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    // Déchiffre une chaîne Base64 encodée en AES à l'aide du mot de passe fourni.
    public static string Decrypt(string base64CipherText, string password)
    {
        using var aes = Aes.Create();
        
        byte[] keyMaterial = Rfc2898DeriveBytes.Pbkdf2(password, Salt, 10000, HashAlgorithmName.SHA256, 48);
        aes.Key = keyMaterial[..32];
        aes.IV = keyMaterial[32..];

        using var ms = new MemoryStream(Convert.FromBase64String(base64CipherText));
        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var sr = new StreamReader(cs, Encoding.UTF8);

        return sr.ReadToEnd();
    }
}