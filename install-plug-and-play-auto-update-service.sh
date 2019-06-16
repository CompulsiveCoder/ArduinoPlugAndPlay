echo "Installing plug and play service file..."

BRANCH=$1
INSTALL_DIR=$2

if [ ! $BRANCH ]; then
  BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')
fi

if [ ! $INSTALL_DIR ]; then
  INSTALL_DIR="/usr/local/ArduinoPlugAndPlay"
fi

SERVICE_TEMPLATE_FILE="arduino-plug-and-play.service.template"
SERVICE_FILE="arduino-plug-and-play.service"
SERVICE_DIR="svc-auto-update"
SERVICE_FILE_PATH="$SERVICE_DIR/$SERVICE_FILE"
SERVICE_TEMPLATE_FILE_PATH="$SERVICE_DIR/$SERVICE_TEMPLATE_FILE"

echo "Template file:"
echo "$SERVICE_TEMPLATE_FILE_PATH"
echo "Service file:"
echo "$SERVICE_FILE_PATH"

sh transform-service-template.sh $BRANCH $INSTALL_DIR $SERVICE_TEMPLATE_FILE_PATH

echo "Service file:"
echo "$SERVICE_FILE_PATH"

cat "$SERVICE_FILE_PATH"

echo "Installing service..."
sh install-service.sh $SERVICE_FILE_PATH || exit 1

echo "Finished creating arduino plug and play service"
