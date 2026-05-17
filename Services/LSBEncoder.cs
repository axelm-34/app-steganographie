using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;

namespace SteganographyApp.Services;

public class LSBEncoder
{
    private List<int> MessageToBits(string message) // Transforme message en binaire
    {
        var bits = new List<int>(); // création de "List" pour bits

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message); // message -> tableau bytes

        foreach (byte b in bytes) // byte de chaque caractère (tableau byte) -> binaire
        {
            for (int i = 7; i >= 0; i--)
            {
                bits.Add((b >> i) & 1);
            }
        }

        return bits;
    }

    private List<int> AddLengthHeader(List<int> messageBits) // Permet de savoir la taille du message a décoder dés le debut de l'image (évite de lire toute l'image au décodage)
    {
        int length = messageBits.Count; // nb total de bits du message
        var result = new List<int>(); // recrée une List finale

        // encode sur 32 bits
        for (int i = 31; i >= 0; i--)
        {
            result.Add((length >> i) & 1); // extrait chaque bit de la longeur donné
        }

        result.AddRange(messageBits); // ajout du message
        return result;
    }

    public WriteableBitmap Encode(WriteableBitmap image, string message) // fct principale
    {
        // Création de la nouvelle image avec les paramètres d'origine
        var newImage = new WriteableBitmap(
            image.PixelSize,
            image.Dpi,
            image.Format ?? Avalonia.Platform.PixelFormat.Bgra8888,
            image.AlphaFormat ?? Avalonia.Platform.AlphaFormat.Unpremul);

        // Appel des fct précédente
        var bits = MessageToBits(message);
        bits = AddLengthHeader(bits);

        // Dimension de la nouvelle image
        int width = image.PixelSize.Width;
        int height = image.PixelSize.Height;

       
        int maxBits = width * height * 3;  // Capacité dispo de bits

        // Evite overflow
        if (bits.Count > maxBits)
            throw new Exception("Message trop long pour cette image");

        int bitIndex = 0;

        using (var oldBuf = image.Lock())
        using (var newBuf = newImage.Lock())
        {
            // Copie de tous les pixels depuis l'image source vers la nouvelle (Très rapide en mémoire)
            int size = oldBuf.RowBytes * height;
            byte[] temp = new byte[size];
            Marshal.Copy(oldBuf.Address, temp, 0, size);
            Marshal.Copy(temp, 0, newBuf.Address, size);

            // Parcours complet de l'image pour modifier les pixels
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (bitIndex >= bits.Count)
                        return newImage;

                    // Localisation du pixel en mémoire
                    int offset = y * newBuf.RowBytes + x * 4;
                    
                    // Récupération des trois premiers canaux de couleur de l'image (B, G, R)
                    byte c1 = Marshal.ReadByte(newBuf.Address, offset);
                    byte c2 = Marshal.ReadByte(newBuf.Address, offset + 1);
                    byte c3 = Marshal.ReadByte(newBuf.Address, offset + 2);

                    c1 = (byte)((c1 & ~1) | bits[bitIndex++]);

                    if (bitIndex < bits.Count)
                        c2 = (byte)((c2 & ~1) | bits[bitIndex++]);

                    if (bitIndex < bits.Count)
                        c3 = (byte)((c3 & ~1) | bits[bitIndex++]);

                    // Application des nouvelles couleurs au pixel
                    Marshal.WriteByte(newBuf.Address, offset, c1);
                    Marshal.WriteByte(newBuf.Address, offset + 1, c2);
                    Marshal.WriteByte(newBuf.Address, offset + 2, c3);
                }
            }
        }

        return newImage;
    }
}