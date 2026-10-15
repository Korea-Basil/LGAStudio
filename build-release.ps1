$ErrorActionPreference = 'Stop'

$projectRoot = $PSScriptRoot
$projectFile = Join-Path $projectRoot 'LGAStudio.csproj'
$releaseDirectory = Join-Path $projectRoot 'release'
$executablePath = Join-Path $releaseDirectory 'LGAStudio.exe'

foreach ($directory in @(
    (Join-Path $projectRoot 'bin'),
    (Join-Path $projectRoot 'obj'),
    $releaseDirectory
)) {
    if (Test-Path $directory) {
        Remove-Item $directory -Recurse -Force
    }
}

try {
    & dotnet publish $projectFile -c Release -r win-x64 --self-contained true `
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:IncludeAllContentForSelfExtract=true -o $releaseDirectory

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE."
    }

    $releaseEntries = @(Get-ChildItem $releaseDirectory -Force)
    if ($releaseEntries.Count -ne 1 -or $releaseEntries[0].PSIsContainer -or
        $releaseEntries[0].FullName -ne $executablePath) {
        $entryList = ($releaseEntries | ForEach-Object { $_.Name }) -join ', '
        throw "Expected only release\LGAStudio.exe, but found: $entryList"
    }

    Write-Output "Release executable: $executablePath"
}
finally {
    foreach ($directory in @(
        (Join-Path $projectRoot 'bin'),
        (Join-Path $projectRoot 'obj')
    )) {
        if (Test-Path $directory) {
            Remove-Item $directory -Recurse -Force
        }
    }
}
