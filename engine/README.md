# UIWidgets Engine

## Introduction

This is the engine code of UIWidgets.

## How to Build (Windows)

### Install depot_tools
```
git clone 'https://chromium.googlesource.com/chromium/tools/depot_tools.git'
```
Add ${PWD}/depot_tools to PATH

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
modify flutter/third_party/txt/BUILD.gn
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

modify third_party/angnle/BUILD.gn
```
diff --git a/BUILD.gn b/BUILD.gn
index 06bf3bbbe..b4289dfa7 100644
--- a/BUILD.gn
+++ b/BUILD.gn
@@ -1252,3 +1252,17 @@ if (!is_component_build && is_android &&
     ]
   }
 }
+angle_static_library("angle_lib"){
+  complete_static_lib = true
+
+  deps = [
+    ":libANGLE",
+    ":libANGLE_base",
+    ":angle_system_utils",
+    ":angle_version",
+  ]
+
+  public_deps = [
+    ":includes",
+  ]
+}
diff --git a/src/libANGLE/renderer/d3d/d3d11/ExternalImageSiblingImpl11.cpp b/src/libANGLE/renderer/d3d/d3d11/ExternalImageSiblingImpl11.cpp
index adeeb5aa1..c9677bd8d 100644
--- a/src/libANGLE/renderer/d3d/d3d11/ExternalImageSiblingImpl11.cpp
+++ b/src/libANGLE/renderer/d3d/d3d11/ExternalImageSiblingImpl11.cpp
@@ -144,7 +144,7 @@ angle::Result ExternalImageSiblingImpl11::createRenderTarget(const gl::Context *

     mRenderTarget = std::make_unique<TextureRenderTarget11>(
         std::move(rtv), mTexture, std::move(srv), std::move(blitSrv), formatInfo.internalFormat,
-        formatInfo, mSize.width, mSize.height, 1, 1);
+        formatInfo, mSize.width, mSize.height, 1, mSamples);
     return angle::Result::Continue;
 }
```


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
ninja -C out\host_debug_unopt third_party/angle:angle_lib
ninja -C out\host_debug_unopt  third_party/angle:libEGL_static
```

convert icudtl.dat to object file in flutter
```
cd flutterRoot/third_party/icu/flutter/
ld -r -b binary -o icudtl.o icudtl.dat
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

Compile engine:
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


### Build Engine

```
cd <uiwidigets_dir>\engine
mono bee.exe mac
```


## How to Build (Android)
### Build flutter fml + skia + txt

1. Setting up the Engine development environment

Follow https://github.com/flutter/flutter/wiki/Setting-up-the-Engine-development-environment

2. Checkout flutter-1.17-candidate.5

```
cd engine/src/flutter
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
+    "//third_party/libcxx",
+    "//third_party/libcxxabi",
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
remove `visibility = [ "../libcxx:*" ]` from `/build/secondary/third_party/libcxxabi/BUILD.gn`

cmd
```
cd $FLUTTER_ROOT
python ./flutter/tools/gn --unoptimized --android
ninja -C out/android_debug_unopt/ flutter/third_party/txt:txt_lib
```
If the compilation fails because "no available Mac SDK is found" (in flutter-1.17 the build tool will only try to find Mac 10.XX SDKs), please modify the file "/src/build/Mac/find_sdk.py" under flutter root by setting "sdks" as your current sdk, e.g., ['11.0']. 
### build icu
```
cd <uiwidigets_dir>\engine
python $FLUTTER_ROOT_PATH/flutter/sky/tools/objcopy.py --objcopy $FLUTTER_ROOT_PATH/third_party/android_tools/ndk/toolchains/arm-linux-androideabi-4.9/prebuilt/darwin-x86_64/bin/arm-linux-androideabi-objcopy --input $FLUTTER_ROOT_PATH/third_party/icu/flutter/icudtl.dat --output icudtl.o --arch arm
```
### build uiwidgets
```
cd <uiwidigets_dir>\engine
mono bee.exe android
```

## How to Build (IOS)

The UIWidgets engine for iOS can only be built on Mac.

### Preliminaries

There is a special settings, namely "Enable bitcode" we need set properly when building an ios native plugin.
Generally, the program is possible to run faster when "Enable bitcode" is true while the size of the plugin
will also become bigger.

You can choose to build the library with enabled or disabled "Enable bitcode" using different build commands as 
shown in the following two sections.

You can also build UIWidgets engine with enabled or disabled "Enable bitcode" by changing the value of 
"ios_bitcode_enabled" in "Build.bee.cs". Note that the value is set to false by default.

You can also change the setting of "Enable bitcode" in the XCode project generated by Unity. Specifically, you can 
find this setting in "Build Settings/Build Options/Enable Bitcode". This value is true by default.

You should always keep the "Enable bitcode" settings the same in the built library, the UIWidgets engine and the XCode project generated by your UIWidget project.

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
./flutter/tools/gn --unoptimized --ios            //disable bitcode
./flutter/tools/gn --unoptimized --ios --bitcode  //enable bitcode
```

