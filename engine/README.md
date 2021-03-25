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

convert icudtl.dat to object file in flutter
```
cd flutterRoot/third_party/icu/flutter/
ld -r -b binary -o icudtl.o icudtl.dat
```
### Build flutter txt

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
--- a/third_party/txt/BUILD.gn
+++ b/third_party/txt/BUILD.gn
@@ -141,6 +141,7 @@ source_set("txt") {
     "//third_party/harfbuzz",
     "//third_party/icu",
     "//third_party/skia",
+    "//third_party/skia/modules/skottie",
   ]
 
   deps = [
@@ -339,3 +340,10 @@ executable("txt_benchmarks") {
     deps += [ "//third_party/skia/modules/skparagraph" ]
   }
 }
+
+static_library("txt_lib") {
+  complete_static_lib = true
+  deps = [
+    ":txt",
+  ]
+}
```


#### `optional`

update `out\host_debug_unopt\args.gn`
```
skia_use_angle = true
skia_use_egl = true
```

update skia

```
--- a/BUILD.gn
+++ b/BUILD.gn
@@ -524,6 +524,7 @@ optional("gpu") {

  if (skia_use_gl) {
    public_defines += [ "SK_GL" ]
+    include_dirs = [ "//third_party/angle/include" ]
    if (is_android) {
      sources += [ "src/gpu/gl/egl/GrGLMakeNativeInterface_egl.cpp" ]
```

cmd
```
set GYP_MSVS_OVERRIDE_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community
cd engine/src
python ./flutter/tools/gn --unoptimized
ninja -C out\host_debug_unopt flutter/third_party/txt:txt_lib
```

### Build Engine

```
cd <uiwidigets_dir>\engine
bee
```

## How to Build (Mac)

### Build Dependencies

Setting up the Engine development environment

Please follow https://github.com/flutter/flutter/wiki/Setting-up-the-Engine-development-environment.

Check out repo and update dependencies:

```
cd $FLUTTER_ROOT/flutter
git checkout flutter-1.17-candidate.5
gclient sync -D
```

Apply following to end of `flutter/third_party/txt/BUILD.gn`
```
diff --git a/third_party/txt/BUILD.gn b/third_party/txt/BUILD.gn
index 56b73a020..d42e88045 100644
--- a/third_party/txt/BUILD.gn
+++ b/third_party/txt/BUILD.gn
@@ -141,6 +141,7 @@ source_set("txt") {
     "//third_party/harfbuzz",
     "//third_party/icu",
     "//third_party/skia",
+    "//third_party/skia/modules/skottie",
   ]
 
   deps = [
@@ -339,3 +340,10 @@ executable("txt_benchmarks") {
     deps += [ "//third_party/skia/modules/skparagraph" ]
   }
 }
+
+static_library("txt_lib") {
+  complete_static_lib = true
+  deps = [
+    ":txt",
+  ]
+}
```

Comiple engine:
```
cd $FLUTTER_ROOT
./flutter/tools/gn --unoptimized
```

add this line to the end of out/host_debug_unopt/args.gn:
```
icu_use_data_file=false
```

finally run ninja:
```
ninja -C out/host_debug_unopt/ flutter/third_party/txt:txt_lib
```

If the compilation fails because "no available Mac SDK is found" (in flutter-1.17 the build tool will only try to find Mac 10.XX SDKs), please modify the file "/src/build/Mac/find_sdk.py" under flutter root by setting "sdks" as your current sdk, e.g., ['11.0'].


### Creat symbolic

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


