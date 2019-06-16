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
echo "Making install dir..."
mkdir -p $INSTALL_DIR || exit 1

echo ""
echo "Moving to install dir..."
cd $INSTALL_DIR || exit 1

echo ""
echo "Downloading init.sh file..."

wget -q --no-cache -O - https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-installation/init.sh || exit 1

echo ""
echo "Downloading install-service.sh..."

wget -q --no-cache -O - https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-installation/install-service.sh || exit 1

echo ""
echo "Downloading install-package.sh..."

wget -q --no-cache -O - https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-installation/install-package.sh || exit 1

echo ""
echo "Starting init.sh script..."
bash init.sh "$BRANCH" "$SMTP_SERVER" "$ADMIN_EMAIL"

echo ""
echo "Starting install-service.sh script..."
bash install-service.sh "$BRANCH"

echo "Finished setting up plug and play"
