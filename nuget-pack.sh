DIR=$PWD

mkdir -p pkg
mkdir -p pkg/archive

mv -f pkg/*.nupkg pkg/archive

BRANCH=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')

# TODO: Remove if not needed. Disabled because version increment is triggered in jenkinsfile
#if [ "$BRANCH" = "dev" ]
#then    
    # Only increment version during dev builds. The dev version can be used when graduated to the master branch
#    sh increment-version.sh
#fi

VERSION=$(cat version.txt)
BUILD_NUMBER=$(cat buildnumber.txt)

FULL_VERSION=$VERSION.$BUILD_NUMBER

if [ "$BRANCH" = "dev" ]
then
    FULL_VERSION="$FULL_VERSION-dev"
fi

cd lib
sh get-nuget.sh
cd $DIR

mono lib/nuget.exe pack Package.nuspec -version $FULL_VERSION -OutputDirectory pkg/
