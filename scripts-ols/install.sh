echo "Installing plug and play from the web..."

BRANCH=$1
DESTINATION=$2

SMTP_SERVER=$3
ADMIN_EMAIL=$4

EXAMPLE_COMMAND="Example:\n..sh [Branch] [Install_Dir] [SmtpServer] [AdminEmail]"

if [ ! $BRANCH ]; then
  BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')
fi

if [ ! $BRANCH ]; then
  BRANCH="master"
fi

if [ ! $DESTINATION ]; then
  DESTINATION="/usr/local/ArduinoPlugAndPlay"
fi

echo "  Branch: $BRANCH"
echo "  Destination: $DESTINATION"

echo "  SMTP server: $SMTP_SERVER"
echo "  Admin email: $ADMIN_EMAIL"

INSTALL_DIR=$DESTINATION

echo ""
echo "  Making install dir..."
echo "    $INSTALL_DIR"
mkdir -p $INSTALL_DIR || exit 1

echo ""
echo "  Moving to install dir..."
echo "   $INSTALL_DIR"
cd $INSTALL_DIR || exit 1

echo ""
echo "  Downloading init.sh script..."
INIT_FILE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-installation/init.sh"
echo "    URL: $INIT_FILE_URL"
echo "    File name: init.sh"
wget -q --no-cache $INIT_FILE_URL || exit 1

echo ""
echo "  Downloading install-service.sh script..."
INSTALL_SERVICE_FILE="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/install-service.sh"
echo "    URL: $INSTALL_SERVICE_FILE"
echo "    File name: install-service.sh"
wget -q --no-cache $INSTALL_SERVICE_FILE || exit 1

echo ""
echo "  Downloading install-package.sh script..."
INSTALL_PACKAGE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-installation/install-package.sh"
echo "    URL: $INSTALL_PACKAGE_URL"
echo "    File name: install-package.sh"
wget -q --no-cache $INSTALL_PACKAGE_URL || exit 1

echo ""
echo "  Downloading transform-service-template.sh script..."
TRANSFORM_SERVICE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-installation/transform-service-template.sh"
echo "    URL: $TRANSFORM_SERVICE_URL"
echo "    File name: transform-service-template.sh"
wget -q --no-cache $TRANSFORM_SERVICE_URL || exit 1

echo ""
echo "  Downloading systemctl.sh script..."
SYSTEMCTL_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/systemctl.sh"
echo "    URL: $SYSTEMCTL_URL"
echo "    File name: systemctl.sh"
wget -q --no-cache $SYSTEMCTL_URL || exit 1

echo ""
echo "  Downloading service file..."
SERVICE_FILE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/svc/$SERVICE_FILE_NAME.template"
SERVICE_FILE_NAME="arduino-plug-and-play.template"
echo "    URL: $SERVICE_FILE_URL"
echo "    File name: $SERVICE_FILE_NAME"
wget -q --no-cache $SERVICE_FILE_URL -O $SERVICE_FILE_NAME || exit 1

echo ""
echo "Starting init.sh script..."
bash init.sh "$BRANCH" "$SMTP_SERVER" "$ADMIN_EMAIL"

echo ""
echo "Starting transform-service-file.sh script..."
bash transform-service-file.sh "$BRANCH" "$DESTINATION"

echo "Finished setting up plug and play"
