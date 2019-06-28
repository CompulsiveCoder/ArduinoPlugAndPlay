#!/bin/bash

REPOSITORY_OWNER=$1
PACKAGE_NAME=$2
PACKAGE_VERSION=$3

if [ ! "$REPOSITORY_OWNER" ]; then
	echo "Please provide a repository owner as an argument."
	exit 1
fi

if [ ! "$PACKAGE_NAME" ]; then
	echo "Please provide a package name as an argument."
	exit 1
fi

if [ ! "$PACKAGE_VERSION" ]; then
	echo "Please provide a package version as an argument."
	exit 1
fi

echo "Installing package $PACKAGE_NAME $PACKAGE_VERSION..."

PACKAGE_FILE="$PACKAGE_NAME.$PACKAGE_VERSION"
PACKAGE_FILE_EXT="$PACKAGE_NAME.$PACKAGE_VERSION.zip"
PACKAGE_FOLDER="$PACKAGE_NAME"
PACKAGE_FOLDER_WITH_VERSION="$PACKAGE_NAME.$PACKAGE_VERSION"

#echo "  Package file: $PACKAGE_FILE"

# If the package folder isn't found
if [ ! -f "$PACKAGE_FILE_EXT" ]; then

  # Check if the project exists within the Workspace
	[[ $(echo $PWD) =~ "workspace" ]] && IS_IN_WORKSPACE=1 || IS_IN_WORKSPACE=0
	
	if [ "$IS_IN_WORKSPACE" = "1" ]; then
	  # Get the path to the Workspace lib directory
	  WORKSPACE_LIB_DIR=$(readlink -f "../../../lib")
	  
	  #echo "  Workspace lib directory:"
	  #echo "    $WORKSPACE_LIB_DIR"
	  
	  # Check if the package exists in the workspace inject lib directory
    if [ -d "$WORKSPACE_LIB_DIR/$PACKAGE_FOLDER_WITH_VERSION" ]; then
      echo "  From Workspace lib directory"
      # Copy the package from the Workspace lib directory
      cp -r $WORKSPACE_LIB_DIR/$PACKAGE_FOLDER_WITH_VERSION $PACKAGE_FOLDER || exit 1
      cp -r $WORKSPACE_LIB_DIR/$PACKAGE_FILE_EXT $PACKAGE_FILE_EXT || exit 1
    fi
  fi
  
  # If the package still isn't found
  if [ ! -f "$PACKAGE_FILE_EXT" ]; then
    echo "  From the web (libs repository)"
    
    # Download the package from the web
  	wget -q "https://github.com/$REPOSITORY_OWNER/libs/raw/master/$PACKAGE_FILE.zip" -O $PACKAGE_FILE_EXT || exit 1

    # Unzip the package
	  unzip -qq -o "$PACKAGE_FILE_EXT" -d "$PACKAGE_FOLDER/" || exit 1
	  
	  if [ "$IS_IN_WORKSPACE" = "1" ]; then
      # Make the Workspace lib directory if necessary
	    mkdir -p $WORKSPACE_LIB_DIR
	    
	    # Copy the package into the Workspace lib directory
	    cp -r "$PACKAGE_FOLDER" $WORKSPACE_LIB_DIR/$PACKAGE_FOLDER_WITH_VERSION/ || exit 1
      cp -r $PACKAGE_FILE_EXT $WORKSPACE_LIB_DIR/$PACKAGE_FILE_EXT || exit 1
	  fi
  fi
	
else
	echo "  Already exists. Skipping download."
fi


