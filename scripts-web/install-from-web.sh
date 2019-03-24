echo "Installing plug and play..."

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

echo "Making install dir..."
mkdir -p $INSTALL_DIR || exit 1

echo "Moving to install dir..."
cd $INSTALL_DIR

echo "Initializing plug and play (by downloading init-from-web.sh file)..."

wget -v --no-cache -O - https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-web/init-from-web.sh | bash -s - $BRANCH || (echo "Failed to initialize plug and play. Script: init-from-web.sh" && exit 1)

echo "Initializing plug and play (by downloading install-service-from-web.sh file)..."

wget -v --no-cache -O - https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-web/install-service-from-web.sh | bash -s - $BRANCH || (echo "Failed to install plug and play service. Script: install-service-from-web.sh" && exit 1)

echo "Finished setting up plug and play"
