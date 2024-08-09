---
############
# English #
############

# BookmarkManager

BookmarkManager is a command-line interface (CLI) tool designed to import and export bookmarks between different web browsers such as Google Chrome and Microsoft Edge. It also allows setting the startup page of a browser.
Don't forget to unzip the compressed "BookMark Manager" archive containing the application.


## Features

- **Export** bookmarks from a browser to a JSON file.
- **Import** bookmarks from a JSON file into a browser.
- **Set** the startup page of a browser.

## Prerequisites

- .NET Core or .NET Framework installed on your system.
- Visual Studio or any other development environment to compile the project.

## Installation

1. Clone this repository or download the source files.
2. Open the project in Visual Studio.
3. Compile the project to generate the `BookmarkManager.exe` executable.

## Usage

### Available Options

- `-m, --mode`: Specifies the operation mode (`import` or `export`). **Required**.
- `-p, --path`: Specifies the path to the browser's profile folder where bookmarks are stored. **Required**.
- `-s, --startup`: Specifies the URL of the startup page to be set for the browser. **Optional**.
- `-e, --export-file`: Specifies the path and filename for exporting bookmarks. **Optional**.
- `-i, --import-file`: Specifies the path and filename of the JSON file to be imported. **Optional**.

### Exporting Bookmarks

To export bookmarks from a browser to a JSON file:

```bash
.\BookmarkManager.exe --mode export --path "C:\Path\To\Browser\Profile" --export-file "C:\Path\To\Save\MyBookmarks.json"
```

**Example**: Export bookmarks from Microsoft Edge:

```bash
.\BookmarkManager.exe --mode export --path "C:\Users\%USERNAME%\AppData\Local\Microsoft\Edge\User Data\Default" --export-file "C:\MyDocuments\EdgeBookmarks.json"
```

### Importing Bookmarks

To import bookmarks from a JSON file into a browser:

```bash
.\BookmarkManager.exe --mode import --path "C:\Path\To\Browser\Profile" --import-file "C:\Path\To\MyBookmarks.json"
```

**Example**: Import bookmarks into Google Chrome:

```bash
.\BookmarkManager.exe --mode import --path "C:\Users\%USERNAME%\AppData\Local\Google\Chrome\User Data\Default" --import-file "C:\MyDocuments\EdgeBookmarks.json"
```

### Setting the Startup Page

To set a startup page in a browser:

```bash
.\BookmarkManager.exe --mode export --path "C:\Path\To\Browser\Profile" --startup "https://www.example.com"
```

**Example**: Set the startup page in Google Chrome:

```bash
.\BookmarkManager.exe --mode export --path "C:\Users\%USERNAME%\AppData\Local\Google\Chrome\User Data\Default" --startup "https://www.example.com"
```

### Using Default Paths

If you do not specify the export or import path, the program will use the default paths.

**Example**: Export bookmarks from Microsoft Edge using the default path for `ExportedBookmarks.json`:

```bash
.\BookmarkManager.exe --mode export --path "C:\Users\%USERNAME%\AppData\Local\Microsoft\Edge\User Data\Default"
```

**Example**: Import bookmarks into Google Chrome using the default `ExportedBookmarks.json` file:

```bash
.\BookmarkManager.exe --mode import --path "C:\Users\%USERNAME%\AppData\Local\Google\Chrome\User Data\Default"
```

## Notes

- Ensure that the specified path for the browser's profile is correct and that the `Bookmarks` file exists.
- During import, the `Bookmarks.bak` file is also updated to prevent accidental restoration of old bookmarks.

## Troubleshooting

- If the program displays a message that the `Bookmarks` file does not exist, it may be because there are no bookmarks in the browser profile. Manually add a bookmark to create the file.
- Check the browser profile paths using `chrome://version/` for Chrome and `edge://version/` for Edge to confirm the exact path.

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/00MY00/Win_Book_Manager/blob/main/LICENSE) file for more details.

---

To find the path where the browser's bookmarks are located, use the following URLs: [EDG](https://edge://version/) for Edge or [CHROM](https://chrome://version/) for Chrome.


---
---

############
# Francais #
############

# BookmarkManager

BookmarkManager est un outil en ligne de commande (CLI) conçu pour importer et exporter les favoris (bookmarks) entre différents navigateurs web tels que Google Chrome et Microsoft Edge. Il permet également de définir la page de démarrage d'un navigateur.
N'oubliez pas de décompresser l'archive "BookMark Manager" compressée contenant l'application.


## Fonctionnalités

