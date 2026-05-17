# APP STEGANOGRAPHIE

**Ceci est une application simple permettant d'encoder un message dans une image ainsi que de decoder un message présent dans une image**

## Voici comment lancer l'application :

- Voir si dotnet est installer : **dotnet --version**

> Si vous n'avez pas dotnet 10.0 :
- Installer dotnet version 10.0 (net10.0)

- Restorer les dépendances : **dotnet restore**

- Lancer l'application : **dotnet run**

### Voici comment installer dotnet sur Debian/Linux

- wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh

- chmod +x dotnet-install.sh

- ./dotnet-install.sh --channel 10.0

- echo 'export PATH="$PATH:$HOME/.dotnet"' >> ~/.bashrc

- source ~/.bashrc

Verifier avec : dotnet --version