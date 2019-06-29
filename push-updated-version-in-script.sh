#!/bin/bash

BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')

git pull origin $BRANCH --quiet && \
git commit scripts-installation/init.sh -m "Updated version in scripts-installation/init.sh script [ci skip]" && \
git push origin $BRANCH --quiet || echo "Failed to push updated version in init.sh script"

