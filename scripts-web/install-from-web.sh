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

#[ -f "is-mock-install.txt" ] && IS_MOCK_INSTALL=1



INSTALL_DIR=$DESTINATION

#if [ $IS_MOCK_INSTALL == 1 ]; then
#  INSTALL_DIR="mock/ArduinoPlugAndPlay"
#fi

#echo "Branch name: $BRANCH"

echo "Making install dir..."
mkdir -p $INSTALL_DIR || exit 1

echo "Moving to install dir..."
cd $INSTALL_DIR

echo "Initializing plug and play (by downloading init.sh file)..."

wget -v --no-cache -O - https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/web/init-from-web.sh | sudo sh || exit 1

echo "Finished setting up plug and play"
