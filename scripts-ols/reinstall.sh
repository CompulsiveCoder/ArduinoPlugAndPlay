echo "Reinstalling plug and play..."

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

SERVICE_FILE_NAME="arduino-plug-and-play.service"

if [ ! $SERVICE_TEMPLATE_FILE_NAME ]; then
  SERVICE_TEMPLATE_FILE_NAME="$SERVICE_FILE_NAME.template"
fi

echo "  Branch: $BRANCH"
echo "  Destination: $DESTINATION"

echo "  Service template file: $SERVICE_TEMPLATE_FILE_NAME"

INSTALL_DIR=$DESTINATION

echo "  Checking for install dir..."
if [ ! -d $INSTALL_DIR ]; then
  echo "    ArduinoPlugAndPlay doesn't appear to be installed at:"
  echo "      $INSTALL_DIR"
  echo "    Use the scripts-ols/install.sh script to install it."
  exit 1
fi

if [ ! $SMTP_SERVER ]; then
  SMTP_SERVER="$(cat $DESTINATION/smtp-server.txt)"
fi

if [ ! $ADMIN_EMAIL ]; then
  ADMIN_EMAIL="$(cat $DESTINATION/admin-email.txt)"
fi

echo "  SMTP server: $SMTP_SERVER"
echo "  Admin email: $ADMIN_EMAIL"


INSTALL_SCRIPT_FILE_NAME="install.sh"

echo ""
echo "  Getting auto update setting..."
AUTO_UPDATE_SETTING_FILE="enable-auto-update.txt"
if [ -f $AUTO_UPDATE_SETTING_FILE ]; then
  ENABLE_AUTO_UPDATE="$(cat $AUTO_UPDATE_SETTING_FILE)"
else
  ENABLE_AUTO_UPDATE=0
fi

echo "  Enable auto update: $ENABLE_AUTO_UPDATE"

if [ $ENABLE_AUTO_UPDATES = "1" ]; then
  INSTALL_SCRIPT_FILE_NAME="install-auto-update.sh"
fi  

echo ""
echo "  Downloading and running uninstall script..."
wget -q --no-cache -O - https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-ols/uninstall.sh | bash -s -- $BRANCH $INSTALL_DIR || exit 1

echo ""
echo "  Downloading and running install script..."
wget -q --no-cache -O - https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-ols/$INSTALL_SCRIPT_FILE_NAME | bash -s -- $BRANCH $INSTALL_DIR $SMTP_SERVER $ADMIN_EMAIL || exit 1




echo ""
echo "Finished reinstalling plug and play"
