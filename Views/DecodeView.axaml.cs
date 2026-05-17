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

    // Initialise la vue de décodage et relie les événements des boutons.
    public DecodeView()
    {
        InitializeComponent();
        SelectImageButton.Click += OnSelectImageClick;
        DecodeButton.Click += OnDecodeClick;
    }

    // Ouvre un explorateur de fichiers pour sélectionner l'image à analyser.
    private async void OnSelectImageClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Sélectionner une image",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
        });

        if (files.Count > 0)
        {
            try {
                using var stream = await files[0].OpenReadAsync();
                _selectedImage = WriteableBitmap.Decode(stream);
                StatusText.Text = "Image chargée avec succès. Prêt à décoder.";
                MessageTextBox.Text = string.Empty;
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
        
        string decodedText = _decoder.Decode(_selectedImage);
        
        if (decodedText.StartsWith("ENC:"))
        {
            if (string.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                StatusText.Text = "Erreur : Ce message est protégé par un mot de passe.";
                return;
            }
            
            try
            {
                decodedText = CryptoService.Decrypt(decodedText.Substring(4), PasswordTextBox.Text);
            }
            catch
            {
                StatusText.Text = "Erreur : Mot de passe incorrect ou message corrompu.";
                return;
            }
        }

        MessageTextBox.Text = decodedText;
        StatusText.Text = "Décodage terminé avec succès.";
    }
}