
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
.\BookmarkManager.exe --mode import --path "C:\Users\%USERNAME%\AppData\Local\Google\Chrome\User Data\Default" --import-file "C:\MyDocuments\ChromeBookmarks.json"
```

### Setting Startup Page

To set the startup page of a browser:

```bash
.\BookmarkManager.exe --mode <import|export> --path "C:\Path\To\Browser\Profile" --startup "https://www.example.com"
```

**Example**: Set the startup page in Google Chrome:

```bash
.\BookmarkManager.exe --mode export --path "C:\Users\%USERNAME%\AppData\Local\Google\Chrome\User Data\Default" --startup "https://www.example.com"
```

### Using Default Paths

If you do not specify the export or import path, the program will use default paths.

**Example**: Export bookmarks from Microsoft Edge using the default path for `ExportedBookmarks.json`:

```bash
.\BookmarkManager.exe --mode export --path "C:\Users\%USERNAME%\AppData\Local\Microsoft\Edge\User Data\Default"
```

**Example**: Import bookmarks into Google Chrome using the default `ExportedBookmarks.json` file:

```bash
.\BookmarkManager.exe --mode import --path "C:\Users\%USERNAME%\AppData\Local\Google\Chrome\User Data\Default"
```

## Notes

- Ensure that the specified path to the browser profile is correct and that the `Bookmarks` file exists.
- During import, the `Bookmarks.bak` file is also updated to avoid accidental restoration of old bookmarks.

## Troubleshooting

- If the program displays a message stating that the `Bookmarks` file does not exist, it may be due to the absence of bookmarks in the browser profile. Manually add a bookmark to create the file.
- Verify the browser profile paths by using `chrome://version/` for Chrome and `edge://version/` for Edge to confirm the exact path.

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/00MY00/Win_Book_Manager/blob/main/LICENSE) file for more details.

---

Pour connaître le chemin où se trouvent les favoris (Bookmarks) des navigateurs, utilisez les URLs suivantes : [EDG](https://edge://version/) pour Edge ou [CHROM](https://chrome://version/) pour Chrome.


