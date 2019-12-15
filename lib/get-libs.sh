echo "Getting library files..."
echo "  Dir: $PWD"

bash install-package-from-libs-repository.sh GreenSense NUnit 2.6.4 || exit 1
bash install-package-from-libs-repository.sh GreenSense NUnit.Runners 2.6.4 || exit 1

bash install-package-from-github-release.sh CompulsiveCoder duinocom.core 1.2.0.27 || exit 1

echo "Finished getting library files."
