#!/bin/bash

if [[ -z "$DOTNET_ROOT" ]]; then
  echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
  echo 'export DOTNET_CLI_TELEMETRY_OPTOUT=1' >> ~/.bashrc
  echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc
  export DOTNET_ROOT=$HOME/.dotnet
  export DOTNET_CLI_TELEMETRY_OPTOUT=1
  # PATH is updated for this process by the installer script
fi

# install (and maybe even updates?) .NET
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 6.0

dotnet publish -c Release
cp -ra bin/Release/net6.0/publish/. ~/HeatingDataMonitorReceiver/

