echo "Installing plug and play with auto updates..."

BRANCH=$1
DESTINATION=$2

SMTP_SERVER=$3
ADMIN_EMAIL=$4

SERVICE_TEMPLATE_FILE_NAME=$5

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

if [ ! $SMTP_SERVER ]; then
  SMTP_SERVER="na"
fi

if [ ! $ADMIN_EMAIL ]; then
  ADMIN_EMAIL="na"
fi

echo ""
echo "  Downloading install.sh script..."
INSTALL_FILE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-ols/install.sh"
echo "    URL: $INSTALL_FILE_URL"
echo "    File name: install.sh"
wget -q --no-cache $INSTALL_FILE_URL || exit 1

SERVICE_TEMPLATE_FILE_NAME="arduino-plug-and-play-auto-update.service"

echo ""
echo "  Running install.sh script..."
bash install.sh $BRANCH $DESTINATION $SMTP_SERVER $ADMIN_EMAIL $SERVICE_TEMPLATE_FILE_NAME

echo ""
echo "Finished installing arduino plug and play with auto updates"

