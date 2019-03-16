#!/bin/bash

# Note: this script must be run with "bash" not "sh" for the time command to work

CATEGORY=$1

if [ -z "$CATEGORY" ]; then
    CATEGORY="Unit"
fi

echo "Testing project"
echo "  Dir: $PWD"
echo "  Category: $CATEGORY"

mono lib/NUnit.Runners.2.6.4/tools/nunit-console.exe bin/Release/*.dll --include=$CATEGORY
