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

echo "Package name: $PACKAGE_NAME"
echo "Package version: $PACKAGE_VERSION"

PACKAGE_FILE="$PACKAGE_NAME.$PACKAGE_VERSION"
PACKAGE_FILE_EXT="$PACKAGE_NAME.$PACKAGE_VERSION.nupkg"

echo "Package file: $PACKAGE_FILE"

# TODO: Move all packages to a central repository outside the GreenSense project
wget -q "https://github.com/GreenSense/libs/raw/master/$PACKAGE_FILE.nupkg" -O $PACKAGE_FILE_EXT || (echo "Failed to download package." && exit 1)

unzip -q -o "$PACKAGE_FILE_EXT" -d "$PACKAGE_FILE/" || (echo "Failed to unzip package." && exit 1)
