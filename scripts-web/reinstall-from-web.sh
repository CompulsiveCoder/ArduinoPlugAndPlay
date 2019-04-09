echo "Reinstalling plug and play from the web..."

BRANCH=$1
DESTINATION=$2

SMTP_SERVER=$3
ADMIN_EMAIL=$4

EXAMPLE_COMMAND="Example:\n..sh [Branch] [Install_Dir] [SmtpServer] [AdminEmail]"

if [ ! $BRANCH ]; then
  BRANCH="master"
fi

if [ ! $DESTINATION ]; then
  DESTINATION="/usr/local/ArduinoPlugAndPlay"
fi

INSTALL_DIR=$DESTINATION

if [ ! -d $INSTALL_DIR ]; then
  echo "ArduinoPlugAndPlay not found. Can't reinstall."
  exit 1
fi

#echo "Making install dir..."
#mkdir -p $INSTALL_DIR || exit 1

echo "Moving to install dir..."
cd $INSTALL_DIR

echo "Branch: $BRANCH"
echo "Destination: $DESTINATION"

echo "SMTP server: $SMTP_SERVER"
echo "Admin email: $ADMIN_EMAIL"

echo "Uninstalling plug and play (by downloading uninstall-from-web.sh file)..."

wget -q --no-cache -O - https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-web/uninstall-from-web.sh | bash -s -- "$BRANCH" "$DESTINATION"

echo "Installing plug and play (by downloading install-from-web.sh file)..."

wget -q --no-cache -O - https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-web/install-from-web.sh | bash -s -- "$BRANCH" "$DESTINATION" "$SMTP_SERVER" "$ADMIN_EMAIL"

echo "Finished reinstalling plug and play"
