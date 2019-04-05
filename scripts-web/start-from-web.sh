BRANCH=$1

if [ ! $BRANCH ]; then
  BRANCH="master"
fi


echo "Reinitializing plug and play (by downloading init-from-web.sh file)..."

wget -v --no-cache -O init-from-web.sh https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-web/init-from-web.sh || echo "Failed to download init-from-web.sh script"

bash init-from-web.sh $BRANCH || (echo "Failed to initialize plug and play. Script: init-from-web.sh" && exit 1)

echo "Launching arduino plug and play."

mono ArduinoPlugAndPlay/lib/net40/ArduinoPlugAndPlay.exe $1 $2 $3 $4 $5 $6 $7 $8 $9
