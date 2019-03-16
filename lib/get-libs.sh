echo "Getting library files..."
echo "  Dir: $PWD"

sh install-package.sh NUnit 2.6.4
sh install-package.sh NUnit.Runners 2.6.4
sh install-package.sh Newtonsoft.Json 11.0.2
sh install-package.sh ArduinoSerialControllerClient 1.0.9
sh install-package.sh duinocom.core 1.0.6
sh install-package.sh M2Mqtt 4.3.0.0

echo "Finished getting library files."
