echo "Installing plug and play service..."

echo "Current directory:"
echo "  $PWD"

BRANCH=$1
DESTINATION=$2

EXAMPLE_COMMAND="Example:\n..sh [branch] [destination]"

if [ ! $BRANCH ]; then
  BRANCH="master"
fi

if [ ! $DESTINATION ]; then
  DESTINATION="/usr/local/ArduinoPlugAndPlay"
fi

echo "$BRANCH"

# Get the service file

SERVICE_FILE_NAME="arduino-plug-and-play.service"
SERVICE_FILE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/svc/$SERVICE_FILE_NAME"

echo "Downloading service file..."
echo "URL: $SERVICE_FILE_URL"
echo "File name: $SERVICE_FILE_NAME"
wget $SERVICE_FILE_URL -O $SERVICE_FILE_NAME || (echo "Failed to download $SERVICE_FILE_NAME." && exit 1)

# Get the systemctl.sh helper script
SYSTEMCTL_FILE_NAME="systemctl.sh"
SYSTEMCTL_FILE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/$SYSTEMCTL_FILE_NAME"

echo "Downloading systemctl file..."
echo "URL: $SYSTEMCTL_FILE_URL"
echo "File name: $SYSTEMCTL_FILE_NAME"
wget $SYSTEMCTL_FILE_URL -O $SYSTEMCTL_FILE_NAME || (echo "Failed to download $SYSTEMCTL_FILE_NAME." && exit 1)

# Get the install-service.sh helper script
INSTALL_SERVICE_SCRIPT_NAME="install-service.sh"
INSTALL_SERVICE_SCRIPT_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/$INSTALL_SERVICE_SCRIPT_NAME"

echo "Downloading service file..."
echo "URL: $INSTALL_SERVICE_SCRIPT_URL"
echo "File name: $INSTALL_SERVICE_SCRIPT_NAME"
wget $INSTALL_SERVICE_SCRIPT_URL -O $INSTALL_SERVICE_SCRIPT_NAME || (echo "Failed to download $INSTALL_SERVICE_SCRIPT_NAME." && exit 1)


echo "Injecting values into template service file..."

ESCAPED_INSTALL_DIR="${DESTINATION//\//\\/}"

sed -i -e "s/{INSTALL_PATH}/$ESCAPED_INSTALL_DIR/g" "$SERVICE_FILE_NAME" || exit 1
sed -i -e "s/{BRANCH}/$BRANCH/g" "$SERVICE_FILE_NAME" || exit 1


echo "Installing service"

sh install-service.sh $SERVICE_FILE_NAME || (echo "Failed to install service: $SERVICE_FILE_NAME." && exit 1)

echo "Finished installing service"
