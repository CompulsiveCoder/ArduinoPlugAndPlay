echo "Transforming service template file..."

BRANCH=$1
DESTINATION=$2
TEMPLATE_FILE=$3
SERVICE_FILE=$4

EXAMPLE_COMMAND="Example:\n..sh [Branch] [Destination] [TemplateFile] [ServiceFile]"

if [ ! $BRANCH ]; then
  BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')
fi

if [ ! "$BRANCH" ]; then
  BRANCH="master"
fi

if [ ! "$DESTINATION" ]; then
  DESTINATION="/usr/local/ArduinoPlugAndPlay"
fi

if [ ! "$TEMPLATE_FILE" ]; then
  TEMPLATE_FILE="svc/arduino-plug-and-play.service.template"
fi

if [ ! "$SERVICE_FILE" ]; then
  SERVICE_FILE="svc/arduino-plug-and-play.service"
fi

if [ ! -f $TEMPLATE_FILE ]; then
  echo "  Service template file not found"
  echo "    $TEMPLATE_FILE"
  exit 1
fi

echo ""
echo "  Copying template service file to standard service file..."
cp $TEMPLATE_FILE $SERVICE_FILE || exit 1

echo ""
echo "  Template service file content:"
echo ""
echo "--- Start"
echo "$(cat $TEMPLATE_FILE)"
echo "--- End"

echo ""
echo "  Injecting values into template service file..."

ESCAPED_INSTALL_DIR="${DESTINATION//\//\\/}" || exit 1

sed -i -e "s/{INSTALL_PATH}/$ESCAPED_INSTALL_DIR/g" "$SERVICE_FILE" || exit 1
sed -i -e "s/{BRANCH}/$BRANCH/g" "$SERVICE_FILE" || exit 1

echo ""
echo "  Service file content:"
echo ""
SERVICE_FILE_CONTENT="$(cat $SERVICE_FILE)"
echo "--- Start"
echo "${SERVICE_FILE_CONTENT}"
echo "--- End"

echo ""
echo "Finished transforming template file"
