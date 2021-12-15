SERVICE_FILE_PATH=$1
SERVICE_FILE=$(basename -- "$SERVICE_FILE_PATH")

echo "Installing service"
echo "  Path: $SERVICE_FILE_PATH"
echo "  Name: $SERVICE_FILE"

SYSTEMCTL_SCRIPT="systemctl.sh"

MOCK_SYSTEMCTL_FLAG_FILE="is-mock-systemctl.txt"

IS_MOCK_SYSTEMCTL=0

if [ -f "$MOCK_SYSTEMCTL_FLAG_FILE" ]; then
  IS_MOCK_SYSTEMCTL=1
  echo "  Is mock systemctl"
fi

SERVICES_DIR="/lib/systemd/system"

if [ $IS_MOCK_SYSTEMCTL = 1 ]; then
  SERVICES_DIR="mock/services"
fi

#SUDO=""
#if [ ! "$(id -u)" -eq 0 ]; then
#    SUDO='sudo'
#fi
# TODO: Clean up. Sudo is not used. If sudo is required the script can be called with sudo.
SUDO=""

mkdir -p $SERVICES_DIR

echo "  Services directory:"
echo "    $SERVICES_DIR"
echo "  Destination file:"
echo "    $SERVICES_DIR/$SERVICE_FILE"

if [ $IS_MOCK_SYSTEMCTL = 1 ]; then
  echo ""
  echo "  Is mock systemctl. Installing to mock services directory..."
  cp $SERVICE_FILE_PATH $SERVICES_DIR/$SERVICE_FILE
else
  if [ -f $SERVICES_DIR/$SERVICE_FILE ]; then 
    echo ""
    echo "  Service already exists. Stopping it..." 
    sh $SYSTEMCTL_SCRIPT stop $SERVICE_FILE || echo "Failed to stop service. It likely doesn't exist."
  fi
  
  echo ""
  echo "  Copying service file into services directory..." 
  $SUDO cp -fv $SERVICE_FILE_PATH $SERVICES_DIR/$SERVICE_FILE || exit 1
  
  echo ""
  echo "  Setting permissions on service file..." 
  $SUDO chmod 644 $SERVICES_DIR/$SERVICE_FILE || exit 1
  
  #sh $SYSTEMCTL_SCRIPT daemon-reload && \ # TODO: Remove if not needed
  
  echo ""
  echo "  Enabling service..." 
  sh $SYSTEMCTL_SCRIPT enable $SERVICE_FILE || exit 1
  
  echo ""
  echo "  Starting service..." 
  sh $SYSTEMCTL_SCRIPT start $SERVICE_FILE || exit 1
  
  sh $SYSTEMCTL_SCRIPT restart $SERVICE_FILE # TODO: Remove if not needed

  echo ""
  echo "Viewing service status..."
  SERVICE_STATUS=$(sh $SYSTEMCTL_SCRIPT status $SERVICE_FILE) || exit 1

  echo "${SERVICE_STATUS}"

  echo ""
  echo "  Checking service is loaded and active..."
  [[ ! $(echo $SERVICE_STATUS) =~ "Loaded: loaded" ]] && echo "The service isn't loaded" && exit 1
  [[ ! $(echo $SERVICE_STATUS) =~ "Active: active" ]] && echo "The service isn't active" && exit 1
  [[ $(echo $SERVICE_STATUS) =~ "not found" ]] && echo "The service wasn't found" && exit 1

fi

echo "Finished installing service"
