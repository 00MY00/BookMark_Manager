# Check for administrative rights
function Test-AdminRights {
    try {
        fsutil dirty query $env:SystemDrive > $null 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "You have administrative rights." -ForegroundColor Green
        } else {
            Write-Host "You do not have administrative rights." -ForegroundColor Red
            Start-Sleep -Seconds 5
            Read-Host "Press Enter to continue. Some errors may occur!"
        }
    } catch {
        Write-Host "You do not have administrative rights." -ForegroundColor Red
        Start-Sleep -Seconds 5
        exit
    }
}

#Clear-Host
Test-AdminRights
$CurrentDirectory = Get-Location
$projectPath = "BookmarkManager.csproj"
cd "$($CurrentDirectory.Path)\Resources"

# Determine the operating system type
$os = ""
if ($env:OS -eq "Windows_NT") {
    $os = "Windows"
} elseif ($env:OSTYPE -match "linux") {
    $os = "Linux"
} else {
    Write-Host "Operating system not supported or not detected."
    exit 1
}

$dotnetInstalled = $false

# Function to check for internet connection
function Test-InternetConnection {
    try {
        $test = Invoke-WebRequest -Uri "http://www.google.com" -UseBasicParsing -ErrorAction Stop
        if ($test.StatusCode -eq 200) {
            Write-Host "Internet connection is available."
            return $true
        }
    } catch {
        Write-Host "No internet connection. Please check your connection."
        return $false
    }
}

# Function to install or update the .NET SDK
function Install-DotNetSDK {
    param (
        [string]$installerPath,
        [string]$url
    )

    Write-Host "Downloading and installing .NET SDK for $os..."
    Invoke-WebRequest -Uri $url -OutFile $installerPath
    if (Test-Path $installerPath) {
        Start-Process -FilePath $installerPath -ArgumentList "/install", "/quiet", "/norestart" -Wait
        Remove-Item $installerPath
        Write-Host ".NET SDK has been installed or updated."
    } else {
        Write-Host "Failed to download the .NET SDK file."
        exit 1
    }

    # Delay of 5 seconds after installation
    Start-Sleep -Seconds 5

    # Add dotnet to PATH
    $dotnetPath = "C:\Program Files\dotnet\"
    if (-not ($env:PATH -like "*$dotnetPath*")) {
        [System.Environment]::SetEnvironmentVariable("PATH", $env:PATH + ";$dotnetPath", [System.EnvironmentVariableTarget]::Machine)
        Write-Host ".NET SDK has been added to PATH."
        # Update the current session
        $env:PATH += ";$dotnetPath"
    }
}

# Check for internet connection before proceeding
if (-not (Test-InternetConnection)) {
    exit 1
}

# Check if the dotnet command is available
try {
    $dotnetVersion = & dotnet --version
    if ($dotnetVersion) {
        Write-Host ".NET SDK is already installed. Version: $dotnetVersion"
        $dotnetInstalled = $true

        # Ensure the correct NuGet source is configured
        dotnet clean
        dotnet build
        Write-Host "Configuring NuGet source..."
        dotnet nuget add source https://api.nuget.org/v3/index.json --name nuget.org
        

    }
} catch {
    Write-Host ".NET SDK is not installed."
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

    # Check again if dotnet is installed after the installation
    try {
        $dotnetVersion = & dotnet --version
        if ($dotnetVersion) {
            Write-Host ".NET SDK has been successfully installed. Version: $dotnetVersion"
            $dotnetInstalled = $true
        }
    } catch {
        Write-Host "Failed to install .NET SDK or the dotnet command is still not recognized."
        exit 1
    }
}

# Compile the BookmarkManager.csproj project in portable mode


if (Test-Path $projectPath) {
    Write-Host "Compiling the BookmarkManager project in portable mode..."
    
    if ($os -eq "Windows") {
        & dotnet publish $projectPath -c Release -r win-x64 --self-contained=true /p:PublishSingleFile=true -o "$($CurrentDirectory.Path)/output/BookmarkManager-win-x64"
    } elseif ($os -eq "Linux") {
        & dotnet publish $projectPath -c Release -r linux-x64 --self-contained=true /p:PublishSingleFile=true -o "$($CurrentDirectory.Path)/output/BookmarkManager-linux-x64"
    }
    
    Write-Host "Compilation completed."
    dotnet list package

} else {
    Write-Host "The file $projectPath does not exist in the current directory."
}

