echo "Publishing GitHub release..."

. ./project.settings

if [ -f "set-github-token.sh.security" ]; then
 . ./set-github-token.sh.security
else
  GITHUB_TOKEN=$GHTOKEN
fi

BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')

VERSION="$(cat version.txt).$(cat buildnumber.txt)"

echo "  Version: $VERSION"

POSTFIX=""
if [ $BRANCH != "lts" ]; then
  POSTFIX="-$BRANCH"
fi


REPOSITORY="$GITHUB_OWNER/$GITHUB_PROJECT"
TAG="v$VERSION$POSTFIX"
RELEASE_NAME="$GITHUB_PROJECT.$VERSION$POSTFIX"
RELEASE_DESCRIPTION="Release $VERSION$POSTFIX"

PRERELEASE="true"
if [ $BRANCH = "lts" ]; then
  PRERELEASE="false"
fi

echo "  Repository: $REPOSITORY"
echo "  Tag: $TAG"
echo "  Release name: $RELEASE_NAME"
echo "  Prerelease: $PRERELEASE"
echo ""
# Create a release
RELEASE_RESPONSE=$(curl -XPOST -H 'Cache-Control: no-cache' -H "Authorization:token $GITHUB_TOKEN" --data "{\"tag_name\": \"$TAG\", \"target_commitish\": \"$BRANCH\", \"name\": \"$RELEASE_NAME\", \"body\": \"$RELEASE_DESCRIPTION\", \"draft\": false, \"prerelease\": $PRERELEASE}" https://api.github.com/repos/$GITHUB_OWNER/$GITHUB_PROJECT/releases)

echo ""
echo "  Release response:"
echo "$RELEASE_RESPONSE"
echo ""

# Extract the id of the release response from the creation response
RELEASE_ID=$(echo "$RELEASE_RESPONSE" | sed -n -e 's/"id":\ \([0-9]\+\),/\1/p' | head -n 1 | sed 's/[[:blank:]]//g')

echo ""
echo "  Release ID: $RELEASE_ID"
echo ""

# Upload the release zip
curl -XPOST -H "Authorization:token $GITHUB_TOKEN" -H 'Cache-Control: no-cache' -H "Content-Type:application/octet-stream" --data-binary @releases/$RELEASE_NAME.zip https://uploads.github.com/repos/$GITHUB_OWNER/$GITHUB_PROJECT/releases/$RELEASE_ID/assets?name=$RELEASE_NAME.zip

echo ""
echo ""
echo "Finished publishing release."
