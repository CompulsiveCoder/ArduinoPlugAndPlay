DIR=$PWD

TEST_NAME=$1

if [ ! "$TEST_NAME" ]; then
  echo "Specify a test name as an argument."
  exit 1
fi

cd tests/nunit/
sh test-method.sh $TEST_NAME

cd $DIR
