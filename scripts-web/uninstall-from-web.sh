echo "Uninstalling plug and play..."

BRANCH=$1
INSTALL_DIR=$2

EXAMPLE_COMMAND="Example:\n..sh [Branch] [InstallDir]"

if [ ! $BRANCH ]; then
  BRANCH="master"
fi

if [ ! $INSTALL_DIR ]; then
  INSTALL_DIR="/usr/local/ArduinoPlugAndPlay"
fi

echo "Destination: $INSTALL_DIR"

if [ ! -d $INSTALL_DIR ]; then
  echo "ArduinoPlugAndPlay doesn't appear to be installed at:"
  echo "  $INSTALL_DIR"
  echo "Aborting uninstall."
fi

echo "Moving to install dir..."
cd $INSTALL_DIR


SERVICES_DIRECTORY="/lib/systemd/system"

if [ -f "is-mock-systemctl.txt" ]; then
  SERVICES_DIRECTORY="$INSTALL_DIR/mock/services"
fi

echo "Stopping plug and play service..."
sh systemctl.sh stop arduino-plug-and-play.service || (echo "Failed to stop ArduinoPlugAndPlay service: arduino-plug-and-play.service" && exit 1) 

echo "Disabling plug and play service..."
sh systemctl.sh disable arduino-plug-and-play.service || (echo "Failed to disable ArduinoPlugAndPlay service: arduino-plug-and-play.service" && exit 1) 

if [ -f $SERVICES_DIRECTORY/arduino-plug-and-play.service ]; then
  echo "Removing the plug and play service file..."
  rm $SERVICES_DIRECTORY/arduino-plug-and-play.service || (echo "Failed to remove ArduinoPlugAndPlay service file: arduino-plug-and-play.service" && exit 1)
else
  echo "Skipping removal of service file because it's not found: $SERVICES_DIRECTORY/arduino-plug-and-play.service" 
fi

echo "Removing the ArduinoPlugAndPlay install directory..."
rm $INSTALL_DIR -R || (echo "Failed to remove ArduinoPlugAndPlay install directory" && exit 1)

echo "Finished uninstalling plug and play"
