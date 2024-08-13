# Détermine le type de système d'exploitation
cls
$os = ""
if ($env:OS -eq "Windows_NT") {
    $os = "Windows"
} elseif ($env:OSTYPE -match "linux") {
    $os = "Linux"
} else {
    # Teste si uname est disponible pour détecter Linux si $env:OSTYPE n'est pas défini
    try {
        $uname = uname
        if ($uname -match "Linux") {
            $os = "Linux"
        }
    } catch {
        Write-Host "Système d'exploitation non pris en charge ou non détecté."
        exit 1
    }
}

$dotnetInstalled = $false

# Fonction pour installer ou mettre à jour le .NET SDK
function Install-DotNetSDK {
    param (
        [string]$os,
        [string]$url,
        [string]$installerPath
    )
    if ($os -eq "Windows") {
        Write-Host "Téléchargement et installation de .NET SDK-8 pour Windows..."
        Invoke-WebRequest -Uri $url -OutFile $installerPath
        Start-Process -FilePath $installerPath -ArgumentList "/install", "/quiet", "/norestart" -Wait
        Remove-Item $installerPath
        Write-Host ".NET SDK a été installé ou mis à jour."
    } elseif ($os -eq "Linux") {
        Write-Host "Téléchargement et installation de .NET SDK pour Linux..."
        wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
        chmod +x ./dotnet-install.sh
        ./dotnet-install.sh --channel 8.0
        Write-Host ".NET SDK a été installé ou mis à jour."
    } else {
        Write-Host "Système d'exploitation non pris en charge ou non détecté."
        exit 1
    }
}

if ($os -eq "Windows") {
    Write-Host "Système d'exploitation : Windows"
    try {
        $dotnetVersion = dotnet --version
        if ($dotnetVersion) {
            Write-Host ".NET SDK est déjà installé. Version: $dotnetVersion"
            $dotnetInstalled = $true
        }
    } catch {
        Write-Host ".NET SDK n'est pas installé."
    }

    if ($dotnetInstalled -and ([version]$dotnetVersion -lt [version]"8.0")) {
        Write-Host ".NET SDK est installé mais la version est inférieure à 8.0. Mise à jour nécessaire."
        Install-DotNetSDK -os $os -url "https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-8.0.303-windows-x64-installer" -installerPath "$env:TEMP\dotnet-sdk-installer.exe"
    } elseif (-not $dotnetInstalled) {
        Install-DotNetSDK -os $os -url "https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-8.0.303-windows-x64-installer" -installerPath "$env:TEMP\dotnet-sdk-installer.exe"
    }
} elseif ($os -eq "Linux") {
    Write-Host "Système d'exploitation : Linux"
    try {
        $dotnetVersion = dotnet --version
        if ($dotnetVersion) {
            Write-Host ".NET SDK est déjà installé. Version: $dotnetVersion"
            $dotnetInstalled = $true
        }
    } catch {
        Write-Host ".NET SDK n'est pas installé."
    }

    if ($dotnetInstalled -and ([version]$dotnetVersion -lt [version]"8.0")) {
        Write-Host ".NET SDK est installé mais la version est inférieure à 8.0. Mise à jour nécessaire."
        Install-DotNetSDK -os $os
    } elseif (-not $dotnetInstalled) {
        Install-DotNetSDK -os $os
    }
} else {
    Write-Host "Système d'exploitation non pris en charge ou non détecté."
    exit 1
}

# Compile le projet BookmarkManager.csproj en mode portable
$CurrentDirectory = Get-Location
$projectPath = "BookmarkManager.csproj"
cd "$($CurrentDirectory.Path)\Resources"

if (Test-Path $projectPath) {
    Write-Host "Compilation du projet BookmarkManager en mode portable..."
    
    if ($os -eq "Windows") {
        dotnet publish $projectPath -c Release -r win-x64 --self-contained=true -o "$($CurrentDirectory.Path)/output/BookmarkManager-win-x64"
    } elseif ($os -eq "Linux") {
        dotnet publish $projectPath -c Release -r linux-x64 --self-contained=true -o "$($CurrentDirectory.Path)/output/BookmarkManager-linux-x64"
    }
    
    Write-Host "Compilation terminée."
} else {
    Write-Host "Le fichier $projectPath n'existe pas dans le répertoire courant."
}


cd "$($CurrentDirectory.Path)"