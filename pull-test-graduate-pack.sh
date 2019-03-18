echo "Checking out dev branch..." && \

git checkout dev && \

echo "Pulling updates to dev branch..." && \

git pull origin dev && \

echo "Incrementing version..."

sh increment-version.sh && \

echo "Pushing version..."

sh push-version.sh && \

echo "Cleaning..."

sh clean.sh && \

echo "Testing and graduating..." && \

sh test-and-graduate.sh && \

echo "Checkout out master branch..."

git checkout master && \

echo "Injecting version..." && \

sh inject-version.sh && \

echo "Building..." && \

sh build.sh && \

echo "Packing nuget package..." && \

sh nuget-pack.sh && \

echo "Cleaning..."

sh clean.sh && \

echo "Checking out dev branch..." && \

git checkout dev && \

sh clean.sh