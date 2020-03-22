#!/bin/bash

BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')

echo "Graduating master branch to rc branch..."

sh clean.sh || exit 1

if [ "$BRANCH" = "dev" ];  then
  echo "Currently in dev branch. Checking out master branch..."
  git checkout master || exit 1
fi

echo ""
echo "Fetching from origin..."
git fetch origin || exit 1

echo ""
echo "Pulling the master branch from origin (to update it locally)..."
git pull origin master || exit 1

echo ""
echo "Merging the rc branch into the master branch..."
git merge rc || exit 1

echo ""
echo "Checking out the rc branch..."
git checkout rc || exit 1

#echo ""
#echo "Pulling the rc branch from origin (to update it locally)..."
#git pull origin rc || exit 1

echo ""
echo "Merging the master branch into the rc branch..."
git merge -X theirs master || exit 1

echo ""
echo "Pulling the rc branch from origin..."
git pull origin rc || exit 1

echo ""
echo "Pushing the updated rc branch to origin..."
git push origin rc || exit 1

echo ""
echo "Forcing remote test..."
sh force-remote-test.sh || exit 1

echo ""
echo "Checking out the $BRANCH branch again..."
git checkout $BRANCH || exit 1

echo ""
echo "Merging rc into $BRANCH"
git merge rc

echo ""
echo "The 'master' branch has been graduated to the 'rc' branch"
