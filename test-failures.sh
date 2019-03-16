echo "Retesting failed tests..."

FAILURES_DIR="tests/nunit/failures"

if [ -d "$FAILURES_DIR" ]; then
    for f in $FAILURES_DIR/*.txt; do
      echo "Failure file:"
      echo "$f"
      
      TEST_NAME=$(cat $f)
      
      echo "Test name:"
      echo "$TEST_NAME"
      
      sh test-method.sh $TEST_NAME
    done  
fi
