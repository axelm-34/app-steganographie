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

    public EncodeView()
    {
        InitializeComponent();
        SelectImageButton.Click += OnSelectImageClick;
        EncodeButton.Click += OnEncodeClick;
    }

    // Ouvre un explorateur de fichiers pour sélectionner l'image source et calcule sa capacité d'encodage.
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
            using var stream = await fichiersChoisis[0].OpenReadAsync();
            _selectedImage = WriteableBitmap.Decode(stream);
            StatusText.Text = "Image source chargée.";
            
            int largeur = _selectedImage.PixelSize.Width;
            int hauteur = _selectedImage.PixelSize.Height;
            long totalPixels = largeur * hauteur;
            
            long totalBitsDisponibles = totalPixels * 3;
            long bitsSansEnTete = totalBitsDisponibles - 32;
            long capaciteMaxCaracteres = Math.Max(0, bitsSansEnTete / 8); 
            
            CapacityText.Text = $"Capacité maximale d'encodage : ~{capaciteMaxCaracteres} caractères ";
        }
    }

    // Chiffre le message, l'encode dans l'image sélectionnée puis sauvegarde le fichier résultant.
    private async void OnEncodeClick(object? sender, RoutedEventArgs e)
    {
        bool aucuneImage = _selectedImage == null;
        bool aucunMessage = string.IsNullOrWhiteSpace(MessageTextBox.Text);
        if (aucuneImage || aucunMessage) return;
        
        try 
        {
            string messageACacher = MessageTextBox.Text;
            string motDePasse = PasswordTextBox.Text;
            
            if (!string.IsNullOrWhiteSpace(motDePasse))
            {
                string messageChiffre = CryptoService.Encrypt(messageACacher, motDePasse);
                messageACacher = "ENC:" + messageChiffre;
            }

            var imageModifiee = _encoder.Encode(_selectedImage, messageACacher);

            var fenetrePrincipale = TopLevel.GetTopLevel(this);
            var fichierSauvegarde = await fenetrePrincipale!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Sauvegarder l'image encodée",
                DefaultExtension = "png",
                FileTypeChoices = new[] { FilePickerFileTypes.ImagePng }
            });

            if (fichierSauvegarde != null)
            {
                using var stream = await fichierSauvegarde.OpenWriteAsync();
                imageModifiee.Save(stream);
                StatusText.Text = "Image encodée et sauvegardée avec succès !";
            }
        }
        catch (Exception ex) 
        { 
            StatusText.Text = "Erreur : " + ex.Message; 
        }
    }
}