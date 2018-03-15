ECHO OFF
title Kerbal Space Program Launcher
SETLOCAL EnableExtensions
set EXE=KSP.exe
set EXE64=KSP_x64.exe
set LUNAUPDATER=Updater.exe
:CHECK
FOR /F %%x IN ('tasklist /NH /FI "IMAGENAME eq %EXE%"') DO IF %%x == %EXE% goto FOUND
FOR /F %%x IN ('tasklist /NH /FI "IMAGENAME eq %EXE64%"') DO IF %%x == %EXE64% goto FOUND64
FOR /F %%x IN ('tasklist /NH /FI "IMAGENAME eq %LUNAUPDATER%"') DO IF %%x == %LUNAUPDATER% goto LUNAUPDATER
goto MENU
:FOUND
cls 
SET INPUT=
COLOR 0C
echo ERROR: %EXE% IS RUNNING IN BACKGROUND. YOU NEED TO STOP KSP BEFORE CONTIUING!
echo This script requires the directory be free of use while it  updates.
echo Would you like us to close the game for you...
SET /P INPUT=Y or N 

IF /I '%INPUT%'=='Y' taskkill /f /im %EXE%
echo %EXE% has been killed.
TIMEOUT /T 3 /NOBREAK
cls 
goto CHECK
:LUNAUPDATER
cls 
SET INPUT=
COLOR 0C
echo ERROR: %LUNAUPDATER% IS RUNNING IN BACKGROUND. YOU NEED TO STOP PREVIOUS UPDATER BEFORE CONTIUING!
echo This script requires the directory be free of use while it updates.
echo Would you like us to close the game for you...
SET /P INPUT=Y or N 

IF /I '%INPUT%'=='Y' taskkill /f /im %LUNAUPDATER%
echo %LUNAUPDATER% has been killed.
TIMEOUT /T 3 /NOBREAK
cls 
goto CHECK
:FOUND64
cls 
SET INPUT=
COLOR 0C
echo ERROR: %EXE64% IS RUNNING IN BACKGROUND. YOU NEED TO STOP KSP BEFORE CONTIUING!
echo This script requires the directory be free of use while it updates.
echo Would you like us to close the game for you...
SET /P INPUT=Y or N 

IF /I '%INPUT%'=='Y' taskkill /f /im %EXE64%
echo %EXE64% has been killed.
TIMEOUT /T 3 /NOBREAK
cls 
goto CHECK
:MENU
CLS
COLOR A
ECHO =============Kerbal Manager=============
ECHO -------------------------------------
ECHO 1.  Install/Update Luna Multiplayer
ECHO 2.  Play Kerbal Space Program
ECHO Q.	 Quit the Manager
ECHO -------------------------------------
ECHO  Invalid choices will default to installing
ECHO  the update to the KSP directory. %LUNAUPDATER%
ECHO  requires to be in your KSP installation.
ECHO -------------------------------------
ECHO ==========PRESS 'Q' TO QUIT==========
ECHO.

SET INPUT=
COLOR A
SET /P INPUT=Please select a number:

IF /I '%INPUT%'=='1' GOTO OPTION1
IF /I '%INPUT%'=='2' GOTO OPTION2
IF /I '%INPUT%'=='Q' GOTO Quit

CLS
COLOR 0C
ECHO ============INVALID INPUT============
ECHO -------------------------------------
ECHO Defaulting to updating Luna files..
ECHO This process may take a while...
ECHO -------------------------------------
ECHO ======PRESS CTRL + C TO STOP======
GOTO OPTION1
:OPTION1
  IF EXIST %LUNAUPDATER% (
        ECHO Downloading Updates from appvenyor. This might take a min...
		FOR /F %%x IN ('tasklist /NH /FI "IMAGENAME eq %EXE%"') DO IF %%x == %EXE% goto FOUND
		FOR /F %%x IN ('tasklist /NH /FI "IMAGENAME eq %EXE64%"') DO IF %%x == %EXE64% goto FOUND64
		FOR /F %%x IN ('tasklist /NH /FI "IMAGENAME eq %LUNAUPDATER%"') DO IF %%x == %LUNAUPDATER% goto LUNAUPDATER
		start Updater.exe > LunaUpdate.log
		ECHO Update has been completed!
		GOTO MENU
    ) ELSE (
		CLS
		COLOR 0C
        echo Please download the %LUNAUPDATER% and place into the KSP installation directory with %EXE%
		timeout /t -1
		start "" explorer "https://github.com/LunaMultiplayer/LunaMultiplayerUpdater/releases/download/1.0.0/LunaMultiplayerUpdater-Release.zip"
		GOTO MENU
    )

:OPTION2
  IF EXIST %EXE64% (
		Echo Starting
		START /wait  /REALTIME "Kerbal Space Program" %EXE64%
		GOTO MENU
 ) ELSE (
		CLS
        echo Please place this file into your KSP installation folder, next to %EXE% and  %LUNAUPDATER%.
		timeout /t -1
	)
:Quit
CLS
COLOR 1D
ECHO ========Thanks for all the fish===========
ECHO -------------------------------------
TIMEOUT /T 3
EXIT