- **Exporter** les favoris d'un navigateur vers un fichier JSON.
- **Importer** des favoris à partir d'un fichier JSON dans un navigateur.
- **Définir** la page de démarrage d'un navigateur.

## Prérequis

- .NET Core ou .NET Framework installé sur votre système.
- Visual Studio ou tout autre environnement de développement pour compiler le projet.

## Installation

1. Clonez ce dépôt ou téléchargez les fichiers sources.
2. Ouvrez le projet dans Visual Studio.
3. Compilez le projet pour générer l'exécutable BookmarkManager.exe.

## Utilisation

### Options Disponibles

- -m, --mode : Spécifie le mode de fonctionnement (import ou export). **Requis**.
- -p, --path : Spécifie le chemin vers le dossier du profil du navigateur où les favoris sont stockés. **Requis**.
- -s, --startup : Spécifie l'URL de la page de démarrage à définir pour le navigateur. **Optionnel**.
- -e, --export-file : Spécifie le chemin et le nom du fichier pour l'exportation des favoris. **Optionnel**.
- -i, --import-file : Spécifie le chemin et le nom du fichier JSON à importer. **Optionnel**.

### Exporter les Favoris

Pour exporter les favoris d'un navigateur vers un fichier JSON :

```bash
.\BookmarkManager.exe --mode export --path "C:\Chemin\Vers\Le\Profil\Du\Navigateur" --export-file "C:\Chemin\Vers\Exporter\MesFavoris.json"
```

**Exemple** : Exporter les favoris de Microsoft Edge :

```bash
.\BookmarkManager.exe --mode export --path "C:\Users\%USERNAME%\AppData\Local\Microsoft\Edge\User Data\Default" --export-file "C:\MesDocuments\EdgeBookmarks.json"
```

### Importer les Favoris

Pour importer les favoris depuis un fichier JSON dans un navigateur :

```bash
.\BookmarkManager.exe --mode import --path "C:\Chemin\Vers\Le\Profil\Du\Navigateur" --import-file "C:\Chemin\Vers\LeFichier\MesFavoris.json"
```

**Exemple** : Importer les favoris dans Google Chrome :

```bash
.\BookmarkManager.exe --mode import --path "C:\Users\%USERNAME%\AppData\Local\Google\Chrome\User Data\Default" --import-file "C:\MesDocuments\EdgeBookmarks.json"
```

### Définir la Page de Démarrage

Pour définir une page de démarrage dans un navigateur :

```bash
.\BookmarkManager.exe --mode export --path "C:\Chemin\Vers\Le\Profil\Du\Navigateur" --startup "https://www.example.com"
```

**Exemple** : Définir la page de démarrage dans Google Chrome :

```bash
.\BookmarkManager.exe --mode export --path "C:\Users\%USERNAME%\AppData\Local\Google\Chrome\User Data\Default" --startup "https://www.example.com"
```

### Utilisation avec les Chemins par Défaut

Si vous ne spécifiez pas le chemin d'exportation ou d'importation, le programme utilisera les chemins par défaut.

**Exemple** : Exporter les favoris de Microsoft Edge en utilisant le chemin par défaut pour ExportedBookmarks.json :

```bash
.\BookmarkManager.exe --mode export --path "C:\Users\%USERNAME%\AppData\Local\Microsoft\Edge\User Data\Default"
```

**Exemple** : Importer les favoris dans Google Chrome en utilisant le fichier ExportedBookmarks.json par défaut :

```bash
.\BookmarkManager.exe --mode import --path "C:\Users\%USERNAME%\AppData\Local\Google\Chrome\User Data\Default"
```

## Notes

- Assurez-vous que le chemin spécifié pour le profil du navigateur est correct et que le fichier Bookmarks existe.
- Lors de l'importation, le fichier Bookmarks.bak est également mis à jour pour éviter toute restauration accidentelle d'anciens favoris.

## Dépannage

- Si le programme affiche un message indiquant que le fichier Bookmarks n'existe pas, cela peut être dû à l'absence de favoris dans le profil du navigateur. Ajoutez un favori manuellement pour créer le fichier.
- Vérifiez les chemins de profil des navigateurs en utilisant chrome://version/ pour Chrome et edge://version/ pour Edge pour confirmer le chemin exact.

## Licence

Ce projet est sous licence MIT. Voir le fichier [LICENSE](https://github.com/00MY00/Win_Book_Manager/blob/main/LICENSE) pour plus de détails.

---

Pour connaître le chemin où se trouvent les favoris (Bookmarks) des navigateurs, utilisez les URLs suivantes : [EDG](https://edge://version/) pour Edge ou [CHROM](https://chrome://version/) pour Chrome.

