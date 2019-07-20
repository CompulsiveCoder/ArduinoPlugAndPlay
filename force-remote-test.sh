#!/bin/bash

BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')

#if [ "$BRANCH" = "dev" ]
#then
  echo "Forcing remote test"

  sh clean.sh || exit 1

  echo " " >> Jenkinsfile
  
  git pull origin $BRANCH && \
  git commit Jenkinsfile -m "Forcing retest" && \
  git push origin $BRANCH && \
  
  echo "Repository has been updated. Test should now start on test server."
#else
#  echo "Cannot force retest from master branch. Switch to dev branch first."
#fi
