echo "Installing plug and play from the web..."

BRANCH=$1
DESTINATION=$2

SMTP_SERVER=$1
ADMIN_EMAIL=$2

EXAMPLE_COMMAND="Example:\n..sh [Branch] [Install_Dir] [SmtpServer] [AdminEmail]"

if [ ! $BRANCH ]; then
  BRANCH="master"
fi

if [ ! $DESTINATION ]; then
  DESTINATION="/usr/local/ArduinoPlugAndPlay"
fi

echo "Branch: $BRANCH"
echo "Destination: $DESTINATION"

echo "SMTP server: $SMTP_SERVER"
echo "Admin email: $ADMIN_EMAIL"

INSTALL_DIR=$DESTINATION

echo "Making install dir..."
mkdir -p $INSTALL_DIR || exit 1

echo "Moving to install dir..."
cd $INSTALL_DIR

echo "Initializing plug and play (by downloading init-from-web.sh file)..."

wget -v --no-cache -O - https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-web/init-from-web.sh | bash -s -- "$BRANCH" "$SMTP_SERVER" "$ADMIN_EMAIL" || (echo "Failed to initialize plug and play. Script: init-from-web.sh" && exit 1)

echo "Initializing plug and play (by downloading install-service-from-web.sh file)..."

wget -v --no-cache -O - https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-web/install-service-from-web.sh | bash -s -- "$BRANCH" || (echo "Failed to install plug and play service. Script: install-service-from-web.sh" && exit 1)

echo "Finished setting up plug and play"
