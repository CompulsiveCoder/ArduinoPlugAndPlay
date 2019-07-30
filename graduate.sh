#!/bin/bash

BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')

if [ "$BRANCH" = "dev" ];  then
  echo "Graduating dev branch to master branch"

  echo "  Fetching from origin..."
  git fetch origin || exit 1

  echo "  Checking out master branch..."
  git checkout master || exit 1

  # Ensure it's up to date
  #git pull origin master || exit 1
  
  echo "  Merging dev branch into master branch..."
  git merge -X theirs origin/dev || exit 1

  echo "  Pushing updates to master branch..."
  git push origin master || exit 1

  echo "  Checkout out dev branch..."
  git checkout dev || exit 1

  echo "The 'dev' branch has been graduated to the 'master' branch"  || exit 1
else
  echo "You must be in the 'dev' branch to graduate to the 'master' branch, but currently in the '$BRANCH' branch. Skipping."
fi
