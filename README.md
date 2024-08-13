
---
############
# English #
############

# BookmarkManager

BookmarkManager is a command-line interface (CLI) tool designed to import and export bookmarks between different web browsers such as Google Chrome and Microsoft Edge. It also allows setting the startup page of a browser.
To use the program, you will first need to compile the project. Run the script <span style="color: LightGreen;">**Auto_compile.ps1**</span> in PowerShell to do so. A folder named output will be created, containing the fully compiled version for your OS.

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
- `-p, --profile-path`: Specifies the path to the browser's profile folder where bookmarks are stored. **Required**.
- `-s, --startup-url`: Specifies the URL of the startup page to be set for the browser. **Optional**.
- `-e, --export-path`: Specifies the path and filename for exporting bookmarks. **Optional**.
- `-i, --import-path`: Specifies the path and filename of the JSON file to be imported. **Optional**.

### Exporting Bookmarks

To export bookmarks from a browser to a JSON file:

```bash
.\BookmarkManager.exe --mode export --profile-path "C:\Path\To\Browser\Profile" --export-path "C:\Path\To\Save\MyBookmarks.json"
```

**Example**: Export bookmarks from Microsoft Edge:

```bash
.\BookmarkManager.exe --mode export --profile-path "C:\Users\%USERNAME%\AppData\Local\Microsoft\Edge\User Data\Default" --export-path "C:\MyDocuments\EdgeBookmarks.json"
```

### Importing Bookmarks

To import bookmarks from a JSON file into a browser:

```bash
.\BookmarkManager.exe --mode import --profile-path "C:\Path\To\Browser\Profile" --import-path "C:\Path\To\MyBookmarks.json"
```

**Example**: Import bookmarks into Google Chrome:

```bash
.\BookmarkManager.exe --mode import --profile-path "C:\Users\%USERNAME%\AppData\Local\Google\Chrome\User Data\Default" --import-path "C:\MyDocuments\ChromeBookmarks.json"
```

### Setting Startup Page

To set the startup page of a browser:

```bash
.\BookmarkManager.exe --mode set-startup --profile-path "C:\Path\To\Browser\Profile" --startup-url "https://www.example.com"
```

**Example**: Set the startup page in Google Chrome:

```bash
.\BookmarkManager.exe --mode set-startup --profile-path "C:\Users\%USERNAME%\AppData\Local\Google\Chrome\User Data\Default" --startup-url "https://www.example.com"
```

---

############
# Français #
############

# BookmarkManager

BookmarkManager est un outil en ligne de commande (CLI) conçu pour importer et exporter des favoris entre différents navigateurs web tels que Google Chrome et Microsoft Edge. Il permet également de définir la page de démarrage d'un navigateur.
Pour utiliser le programme, vous devez d'abord compiler le projet. Exécutez le script <span style="color: LightGreen;">**Auto_compile.ps1**</span> dans PowerShell pour ce faire. Un dossier nommé output sera créé et contiendra la version entièrement compilée pour votre système d'exploitation.

## Fonctionnalités

- **Exporter** des favoris depuis un navigateur vers un fichier JSON.
- **Importer** des favoris depuis un fichier JSON dans un navigateur.
- **Définir** la page de démarrage d'un navigateur.

## Prérequis

- .NET Core ou .NET Framework installé sur votre système.
- Visual Studio ou tout autre environnement de développement pour compiler le projet.

## Installation

1. Clonez ce dépôt ou téléchargez les fichiers sources.
2. Ouvrez le projet dans Visual Studio.
3. Compilez le projet pour générer l'exécutable `BookmarkManager.exe`.

## Utilisation

### Options Disponibles

- `-m, --mode`: Spécifie le mode d'opération (`import` ou `export`). **Obligatoire**.
- `-p, --profile-path`: Spécifie le chemin vers le dossier de profil du navigateur où les favoris sont stockés. **Obligatoire**.
- `-s, --startup-url`: Spécifie l'URL de la page de démarrage à définir pour le navigateur. **Optionnel**.
- `-e, --export-path`: Spécifie le chemin et le nom du fichier pour exporter les favoris. **Optionnel**.
- `-i, --import-path`: Spécifie le chemin et le nom du fichier JSON à importer. **Optionnel**.

### Exporter des Favoris

Pour exporter des favoris depuis un navigateur vers un fichier JSON :

```bash
.\BookmarkManager.exe --mode export --profile-path "C:\Chemin\Vers\Profil\Navigateur" --export-path "C:\Chemin\Pour\Sauvegarder\MesFavoris.json"
```

**Exemple** : Exporter des favoris depuis Microsoft Edge :

```bash
.\BookmarkManager.exe --mode export --profile-path "C:\Users\%USERNAME%\AppData\Local\Microsoft\Edge\User Data\Default" --export-path "C:\MesDocuments\FavorisEdge.json"
```

### Importer des Favoris

Pour importer des favoris depuis un fichier JSON dans un navigateur :

```bash
.\BookmarkManager.exe --mode import --profile-path "C:\Chemin\Vers\Profil\Navigateur" --import-path "C:\Chemin\Pour\MesFavoris.json"
```

**Exemple** : Importer des favoris dans Google Chrome :

```bash
.\BookmarkManager.exe --mode import --profile-path "C:\Users\%USERNAME%\AppData\Local\Google\Chrome\User Data\Default" --import-path "C:\MesDocuments\FavorisChrome.json"
```

### Définir la Page de Démarrage

Pour définir la page de démarrage d'un navigateur :

```bash
.\BookmarkManager.exe --mode set-startup --profile-path "C:\Chemin\Vers\Profil\Navigateur" --startup-url "https://www.example.com"
```

**Exemple** : Définir la page de démarrage dans Google Chrome :

```bash
.\BookmarkManager.exe --mode set-startup --profile-path "C:\Users\%USERNAME%\AppData\Local\Google\Chrome\User Data\Default" --startup-url "https://www.example.com"
```
