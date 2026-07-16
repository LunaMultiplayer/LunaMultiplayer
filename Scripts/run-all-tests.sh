#!/usr/bin/env bash
# Runs every automated test project in the solution (ServerTest, LmpMasterServerTest, LmpCommonTest).
# LmpClient targets .NET Framework 4.7.2 for KSP; there is no client unit-test project here.
set -euo pipefail
cd "$(dirname "$0")/.."
CONFIGURATION="${1:-Release}"
dotnet test LunaMultiPlayer.sln -c "$CONFIGURATION" --verbosity minimal "${@:2}"