cd "$($CurrentDirectory.Path)"



Start-Sleep 15
#Clear-Host

# Displaying instructions with colors

Write-Host "1. Retrieve Microsoft Edge bookmarks" -ForegroundColor Yellow
Write-Host "powershell" -ForegroundColor Cyan
Write-Host 'Copy the following code and run it in PowerShell:' -ForegroundColor Green
Write-Host "BookmarkManager.exe --mode export --path `"C:\Users\$($env:USERNAME)\AppData\Local\Microsoft\Edge\User Data\Default`" --export-file `"C:\path\to\file\EdgeBookmarks.html`" --browser `"edge`"" -ForegroundColor White

Write-Host ""
Write-Host "2. Export Microsoft Edge bookmarks to Google Chrome" -ForegroundColor Yellow
Write-Host "powershell" -ForegroundColor Cyan
Write-Host 'Copy the following code and run it in PowerShell:' -ForegroundColor Green
Write-Host "BookmarkManager.exe --mode import --path `"C:\Users\$($env:USERNAME)\AppData\Local\Google\Chrome\User Data\Default`" --import-file `"C:\path\to\file\EdgeBookmarks.html`" --browser `"chrome`"" -ForegroundColor White

Write-Host ""
Write-Host "3. Set https://googl.ch as the startup page of Microsoft Edge" -ForegroundColor Yellow
Write-Host "powershell" -ForegroundColor Cyan
Write-Host 'Copy the following code and run it in PowerShell:' -ForegroundColor Green
Write-Host "BookmarkManager.exe --mode set-startup --path `"C:\Users\$($env:USERNAME)\AppData\Local\Microsoft\Edge\User Data\Default`" --startup `"https://googl.ch`" --browser `"edge`"" -ForegroundColor White

# Additional examples after installation
Write-Host ""
Write-Host "Additional examples after installation:" -ForegroundColor Magenta
Write-Host '1. To verify that the bookmarks were correctly exported, open the "EdgeBookmarks.html" file in a browser.' -ForegroundColor White
Write-Host "2. To change the startup page of Google Chrome, use a similar command, replacing `"edge`" with `"chrome`"." -ForegroundColor White
Write-Host "3. You can also use BookmarkManager.exe to import bookmarks into Firefox by changing the --browser parameter." -ForegroundColor White

Write-Host "For more information on the browser paths, use this URL for Edge: 'edge://version/'" -ForegroundColor Yellow
# Demande à l'utilisateur s'il souhaite ajouter le programme au PATH
Write-Host "Would you like to add the program to the PATH?" -ForegroundColor Cyan
$PathAdd = Read-Host "Y/n   -> "

if (-Not ($PathAdd.ToLower() -eq "n")) {
    # Définir le chemin source et destination
    $CurrentDirectory = Get-Location
    $sourcePath = "$($CurrentDirectory.Path)\output\BookmarkManager-win-x64"
    $destinationPath = "C:\Users\$env:USERNAME\BookmarkManager-win-x64"

    # Copier le dossier dans le répertoire utilisateur
    Write-Host "Copying the program to $destinationPath..." -ForegroundColor Cyan
    Copy-Item -Path $sourcePath -Destination $destinationPath -Recurse -Force 2>$null

    # Ajouter le chemin au PATH global (de l'utilisateur)
    
        [System.Environment]::SetEnvironmentVariable("PATH", $env:PATH + ";$destinationPath\BookmarkManager.exe", [System.EnvironmentVariableTarget]::User)
        Write-Host "The path $destinationPath has been added to the user PATH." -ForegroundColor Green
        # Mettre à jour la session actuelle pour que le changement soit immédiat
        $env:PATH += ";$destinationPath"
    
} else {
    Write-Host "The program was not added to the PATH." -ForegroundColor Red
}

Read-Host "Press Enter to continue"