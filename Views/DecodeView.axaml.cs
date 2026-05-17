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
                MessageTextBox.Text = string.Empty; // On vide la zone de message pour éviter la confusion
            } catch (Exception ex) {
                StatusText.Text = "Erreur de chargement : " + ex.Message;
            }
        }
    }

    private void OnDecodeClick(object? sender, RoutedEventArgs e)
    {
        if (_selectedImage == null) {
            StatusText.Text = "Veuillez d'abord sélectionner une image.";
            return;
        }
        MessageTextBox.Text = _decoder.Decode(_selectedImage);
        StatusText.Text = "Décodage terminé.";
    }
}