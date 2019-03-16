VERSION=$(cat version.txt)
BUILD=$(cat buildnumber.txt)

FULL_VERSION="$VERSION.$BUILD"

echo "Injecting version into AssemblyInfo.cs file..."
echo "Version: $FULL_VERSION"

ASSEMBLY_INFO_FILE="src/ArduinoPlugAndPlay/Properties/AssemblyInfo.cs"

sed -i -e "s/1.0.\*/$FULL_VERSION/g" $ASSEMBLY_INFO_FILE

echo "Finished injecting version"