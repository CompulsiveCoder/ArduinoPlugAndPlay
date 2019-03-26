echo "Retrieving required libraries..."

echo "Current directory:"
echo "  $PWD"

DIR=$PWD

BRANCH=$1

EXAMPLE_COMMAND="Example:\n..sh [branch]"

if [ ! $BRANCH ]; then
  BRANCH="master"
fi

echo "$BRANCH"

echo "Installing libraries..."

CONFIG_FILE="ArduinoPlugAndPlay/lib/net40/ArduinoPlugAndPlay.exe.config";
CONFIG_FILE_SAVED="ArduinoPlugAndPlay.exe.config";

echo "Library config file:"
echo "  $PWD/$CONFIG_FILE"
echo "Saved config file:"
echo "  $PWD/$CONFIG_FILE_SAVED"

# If the config file is found in the downloaded package
if [ -f $CONFIG_FILE ]; then
  echo "Config file found. Preserving."

  # If no custom config file is found
  if [ ! -f $CONFIG_FILE_SAVED ]; then
    # Copy the config file from the package into the saved location
    cp -v $CONFIG_FILE $CONFIG_FILE_SAVED || ("Failed to save a copy of the ArduinoPlugAndPlay.exe.config file" && exit 1)
  fi
fi

if [ ! -f "install-package.sh" ]; then
  echo "Downloading install-package.sh script...."
  INSTALL_SCRIPT_FILE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-web/install-package-from-web.sh"
  wget --no-cache -O install-package-from-web.sh $INSTALL_SCRIPT_FILE_URL || ("Failed to download install-package.sh script" && exit 1)
else
  echo "The install-package.sh script already exists. Skipping download."
fi

echo "Installing the ArduinoPlugAndPlay library..."
sh install-package-from-web.sh ArduinoPlugAndPlay 1.0.0.77 || ("Failed to install ArduinoPlugAndPlay package" && exit 1)

# If a saved/custom config file is found then install it
if [ -f $CONFIG_FILE_SAVED ]; then
  echo "Installing saved config file..."

  # Copy the default config file to a .bak file
  echo "Backing up empty config file"
  cp $CONFIG_FILE $CONFIG_FILE.bak || ("Failed to backup default config file" && exit 1)

  echo "Restoring saved config file"
  # Install the saved/custom config file into the library
  cp $CONFIG_FILE_SAVED $CONFIG_FILE || ("Failed to install saved config file" && exit 1)

fi

cd $DIR

echo "Installation complete. Launching plug and play."


