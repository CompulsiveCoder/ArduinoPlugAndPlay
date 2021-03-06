echo "Installing plug and play from the web..."

BRANCH=$1
DESTINATION=$2

SMTP_SERVER=$3
ADMIN_EMAIL=$4
SMTP_USERNAME=$5
SMTP_PASSWORD=$6
SMTP_PORT=$7

SERVICE_TEMPLATE_FILE_NAME=$8

EXAMPLE_COMMAND="Example:\n..sh [Branch] [Install_Dir] [SmtpServer] [AdminEmail] [SmtpUsername] [SmtpPassword] [SmtpPort] [ServiceTemplateName]"

if [ ! $BRANCH ]; then
  BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')
fi

if [ ! $BRANCH ]; then
  BRANCH="master"
fi

if [ ! $DESTINATION ]; then
  DESTINATION="/usr/local/ArduinoPlugAndPlay"
fi

if [ ! $SMTP_SERVER ]; then
  SMTP_SERVER="na"
fi

if [ ! $ADMIN_EMAIL ]; then
  ADMIN_EMAIL="na"
fi

if [ ! $SMTP_USERNAME ]; then
  SMTP_USERNAME="na"
fi

if [ ! $SMTP_PASSWORD ]; then
  SMTP_PASSWORD="na"
fi

if [ ! $SMTP_PORT ]; then
  SMTP_PORT="25"
fi

SERVICE_FILE_NAME="arduino-plug-and-play.service"

if [ ! $SERVICE_TEMPLATE_FILE_NAME ]; then
  SERVICE_TEMPLATE_FILE_NAME="$SERVICE_FILE_NAME.template"
fi

echo "  Branch: $BRANCH"
echo "  Destination: $DESTINATION"

echo "  SMTP server: $SMTP_SERVER"
echo "  Admin email: $ADMIN_EMAIL"
echo "  SMTP username: $SMTP_USERNAME"
echo "  SMTP password: [hidden]"
echo "  SMTP port: $SMTP_PORT"

echo "  Service template file: $SERVICE_TEMPLATE_FILE_NAME"

INSTALL_DIR=$DESTINATION

echo ""
echo "  Making install dir..."
echo "    $INSTALL_DIR"
mkdir -p $INSTALL_DIR || exit 1

echo ""
echo "  Moving to install dir..."
echo "    $INSTALL_DIR"
cd $INSTALL_DIR || exit 1

echo ""
echo "  Downloading init.sh script..."
INIT_FILE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-installation/init.sh"
echo "    URL: $INIT_FILE_URL"
echo "    File name: init.sh"
curl -s -LO -H 'Cache-Control: no-cache' -f $INIT_FILE_URL || exit 1

echo ""
echo "  Downloading install-service.sh script..."
INSTALL_SERVICE_FILE="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/install-service.sh"
echo "    URL: $INSTALL_SERVICE_FILE"
echo "    File name: install-service.sh"
curl -s -LO -H 'Cache-Control: no-cache' -f $INSTALL_SERVICE_FILE || exit 1

echo ""
echo "  Downloading install-package-from-github-release.sh script..."
INSTALL_PACKAGE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-installation/install-package-from-github-release.sh"
echo "    URL: $INSTALL_PACKAGE_URL"
echo "    File name: install-package.sh"
curl -s -LO -H 'Cache-Control: no-cache' -f $INSTALL_PACKAGE_URL || exit 1

START_SCRIPT_NAME="start-plug-and-play.sh"

if [ "$SERVICE_TEMPLATE_FILE_NAME" = "arduino-plug-and-play-auto-update.service.template" ]; then
  START_SCRIPT_NAME="start-plug-and-play-after-update.sh"
fi

echo ""
echo "  Downloading $START_SCRIPT_NAME script..."
START_SCRIPT_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-installation/$START_SCRIPT_NAME"
echo "    URL: $START_SCRIPT_URL"
echo "    File name: $START_SCRIPT_NAME"
curl -s -LO -H 'Cache-Control: no-cache' -f $START_SCRIPT_URL || exit 1

echo ""
echo "  Downloading transform-service-template.sh script..."
TRANSFORM_SERVICE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/transform-service-template.sh"
echo "    URL: $TRANSFORM_SERVICE_URL"
echo "    File name: transform-service-template.sh"
wget -nv --no-cache $TRANSFORM_SERVICE_URL || exit 1

echo ""
echo "  Downloading systemctl.sh script..."
SYSTEMCTL_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/systemctl.sh"
echo "    URL: $SYSTEMCTL_URL"
echo "    File name: systemctl.sh"
curl -s -LO -H 'Cache-Control: no-cache' -f $SYSTEMCTL_URL || exit 1

echo ""
echo "  Downloading service template file..."
SERVICE_TEMPLATE_FILE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/svc/$SERVICE_TEMPLATE_FILE_NAME"
echo "    URL: $SERVICE_TEMPLATE_FILE_URL"
echo "    File name: $SERVICE_TEMPLATE_FILE_NAME"
curl -s -L -H 'Cache-Control: no-cache' -f $SERVICE_TEMPLATE_FILE_URL -o $SERVICE_TEMPLATE_FILE_NAME || exit 1


echo ""
echo "  Starting init.sh script..."
bash init.sh "$BRANCH" "$SMTP_SERVER" "$ADMIN_EMAIL" "$SMTP_USERNAME" "$SMTP_PASSWORD" "$SMTP_PORT" || exit 1

echo ""
echo "  Starting transform-service-template.sh script..."
bash transform-service-template.sh "$BRANCH" "$DESTINATION" "$SERVICE_TEMPLATE_FILE_NAME" "$SERVICE_FILE_NAME" || exit 1

echo ""
echo "  Starting install-service.sh script..."
bash install-service.sh $SERVICE_FILE_NAME || exit 1

echo ""
echo "  Saving email details to file..."
echo $SMTP_SERVER > "smtp-server.txt"
echo $ADMIN_EMAIL > "admin-email.txt"

echo ""
echo "  Saving version details to file..."
BUILD_NUMBER=$(curl -s -H 'Cache-Control: no-cache' "https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/buildnumber.txt")
VERSION_NUMBER=$(curl -s -H 'Cache-Control: no-cache' "https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/version.txt")

VERSION="$VERSION_NUMBER.$BUILD_NUMBER"

echo "    Version: $VERSION"
echo $VERSION > "version.txt"

echo ""
echo "Finished setting up plug and play"
