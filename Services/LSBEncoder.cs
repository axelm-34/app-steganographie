using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;

namespace SteganographyApp.Services;

public class LSBEncoder
{
    // Convertit le message texte en une liste de bits (0 et 1).
    private List<int> MessageToBits(string message)
    {
        var bits = new List<int>();

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);

        foreach (byte b in bytes)
        {
            for (int i = 7; i >= 0; i--)
            {
                bits.Add((b >> i) & 1);
            }
        }

        return bits;
    }

    // Ajoute un en-tête de 32 bits au début de la liste pour stocker la taille du message.
    private List<int> AddLengthHeader(List<int> messageBits)
    {
        int length = messageBits.Count;
        var result = new List<int>();

        for (int i = 31; i >= 0; i--)
        {
            result.Add((length >> i) & 1);
        }

        result.AddRange(messageBits);
        return result;
    }

    // Modifie les bits de poids faible de l'image source pour y dissimuler le message.
    public WriteableBitmap Encode(WriteableBitmap image, string message)
    {
        var newImage = new WriteableBitmap(
            image.PixelSize,
            image.Dpi,
            image.Format ?? Avalonia.Platform.PixelFormat.Bgra8888,
            image.AlphaFormat ?? Avalonia.Platform.AlphaFormat.Unpremul);

        var bits = MessageToBits(message);
        bits = AddLengthHeader(bits);

        int width = image.PixelSize.Width;
        int height = image.PixelSize.Height;

       
        int maxBits = width * height * 3;

        if (bits.Count > maxBits)
            throw new Exception("Message trop long pour cette image");

        int bitIndex = 0;

        using (var oldBuf = image.Lock())
        using (var newBuf = newImage.Lock())
        {
            int size = oldBuf.RowBytes * height;
            byte[] temp = new byte[size];
            Marshal.Copy(oldBuf.Address, temp, 0, size);
            Marshal.Copy(temp, 0, newBuf.Address, size);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (bitIndex >= bits.Count)
                        return newImage;

                    int offset = y * newBuf.RowBytes + x * 4;
                    
                    byte c1 = Marshal.ReadByte(newBuf.Address, offset);
                    byte c2 = Marshal.ReadByte(newBuf.Address, offset + 1);
                    byte c3 = Marshal.ReadByte(newBuf.Address, offset + 2);

                    c1 = (byte)((c1 & ~1) | bits[bitIndex++]);

                    if (bitIndex < bits.Count)
                        c2 = (byte)((c2 & ~1) | bits[bitIndex++]);

                    if (bitIndex < bits.Count)
                        c3 = (byte)((c3 & ~1) | bits[bitIndex++]);

                    Marshal.WriteByte(newBuf.Address, offset, c1);
                    Marshal.WriteByte(newBuf.Address, offset + 1, c2);
                    Marshal.WriteByte(newBuf.Address, offset + 2, c3);
                }
            }
        }

        return newImage;
    }
}