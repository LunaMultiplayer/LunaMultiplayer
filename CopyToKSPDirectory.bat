::THIS FILE IS BEING UNTRACKED FROM GIT with the command: git update-index --assume-unchanged <file>
::To resume the tracking of changes to this file use the command: git update-index --no-assume-unchanged <file>

::Set the path below as you need and then run this bat to copy the files and make them debuggeable
SET KSPPATH=D:\Steam\SteamApps\common\Kerbal Space Program

mkdir "%KSPPATH%\GameData\LunaMultiPlayer\"
mkdir "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"

".\ExternalLibraries\pdb2mdb\pdb2mdb.exe" ".\Client\bin\Debug\LunaClient.dll"
xcopy /Y ".\Client\bin\Debug\LunaClient.dll" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\Client\bin\Debug\LunaClient.pdb" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\Client\bin\Debug\LunaClient.dll.mdb" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"

".\ExternalLibraries\pdb2mdb\pdb2mdb.exe" ".\Common\bin\Debug\LunaCommon.dll"
xcopy /Y ".\Common\bin\Debug\LunaCommon.dll" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\Common\bin\Debug\LunaCommon.pdb" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\Common\bin\Debug\LunaCommon.dll.mdb" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"

xcopy /Y ".\ExternalLibraries\FastMember.dll" "D:\Steam\SteamApps\common\Kerbal Space Program\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\ExternalLibraries\Lidgren.Network.dll" "D:\Steam\SteamApps\common\Kerbal Space Program\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\ExternalLibraries\System.Threading.dll" "D:\Steam\SteamApps\common\Kerbal Space Program\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\ExternalLibraries\System.Data.dll" "D:\Steam\SteamApps\common\Kerbal Space Program\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\ExternalLibraries\Mono.Data.Tds.dll" "D:\Steam\SteamApps\common\Kerbal Space Program\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\ExternalLibraries\System.Transactions.dll" "D:\Steam\SteamApps\common\Kerbal Space Program\GameData\LunaMultiPlayer\Plugins"
