#!/bin/bash

echo ""
echo "Setting MQTT credentials..."
echo ""

HOST=$1
USERNAME=$2
PASSWORD=$3
PORT=$4

if [ ! "$PORT" ]; then
  PORT="1883"
fi

if [ "$PASSWORD" ]; then

  echo "Host: $HOST"
  echo "Username: $USERNAME"
  echo "Port: $PORT"

  echo $HOST > "mqtt-host.security"
  echo $USERNAME > "mqtt-username.security"
  echo $PASSWORD > "mqtt-password.security"
  echo $PORT > "mqtt-port.security"

  echo ""
  echo "Finished setting MQTT credentials"
else
  echo "Please provide host, username and password as arguments"
fi
