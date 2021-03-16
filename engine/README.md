# UIWidgets Engine

## Introduction

This is the engine code of UIWidgets.

## How to Build (Windows)

### Build Skia

1. Install depot_tools
```
git clone 'https://chromium.googlesource.com/chromium/tools/depot_tools.git'
```
Add ${PWD}/depot_tools to PATH

2. Clone the skia Repo
```
git clone https://skia.googlesource.com/skia.git
cd skia
git checkout chrome/m85
python2 tools/git-sync-deps
```

3. Install LLVM

https://clang.llvm.org/get_started.html

4. Build skia
```
bin/gn gen out/Debug
```

Update out/Debug/args.gn with the following content:
```
clang_win = "C:\Program Files\LLVM"
win_vc = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC"
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

convert icudtl.dat to object file in skia
```
cd SkiaRoot/third_party/externals/icu/flutter/
ld -r -b binary -o icudtl.o icudtl.dat
```
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

### Create symbolic

cmd
```
cd <uiwidigets_dir>\engine
cd third_party   \\ create the directory if not exists
mklink /D skia <SKIA_ROOT>
```
Flutter engine txt include skia header in this pattern 'third_party/skia/*', so without symbolic, the txt lib will include skia
header file in flutter engine, instead of headers in skia repo.

### Build Engine

```
cd <uiwidigets_dir>\engine
bee.exe win
```

## How to Build (Mac)

### Install Depot_tools
```
git clone 'https://chromium.googlesource.com/chromium/tools/depot_tools.git'

export PATH="${PWD}/depot_tools:${PATH}"
```

### Build Skia

Please ensure that you are using python2 when building skia.

```
git clone https://skia.googlesource.com/skia.git

git checkout chrome/m85

bin/gn gen out/Debug

python tools/git-sync-deps

ninja -C out/Debug -k 0
```


### Build Flutter Engine

Setting up the Engine development environment

Please follow https://github.com/flutter/flutter/wiki/Setting-up-the-Engine-development-environment.

Check out repo and update dependencies:

```
git checkout flutter-1.17-candidate.5
gclient sync -D
```

Apply changes to BUILD.gn (src/flutter/fml/BUILD.gn)

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

Comiple engine:
```
cd engine/src

./flutter/tools/gn --unoptimized

ninja -C ./out/host_debug_unopt/ flutter/fml:fml_lib
```

If the compilation fails because "no available Mac SDK is found" (in flutter-1.17 the build tool will only try to find Mac 10.XX SDKs), please modify the file "/src/build/Mac/find_sdk.py" under flutter root by setting "sdks" as your current sdk, e.g., ['11.0']. 

### Build Lib

set SKIA_ROOT and FLUTTER_ROOT to your $PATH. SKIA_ROOT is the root folder of your skia repository. FLUTTER_ROOT is the root folder of your flutter engine repository. 


Create symbolic as follows. Flutter engine txt include skia header in this pattern 'third_party/skia/*', so without symbolic, the txt lib will include skia
header file in flutter engine, instead of headers in skia repo.

cmd
```
cd <uiwidigets_dir>\engine
cd third_party   \\ create the directory if not exists
ln -s <SKIA_ROOT> skia
```

### Build Engine

```
cd <uiwidigets_dir>\engine
mono bee.exe mac
```

## How to Build (IOS)

There is a special settings, namely "Enable bitcode" we need set properly when building an ios native plugin.
Generally, the program is possible to run faster when "Enable bitcode" is true while the size of the plugin
will also become bigger.

You can choose to build skia and flutter engine with enabled or disabled "Enable bitcode" using different build commands as 
shown in the following two sections.

You can also build UIWidgets engine with enabled or disabled "Enable bitcode" by changing the value of 
"ios_bitcode_enabled" in "Build.bee.cs". Note that the value is set to false by default.

You can also change the setting of "Enable bitcode" in the XCode project generated by Unity. Specifically, you can 
find this setting in "Build Settings/Build Options/Enable Bitcode". This value is true by default.

You should always keep the "Enable bitcode" settings the same in the built skia and flutter library, the UIWidgets engine and the XCode project generated by your UIWidget project.

### Install Depot_tools

```
git clone 'https://chromium.googlesource.com/chromium/tools/depot_tools.git'

export PATH="${PWD}/depot_tools:${PATH}"
```

### Build Skia

```
git clone https://skia.googlesource.com/skia.git

git checkout chrome/m85
```

If you want to enable "Enable bitcode", use
```
bin/gn gen out/ios64 --args='target_os="ios" target_cpu="arm64" extra_cflags = [ "-fembed-bitcode"]'
```

Otherwise, use
```
bin/gn gen out/ios64 --args='target_os="ios" target_cpu="arm64"'
```

Finally,
```
python tools/git-sync-deps

ninja -C out/ios64 -k 0

```

### Build Flutter Engine

Because the dependencies of "flutter-1.17-candidate.5" for iOS is problematic, we need first sync up our 
local build dependencies to that of "flutter-1.18-candidate.6". 

```
git checkout flutter-1.18-candidate.6
gclient sync -D
```

Then switch back to "flutter-1.17-candidate.5":

```
git checkout flutter-1.17-candidate.5
```

Apply changes to BUILD.gn (src/flutter/fml/BUILD.gn)

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

If you want to enable "Enable bitcode", use
```
./flutter/tools/gn --ios --unoptimized --bitcode
```

Otherwise, use
```
./flutter/tools/gn --ios --unoptimized
```

Finally,
```
ninja -C ./out/ios_debug_unopt/ flutter/fml:fml_lib
```


If the compilation fails because "no available Mac SDK is found" (in flutter-1.17 the build tool will only try to find Mac 10.XX SDKs), please modify the file "/src/build/Mac/find_sdk.py" under flutter root by setting "sdks" as your current sdk, e.g., ['11.0']. 

### Build Lib

set SKIA_ROOT and FLUTTER_ROOT to your $PATH. SKIA_ROOT is the root folder of your skia repository. FLUTTER_ROOT is the root folder of your flutter engine repository. 


Create symbolic as follows. Flutter engine txt include skia header in this pattern 'third_party/skia/*', so without symbolic, the txt lib will include skia
header file in flutter engine, instead of headers in skia repo.

cmd
```
cd <uiwidigets_dir>\engine
cd third_party   \\ create the directory if not exists
ln -s <SKIA_ROOT> skia
```

### Build Engine

If you want to enable "Enable bitcode", change the value of "ios_bitcode_enabled" in "Build.bee.cs" to True. Otherwise change it to False.

```
cd <uiwidigets_dir>\engine
mono bee.exe ios
```