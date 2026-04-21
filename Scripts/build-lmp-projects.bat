@echo off
setlocal
REM Builds LMPClient, Server, and MasterServer.
REM Usage: build-lmp-projects.bat [--debug | --release]
REM   --debug    Build Debug only
REM   --release  Build Release only
REM   (no arg)   Build Debug then Release
cd /d "%~dp0.."

set BUILD_DEBUG=1
set BUILD_RELEASE=1

if /i "%~1"=="--debug"   set BUILD_RELEASE=0
if /i "%~1"=="--release" set BUILD_DEBUG=0

if "%~1" NEQ "" if /i "%~1" NEQ "--debug" if /i "%~1" NEQ "--release" (
    echo Unknown argument: %~1
    echo Usage: build-lmp-projects.bat [--debug ^| --release]
    exit /b 1
)

if %BUILD_DEBUG%==1 (
    dotnet build LmpClient\LmpClient.csproj -c Debug || exit /b 1
    echo.
    echo.
    echo.
)

if %BUILD_RELEASE%==1 (
    dotnet build LmpClient\LmpClient.csproj -c Release || exit /b 1
    echo.
    echo.
    echo.
)

if %BUILD_DEBUG%==1 (
    dotnet build Server\Server.csproj -c Debug || exit /b 1
    echo.
    echo.
    echo.
)

if %BUILD_RELEASE%==1 (
    dotnet build Server\Server.csproj -c Release || exit /b 1
    echo.
    echo.
    echo.
)

if %BUILD_DEBUG%==1 (
    dotnet build MasterServer\MasterServer.csproj -c Debug || exit /b 1
    echo.
    echo.
    echo.
)

if %BUILD_RELEASE%==1 (
    dotnet build MasterServer\MasterServer.csproj -c Release || exit /b 1
)

endlocal
