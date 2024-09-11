#!/bin/bash

BLUE='\033[1;34m'
RESET='\033[0m'

info() {
    echo -e "${BLUE}$*${RESET}"
}

# Install SteamCMD if not present
if [ ! -d "./SteamCMD" ]; then
  info "Installing SteamCMD..."
  mkdir ./SteamCMD && cd ./SteamCMD
  curl -sqL "https://steamcdn-a.akamaihd.net/client/installer/steamcmd_osx.tar.gz" | tar zxvf -
  chmod +x steamcmd.sh
  cd ..
fi

info "Installing/updating Unturned..."
INSTALL_DIR="$(pwd)/unturned-dedicated"
./SteamCMD/steamcmd.sh  +@sSteamCmdForcePlatformType linux +force_install_dir "$INSTALL_DIR" +login anonymous +app_update 1110390 validate +quit

cd unturned-dedicated
info "Update completed."