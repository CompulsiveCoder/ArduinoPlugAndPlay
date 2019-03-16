echo "Preparing project"

export DEBIAN_FRONTEND=noninteractive

sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF

# Create the source list file then copy (directly creating causes permission denied errors on c9.io)
sudo echo "deb http://download.mono-project.com/repo/debian wheezy main" > mono-xamarin.list
sudo cp mono-xamarin.list /etc/apt/sources.list.d/mono-xamarin.list

sudo apt-get update -qq
sudo apt-get install -y --no-install-recommends udev git wget mono-complete msbuild mono-devel ca-certificates-mono
