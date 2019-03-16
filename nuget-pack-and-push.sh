#!/bin/bash
echo "Packing and pushing nuget package"

sh nuget-pack.sh && \
sh nuget-push.sh