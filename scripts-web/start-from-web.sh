
echo "Reinitializing plug and play (by downloading init-from-web.sh file)..."

wget -v --no-cache -O - https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/scripts-web/init-from-web.sh | bash -s $BRANCH || (echo "Failed to initialize plug and play. Script: init-from-web.sh" && exit 1)

echo "Launching arduino plug and play."

mono ArduinoPlugAndPlay/lib/net40/ArduinoPlugAndPlay.exe $1 $2 $3 $4 $5 $6 $7 $8 $9
