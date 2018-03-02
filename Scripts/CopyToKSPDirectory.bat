::You must keep this file in the solution folder for it to work. 
::Make sure to pass the solution configuration when calling it (either Debug or Release)

::Set the directories in the setdirectories.bat file if you want a different folder than Kerbal Space Program
::EXAMPLE:
::SET KSPPATH=C:\Kerbal Space Program
::SET KSPPATH2=C:\Kerbal Space Program2
call "%~dp0\SetDirectories.bat"

IF DEFINED KSPPATH (ECHO KSPPATH is defined) ELSE (SET KSPPATH=C:\Kerbal Space Program)
IF DEFINED KSPPATH2 (ECHO KSPPATH2 is defined)

SET SOLUTIONCONFIGURATION=%1

mkdir "%KSPPATH%\GameData\LunaMultiPlayer\"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiPlayer\")

mkdir "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiPlayer\Plugins")

del "%KSPPATH%\GameData\LunaMultiPlayer\Plugins\*.*" /Q /F
IF DEFINED KSPPATH2 (del "%KSPPATH2%\GameData\LunaMultiPlayer\Plugins\*.*" /Q /F)

mkdir "%KSPPATH%\GameData\LunaMultiPlayer\Button"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiPlayer\Button")

mkdir "%KSPPATH%\GameData\LunaMultiPlayer\Localization"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiPlayer\Localization")

mkdir "%KSPPATH%\GameData\LunaMultiPlayer\PartSync"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiPlayer\PartSync")

mkdir "%KSPPATH%\GameData\LunaMultiPlayer\Icons"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiPlayer\Icons")

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaClient.dll"
xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaClient.*" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaClient.*" "%KSPPATH2%\GameData\LunaMultiPlayer\Plugins")

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaUpdater.dll"
xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaUpdater.*" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaUpdater.*" "%KSPPATH2%\GameData\LunaMultiPlayer\Plugins")

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\LmpGlobal\bin\%SOLUTIONCONFIGURATION%\LmpGlobal.dll"
xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LmpGlobal.*" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\LmpGlobal\bin\%SOLUTIONCONFIGURATION%\LmpGlobal.*" "%KSPPATH2%\GameData\LunaMultiPlayer\Plugins")

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\TPL\bin\%SOLUTIONCONFIGURATION%\*.dll"
xcopy /Y "%~dp0..\TPL\bin\%SOLUTIONCONFIGURATION%\*.*" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\TPL\bin\%SOLUTIONCONFIGURATION%\*.*" "%KSPPATH2%\GameData\LunaMultiPlayer\Plugins")

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

xcopy /Y "%~dp0..\Client\Localization\XML\*.xml" "%KSPPATH%\GameData\LunaMultiPlayer\Localization"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\Localization\XML\*.xml" "%KSPPATH2%\GameData\LunaMultiPlayer\Localization")

xcopy /Y "%~dp0..\Client\ModuleStore\XML\*.xml" "%KSPPATH%\GameData\LunaMultiPlayer\PartSync"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\ModuleStore\XML\*.xml" "%KSPPATH2%\GameData\LunaMultiPlayer\PartSync")

xcopy /Y "%~dp0..\Client\Resources\Icons\*.*" "%KSPPATH%\GameData\LunaMultiPlayer\Icons"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\Resources\Icons\*.*" "%KSPPATH2%\GameData\LunaMultiPlayer\Icons")
