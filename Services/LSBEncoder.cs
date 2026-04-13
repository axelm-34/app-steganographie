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

public Bitmap Encode(Bitmap image, string message) // fct principale
{
    Bitmap newImage = new Bitmap(image); // Clonage de l'image de base

    // Appel des fct précédente
    var bits = MessageToBits(message);
    bits = AddLengthHeader(bits);

    // Dimension de la nouvelle image
    int width = newImage.Width;
    int height = newImage.Height;

   
    int maxBits = width * height * 3;  // Capacité dispo de bits

    // Evite overflow
    if (bits.Count > maxBits)
        throw new Exception("Message trop long pour cette image");

    int bitIndex = 0;

    // Parcours complet de l'image
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            Color pixel = newImage.GetPixel(x, y); // recup couleur

            // sépare les canaux (couleur)
            int r = pixel.R;
            int g = pixel.G;
            int b = pixel.B;

            if (bitIndex < bits.Count)
                r = (r & ~1) | bits[bitIndex++];

            if (bitIndex < bits.Count)
                g = (g & ~1) | bits[bitIndex++];

            if (bitIndex < bits.Count)
                b = (b & ~1) | bits[bitIndex++];

            Color newPixel = Color.FromArgb(pixel.A, r, g, b); // recréation des pixels (On conserve Alpha)
            newImage.SetPixel(x, y, newPixel); // Remplacement des anciens pixels avec les nouveau recréé 

            // Quand on arrive au dernier pixel on arrête et on donne la nouvelle image
            if (bitIndex >= bits.Count)
                return newImage;
        }
    }

    return newImage;
}