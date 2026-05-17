using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using SteganographyApp.Services;

namespace SteganographyApp.Views;

public partial class EncodeView : UserControl
{
    private WriteableBitmap? _selectedImage;
    private readonly LSBEncoder _encoder = new();

    // Initialise la vue d'encodage et relie les événements des boutons.
    public EncodeView()
    {
        InitializeComponent();
        SelectImageButton.Click += OnSelectImageClick;
        EncodeButton.Click += OnEncodeClick;
    }

    // Ouvre un explorateur de fichiers pour sélectionner l'image source et calcule sa capacité d'encodage.
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
            using var stream = await files[0].OpenReadAsync();
            _selectedImage = WriteableBitmap.Decode(stream);
            StatusText.Text = "Image source chargée.";
            
            long totalPixels = _selectedImage.PixelSize.Width * _selectedImage.PixelSize.Height;
            long maxChars = Math.Max(0, ((totalPixels * 3) - 32) / 8); 
            CapacityText.Text = $"Capacité maximale d'encodage : ~{maxChars} caractères ";
        }
    }

    // Chiffre le message, l'encode dans l'image sélectionnée puis sauvegarde le fichier résultant.
    private async void OnEncodeClick(object? sender, RoutedEventArgs e)
    {
        if (_selectedImage == null || string.IsNullOrWhiteSpace(MessageTextBox.Text)) return;
        
        try 
        {
            string textToEncode = MessageTextBox.Text;
            
            if (!string.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                textToEncode = "ENC:" + CryptoService.Encrypt(textToEncode, PasswordTextBox.Text);
            }

            var newImage = _encoder.Encode(_selectedImage, textToEncode);

            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Sauvegarder l'image encodée",
                DefaultExtension = "png",
                FileTypeChoices = new[] { FilePickerFileTypes.ImagePng }
            });

            if (file == null) return;
            using var stream = await file.OpenWriteAsync();
            newImage.Save(stream);
            StatusText.Text = "Image encodée et sauvegardée avec succès !";
        }
        catch (Exception ex) { StatusText.Text = "Erreur : " + ex.Message; }
    }
}