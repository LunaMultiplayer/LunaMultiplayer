::THIS FILE IS BEING UNTRACKED FROM GIT with the command: git update-index --assume-unchanged <file>
::To resume the tracking of changes to this file use the command: git update-index --no-assume-unchanged <file>
::You must keep this file in the solution folder for it to work

::Set the path below as you need and then run this bat to copy the files and make them debuggeable
SET KSPPATH=C:\games\Steam\SteamApps\common\Kerbal Space Program

mkdir "%KSPPATH%\GameData\LunaMultiPlayer\"
mkdir "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
mkdir "%KSPPATH%\GameData\LunaMultiPlayer\Button"

"%~dp0\External\pdb2mdb\pdb2mdb.exe" "%~dp0\Client\bin\Debug\LunaClient.dll"
xcopy /Y "%~dp0\Client\bin\Debug\LunaClient.*" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"

"%~dp0\External\pdb2mdb\pdb2mdb.exe" "%~dp0\Common\bin\Debug\LunaCommon.dll"
xcopy /Y "%~dp0\Common\bin\Debug\LunaCommon.*" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"

xcopy /Y "%~dp0\External\Dependencies\*.dll" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
xcopy /Y "%~dp0\Client\Resources\*.png" "%KSPPATH%\GameData\LunaMultiPlayer\Button"
