@echo off
setlocal enabledelayedexpansion

rem ============================================================================
rem  LunaServer launcher
rem ----------------------------------------------------------------------------
rem  Pre-flight check that ensures the .NET 6 Runtime is installed before we
rem  hand off to Server.exe. Needed because when the runtime is missing,
rem  apphost's native "You must install .NET to run this application" message
rem  flashes past too quickly to read when users double-click Server.exe, so
rem  they never see why the window closed.
rem
rem  Use this .bat file to start the server on Windows. Server.exe still works
rem  on its own when the runtime is already installed.
rem ============================================================================

set "SCRIPT_DIR=%~dp0"
set "SERVER_EXE=%SCRIPT_DIR%Server.exe"

if not exist "%SERVER_EXE%" (
    echo.
    echo ERROR: Could not find Server.exe next to this launcher.
    echo Expected at: %SERVER_EXE%
    echo.
    pause
    exit /b 1
)

set "FOUND_BASE_NET6="
set "DOTNET_AVAILABLE="

rem 'dotnet --list-runtimes' prints one line per installed runtime, e.g.
rem   Microsoft.NETCore.App 6.0.25 [C:\Program Files\dotnet\shared\...]
rem We only accept a match on the base "Microsoft.NETCore.App" moniker whose
rem version starts with "6." - that's the one LunaServer actually needs.
for /f "usebackq tokens=1,2,*" %%A in (`dotnet --list-runtimes 2^>nul`) do (
    set "DOTNET_AVAILABLE=1"
    if /i "%%A"=="Microsoft.NETCore.App" (
        for /f "tokens=1 delims=." %%V in ("%%B") do (
            if "%%V"=="6" set "FOUND_BASE_NET6=1"
        )
    )
)

if not defined FOUND_BASE_NET6 (
    echo.
    echo ========================================================================
    echo  ERROR: The .NET 6 Runtime is not installed.
    echo ------------------------------------------------------------------------
    echo  LunaServer requires the .NET 6.0 Runtime to run.
    echo.
    echo  Please download and install it from:
    echo    https://dotnet.microsoft.com/en-us/download/dotnet/6.0
    echo.
    echo  On that page pick the "Runtime" ^(or "ASP.NET Core Runtime"^) download
    echo  that matches your operating system and architecture, then re-run this
    echo  launcher.
    echo.
    if defined DOTNET_AVAILABLE (
        echo  Currently installed .NET runtimes on this machine:
        echo.
        dotnet --list-runtimes 2^>nul
    ) else (
        echo  No .NET installation was detected at all ^(the 'dotnet' command is
        echo  not on PATH^).
    )
    echo ========================================================================
    echo.
    pause
    exit /b 1
)

rem Runtime looks good - launch the server in this window so its output is
rem visible, and keep the window open afterwards if it exits with an error.
"%SERVER_EXE%" %*
set "EXIT_CODE=%ERRORLEVEL%"

if not "%EXIT_CODE%"=="0" (
    echo.
    echo Server exited with code %EXIT_CODE%.
    pause
)

exit /b %EXIT_CODE%
