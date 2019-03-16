DIR=$PWD

echo "Initializing project"
echo "Dir: $PWD"

cd lib
sh get-libs.sh
cd $DIR
