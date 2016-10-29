::THIS FILE IS BEING UNTRACKED FROM GIT with the command: git update-index --assume-unchanged <file>
::To resume the tracking of changes to this file use the command: git update-index --no-assume-unchanged <file>

::Set the path below as you need and then run this bat to copy the files and make them debuggeable
SET KSPPATH=C:\games\Steam\SteamApps\common\Kerbal Space Program

mkdir "%KSPPATH%\GameData\LunaMultiPlayer\"
mkdir "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"

".\External\pdb2mdb\pdb2mdb.exe" ".\Client\bin\Debug\LunaClient.dll"
xcopy /Y ".\Client\bin\Debug\LunaClient.dll" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\Client\bin\Debug\LunaClient.pdb" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\Client\bin\Debug\LunaClient.dll.mdb" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"

".\External\pdb2mdb\pdb2mdb.exe" ".\Common\bin\Debug\LunaCommon.dll"
xcopy /Y ".\Common\bin\Debug\LunaCommon.dll" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\Common\bin\Debug\LunaCommon.pdb" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\Common\bin\Debug\LunaCommon.dll.mdb" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"

xcopy /Y ".\External\Dependencies\FastMember.dll" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\External\Dependencies\Lidgren.Network.dll" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\External\Dependencies\System.Threading.dll" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\External\Dependencies\System.Data.dll" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\External\Dependencies\Mono.Data.Tds.dll" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
xcopy /Y ".\External\Dependencies\System.Transactions.dll" "%KSPPATH%\GameData\LunaMultiPlayer\Plugins"
