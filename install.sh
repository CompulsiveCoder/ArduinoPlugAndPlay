echo "Installing plug and play..."

echo "Building project..."
sh build.sh || exit 1

echo "Installing plug and play service..."
bash install-plug-and-play-service.sh || exit 1
