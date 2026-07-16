# Runs every automated test project in the solution (ServerTest, LmpMasterServerTest, LmpCommonTest).
# LmpClient targets .NET Framework 4.7.2 for KSP; there is no client unit-test project here.
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Set-Location $repoRoot

dotnet test (Join-Path $repoRoot 'LunaMultiPlayer.sln') -c $Configuration --verbosity minimal @args
