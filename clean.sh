# Reset the version back to 1.0.0.1 in the source code to avoid git changes.
# The real version can be injected during build
sh inject-version.sh 1.0.0.1

if [ -d "mock" ]; then
  rm "mock" -r
fi

if [ -d "_tmp" ]; then
  rm "_tmp" -r
fi
