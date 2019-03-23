echo "Retrieving required libraries..."

BRANCH=$1
DESTINATION=$2

EXAMPLE_COMMAND="Example:\n..sh [branch] [destination]"

if [ ! $BRANCH ]; then
  BRANCH="master"
fi

echo "Installing libraries..."

CONFIG_FILE="ArduinoPlugAndPlay/lib/net40/ArduinoPlugAndPlay.exe.config";
CONFIG_FILE_SAVED="ArduinoPlugAndPlay.exe.config";


if [ -f $CONFIG_FILE ]; then
  echo "Config file found. Preserving."

  if [ ! -f $CONFIG_FILE_SAVED ]; then
    cp -v $CONFIG_FILE $CONFIG_FILE_SAVED || ("Failed to save a copy of the ArduinoPlugAndPlay.exe.config file" && exit 1)
  fi
fi

if [ ! -f "install-package.sh" ]; then
  echo "Downloading install-package.sh script...."
  INSTALL_SCRIPT_FILE_URL="https://raw.githubusercontent.com/GreenSense/ArduinoPlugAndPlay/$BRANCH/scripts-web/install-package-from-web.sh"
  wget --no-cache -O install-package-from-web.sh $INSTALL_SCRIPT_FILE_URL || ("Failed to download install-package.sh script" && exit 1)
else
  echo "The install-package.sh script already exists. Skipping download."
fi

echo "Installing the ArduinoPlugAndPlay library..."
sh install-package-from-web.sh ArduinoPlugAndPlay 1.0.0.28 || ("Failed to install ArduinoPlugAndPlay package" && exit 1)

if [ -f $CONFIG_FILE_TMP ]; then
  echo "Preserved config file found. Restoring."

  echo "Backing up empty config file"
  cp $CONFIG_FILE $CONFIG_FILE.bak

  echo "Restoring existing config file"
  cp $CONFIG_FILE_TMP $CONFIG_FILE || exit 1

fi

echo "Installation complete. Launching plug and play."


