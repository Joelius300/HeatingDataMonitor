#!/bin/bash

set -e

if [[ -z "$DOTNET_ROOT" ]]; then
  echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
  echo 'export DOTNET_CLI_TELEMETRY_OPTOUT=1' >> ~/.bashrc
  echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc
  # sourcing .bashrc from here doesn't work so update it for this process manually
  export DOTNET_ROOT=$HOME/.dotnet
  export DOTNET_CLI_TELEMETRY_OPTOUT=1
  export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools  # if the .NET install script were sourced, this wouldn't be necessary
fi

# installs (and maybe even updates?) .NET
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 6.0

dotnet publish -c Release
cp -ra bin/Release/net6.0/publish/. ~/HeatingDataMonitorReceiver/

