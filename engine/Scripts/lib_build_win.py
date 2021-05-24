from genericpath import exists
import os
import sys
import getopt

work_path=os.getcwd()
engine_path=""
platform=""
gn_params=""
optimize=""
ninja_params1=""
ninja_params2=""
ninja_params3=""
runtime_mode=""
bitcode=""
flutter_root_path=""

def get_opts():
    # get intput agrs
    global engine_path
    global gn_params
    global runtime_mode
    global bitcode

    options, args = getopt.getopt(sys.argv[1:], 'r:p:m:eo')
    for opt, arg in options:
        if opt == '-r':
            engine_path = arg # set engine_path, depot_tools and flutter engine folder will be put into this path
        elif opt == '-p':
            gn_params += gn_params + " --" + arg # set the target platform android/ios
        elif opt == '-m':
            runtime_mode = arg
            gn_params += gn_params + " --runtime-mode=" + runtime_mode # set runtime mode release/debug

def engine_path_check():
    global engine_path
    if not os.path.exists(engine_path):
        os.makedirs(engine_path)

def set_params():
    global output_path
    global ninja_params1
    global ninja_params2
    global ninja_params3
    global gn_params

    print("setting environment variable and other params...")


    if runtime_mode == "release":
        optimize="" 
        output_path="host_release"    
    elif runtime_mode == "debug":
        optimize="--unoptimized"
        output_path="host_debug_unopt"
    else:
        assert False, "In func set_params(), unknown param"

    ninja_params1="-C out/" +output_path + " flutter/third_party/txt:txt_lib"
    ninja_params2="-C out/" +output_path + " third_party/angle:angle_lib"
    ninja_params3="-C out/" +output_path + " third_party/angle:libEGL_static"
    gn_params=gn_params + " " + optimize

def set_env_verb():
    global flutter_root_path
    flutter_root_path = os.getenv('FLUTTER_ROOT_PATH', 'null')
    if flutter_root_path == 'null':
        # command ="setx /M FLUTTER_ROOT_PATH " + engine_path + "/engine/src"
        # os.system(command)
        os.environ["FLUTTER_ROOT_PATH"] = engine_path + "/engine/src"
        flutter_root_path = os.getenv('FLUTTER_ROOT_PATH')
    else:
        print("This environment variable has been set, skip")
    env_path = os.getenv('Path')
    path_strings = env_path.split(';')
    for path in path_strings:
        if path.startswith(engine_path):
            print("This environment variable has been set, skip")
            return
    os.environ["Path"] = engine_path + "/depot_tools;" + os.environ["Path"]

def get_depot_tools():
    print("\nGetting Depot Tools...")
    if not os.path.exists(engine_path):
        assert False,"Flutter engine path is not exist, please set the path by using \"-r\" param to set a engine path."
    if os.path.exists(engine_path + "/depot_tools") and os.path.exists(engine_path + "/depot_tools/.git"):
        print("depot_tools already installed, skip")
    else:
        os.chdir(engine_path)
        os.system("git clone https://chromium.googlesource.com/chromium/tools/depot_tools.git")
        os.system("gclient")

def get_flutter_engine():
    global engine_path
    global flutter_root_path
    print("\nGetting flutter engine...")
    if os.path.exists(engine_path + "/engine"):
        print("engine folder already exist, skip")
    else:
        os.makedirs(engine_path + "/engine")
    content = '''
solutions = [
{
    "managed": False,
    "name": "src/flutter",
    "url": "git@github.com:flutter/engine.git", 
    "custom_deps": {},
    "deps_file": "DEPS",
    "safesync_url": "",
},
]
'''
    f = open(engine_path + "/engine/.gclient", "w")
    f.write(content)
    f.close()
    os.chdir(engine_path + "/engine")
    os.system("gclient sync")
    os.chdir(flutter_root_path + "/flutter")
    os.system("git checkout flutter-1.17-candidate.5")
    os.system("gclient sync -D")

def compile_engine():
    global flutter_root_path
    global work_path
    global gn_params
    global output_path
    global ninja_params1
    global ninja_params2
    global ninja_params3

    print("\nSCompiling engine...")
    os.chdir(flutter_root_path + "/flutter/third_party/txt")
    os.system("cp -f " + work_path + "/patches/BUILD.gn.patch BUILD.gn.patch")
    os.system("patch < BUILD.gn.patch -N")
    
    os.chdir(flutter_root_path + "/third_party/angle")
    os.system("cp -f " + work_path + "/patches/windows/BUILD.gn.patch BUILD.gn.patch")
    os.system("patch < BUILD.gn.patch -N")

    os.chdir(flutter_root_path + "/third_party/angle/src/libANGLE/renderer/d3d/d3d11/")
    os.system("cp -f " + work_path + "/patches/windows/cpp.patch cpp.patch")
    os.system("patch < cpp.patch -N")

    os.chdir(flutter_root_path + "/third_party/skia/")
    os.system("cp -f " + work_path + "/patches/windows/BUILD_2.gn.patch BUILD_2.gn.patch")
    os.system("patch < BUILD_2.gn.patch -N")

    os.chdir(flutter_root_path)
    os.system("python ./flutter/tools/gn " + gn_params)
    print(gn_params)

    f = open(flutter_root_path + "/out/" + output_path + "/args.gn", 'a')
    f.write("skia_use_angle = true\nskia_use_egl = true")
    f.close()
    os.system("ninja " + ninja_params1)
    os.system("ninja " + ninja_params2)
    os.system("ninja " + ninja_params3)
    os.chdir(flutter_root_path + "/third_party/icu/flutter/")
    os.system("ld -r -b binary -o icudtl.o icudtl.dat")


def build_engine():
    global work_path
    global runtime_mode

    print("\nStarting build engine...")
    if not os.path.exists(work_path + "/../../com.unity.uiwidgets/Runtime/Plugins/x86_64"):
        os.makedirs(work_path + "/../../com.unity.uiwidgets/Runtime/Plugins/x86_64")
    os.chdir(work_path + "/../")
    if runtime_mode == "release":
        os.system("rm -rf build_release/*")
        os.system("bee.exe win_release")
        os.system("cp -r build_release/. ../com.unity.uiwidgets/Runtime/Plugins/x86_64")
    if runtime_mode == "debug":
        os.system("rm -rf build_debug/*")
        os.system("bee.exe win_debug")
        os.system("cp -r build_debug/. ../com.unity.uiwidgets/Runtime/Plugins/x86_64")

def revert_patches():
    print("\nRevert patches...")
    os.chdir(flutter_root_path + "/flutter/third_party/txt")
    os.system("patch -R < BUILD.gn.patch")
    
    os.chdir(flutter_root_path + "/third_party/angle")
    os.system("patch -R < BUILD.gn.patch")

    os.chdir(flutter_root_path + "/third_party/angle/src/libANGLE/renderer/d3d/d3d11/")
    os.system("patch -R < cpp.patch")

    os.chdir(flutter_root_path + "/third_party/skia/")
    os.system("patch -R < BUILD_2.gn.patch")

def main():
    get_opts()
    engine_path_check()
    set_params()
    set_env_verb()   
    get_depot_tools()
    get_flutter_engine()
    compile_engine()
    build_engine()
    revert_patches()

if __name__=="__main__":
    main()