BRANCH=$1

if [ ! $BRANCH ]; then
  BRANCH="master"
fi

echo "Launching arduino plug and play..."

mono ArduinoPlugAndPlay/lib/net40/ArduinoPlugAndPlay.exe $1 $2 $3 $4 $5 $6 $7 $8 $9
