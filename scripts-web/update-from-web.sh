echo "Updating plug and play..."

BRANCH=$1
DESTINATION=$2

EXAMPLE_COMMAND="Example:\n..sh [branch] [destination]"

if [ ! $BRANCH ]; then
  BRANCH="master"
fi

if [ ! $DESTINATION ]; then
  DESTINATION="/usr/local/ArduinoPlugAndPlay"
fi

echo "Branch: $BRANCH"
echo "Destination: $DESTINATION"

INSTALL_DIR=$DESTINATION

echo "Branch name: $BRANCH"

echo "Checking for install dir..."
if [ ! -d $INSTALL_DIR ]; then
  echo "ArduinoPlugAndPlay doesn't appear to be installed at:"
  echo "  $INSTALL_DIR"
  echo "Use the install-from-web-sh script instead."
  exit 1
fi

echo "Moving to install dir..."
cd $INSTALL_DIR


echo "Stopping plug and play service..."
sh systemctl.sh stop arduino-plug-and-play.service || (echo "Failed to stop ArduinoPlugAndPlay service: arduino-plug-and-play.service" && exit 1) 

echo "Removing old scripts..."
rm *.sh || (echo "Failed to remove old scripts." && exit 1)

echo "Removing old libraries..."
rm ArduinoPlugAndPlay -R && \
rm ArduinoPlugAndPlay*.nupkg || (echo "Failed to remove old ArduinoPlugAndPlay libraries." && exit 1)

echo "Reinitializing plug and play (by downloading init-from-web.sh file)..."

wget -v --no-cache -O - https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-web/init-from-web.sh | bash -s - $BRANCH || (echo "Failed to initialize plug and play. Script: init-from-web.sh" && exit 1)

echo "Reinstalling the service file (by downloading install-service-from-web.sh file)..."

wget -v --no-cache -O - https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-web/install-service-from-web.sh | bash -s - $BRANCH || (echo "Failed to install plug and play service. Script: install-service-from-web.sh" && exit 1)

echo "Restarting service..."
sh systemctl.sh restart arduino-plug-and-play.service || (echo "Failed to restart ArduinoPlugAndPlay service: arduino-plug-and-play.service" && exit 1) 

echo "Finished setting up plug and play"
