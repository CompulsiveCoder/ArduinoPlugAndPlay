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
SERVICE_PATH="svc-auto-update"
SERVICE_FILE_PATH="$SERVICE_PATH/$SERVICE_FILE"
SERVICE_TEMPLATE_FILE_PATH="$SERVICE_PATH/$SERVICE_TEMPLATE_FILE"

echo "Template file:"
echo "$SERVICE_TEMPLATE_FILE_PATH"
echo "Service file:"
echo "$SERVICE_FILE_PATH"

echo "Copying service file..."

cp $SERVICE_TEMPLATE_FILE_PATH $SERVICE_FILE_PATH || exit 1

echo "Injecting values into template service file..."

ESCAPED_INSTALL_DIR="${INSTALL_DIR//\//\\/}"

sed -i -e "s/{INSTALL_PATH}/$ESCAPED_INSTALL_DIR/g" "$SERVICE_FILE_PATH" || exit 1
sed -i -e "s/{BRANCH}/$BRANCH/g" "$SERVICE_FILE_PATH" || exit 1

echo "Service file:"
echo "$SERVICE_FILE_PATH"

cat "$SERVICE_FILE_PATH"

echo "Installing service..."
sh install-service.sh $SERVICE_FILE_PATH || exit 1

echo "Finished creating arduino plug and play service"
