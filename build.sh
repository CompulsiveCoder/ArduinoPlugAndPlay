echo "Starting build"
echo "  Dir: $PWD"

MODE=$1

if [ -z "$MODE" ]; then
    MODE="Release"
fi

echo "  Mode: $MODE"

xbuild src/*.sln /p:Configuration=$MODE /verbosity:quiet || exit 1

echo "Finished building project tests."
