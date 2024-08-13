# Vérifier les droits administratifs
function Test-AdminRights {
    try {
        fsutil dirty query $env:SystemDrive > $null 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Vous avez des droits administratifs." -ForegroundColor Green
        } else {
            Write-Host "Vous n'avez pas les droits administratifs." -ForegroundColor Red
            Start-Sleep -Seconds 5
            exit
        }
    } catch {
        Write-Host "Vous n'avez pas les droits administratifs." -ForegroundColor Red
        Start-Sleep -Seconds 5
        exit
    }
}





Clear-Host
Test-AdminRights


# Détermine le type de système d'exploitation
$os = ""
if ($env:OS -eq "Windows_NT") {
    $os = "Windows"
} elseif ($env:OSTYPE -match "linux") {
    $os = "Linux"
} else {
    Write-Host "Système d'exploitation non pris en charge ou non détecté."
    exit 1
}

$dotnetInstalled = $false

# Fonction pour vérifier la connexion Internet
function Test-InternetConnection {
    try {
        $test = Invoke-WebRequest -Uri "http://www.google.com" -UseBasicParsing -ErrorAction Stop
        if ($test.StatusCode -eq 200) {
            Write-Host "Connexion Internet disponible."
            return $true
        }
    } catch {
        Write-Host "Pas de connexion Internet. Veuillez vérifier votre connexion."
        return $false
    }
}

# Fonction pour installer ou mettre à jour le .NET SDK
function Install-DotNetSDK {
    param (
        [string]$installerPath,
        [string]$url
    )

    Write-Host "Téléchargement et installation de .NET SDK pour $os..."
    Invoke-WebRequest -Uri $url -OutFile $installerPath
    if (Test-Path $installerPath) {
        Start-Process -FilePath $installerPath -ArgumentList "/install", "/quiet", "/norestart" -Wait
        Remove-Item $installerPath
        Write-Host ".NET SDK a été installé ou mis à jour."
    } else {
        Write-Host "Le téléchargement du fichier .NET SDK a échoué."
        exit 1
    }

    # Délai de 5 secondes après l'installation
    Start-Sleep -Seconds 5

    # Ajoute dotnet au PATH
    $dotnetPath = "C:\Program Files\dotnet\"
    if (-not ($env:PATH -like "*$dotnetPath*")) {
        [System.Environment]::SetEnvironmentVariable("PATH", $env:PATH + ";$dotnetPath", [System.EnvironmentVariableTarget]::Machine)
        Write-Host ".NET SDK a été ajouté au PATH."
        # Met à jour la session actuelle
        $env:PATH += ";$dotnetPath"
    }
}

# Vérifie la connexion Internet avant de procéder
if (-not (Test-InternetConnection)) {
    exit 1
}

# Vérifie si la commande dotnet est disponible
try {
    $dotnetVersion = & dotnet --version
    if ($dotnetVersion) {
        Write-Host ".NET SDK est déjà installé. Version: $dotnetVersion"
        $dotnetInstalled = $true
    }
} catch {
    Write-Host ".NET SDK n'est pas installé."
}

if (-not $dotnetInstalled) {
    if ($os -eq "Windows") {
        $installerPath = "$env:TEMP\dotnet-sdk-installer.exe"
        $url = "https://download.visualstudio.microsoft.com/download/pr/d1adccfa-62de-4306-9410-178eafb4eeeb/48e3746867707de33ef01036f6afc2c6/dotnet-sdk-8.0.303-win-x64.exe"
        Install-DotNetSDK -installerPath $installerPath -url $url
    } elseif ($os -eq "Linux") {
        $installerPath = "./dotnet-install.sh"
        $url = "https://dot.net/v1/dotnet-install.sh"
        Install-DotNetSDK -installerPath $installerPath -url $url
        chmod +x $installerPath
        ./$installerPath --channel 8.0
    }

    # Vérifie à nouveau si dotnet est installé après l'installation
    try {
        $dotnetVersion = & dotnet --version
        if ($dotnetVersion) {
            Write-Host ".NET SDK a été installé avec succès. Version: $dotnetVersion"
            $dotnetInstalled = $true
        }
    } catch {
        Write-Host "L'installation de .NET SDK a échoué ou la commande dotnet n'est toujours pas reconnue."
        exit 1
    }
}

# Compile le projet BookmarkManager.csproj en mode portable
$CurrentDirectory = Get-Location
$projectPath = "BookmarkManager.csproj"
cd "$($CurrentDirectory.Path)\Resources"

if (Test-Path $projectPath) {
    Write-Host "Compilation du projet BookmarkManager en mode portable..."
    
    if ($os -eq "Windows") {
        & dotnet publish $projectPath -c Release -r win-x64 --self-contained=true -o "$($CurrentDirectory.Path)/output/BookmarkManager-win-x64"
    } elseif ($os -eq "Linux") {
        & dotnet publish $projectPath -c Release -r linux-x64 --self-contained=true -o "$($CurrentDirectory.Path)/output/BookmarkManager-linux-x64"
    }
    
    Write-Host "Compilation terminée."
} else {
    Write-Host "Le fichier $projectPath n'existe pas dans le répertoire courant."
}

cd "$($CurrentDirectory.Path)"


Start-Sleep 15
Clear-Host

# Displaying instructions with colors

Write-Host "1. Retrieve Microsoft Edge bookmarks" -ForegroundColor Yellow
Write-Host "powershell" -ForegroundColor Cyan
Write-Host 'Copy the following code and run it in PowerShell:' -ForegroundColor Green
Write-Host 'BookmarkManager.exe --mode export --path "C:\Users\<YourUserName>\AppData\Local\Microsoft\Edge\User Data\Default" --export-file "C:\path\to\file\EdgeBookmarks.html" --browser "edge"' -ForegroundColor White

Write-Host ""
Write-Host "2. Export Microsoft Edge bookmarks to Google Chrome" -ForegroundColor Yellow
Write-Host "powershell" -ForegroundColor Cyan
Write-Host 'Copy the following code and run it in PowerShell:' -ForegroundColor Green
Write-Host 'BookmarkManager.exe --mode import --path "C:\Users\<YourUserName>\AppData\Local\Google\Chrome\User Data\Default" --import-file "C:\path\to\file\EdgeBookmarks.html" --browser "chrome"' -ForegroundColor White

Write-Host ""
Write-Host "3. Set https://googl.ch as the startup page of Microsoft Edge" -ForegroundColor Yellow
Write-Host "powershell" -ForegroundColor Cyan
Write-Host 'Copy the following code and run it in PowerShell:' -ForegroundColor Green
Write-Host 'BookmarkManager.exe --mode set-startup --path "C:\Users\<YourUserName>\AppData\Local\Microsoft\Edge\User Data\Default" --startup "https://googl.ch" --browser "edge"' -ForegroundColor White

# Additional examples after installation
Write-Host ""
Write-Host "Additional examples after installation:" -ForegroundColor Magenta
Write-Host '1. To verify that the bookmarks were correctly exported, open the "EdgeBookmarks.html" file in a browser.' -ForegroundColor White
Write-Host "2. To change the startup page of Google Chrome, use a similar command, replacing `"edge`" with `"chrome`"." -ForegroundColor White
Write-Host "3. You can also use BookmarkManager.exe to import bookmarks into Firefox by changing the --browser parameter." -ForegroundColor White

Write-Host "For more information on the browser paths, use this URL for Edge: 'edge://version/'" -ForegroundColor Yellow


