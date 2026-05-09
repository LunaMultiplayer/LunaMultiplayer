#!/usr/bin/env bash

# Builds LMPClient, Server, and MasterServer
# Usage: ./build-lmp-projects.sh [--debug | --release]
#   --debug    Build Debug only
#   --release  Build Release only
#   --LmpClient
#   --Server
#   --MasterServer
#   (no arg)   Build Debug then Release

set -e

BUILD_DEBUG=1
BUILD_RELEASE=1
BUILD_CLIENT=0
BUILD_SERVER=0
BUILD_MASTERSERVER=0

for i in "$@" 
do
    if [[ "$i" == "--debug" ]]; then
        BUILD_RELEASE=0
    elif [[ "$i" == "--release" ]]; then
        BUILD_DEBUG=0
    elif [[ "$i" == "--LmpClient" ]]; then
        BUILD_CLIENT=1
    elif [[ "$i" == "--Server" ]]; then
        BUILD_SERVER=1
    elif [[ "$i" == "--MasterServer" ]]; then
        BUILD_MASTERSERVER=1
    elif [[ "$i" != "" ]]; then
        echo "Unknown argument: $1"
        echo "Usage: build-lmp-projects.sh [--debug | --release] [--LmpClient | --Server | -- MasterServer]"
        exit 1
    fi
done

if [[ $BUILD_CLIENT -eq 0 && $BUILD_SERVER -eq 0 && $BUILD_MASTERSERVER -eq 0 ]]; then
    BUILD_CLIENT=1
    BUILD_SERVER=1
    BUILD_MASTERSERVER=1
fi

cd "$(dirname "$0")/.."

build_project () {
    local project=$1
    local config=$2
    echo "Building $project ($config)..."
    dotnet build "$project/$project.csproj" -c "$config"
    echo
}

if [[ $BUILD_CLIENT -eq 1 ]]; then
    if [[ $BUILD_DEBUG -eq 1 ]]; then
        build_project "LmpClient" "Debug"
    fi

    if [[ $BUILD_RELEASE -eq 1 ]]; then
        build_project "LmpClient" "Release"
    fi
fi

if [[ $BUILD_SERVER -eq 1 ]]; then
    if [[ $BUILD_DEBUG -eq 1 ]]; then
        build_project "Server" "Debug"
    fi

    if [[ $BUILD_RELEASE -eq 1 ]]; then
        build_project "Server" "Release"
    fi
fi

if [[ $BUILD_MASTERSERVER -eq 1 ]]; then
    if [[ $BUILD_DEBUG -eq 1 ]]; then
        build_project "MasterServer" "Debug"
    fi

    if [[ $BUILD_RELEASE -eq 1 ]]; then
        build_project "MasterServer" "Release"
    fi
fi