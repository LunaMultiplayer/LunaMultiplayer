@echo off
setlocal
REM Builds LMPClient, Server, and MasterServer.
REM Each project's output folder is wiped before building so it only
REM contains freshly generated files.
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
    call :build LmpClient\LmpClient.csproj LmpClient Debug || exit /b 1
)

if %BUILD_RELEASE%==1 (
    call :build LmpClient\LmpClient.csproj LmpClient Release || exit /b 1
)

if %BUILD_DEBUG%==1 (
    call :build Server\Server.csproj Server Debug || exit /b 1
)

if %BUILD_RELEASE%==1 (
    call :build Server\Server.csproj Server Release || exit /b 1
)

if %BUILD_DEBUG%==1 (
    call :build MasterServer\MasterServer.csproj MasterServer Debug || exit /b 1
)

if %BUILD_RELEASE%==1 (
    call :build MasterServer\MasterServer.csproj MasterServer Release || exit /b 1
)

endlocal
exit /b 0

REM ---------------------------------------------------------------------------
REM :build <projectFile> <projectDir> <configuration>
REM Wipes <projectDir>\bin\<configuration> then builds the project.
REM ---------------------------------------------------------------------------
:build
set "_outdir=%~2\bin\%~3"
if exist "%_outdir%" (
    echo Clearing "%_outdir%"
    rmdir /s /q "%_outdir%" || exit /b 1
)
dotnet build "%~1" -c %~3 || exit /b 1
echo.
echo.
echo.
exit /b 0
