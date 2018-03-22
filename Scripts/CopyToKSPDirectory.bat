::You must keep this file in the solution folder for it to work. 
::Make sure to pass the solution configuration when calling it (either Debug or Release)

::Set the directories in the setdirectories.bat file if you want a different folder than Kerbal Space Program
::EXAMPLE:
SET KSPPATH=C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program
SET KSPPATH2=C:\Users\Malte\Desktop\Kerbal Space Program
call "%~dp0\SetDirectories.bat"

IF DEFINED KSPPATH (ECHO KSPPATH is defined) ELSE (SET KSPPATH=C:\Kerbal Space Program)
IF DEFINED KSPPATH2 (ECHO KSPPATH2 is defined)
::%1
SET SOLUTIONCONFIGURATION=Debug

mkdir "%KSPPATH%\GameData\LunaMultiplayer\"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiplayer\")

mkdir "%KSPPATH%\GameData\LunaMultiplayer\Plugins"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiplayer\Plugins")

del "%KSPPATH%\GameData\LunaMultiplayer\Plugins\*.*" /Q /F
IF DEFINED KSPPATH2 (del "%KSPPATH2%\GameData\LunaMultiplayer\Plugins\*.*" /Q /F)

mkdir "%KSPPATH%\GameData\LunaMultiplayer\Button"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiplayer\Button")

mkdir "%KSPPATH%\GameData\LunaMultiplayer\Localization"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiplayer\Localization")

mkdir "%KSPPATH%\GameData\LunaMultiplayer\PartSync"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiplayer\PartSync")

mkdir "%KSPPATH%\GameData\LunaMultiplayer\Icons"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiplayer\Icons")

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaClient.dll"
xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaClient.*" "%KSPPATH%\GameData\LunaMultiplayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaClient.*" "%KSPPATH2%\GameData\LunaMultiplayer\Plugins")

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaUpdater.dll"
xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaUpdater.*" "%KSPPATH%\GameData\LunaMultiplayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaUpdater.*" "%KSPPATH2%\GameData\LunaMultiplayer\Plugins")

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\LmpGlobal\bin\%SOLUTIONCONFIGURATION%\LmpGlobal.dll"
xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LmpGlobal.*" "%KSPPATH%\GameData\LunaMultiplayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\LmpGlobal\bin\%SOLUTIONCONFIGURATION%\LmpGlobal.*" "%KSPPATH2%\GameData\LunaMultiplayer\Plugins")

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\TPL\bin\%SOLUTIONCONFIGURATION%\*.dll"
xcopy /Y "%~dp0..\TPL\bin\%SOLUTIONCONFIGURATION%\*.*" "%KSPPATH%\GameData\LunaMultiplayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\TPL\bin\%SOLUTIONCONFIGURATION%\*.*" "%KSPPATH2%\GameData\LunaMultiplayer\Plugins")

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\\Common\bin\%SOLUTIONCONFIGURATION%\LunaCommon.dll"
xcopy /Y "%~dp0..\Common\bin\%SOLUTIONCONFIGURATION%\LunaCommon.*" "%KSPPATH%\GameData\LunaMultiplayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Common\bin\%SOLUTIONCONFIGURATION%\LunaCommon.*" "%KSPPATH2%\GameData\LunaMultiplayer\Plugins")

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\\Common\bin\%SOLUTIONCONFIGURATION%\Lidgren.Network.dll"
xcopy /Y "%~dp0..\Common\bin\%SOLUTIONCONFIGURATION%\Lidgren.Network.*" "%KSPPATH%\GameData\LunaMultiplayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Common\bin\%SOLUTIONCONFIGURATION%\Lidgren.Network.*" "%KSPPATH2%\GameData\LunaMultiplayer\Plugins")

xcopy /Y "%~dp0..\External\Dependencies\*.dll" "%KSPPATH%\GameData\LunaMultiplayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\External\Dependencies\*.dll" "%KSPPATH2%\GameData\LunaMultiplayer\Plugins")

xcopy /Y "%~dp0..\Client\Resources\*.png" "%KSPPATH%\GameData\LunaMultiplayer\Button"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\Resources\*.png" "%KSPPATH2%\GameData\LunaMultiplayer\Button")

xcopy /Y "%~dp0..\Client\Localization\XML\*.xml" "%KSPPATH%\GameData\LunaMultiplayer\Localization"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\Localization\XML\*.xml" "%KSPPATH2%\GameData\LunaMultiplayer\Localization")

xcopy /Y "%~dp0..\Client\ModuleStore\XML\*.xml" "%KSPPATH%\GameData\LunaMultiplayer\PartSync"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\ModuleStore\XML\*.xml" "%KSPPATH2%\GameData\LunaMultiplayer\PartSync")

xcopy /Y "%~dp0..\Client\Resources\Icons\*.*" "%KSPPATH%\GameData\LunaMultiplayer\Icons"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\Resources\Icons\*.*" "%KSPPATH2%\GameData\LunaMultiplayer\Icons")
