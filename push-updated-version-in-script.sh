#!/bin/bash

BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')

if [ "$BRANCH" = "dev" ];  then
  git pull origin $BRANCH --quiet && \
  git commit scripts-installation/init.sh -m "Updated version in scripts-installation/init.sh script [ci skip]" && \
  git push origin $BRANCH --quiet
else
  echo "Skipping push updated version in script. Only pushed for 'dev' branch not '$BRANCH'"
fi
