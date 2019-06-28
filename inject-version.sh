VERSION_ARGUMENT=$1

if [ ! $VERSION_ARGUMENT ]; then
  VERSION=$(cat version.txt)
  BUILD=$(cat buildnumber.txt)

  FULL_VERSION="$VERSION.$BUILD"
else
  FULL_VERSION=$VERSION_ARGUMENT
fi

echo "Injecting version into files..."
echo "  Version: $FULL_VERSION"

echo ""
echo "  AssemblyInfo.cs files..."

sed -i -e "s/1\.0\.0\.[.0-9]*/$FULL_VERSION/g" "src/ArduinoPlugAndPlay/Properties/AssemblyInfo.cs" || exit 1
sed -i -e "s/1\.0\.0\.[.0-9]*/$FULL_VERSION/g" "src/ArduinoPlugAndPlay.ClientConsole/Properties/AssemblyInfo.cs" || exit 1
sed -i -e "s/1\.0\.0\.[.0-9]*/$FULL_VERSION/g" "src/ArduinoPlugAndPlay.Tests/Properties/AssemblyInfo.cs" || exit 1
sed -i -e "s/1\.0\.0\.[.0-9]*/$FULL_VERSION/g" "src/ArduinoPlugAndPlay.Tests.Integration/Properties/AssemblyInfo.cs" || exit 1
sed -i -e "s/1\.0\.0\.[.0-9]*/$FULL_VERSION/g" "src/ArduinoPlugAndPlay.Tests.Unit/Properties/AssemblyInfo.cs" || exit 1
sed -i -e "s/1\.0\.0\.[.0-9]*/$FULL_VERSION/g" "src/ArduinoPlugAndPlay.Tests.Scripts/Properties/AssemblyInfo.cs" || exit 1
sed -i -e "s/1\.0\.0\.[.0-9]*/$FULL_VERSION/g" "src/ArduinoPlugAndPlay.Tests.Scripts.Install/Properties/AssemblyInfo.cs" || exit 1
sed -i -e "s/1\.0\.0\.[.0-9]*/$FULL_VERSION/g" "src/ArduinoPlugAndPlay.Tests.Scripts.Install.OLS/Properties/AssemblyInfo.cs" || exit 1

echo ""

echo "Finished injecting version"
