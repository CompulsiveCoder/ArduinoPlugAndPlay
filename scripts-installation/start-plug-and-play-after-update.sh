BRANCH=$1

if [ ! $BRANCH ]; then
  BRANCH="master"
fi

echo "Starting arduino plug and play after update..."

echo ""
echo "  Downloading init.sh script..."
curl --connect-timeout 3 -o init.sh -f https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-installation/init.sh || echo "Failed to download init.sh script"

echo ""
echo "  Running init.sh script..."
bash init.sh $BRANCH || exit 1

bash start-plug-and-play.sh
