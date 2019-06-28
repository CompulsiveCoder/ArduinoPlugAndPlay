VERSION_ARGUMENT=$1

if [ ! $VERSION_ARGUMENT ]; then
  VERSION=$(cat version.txt)
  BUILD=$(cat buildnumber.txt)

  FULL_VERSION="$VERSION.$BUILD"
else
  FULL_VERSION=$VERSION_ARGUMENT
fi

echo "Updating version in scripts-installation/init.sh script..."
echo "  Version: $FULL_VERSION"

INIT_SCRIPT="scripts-installation/init.sh"
echo "  $INIT_SCRIPT"

sed -i "s/ArduinoPlugAndPlay .* |/ArduinoPlugAndPlay $FULL_VERSION |/" $INIT_SCRIPT || exit 1

echo ""

echo "Finished updating version"
