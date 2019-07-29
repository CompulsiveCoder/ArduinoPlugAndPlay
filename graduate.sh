#!/bin/bash

BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')

if [ "$BRANCH" = "dev" ];  then
  echo "Graduating dev branch to master branch"

  echo "  Fetching from origin..."
  git fetch origin || exit 1

  echo "  Merging master branch into dev branch..."
  git merge -X ours origin/master || exit 1

  echo "  Stashing updated build number..."
  git stash || exit 1
  
  echo "  Checking out master branch..."
  git checkout master || exit 1

  echo "  Restoring updated build number..."
  git stash pop || exit 1

  # Ensure it's up to date
  #git pull origin master --quiet && \

  echo "  Merging dev branch into master branch..."
  git merge -X theirs dev || exit 1

#  echo "  Incrementing version number (again)..."
#  bash increment-version.sh

  echo "  Pushing updates to master branch..."
  git push origin master || exit 1

  echo "  Checkout out dev branch..."
  git checkout dev || exit 1

  echo "The 'dev' branch has been graduated to the 'master' branch"  || exit 1
else
  echo "You must be in the 'dev' branch to graduate to the 'master' branch, but currently in the '$BRANCH' branch. Skipping."
fi
