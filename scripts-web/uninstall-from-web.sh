echo "Uninstalling plug and play..."

DESTINATION=$1

EXAMPLE_COMMAND="Example:\n..sh [branch] [destination]"

if [ ! $DESTINATION ]; then
  DESTINATION="/usr/local/ArduinoPlugAndPlay"
fi

echo "Destination: $DESTINATION"

INSTALL_DIR=$DESTINATION

if [ ! -d $INSTALL_DIR ]; then
  echo "ArduinoPlugAndPlay doesn't appear to be installed at:"
  echo "  $INSTALL_DIR"
  echo "Aborting uninstall."
  exit 1
fi

echo "Moving to install dir..."
cd $INSTALL_DIR


echo "Stopping plug and play service..."
sh systemctl.sh stop arduino-plug-and-play.service || (echo "Failed to stop ArduinoPlugAndPlay service: arduino-plug-and-play.service" && exit 1) 
echo "Disabling plug and play service..."
sh systemctl.sh disable arduino-plug-and-play.service || (echo "Failed to disable ArduinoPlugAndPlay service: arduino-plug-and-play.service" && exit 1) 

rm /lib/systemd/system/arduino-plug-and-play.service  || (echo "Failed to remove ArduinoPlugAndPlay service file: arduino-plug-and-play.service" && exit 1) 

rm * -R

echo "Finished uninstalling plug and play"
