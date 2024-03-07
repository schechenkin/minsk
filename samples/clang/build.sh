#!/bin/bash

#cmake -S llvm -B build -DCMAKE_BUILD_TYPE=Debug -DCMAKE_INSTALL_PREFIX=install -DLLVM_USE_LINKER=lld -DLLVM_ENABLE_PROJECTS="clang;clang-tools-extra" -DLLVM_TARGETS_TO_BUILD="x86-64" -DLLVM_PARALLEL_COMPILE_JOBS=6 -DLLVM_PARALLEL_LINK_JOBS=2
#cmake --build build --target clang --parallel 4

echo "clang version"
~/projects/llvm-project/build/bin/clang --version

#echo ""

#echo "ast"
#~/projects/llvm-project/build/bin/clang  -Xclang -dump-tokens -fsyntax-only quicksort.c || exit

#echo "ast"
#~/projects/llvm-project/build/bin/clang  -Xclang -ast-dump -fsyntax-only quicksort.c || exit

echo "IR"
#~/projects/llvm-project/build/bin/clang  -Xclang -emit-llvm -S quicksort.c || exit

#echo "ASM"
#~/projects/llvm-project/build/bin/clang  -S quicksort.c || exit

#echo "HTML"
#file not recognized: file format not recognized
#~/projects/llvm-project/build/bin/clang quicksort.c minsklib.c helper.c -O2 -fuse-ld=lld || exit

#echo "Compiler options"
#~/projects/llvm-project/build/bin/clang quicksort.c -Xclang -compiler-options-dump -fsyntax-only || exit

#echo "print-phases" 
#~/projects/llvm-project/build/bin/clang  -ccc-print-phases quicksort.c minsklib.c helper.c -o quicksort -O2 -fuse-ld=lld || exit

#echo "print-bindings" 
#~/projects/llvm-project/build/bin/clang  -ccc-print-bindings quicksort.c minsklib.c helper.c -o quicksort || exit

#cho "steps" 
#~/projects/llvm-project/build/bin/clang  -### quicksort.c minsklib.c helper.c -o quicksort -O2 -fuse-ld=lld || exit

echo "build" 
~/projects/llvm-project/build/bin/clang  quicksort.c minsklib.c helper.c -o quicksort -O2  -fuse-ld=lld -fsave-optimization-record  || exit

echo "generate opt report" 
~/projects/llvm-project/llvm/tools/opt-viewer/opt-viewer.py quicksort.opt.yaml