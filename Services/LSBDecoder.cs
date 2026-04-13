private int BitsToInt(List<int> bits) {// List -> nombre entiers

    int value = 0;

    // Lire chaque bit un par un
    foreach (int bit in bits) 
    {
        value = (value << 1) | bit;
    }

    return value;
}

private string BitsToMessage(List<int> bits) { // Bits -> Texte

    List<byte> bytes = new List<byte>(); // Stock charactères en byte

    for (int i = 0; i < bits.Count; i += 8) // Avance de 8 bit en 8 bit (1 caractère = 8 bits)
    {
        byte b = 0;

        for (int j = 0; j < 8; j++) // Lis les 8 bits du caractère
        {
            b = (byte)((b << 1) | bits[i + j]);
        }

        bytes.Add(b); // Ajout du caractère reconstruit
    }

    return System.Text.Encoding.UTF8.GetString(bytes.ToArray()); // Bytes (caractère reconstuit) -> texte lisible
}

public string Decode(Bitmap image) {
    List<int> bits = new List<int>();

    int width = image.Width;
    int height = image.Height;

    // Lire tous les bits de l'image
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            Color pixel = image.GetPixel(x, y);

            bits.Add(pixel.R & 1);
            bits.Add(pixel.G & 1);
            bits.Add(pixel.B & 1);
        }
    }

    // Lire les 32 premiers bits pour avoir la longueur du message
    int messageLength = BitsToInt(bits.Take(32).ToList());

    // Recup seulement le message
    List<int> messageBits = bits.Skip(32).Take(messageLength).ToList();

    // Convertir en texte
    return BitsToMessage(messageBits);
}