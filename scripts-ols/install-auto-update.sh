echo "Installing plug and play with auto updates..."

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

SERVICE_TEMPLATE_FILE_NAME="arduino-plug-and-play-auto-update.service.template"

echo "  Branch: $BRANCH"
echo "  Destination: $DESTINATION"

echo "  SMTP server: $SMTP_SERVER"
echo "  Admin email: $ADMIN_EMAIL"

echo "  Service template file: $SERVICE_TEMPLATE_FILE_NAME"

echo ""
echo "  Downloading install.sh script..."
INSTALL_FILE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-ols/install.sh"
echo "    URL: $INSTALL_FILE_URL"
echo "    File name: install.sh"
wget -nv --no-cache $INSTALL_FILE_URL || exit 1

echo ""
echo "  Running install.sh script..."
echo "    bash install.sh \"$BRANCH\" \"$DESTINATION\" \"$SMTP_SERVER\" \"$ADMIN_EMAIL\" \"$SMTP_USERNAME\" \"$SMTP_PASSWORD\" \"$SMTP_PORT\" \"$SERVICE_TEMPLATE_FILE_NAME\""
echo ""
bash install.sh "$BRANCH" "$DESTINATION" "$SMTP_SERVER" "$ADMIN_EMAIL" "$SMTP_USERNAME" "$SMTP_PASSWORD" "$SMTP_PORT" "$SERVICE_TEMPLATE_FILE_NAME"

echo ""
echo "  Moving to destination..."
cd $DESTINATION

echo "1" > "enable-auto-update.txt"

echo ""
echo "Finished installing arduino plug and play with auto updates"

