Commencer par le back (encodage puis décodage) et quand sa marche on fait le front

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

Faire que pour PNG