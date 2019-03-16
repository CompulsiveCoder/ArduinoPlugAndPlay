#!/bin/bash

echo ""
echo "Setting MQTT device name..."
echo ""

DEVICE_NAME=$1

if [ "$DEVICE_NAME" ]; then

  echo "Device name: $DEVICE_NAME"
  
  echo $DEVICE_NAME > "mqtt-device-name.security"

  echo ""
  echo "Finished setting MQTT device name"
else
  echo "Please provide device name as an argument"
fi
