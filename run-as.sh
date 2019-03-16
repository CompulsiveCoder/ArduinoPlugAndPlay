echo "Running as specific device"
echo "  Device name: $1"
echo "  Serial port: $2"
mono app/ArduinoPlugAndPlay.exe --DeviceName=$1 --SerialPort=$2
