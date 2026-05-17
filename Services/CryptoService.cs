using System;
using System.Security.Cryptography;
using System.Text;

namespace SteganographyApp.Services;

public static class CryptoService
{
    public static string Encrypt(string texteClair, string motDePasse)
    {
        using var moteurAes = Aes.Create();
        moteurAes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(motDePasse));

        byte[] TexteCUTF8  = Encoding.UTF8.GetBytes(texteClair);

        byte[] TexteChiffres = moteurAes.EncryptEcb(TexteCUTF8, PaddingMode.PKCS7); //(PaddingMode.PKCS7) sert a combler les derniers octets si le texte n'est pas un multiple de la taille de bloc (16 octets pour AES)

        return Convert.ToBase64String(TexteChiffres);
    }

    public static string Decrypt(string texteChiffreBase64, string motDePasse)
    {
        using var moteurAes = Aes.Create();
        moteurAes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(motDePasse));

        byte[] TexteChiffres = Convert.FromBase64String(texteChiffreBase64);
        
        byte[] TexteCUTF8 = moteurAes.DecryptEcb(TexteChiffres, PaddingMode.PKCS7);  //(PaddingMode.PKCS7) sert a combler les derniers octets si le texte n'est pas un multiple de la taille de bloc (16 octets pour AES

        return Encoding.UTF8.GetString(TexteCUTF8);
    }
}