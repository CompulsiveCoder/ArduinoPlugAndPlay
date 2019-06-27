echo "Publishing GitHub release..."

if [ -f "set-github-token.sh.security" ]; then
 . ./set-github-token.sh.security
else
  GITHUB_TOKEN=$GHTOKEN
fi

BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')

VERSION="$(cat version.txt).$(cat buildnumber.txt)"

POSTFIX=""

if [ "$BRANCH" = "dev" ]; then
  POSTFIX="-dev"
fi

echo "  Version: $VERSION$POSTFIX"

github-release upload \
  --owner CompulsiveCoder \
  --repo ArduinoPlugAndPlay \
  --tag "$BRANCH" \
  --name "v$VERSION$POSTFIX" \
  --body "$BRANCH" \
  releases/ArduinoPlugAndPlay.$VERSION$POSTFIX.zip || exit 1
  
echo "Finished publishing release."
