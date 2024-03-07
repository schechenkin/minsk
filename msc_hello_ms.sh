#!/bin/bash

# Vars
slndir="$(dirname "${BASH_SOURCE[0]}")/src"

cd $slndir
cd msc

# Restore + Build
dotnet build --nologo || exit

# Run
dotnet run --no-build -- "$@" || exit

#run executable
echo "run program"
./a.out
