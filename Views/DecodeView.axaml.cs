using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using SteganographyApp.Services;

namespace SteganographyApp.Views;

public partial class DecodeView : UserControl
{
    private WriteableBitmap? _selectedImage;
    private readonly LSBDecoder _decoder = new();

    public DecodeView()
    {
        InitializeComponent();
        SelectImageButton.Click += OnSelectImageClick;
        DecodeButton.Click += OnDecodeClick;
    }

    // Ouvre un explorateur de fichiers pour sélectionner l'image à analyser.
    private async void OnSelectImageClick(object? sender, RoutedEventArgs e)
    {
        var fenetrePrincipale = TopLevel.GetTopLevel(this);
        if (fenetrePrincipale == null) return;

        var fichiersChoisis = await fenetrePrincipale.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Sélectionner une image",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
        });

        if (fichiersChoisis.Count > 0)
        {
            try {
                using var stream = await fichiersChoisis[0].OpenReadAsync();
                _selectedImage = WriteableBitmap.Decode(stream);
                
                StatusText.Text = "Image chargée avec succès. Prêt à décoder.";
                MessageTextBox.Text = ""; 
            } catch (Exception ex) {
                StatusText.Text = "Erreur de chargement : " + ex.Message;
            }
        }
    }

    // Extrait le message dissimulé dans l'image et le déchiffre si un mot de passe a été fourni.
    private void OnDecodeClick(object? sender, RoutedEventArgs e)
    {
        if (_selectedImage == null) {
            StatusText.Text = "Veuillez d'abord sélectionner une image.";
            return;
        }
        
        string texteExtrait = _decoder.Decode(_selectedImage);
        string motDePasse = PasswordTextBox.Text;
        
        bool estChiffre = texteExtrait.StartsWith("ENC:");
        
        if (estChiffre)
        {
            if (string.IsNullOrWhiteSpace(motDePasse))
            {
                StatusText.Text = "Erreur : Ce message est protégé par un mot de passe.";
                return;
            }
            
            try
            {
                string vraiMessageCrypte = texteExtrait.Substring(4);
                
                texteExtrait = CryptoService.Decrypt(vraiMessageCrypte, motDePasse);
            }
            catch
            {
                StatusText.Text = "Erreur : Mot de passe incorrect ou message corrompu.";
                return;
            }
        }

        MessageTextBox.Text = texteExtrait;
        StatusText.Text = "Décodage terminé avec succès.";
    }
}