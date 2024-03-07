#!/bin/bash

echo "IR"
~/projects/llvm-project/build/bin/clang  -Xclang -emit-llvm -S hello.c || exit