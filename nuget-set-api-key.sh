DIR=$PWD

cd lib
sh get-nuget.sh
cd $DIR

mono lib/nuget.exe setApiKey -verbosity quiet $1
