::THIS FILE IS BEING UNTRACKED FROM GIT with the command: git update-index --assume-unchanged <file>
::To resume the tracking of changes to this file use the command: git update-index --no-assume-unchanged <file>
::You must keep this file in the solution folder for it to work. 
::Make sure to pass the solution configuration when calling it (either Debug or Release)

::Set the path below as you need and then run this bat to copy the files and make them debuggeable
SET KSPPATH=C:\Kerbal Space Program
::Set another path in case you run two KSP instances otherwise leave empty
::EXAMPLE: SET KSPPATH2=C:\Kerbal Space Program2
SET KSPPATH2=
SET SOLUTIONCONFIGURATION=%1

mkdir "%KSPPATH%\GameData\LunaMultiPlayer\"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiPlayer\")

mkdir "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiPlayer\Plugins")

mkdir "%KSPPATH%\GameData\LunaMultiPlayer\Button"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiPlayer\Button")

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaClient.dll"
xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaClient.*" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaClient.*" "%KSPPATH2%\GameData\LunaMultiPlayer\Plugins")

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaUpdater.dll"
xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaUpdater.*" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaUpdater.*" "%KSPPATH2%\GameData\LunaMultiPlayer\Plugins")

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LmpGlobal.dll"
xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LmpGlobal.*" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LmpGlobal.*" "%KSPPATH2%\GameData\LunaMultiPlayer\Plugins")

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\\Common\bin\%SOLUTIONCONFIGURATION%\LunaCommon.dll"
xcopy /Y "%~dp0..\Common\bin\%SOLUTIONCONFIGURATION%\LunaCommon.*" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Common\bin\%SOLUTIONCONFIGURATION%\LunaCommon.*" "%KSPPATH2%\GameData\LunaMultiPlayer\Plugins")

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\\Common\bin\%SOLUTIONCONFIGURATION%\Lidgren.Network.dll"
xcopy /Y "%~dp0..\Common\bin\%SOLUTIONCONFIGURATION%\Lidgren.Network.*" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Common\bin\%SOLUTIONCONFIGURATION%\Lidgren.Network.*" "%KSPPATH2%\GameData\LunaMultiPlayer\Plugins")

xcopy /Y "%~dp0..\External\Dependencies\*.dll" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\External\Dependencies\*.dll" "%KSPPATH2%\GameData\LunaMultiPlayer\Plugins")

xcopy /Y "%~dp0..\Client\Resources\*.png" "%KSPPATH%\GameData\LunaMultiPlayer\Button"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\Resources\*.png" "%KSPPATH2%\GameData\LunaMultiPlayer\Button")
