::THIS FILE IS BEING UNTRACKED FROM GIT with the command: git update-index --assume-unchanged <file>
::To resume the tracking of changes to this file use the command: git update-index --no-assume-unchanged <file>
::You must keep this file in the solution folder for it to work. 
::Make sure to pass the solution configuration when calling it (either Debug or Release)

::Set the path below as you need and then run this bat to copy the files and make them debuggeable
SET KSPPATH=C:\Kerbal Space Program
SET SOLUTIONCONFIGURATION=%1

mkdir "%KSPPATH%\GameData\LunaMultiPlayer\"
mkdir "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
mkdir "%KSPPATH%\GameData\LunaMultiPlayer\Button"

"%~dp0\External\pdb2mdb\pdb2mdb.exe" "%~dp0\Client\bin\%SOLUTIONCONFIGURATION%\LunaClient.dll"
xcopy /Y "%~dp0Client\bin\%SOLUTIONCONFIGURATION%\LunaClient.*" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"

"%~dp0\External\pdb2mdb\pdb2mdb.exe" "%~dp0\Common\bin\%SOLUTIONCONFIGURATION%\LunaCommon.dll"
xcopy /Y "%~dp0Common\bin\%SOLUTIONCONFIGURATION%\LunaCommon.*" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"

xcopy /Y "%~dp0External\Dependencies\*.dll" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
xcopy /Y "%~dp0Client\Resources\*.png" "%KSPPATH%\GameData\LunaMultiPlayer\Button"
