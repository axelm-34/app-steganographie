Projet en version : net10.0
  pour vérifier votre version : dotnet --version

Restaurer les dépendances :
  dotnet restore

Compiler le projet :
  dotnet build

Lancer l'application :
  dotnet run



Flux de fonctionnement
Encodage
UI (EncodeView)
 → ViewModel
   → LSBEncoder (Service)
     → retourne image modifiée
       → affichée + sauvegardée

Décodage
UI (DecodeView)
 → ViewModel
   → LSBDecoder
     → retourne message
       → affiché à l’écran