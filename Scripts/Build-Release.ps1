<#
.SYNOPSIS
    Builds the Luna Multiplayer release zip artifacts locally - the four
    stable-release zips plus a self-contained linux-x64 Server zip per
    configuration.

.DESCRIPTION
    Produces the stable LMP release zips plus a self-contained linux-x64
    Server zip per configuration:
        LunaMultiplayer-Client-Debug.zip
        LunaMultiplayer-Client-Release.zip
        LunaMultiplayer-Server-Debug.zip                 (framework-dependent, "any" RID)
        LunaMultiplayer-Server-Release.zip               (framework-dependent, "any" RID)
        LunaMultiplayer-Server-linux-x64-Debug.zip       (self-contained, trimmed)
        LunaMultiplayer-Server-linux-x64-Release.zip     (self-contained, trimmed)

    The portable "any" Server zip is small (~2 MB) and requires the .NET 6
    runtime on the host (Windows or Linux). The linux-x64 Server zip bundles
    the runtime so a Linux host needs no .NET install - matching the
    linux-x64 artifact shipped by AppVeyor's nightly pipeline.

    AppVeyor's nightly pipeline (appveyor.yml) produces an even wider set
    (additional per-RID Server self-contained builds + a MasterServer zip);
    this script narrows to the stable-release shape plus the linux-x64
    self-contained server so a developer can assemble a stable release
    locally without sifting through nightly artifacts.

.PARAMETER Configuration
    Build configuration to produce. 'All' (default) builds both Debug and
    Release - i.e. all four zips. Pass 'Debug' or 'Release' to build only that
    config's pair (one Client zip + one Server zip).

.PARAMETER OutputDir
    Destination directory for the final zip artifacts. Defaults to
    <repo>\release_files (gitignored).

.PARAMETER NoClean
    Skip wiping the FinalFiles staging directory and the legacy-project obj/
    folders before building.

.PARAMETER SkipClient
    Skip the LmpClient build/stage/zip steps. (Server zips will still build.)

.PARAMETER SkipServer
    Skip the Server publish/zip step. (Client zips will still build.) Also
    skips the linux-x64 self-contained Server build.

.PARAMETER SkipLinuxServer
    Skip only the linux-x64 self-contained Server publish/zip step. The
    portable ("any" RID) Server zip is still produced.

.EXAMPLE
    .\Scripts\Build-Release.ps1
    Produces all six zips (Client+Server portable+Server linux-x64, Debug+Release)
    into .\release_files\.

.EXAMPLE
    .\Scripts\Build-Release.ps1 -Configuration Release
    Produces only the three Release zips (Client + Server portable + Server linux-x64).

.EXAMPLE
    .\Scripts\Build-Release.ps1 -SkipLinuxServer
    Produces only the four "stable release" zips, skipping the self-contained
    linux-x64 Server builds.
#>

[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release', 'All')]
    [string]$Configuration = 'All',

    [string]$OutputDir,

    [switch]$NoClean,
    [switch]$SkipClient,
    [switch]$SkipServer,
    [switch]$SkipLinuxServer
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Repo root is the parent of the Scripts directory this script lives in.
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if (-not $OutputDir) { $OutputDir = Join-Path $RepoRoot 'release_files' }

function Write-Section {
    param([string]$Title)
    Write-Host ''
    Write-Host ('=' * 60) -ForegroundColor DarkCyan
    Write-Host (' ' + $Title) -ForegroundColor Cyan
    Write-Host ('=' * 60) -ForegroundColor DarkCyan
}

function Invoke-External {
    param(
        [Parameter(Mandatory)] [string]$Exe,
        [Parameter(ValueFromRemainingArguments)] $Arguments
    )
    & $Exe @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "External tool '$Exe' exited with code $LASTEXITCODE"
    }
}

