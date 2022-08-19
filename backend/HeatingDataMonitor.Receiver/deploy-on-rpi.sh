# TODO check if DOTNET_ROOT is set and if not do this (you could also just run dotnet --info and see if it fails):
# echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
# echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc
# echo 'export DOTNET_CLI_TELEMETRY_OPTOUT=1' >> ~/.bashrc
export DOTNET_CLI_TELEMETRY_OPTOUT=1

# install (and update? TODO if not, put it inside the if as well) .NET
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 6.0

dotnet publish -c Release
cp -ra bin/Release/net6.0/publish/. /home/pi/HeatingDataMonitor/

# TODO Prepare, register, enable systemd service
