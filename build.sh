echo "Starting build"
echo "Dir: $PWD"

MODE=$1

if [ -z "$MODE" ]; then
    MODE="Release"
fi

echo "Mode: $MODE"

xbuild src/*.sln /p:Configuration=$MODE /verbosity:quiet && \

echo "Finished building project tests." ||

(echo "Failed building project tests!" && exit 1)