function Find-MSBuild {
    $candidates = @(
        "${env:ProgramFiles}\Microsoft Visual Studio\Installer\vswhere.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    )
    foreach ($vswhere in $candidates) {
        if (Test-Path $vswhere) {
            $path = & $vswhere -latest -products * `
                -requires Microsoft.Component.MSBuild `
                -find 'MSBuild\**\Bin\MSBuild.exe' |
                Select-Object -First 1
            if ($path -and (Test-Path $path)) { return $path }
        }
    }
    $cmd = Get-Command msbuild.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    throw "MSBuild not found. Install Visual Studio 2022 (or Build Tools) with the .NET desktop workload."
}

function Find-SevenZip {
    $candidates = @(
        "${env:ProgramFiles}\7-Zip\7z.exe",
        "${env:ProgramFiles(x86)}\7-Zip\7z.exe"
    )
    foreach ($p in $candidates) { if (Test-Path $p) { return $p } }
    $cmd = Get-Command 7z.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    throw "7-Zip (7z.exe) not found. Install from https://www.7-zip.org/ or add it to PATH."
}

function Find-NuGet {
    param([Parameter(Mandatory)] [string]$RepoRoot)
    # Prefer a repo-local nuget.exe so the script is self-contained and never
    # interferes with whatever NuGet CLI a developer might have on PATH.
    $local = Join-Path $RepoRoot 'External\Nuget\nuget.exe'
    if (Test-Path $local) { return $local }

    $cmd = Get-Command nuget.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    # Auto-download the official latest CLI. Small (~8 MB), single-file exe.
    Write-Host "nuget.exe not found - downloading latest CLI to $local"
    $dir = Split-Path $local -Parent
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    $url = 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe'
    try {
        # TLS 1.2 is required by nuget.org on older PowerShell hosts.
        [Net.ServicePointManager]::SecurityProtocol =
            [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
        Invoke-WebRequest -Uri $url -OutFile $local -UseBasicParsing
    } catch {
        throw "Failed to download nuget.exe from $url. Install it manually to $local. ($_)"
    }
    return $local
}

function New-StagingDirs {
    param([string]$ClientStage)
    $subs = @('Button', 'Plugins', 'Localization', 'PartSync', 'Icons', 'Flags')
    foreach ($sub in $subs) {
        New-Item -ItemType Directory -Force -Path (Join-Path $ClientStage $sub) | Out-Null
    }
}

# --- Resolve tools / preconditions -------------------------------------------------

Push-Location $RepoRoot
try {
    Write-Section "Luna Multiplayer release builder"

    $msbuild  = Find-MSBuild
    $sevenZip = Find-SevenZip
    $dotnet   = (Get-Command dotnet -ErrorAction Stop).Source
    $nuget    = Find-NuGet -RepoRoot $RepoRoot

    $versionPath = Join-Path $RepoRoot 'LunaMultiplayer.version'
    if (-not (Test-Path $versionPath)) { throw "Missing $versionPath" }
    $versionJson = Get-Content -Raw $versionPath | ConvertFrom-Json
    $version = '{0}.{1}.{2}' -f `
        $versionJson.VERSION.MAJOR, $versionJson.VERSION.MINOR, $versionJson.VERSION.PATCH

    $kspDir = Join-Path $RepoRoot 'External\KSPLibraries'
    if (-not (Test-Path (Join-Path $kspDir 'Assembly-CSharp.dll'))) {
        throw "KSP libraries not extracted in $kspDir. Extract KSPLibraries.7z (or place the KSP DLLs there) before building."
    }

    $harmonyDir = Join-Path $RepoRoot 'External\Dependencies\Harmony'
    if (-not (Test-Path $harmonyDir)) {
        throw "Harmony dependency missing at $harmonyDir."
    }

    $clientProj = Join-Path $RepoRoot 'LmpClient\LmpClient.csproj'
    if (-not (Test-Path $clientProj)) { throw "LmpClient project not found at $clientProj" }

    $configsToBuild = if ($Configuration -eq 'All') { @('Debug', 'Release') } else { @($Configuration) }
    $finalRoot = Join-Path $RepoRoot 'FinalFiles'

    Write-Host "Repo root      : $RepoRoot"
    Write-Host "Output dir     : $OutputDir"
    Write-Host "Version        : $version"
    Write-Host "Configurations : $($configsToBuild -join ', ')"
    Write-Host "MSBuild        : $msbuild"
    Write-Host "dotnet         : $dotnet"
    Write-Host "7-Zip          : $sevenZip"
    Write-Host "NuGet          : $nuget"

    if (-not $NoClean) {
        if (Test-Path $finalRoot) {
            Write-Host "Cleaning $finalRoot..."
            Remove-Item $finalRoot -Recurse -Force
        }

        # Wipe obj\ for LmpClient and every legacy non-SDK project it
        # transitively references. Stale `project.assets.json` files left over
        # from a previous `msbuild -restore` attempt cause NuGet's build-time
        # target to fail with the misleading
        #     "Your project does not reference '.NETFramework,Version=v4.6'"
        # error on subsequent builds, even after we switch to standalone
        # `nuget restore`. We keep this list explicit (rather than scanning the
        # whole repo) so we never accidentally nuke obj\ for SDK-style projects
        # that `dotnet publish` will manage on its own.
        $legacyObjDirs = @(
            (Join-Path $RepoRoot 'LmpClient\obj'),
            (Join-Path $RepoRoot 'Lidgren.Net\obj')
        )
        foreach ($dir in $legacyObjDirs) {
            if (Test-Path $dir) {
                Write-Host "Cleaning $dir..."
                Remove-Item $dir -Recurse -Force
            }
        }
    }
    New-Item -ItemType Directory -Force -Path $OutputDir  | Out-Null
    New-Item -ItemType Directory -Force -Path $finalRoot  | Out-Null

    foreach ($cfg in $configsToBuild) {
        $stage       = Join-Path $finalRoot $cfg
        $clientStage = Join-Path $stage 'LMPClient\GameData\LunaMultiplayer'
        New-Item -ItemType Directory -Force -Path $clientStage | Out-Null

        # Copy Harmony into GameData (mirrors the appveyor before_build xcopy /y /s).
        Copy-Item (Join-Path $harmonyDir '*') -Destination (Join-Path $stage 'LMPClient\GameData') -Recurse -Force

        if (-not $SkipClient) {
            # Restore LmpClient's packages.config via the standalone NuGet CLI.
            # This honors .nuget\nuget.config (which redirects repositoryPath to
            # External\Nuget\), populating the layout that LmpClient.csproj's
            # HintPaths expect (..\External\Nuget\<pkg>\lib\<tfm>\*.dll).
            #
            # We cannot use `msbuild -restore` here: that uses the modern
            # PackageReference restore flow, which scans every transitively
            # referenced project. Lidgren.Net.csproj is a legacy non-SDK csproj
            # with no packages.config and no PackageReferences, so the modern
            # restore emits the misleading
            #   "Your project does not reference '.NETFramework,Version=v4.6'"
            # error and fails the build.
            $clientPackagesConfig = Join-Path $RepoRoot 'LmpClient\packages.config'
            if (Test-Path $clientPackagesConfig) {
                Write-Section "NuGet restore LmpClient packages.config"
                Invoke-External $nuget 'restore' $clientPackagesConfig `
                    '-PackagesDirectory' (Join-Path $RepoRoot 'External\Nuget') `
                    '-SolutionDirectory' $RepoRoot `
                    '-NonInteractive' '-Verbosity' 'quiet'
            }

            # Build the LmpClient project ONLY (not the .sln). The .sln pulls in
            # SDK-style projects (Server, MasterServer, ...) and shared projects
            # (LmpCommon.shproj, LmpGlobal.shproj). Legacy MSBuild from VS Build
            # Tools doesn't ship the Microsoft.NET.Sdk resolver or the Shared
            # Project (CodeSharing) targets, which makes a full-solution build
            # explode. The Server / MasterServer projects are published below
            # via `dotnet publish`, which handles the SDK-style projects.
            #
            # SolutionDir must be explicitly supplied because LmpClient.csproj
            # has a PreBuildEvent that runs `xcopy "$(SolutionDir)External\..."`.
            # When MSBuild builds a .csproj directly (no .sln context),
            # $(SolutionDir) evaluates to *Undefined* and the xcopy fails.
            # MSBuild convention: SolutionDir always ends with a separator.
            Write-Section "MSBuild build LmpClient ($cfg)"
            $solutionDir = $RepoRoot.TrimEnd('\','/') + '\'
            Invoke-External $msbuild $clientProj `
                "/p:Configuration=$cfg" '/p:Platform=AnyCPU' `
                "/p:SolutionDir=$solutionDir" `
                '/m' '/v:minimal' '/nologo'

            Write-Section "Stage client artifacts ($cfg)"
            New-StagingDirs -ClientStage $clientStage

            Copy-Item (Join-Path $RepoRoot 'LMP Readme.txt') (Join-Path $stage 'LMP Readme.txt') -Force
            Copy-Item $versionPath (Join-Path $clientStage 'LunaMultiplayer.version') -Force

            # Top-level files in Resources\ (e.g. LMPButton.png) -> Button\
            Get-ChildItem (Join-Path $RepoRoot 'LmpClient\Resources') -File |
                Copy-Item -Destination (Join-Path $clientStage 'Button') -Force

            # LmpClient build output -> Plugins\
            $clientBin = Join-Path $RepoRoot "LmpClient\bin\$cfg"
            if (-not (Test-Path $clientBin)) {
                throw "LmpClient build output missing at $clientBin"
            }
            Get-ChildItem $clientBin -File |
                Copy-Item -Destination (Join-Path $clientStage 'Plugins') -Force

            # Recursive XML trees
            Copy-Item (Join-Path $RepoRoot 'LmpClient\Localization\XML\*') `
                      (Join-Path $clientStage 'Localization') -Recurse -Force
            Copy-Item (Join-Path $RepoRoot 'LmpClient\ModuleStore\XML\*') `
                      (Join-Path $clientStage 'PartSync')     -Recurse -Force

            # Icons / Flags top-level files only (matches xcopy without /s)
            Get-ChildItem (Join-Path $RepoRoot 'LmpClient\Resources\Icons') -File |
                Copy-Item -Destination (Join-Path $clientStage 'Icons') -Force
            Get-ChildItem (Join-Path $RepoRoot 'LmpClient\Resources\Flags') -File |
                Copy-Item -Destination (Join-Path $clientStage 'Flags') -Force
        }

        if (-not $SkipServer) {
            $serverProj = Join-Path $RepoRoot 'Server\Server.csproj'

            # Framework-dependent Server publish (matches the 0.29.0 stable
            # release's "Server-<cfg>.zip" shape: portable across Windows and
            # Linux as long as .NET 6 is installed on the host).
            Write-Section "Publish Server ($cfg, framework-dependent)"
            $publishOut = Join-Path $stage 'LMPServer'
            Invoke-External $dotnet 'publish' $serverProj `
                '--configuration' $cfg `
                '--output' $publishOut `
                '--self-contained' 'false' `
                '-p:PublishSingleFile=false'

            if (-not $SkipLinuxServer) {
                # Self-contained linux-x64 Server publish (mirrors the
                # linux-x64 artifact produced by AppVeyor's nightly pipeline -
                # bundles the .NET 6 runtime so the host does not need .NET
                # installed). Flags match appveyor.yml: --os linux,
                # --self-contained true, PublishSingleFile=false, and
                # PublishTrimmed=true to keep the zip size reasonable.
                Write-Section "Publish Server ($cfg, linux-x64 self-contained)"
                $linuxPublishOut = Join-Path $stage 'LMPServer-linux-x64'
                Invoke-External $dotnet 'publish' $serverProj `
                    '--configuration' $cfg `
                    '--output' $linuxPublishOut `
                    '--os' 'linux' `
                    '--self-contained' 'true' `
                    '-p:PublishSingleFile=false' `
                    '-p:PublishTrimmed=true'
            }
        }

        Write-Section "Package zip artifacts ($cfg)"
        $readme = Join-Path $stage 'LMP Readme.txt'

        if (-not $SkipClient) {
            $clientZip = Join-Path $OutputDir "LunaMultiplayer-Client-$cfg.zip"
            Remove-Item $clientZip -Force -ErrorAction SilentlyContinue
            Invoke-External $sevenZip 'a' '-bd' '-mx=7' $clientZip `
                $readme (Join-Path $stage 'LMPClient\GameData')
        }

        if (-not $SkipServer) {
            $serverZip = Join-Path $OutputDir "LunaMultiplayer-Server-$cfg.zip"
            Remove-Item $serverZip -Force -ErrorAction SilentlyContinue
            Invoke-External $sevenZip 'a' '-bd' '-mx=7' $serverZip `
                $readme (Join-Path $stage 'LMPServer')

            if (-not $SkipLinuxServer) {
                # The self-contained publish folder is renamed to "LMPServer"
                # inside the zip so the extracted layout matches the portable
                # zip (same top-level directory name). 7-Zip preserves the
                # source directory's basename, so stage the rename by zipping
                # the folder itself.
                $linuxServerZip = Join-Path $OutputDir "LunaMultiplayer-Server-linux-x64-$cfg.zip"
                Remove-Item $linuxServerZip -Force -ErrorAction SilentlyContinue
                Invoke-External $sevenZip 'a' '-bd' '-mx=7' $linuxServerZip `
                    $readme (Join-Path $stage 'LMPServer-linux-x64')
            }
        }
    }

    Write-Section "Done"
    Write-Host "Release artifacts written to: $OutputDir"
    Get-ChildItem $OutputDir -File | Sort-Object Name |
        Format-Table Name,
            @{ Name = 'Size (MB)'; Expression = { [Math]::Round($_.Length / 1MB, 2) } } -AutoSize
}
finally {
    Pop-Location
}