add this line to the end of out/ios_debug_unopt/args.gn, also 
ensure that "bitcode_marker=false" if bitcode is enabled
```
icu_use_data_file=false
```

finally run ninja:
```
ninja -C out/ios_debug_unopt/ flutter/third_party/txt:txt_lib
```

If the compilation fails because "no available Mac SDK is found" (in flutter-1.17 the build tool will only try to find Mac 10.XX SDKs), please modify the file "/src/build/Mac/find_sdk.py" under flutter root by setting "sdks" as your current sdk, e.g., ['11.0']. 

If the compilation fails because "'Foundation/NSURLHandle.h' file not found" (flutter-1.17 assumes that you are using some low iphone SDK (e.g., older than 12.2), in which some platform-dependent Macros are defined differently from new SDKs like 12.2), please modify the file "/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS.sdk/usr/include/TargetConditionals.h" in your system by making "TARGET_OS_EMBEDDED = 1" and "TARGET_OS_MACCATALYST = 0" for arm64-iphone architecture. You can also work-around this issue by checking out a new version of flutter (e.g., "flutter-1.18-candidate.6") and run "gclient sync -D" to get dependencies on newer iphone SDKs. Then switch back and build.

### Build Engine

```
cd <uiwidigets_dir>\engine
mono bee.exe ios
```

### Prelink Library (TODO: make it simpler)

Different from other platforms, the engine is built as a static library on iOS. One drawback of this is that, there are potential library conflicts between our library and the Unity build-in library (libiPhone-lib.a). To address this issue, an additional prelink step is required. Since prelink is not supported by Bee yet (https://unity.slack.com/archives/C1RM0NBLY/p1617696912101700), currently we have to do it manually as follows:

Generate the symbols that need to be stripped from libtxt_lib.a:
```
nm -j $FLUTTER_ROOT/out/ios_debug_unopt/obj/flutter/third_party/txt/libtxt_lib.a > third.symbol
```

Prelink our library with libtxt_lib.a and produce one single target file, namely libUIWidgets_d.o, inside engine folder: 
```
"/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/ld" -r -arch arm64 -syslibroot $IOS_SDK_PATH -unexported_symbols_list third.symbol $TARGET_FILES $FLUTTER_ROOT/out/ios_debug_unopt/obj/flutter/third_party/txt/libtxt_lib.a -o "libUIWidgets_d.o"                 //disable bitcode
"/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/ld" -r -arch arm64 -bitcode_bundle -bitcode_verify -syslibroot $IOS_SDK_PATH -unexported_symbols_list third.symbol $TARGET_FILES $FLUTTER_ROOT/out/ios_debug_unopt/obj/flutter/third_party/txt/libtxt_lib.a -o "libUIWidgets_d.o"        //enable bitcode
```

where **$IOS_SDK_PATH** is the ios sdk path on your computer, which is usually inside "/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer/SDKs/" and named like iPhoneOS*XX*.*YY*.sdk. **$TARGET_FILES** is a list of target files (i.e., .o files) in our library. You can find all the target files in the file "artifacts/tundra.dag.json" by collecting all the file names with suffix ".o" from the **Inputs** section under this **Annotation** "Lib_iOS_arm64 artifacts/libUIWidgets/debug_iOS_arm64/libUIWidgets_d.a".


Archive the generated target file into a static library:
```
"/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/libtool" -arch_only arm64 -static "libUIWidgets_d.o" -o "libUIWidgets_d.a"
```

Strip all the internal symbols from libtxt_lib.a:
```
"/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/strip" -x "libUIWidgets_d.a"
```

Copy the generated libUIWidgets_d.a from the engine folder to the plugin folder of your target project (e.g., Samples/UIWidgetsSamples_2019_4/Assets/Plugins/iOS/) to replace the original libraries there (i.e., libtxt_lib.a and libUIWidgets_d.a)


