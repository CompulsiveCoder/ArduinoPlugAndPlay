echo "Installing package...."

PACKAGE_NAME=$1
PACKAGE_VERSION=$2

if [ ! "$PACKAGE_NAME" ]; then
	echo "Please provide a package name as an argument."
	exit 1
fi

if [ ! "$PACKAGE_VERSION" ]; then
	echo "Please provide a package version as an argument."
	exit 1
fi

echo "  Name: $PACKAGE_NAME"
echo "  Version: $PACKAGE_VERSION"

PACKAGE_FILE="$PACKAGE_NAME.$PACKAGE_VERSION"
PACKAGE_FILE_EXT="$PACKAGE_NAME.$PACKAGE_VERSION.nupkg"
# TODO: Remove the reference to GreenSense
PACKAGE_URL="https://github.com/GreenSense/libs/raw/master/$PACKAGE_FILE.nupkg"

if [ ! -f "$PACKAGE_FILE_EXT" ]; then
  echo ""
  echo "  Downloading package..."
  echo "    $PACKAGE_URL"
	curl -s -LO -H 'Cache-Control: no-cache' -f $PACKAGE_URL -o $PACKAGE_FILE_EXT || exit 1

  echo ""
  echo "  Unzipping package..."
	unzip -q -o "$PACKAGE_FILE_EXT" -d "$PACKAGE_NAME/" || exit 1
else
	echo "  Already exists. Skipping download."
fi

echo ""
echo "Finished installing package"
