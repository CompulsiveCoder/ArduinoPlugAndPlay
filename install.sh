echo "Installing plug and play..."

echo "Building project..."
sh build.sh || (echo "Failed to build project" && exit 1)

echo "Installing plug and play service..."
bash install-plug-and-play-service.sh || (echo "Failed to install plug and play service" && exit 1)
