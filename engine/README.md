# UIWidgets Engine

## Introduction

This is the engine code of UIWidgets.

## How to Build Depedencies (Windows)

### Build Skia

1. Install depot_tools
```
git clone 'https://chromium.googlesource.com/chromium/tools/depot_tools.git'
```
Add ${PWD}/depot_tools to PATH

2. Clone the skia Repo

git clone https://skia.googlesource.com/skia.git
cd skia
git checkout chrome/m85
python2 tools/git-sync-deps

3. Build skia

```
bin/gn gen out/Debug
```
Update out/Debug/args.gn with the following content:
```
clang_win = "C:\Program Files\LLVM"
cc = "clang"
cxx = "clang++"
is_debug = true
skia_use_angle = true
skia_use_egl = true
extra_cflags = [
  "/MTd",
  "-I../../third_party/externals/angle2/include",
]
```
```
ninja -C out/Debug -k 0
```
Ignore this error: "lld-link: error: could not open 'EGL': no such file or directory"

### Build flutter fml

1. Setting up the Engine development environment

Follow https://github.com/flutter/flutter/wiki/Setting-up-the-Engine-development-environment

2. Compiling for Windows

Follow https://github.com/flutter/flutter/wiki/Compiling-the-engine#compiling-for-windows

3. Checkout flutter-1.17-candidate.5

```
cd engine/src/flutter
git checkout flutter-1.17-candidate.5
gclient sync -D
```

Apply the following diff:
```
diff --git a/fml/BUILD.gn b/fml/BUILD.gn
index 9b5626e78..da1322ce5 100644
--- a/fml/BUILD.gn
+++ b/fml/BUILD.gn
@@ -295,3 +295,10 @@ executable("fml_benchmarks") {
     "//flutter/runtime:libdart",
   ]
 }
+
+static_library("fml_lib") {
+  complete_static_lib = true
+  deps = [
+    "//flutter/fml",
+  ]
+}
```
cmd
```
set GYP_MSVS_OVERRIDE_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community
cd engine/src
python ./flutter/tools/gn --unoptimized
ninja -C .\out\host_debug_unopt\ flutter/fml:fml_lib
```
powershell 
```
$env:GYP_MSVS_OVERRIDE_PATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community"
$env:FLUTTER_ROOT="E:\c\src" # target to flutter
$env:SKIA_ROOT="C:\Users\siyao\skia_repo\skia\" # target to skia
```

## Create symbolic

cmd
```
cd <uiwidigets_dir>\engine
cd third_party   \\ create the directory if not exists
mklink /D skia <SKIA_ROOT>
```
powershell (run as administrator)
```
cd <uiwidigets_dir>\engine
cd third_party   # create the directory if not exists
New-Item -Path skia -ItemType SymbolicLink -Value C:\Users\siyao\skia_repo\skia\ 
```
Flutter engine txt include skia header in this pattern 'third_party/skia/*', so without symbolic, the txt lib will include skia
header file in flutter engine, instead of headers in skia repo.

## How to Build Engine
```
bee
```

## Set ICU Data Enviroment Varaible
cmd
```
set UIWIDGETS_ICUDATA=<SKIA_ROOT>/out/Debug/icudtl.dat
```
powershell
```
$env:UIWIDGETS_ICUDATA="$env:SKIA_ROOT/out/Debug/icudtl.dat"
```
Unity Editor need to run with those environment variables set.
