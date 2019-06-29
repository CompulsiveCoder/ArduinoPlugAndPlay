echo "Packaging release zip file..."

DIR=$PWD

TMP_RELEASE_FOLDER=".tmp/ArduinoPlugAndPlay/lib/net40/"
BIN_RELEASE_FOLDER="bin/Release"
RELEASES_FOLDER="releases"

BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')

VERSION_POSTFIX=""

if [ "$BRANCH" != "lts" ]; then
  VERSION_POSTFIX="-$BRANCH"
fi

VERSION="$(cat version.txt).$(cat buildnumber.txt)"

mkdir -p $TMP_RELEASE_FOLDER

cp $BIN_RELEASE_FOLDER/ArduinoPlugAndPlay.dll $TMP_RELEASE_FOLDER/
cp $BIN_RELEASE_FOLDER/ArduinoPlugAndPlay.dll.mdb $TMP_RELEASE_FOLDER/
cp $BIN_RELEASE_FOLDER/ArduinoPlugAndPlay.exe.config $TMP_RELEASE_FOLDER/
cp $BIN_RELEASE_FOLDER/ArduinoPlugAndPlay.exe $TMP_RELEASE_FOLDER/
cp $BIN_RELEASE_FOLDER/ArduinoPlugAndPlay.exe.mdb $TMP_RELEASE_FOLDER/
cp $BIN_RELEASE_FOLDER/ArduinoPlugAndPlay.exe.config $TMP_RELEASE_FOLDER/
cp $BIN_RELEASE_FOLDER/ArduinoPlugAndPlay.Tests.dll $TMP_RELEASE_FOLDER/
cp $BIN_RELEASE_FOLDER/ArduinoPlugAndPlay.Tests.dll.mdb $TMP_RELEASE_FOLDER/
cp $BIN_RELEASE_FOLDER/ArduinoPlugAndPlay.Tests.Integration.dll $TMP_RELEASE_FOLDER/
cp $BIN_RELEASE_FOLDER/ArduinoPlugAndPlay.Tests.Integration.dll.mdb $TMP_RELEASE_FOLDER/
cp $BIN_RELEASE_FOLDER/ArduinoPlugAndPlay.Tests.Unit.dll $TMP_RELEASE_FOLDER/
cp $BIN_RELEASE_FOLDER/ArduinoPlugAndPlay.Tests.Unit.dll.mdb $TMP_RELEASE_FOLDER/
cp $BIN_RELEASE_FOLDER/duinocom.core.dll $TMP_RELEASE_FOLDER/
cp $BIN_RELEASE_FOLDER/nunit.framework.dll $TMP_RELEASE_FOLDER/

mkdir -p $RELEASES_FOLDER

cd .tmp/ArduinoPlugAndPlay

zip -r $DIR/releases/ArduinoPlugAndPlay.$VERSION$VERSION_POSTFIX.zip *

cd $DIR

rm .tmp -r

echo "Finished packaging release zip file."
