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

echo "Installing package $PACKAGE_NAME $PACKAGE_VERSION..."

PACKAGE_FILE="$PACKAGE_NAME.$PACKAGE_VERSION"
PACKAGE_FILE_EXT="$PACKAGE_NAME.$PACKAGE_VERSION.nupkg"

if [ ! -f "$PACKAGE_FILE_EXT" ]; then
	wget -q "https://github.com/CompulsiveCoder/libs/raw/master/$PACKAGE_FILE.nupkg" -O $PACKAGE_FILE_EXT || (echo "Failed to download $PACKAGE_NAME library package." && exit 1)

	unzip -qq -o "$PACKAGE_FILE_EXT" -d "$PACKAGE_NAME/" || (echo "Failed to unzip package" && exit 1)
else
	echo "  Already exists. Skipping download."
fi
