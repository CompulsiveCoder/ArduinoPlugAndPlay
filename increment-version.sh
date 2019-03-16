#!/bin/sh

echo "Incrementing version"

CURRENT_VERSION=$(cat version.txt)
CURRENT_BUILD=$(cat buildnumber.txt)

echo "Current: $CURRENT_VERSION.$CURRENT_BUILD"

CURRENT_BUILD=$(($CURRENT_BUILD + 1))

echo "New version: $CURRENT_VERSION.$CURRENT_BUILD"

echo $CURRENT_BUILD > buildnumber.txt
