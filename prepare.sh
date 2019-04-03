echo "Preparing project"

export DEBIAN_FRONTEND=noninteractive

sudo apt-get update && apt-get install -y xmlstarlet && \

sh install-mono.sh
