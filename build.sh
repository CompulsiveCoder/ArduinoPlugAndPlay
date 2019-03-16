echo "Starting build"
echo "Dir: $PWD"

MODE=$1

if [ -z "$MODE" ]; then
    MODE="Release"
fi

echo "Mode: $MODE"

xbuild src/*.sln /p:Configuration=$MODE
