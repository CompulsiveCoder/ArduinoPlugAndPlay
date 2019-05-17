SUDO=""
if [ ! "$(id -u)" -eq 0 ]; then
    SUDO='sudo'
fi

echo "USE_MONO4=$USE_MONO4"
if [ "$USE_MONO4" = 1 ]; then
  if ! type "xbuild" > /dev/null; then
    echo "Using mono4"
    $SUDO apt-get install -qq -y tzdata mono-devel mono-complete ca-certificates-mono
  else
    echo "Mono is already installed. Skipping install."
  fi
else
  if ! type "xbuild" > /dev/null; then
    echo "Installing latest mono"
    VERSION_NAME=$(lsb_release -cs)
  
    $SUDO apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
    echo "deb http://download.mono-project.com/repo/ubuntu stable-$VERSION_NAME main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
  
    $SUDO apt-get update -qq && $SUDO apt-get install -qq -y --allow-unauthenticated mono-devel mono-complete ca-certificates-mono msbuild
  else
    echo "Mono is already installed. Skipping install."
  fi
fi
echo "Checking mono version..."
mono --version
