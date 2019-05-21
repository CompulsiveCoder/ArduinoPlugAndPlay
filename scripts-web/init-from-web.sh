echo "Initializing plug and play from the web..."

echo "Current directory:"
echo "  $PWD"

DIR=$PWD

BRANCH=$1

SMTP_SERVER=$2
ADMIN_EMAIL=$3

EXAMPLE_COMMAND="Example:\n..sh [Branch] [SmtpServer] [AdminEmail]"

if [ ! $BRANCH ]; then
  BRANCH="master"
fi

echo "Branch: $BRANCH"

echo "SMTP server: $SMTP_SERVER"
echo "Admin email: $ADMIN_EMAIL"

echo "Installing libraries..."

CONFIG_FILE="ArduinoPlugAndPlay/lib/net40/ArduinoPlugAndPlay.exe.config";
CONFIG_FILE_SAVED="ArduinoPlugAndPlay.exe.config";

echo "Library config file:"
echo "  $PWD/$CONFIG_FILE"
echo "Saved config file:"
echo "  $PWD/$CONFIG_FILE_SAVED"

if [ ! -f "install-package.sh" ]; then
  echo "Downloading install-package.sh script...."
  INSTALL_SCRIPT_FILE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-web/install-package-from-web.sh"
  curl -o install-package-from-web.sh -f $INSTALL_SCRIPT_FILE_URL || echo "Failed to download install-package-from-web.sh script."
else
  echo "The install-package.sh script already exists. Skipping download."
fi

echo "Installing the ArduinoPlugAndPlay library..."
sh install-package-from-web.sh ArduinoPlugAndPlay 1.0.0.158 || exit 1

# If the config file is found in the downloaded package
if [ -f $CONFIG_FILE ]; then
  echo "Config file found. Preserving."

  # If no custom config file is found
  if [ ! -f $CONFIG_FILE_SAVED ]; then
    # Copy the config file from the package into the saved location
    cp -v $CONFIG_FILE $CONFIG_FILE_SAVED || exit 1
  fi
else
  echo "Can't find config file in library:"
  echo "  $CONFIG_FILE"
  exit 1
fi

echo "Injecting email details into configuration file"
if [ $SMTP_SERVER ]; then
  xmlstarlet ed -L -u '/configuration/appSettings/add[@key="SmtpServer"]/@value' -v "$SMTP_SERVER" $CONFIG_FILE_SAVED
fi
if [ $ADMIN_EMAIL ]; then
  xmlstarlet ed -L -u '/configuration/appSettings/add[@key="EmailAddress"]/@value' -v "$ADMIN_EMAIL" $CONFIG_FILE_SAVED
fi

# If a saved/custom config file is found then install it
if [ -f $CONFIG_FILE_SAVED ]; then
  echo "Installing saved config file..."

  # Copy the default config file to a .bak file
  echo "Backing up empty config file"
  cp $CONFIG_FILE $CONFIG_FILE.bak || exit 1

  echo "Restoring saved config file"
  # Install the saved/custom config file into the library
  cp $CONFIG_FILE_SAVED $CONFIG_FILE || exit 1

fi

cd $DIR

echo "Installation complete. Launching plug and play."


