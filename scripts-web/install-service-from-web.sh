echo "Installing plug and play service..."

echo "Current directory:"
echo "  $PWD"

BRANCH=$1

EXAMPLE_COMMAND="Example:\n..sh [branch]"

if [ ! $BRANCH ]; then
  BRANCH="master"
fi

echo "$BRANCH"

#SERVICE_FILE_PATH=$1
#SERVICE_FILE=$(basename -- "$SERVICE_FILE_PATH")

SYSTEMCTL_FILE_NAME="systemctl.sh"
SYSTEMCTL_FILE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/$SYSTEMCTL_FILE_NAME"

echo "Downloading systemctl file..."
echo "URL: $SYSTEMCTL_FILE_URL"
echo "File name: $SYSTEMCTL_FILE_NAME"
wget $SYSTEMCTL_FILE_URL -O $SYSTEMCTL_FILE_NAME || (echo "Failed to download $SYSTEMCTL_FILE_NAME." && exit 1)

SERVICE_FILE_NAME="arduino-plug-and-play.service"
SERVICE_FILE_URL="https://raw.githubusercontent.com/CompulsiveCoder/ArduinoPlugAndPlay/$BRANCH/svc/$SERVICE_FILE_NAME"

echo "Downloading service file..."
echo "URL: $SERVICE_FILE_URL"
echo "File name: $SERVICE_FILE_NAME"
wget $SERVICE_FILE_URL -O $SERVICE_FILE_NAME || (echo "Failed to download $SERVICE_FILE_NAME." && exit 1)

echo "Installing service"
#echo "Path: $SERVICE_FILE_PATH"
#echo "Name: $SERVICE_FILE"

#SYSTEMCTL_SCRIPT="systemctl.sh"

#MOCK_SYSTEMCTL_FLAG_FILE="is-mock-systemctl.txt"

#IS_MOCK_SYSTEMCTL=0

#if [ -f "$MOCK_SYSTEMCTL_FLAG_FILE" ]; then
#  IS_MOCK_SYSTEMCTL=1
#  echo "Is mock systemctl"
#fi

#SERVICES_DIR="/lib/systemd/system"

#if [ $IS_MOCK_SYSTEMCTL = 1 ]; then
#  SERVICES_DIR="mock/services"
#fi

#SUDO=""
#if [ ! "$(id -u)" -eq 0 ]; then
#    SUDO='sudo'
#fi

#mkdir -p $SERVICES_DIR

#echo "Services directory:"
#echo "  $SERVICES_DIR"
#echo "Destination file:"
#echo "  $SERVICES_DIR/$SERVICE_FILE"

#if [ $IS_MOCK_SYSTEMCTL = 1 ]; then
#  echo "Is mock systemctl. Installing to mock directory."
#  cp $SERVICE_FILE_PATH $SERVICES_DIR/$SERVICE_FILE
#else
#  $SUDO cp -fv $SERVICE_FILE_PATH $SERVICES_DIR/$SERVICE_FILE && \
#  $SUDO chmod 644 $SERVICES_DIR/$SERVICE_FILE && \
#  sh $SYSTEMCTL_SCRIPT daemon-reload && \
#  sh $SYSTEMCTL_SCRIPT enable $SERVICE_FILE && \
#  sh $SYSTEMCTL_SCRIPT start $SERVICE_FILE && \
#  sh $SYSTEMCTL_SCRIPT restart $SERVICE_FILE
#fi

echo "Finished installing service"
