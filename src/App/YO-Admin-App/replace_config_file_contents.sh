#!/bin/bash
set -e

FROM_PATH="$1"
TO_PATH="$2"

echo "Replacing contents of '$TO_PATH' with '$FROM_PATH'"

# Check if source file exists
if [ ! -f "$FROM_PATH" ]; then
    echo "ERROR: Source file does not exist: $FROM_PATH"
    exit 1
fi

# Check if target file exists
if [ ! -f "$TO_PATH" ]; then
    echo "ERROR: Target file does not exist: $TO_PATH"
    exit 1
fi

# Perform the replacement
cp -f "$FROM_PATH" "$TO_PATH"

if [ $? -eq 0 ]; then
    echo "Replacement successful."
else
    echo "ERROR: Failed to replace file."
    exit 1
fi
