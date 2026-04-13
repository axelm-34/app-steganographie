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