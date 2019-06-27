BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')

VERSION="$(cat version.txt).$(cat buildnumber.txt)"

POSTFIX=""
if [ $BRANCH != "lts" ]; then
  POSTFIX="-$BRANCH"
fi

git tag v$VERSION$POSTFIX

git push origin v$VERSION$POSTFIX
