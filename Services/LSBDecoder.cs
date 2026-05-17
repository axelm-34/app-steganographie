using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;

namespace SteganographyApp.Services;

public class LSBDecoder
{
    // Convertit une liste de bits en un nombre entier.
    private int BitsToInt(List<int> bits) {
        int value = 0;

        foreach (int bit in bits) 
        {
            value = (value << 1) | bit;
        }

        return value;
    }

    // Convertit une liste de bits en chaîne de caractères UTF-8.
    private string BitsToMessage(List<int> bits) {
        List<byte> bytes = new List<byte>();

        for (int i = 0; i < bits.Count; i += 8)
        {
            byte b = 0;

            for (int j = 0; j < 8; j++)
            {
                b = (byte)((b << 1) | bits[i + j]);
            }

            bytes.Add(b);
        }

        return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
    }

    // Extrait et décode le message caché dans les pixels de l'image sélectionnée.
    public string Decode(WriteableBitmap image) {
        List<int> bits = new List<int>();

        int width = image.PixelSize.Width;
        int height = image.PixelSize.Height;

        using (var buf = image.Lock())
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * buf.RowBytes + x * 4;
                    byte c1 = Marshal.ReadByte(buf.Address, offset);
                    byte c2 = Marshal.ReadByte(buf.Address, offset + 1);
                    byte c3 = Marshal.ReadByte(buf.Address, offset + 2);

                    bits.Add(c1 & 1);
                    bits.Add(c2 & 1);
                    bits.Add(c3 & 1);
                }
            }
        }

        int messageLength = BitsToInt(bits.Take(32).ToList());
        
        if (messageLength <= 0 || messageLength > bits.Count - 32)
            return "Erreur : Aucun message stéganographié trouvé ou image corrompue.";

        List<int> messageBits = bits.Skip(32).Take(messageLength).ToList();

        return BitsToMessage(messageBits);
    }
}