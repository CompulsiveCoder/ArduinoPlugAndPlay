echo "Upgrading plug and play..."

BRANCH=$1
DESTINATION=$2

EXAMPLE_COMMAND="Example:\n..sh [branch] [destination]"

if [ ! $BRANCH ]; then
  BRANCH="master"
fi

if [ ! $DESTINATION ]; then
  DESTINATION="/usr/local/ArduinoPlugAndPlay"
fi

echo "  Branch: $BRANCH"
echo "  Destination: $DESTINATION"

INSTALL_DIR=$DESTINATION

echo "  Checking for install dir..."
if [ ! -d $INSTALL_DIR ]; then
  echo "    ArduinoPlugAndPlay doesn't appear to be installed at:"
  echo "      $INSTALL_DIR"
  echo "    Use the scripts-ols/install.sh script to install it."
  exit 1
fi

echo ""
echo "  Moving to install dir..."
cd $INSTALL_DIR

INSTALLED_VERSION="$(cat version.txt)"

LATEST_BUILD_NUMBER=$(curl -s -H 'Cache-Control: no-cache' "https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/buildnumber.txt")
LATEST_VERSION_NUMBER=$(curl -s -H 'Cache-Control: no-cache' "https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/version.txt")

LATEST_VERSION="$LATEST_VERSION_NUMBER.$LATEST_BUILD_NUMBER"

echo "  Installed version: $INSTALLED_VERSION"
echo "  Latest version: $LATEST_VERSION"

if [ "$LATEST_VERSION" != "" ] & [ "$INSTALLED_VERSION" != "$LATEST_VERSION" ]; then

  echo ""
  echo "  Stopping plug and play service..."
  sh systemctl.sh stop arduino-plug-and-play.service || exit 1

  echo ""
  echo "  Removing old scripts..."
  rm *.sh || exit 1

  echo ""
  echo "  Removing old libraries..."
  rm ArduinoPlugAndPlay -R && \
  rm ArduinoPlugAndPlay*.nupkg || exit 1

  INSTALL_SCRIPT_FILE_NAME="install.sh"
  
  echo ""
  echo "  Getting auto update setting..."
  AUTO_UPDATE_SETTING_FILE="enable-auto-update.txt"
  if [ -f $AUTO_UPDATE_SETTING_FILE ]; then
    ENABLE_AUTO_UPDATE=$(cat $AUTO_UPDATE_SETTING_FILE)
  else
    ENABLE_AUTO_UPDATE=0
  fi
  
  echo "  Enable auto update: $ENABLE_AUTO_UPDATE"
  
  echo ""
  echo "  Getting email details from file..."
  SMTP_SERVER=$(cat smtp-server.txt)
  ADMIN_EMAIL=$(cat admin-email.txt)
  echo "    SMTP Server: $SMTP_SERVER"
  echo "    Admin email: $ADMIN_EMAIL"
  
  if [ "$ENABLE_AUTO_UPDATE" = "1" ]; then
    INSTALL_SCRIPT_FILE_NAME="install-auto-update.sh"
  fi  

  echo ""
  echo "  Executing $INSTALL_SCRIPT_FILE_NAME script..."
  INSTALL_SCRIPT_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-ols/$INSTALL_SCRIPT_FILE_NAME"
  echo "    URL: $INSTALL_SCRIPT_URL"
  echo "    File name: $INSTALL_SCRIPT_FILE_NAME"
  wget -nv --no-cache -O - $INSTALL_SCRIPT_URL | bash -s "$BRANCH" "$DESTINATION" "$SMTP_SERVER" "$ADMIN_EMAIL" || exit 1
else
  echo "  Up to date. Skipping upgrade."
fi

echo "Finished setting up plug and play"
