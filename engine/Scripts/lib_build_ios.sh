work_path=$(pwd)
engine_path=
platform=
gn_params=""
optimize=""
ninja_params=""
runtime_mode=
bitcode=""
output_path=

echo "setting environment variable and other params..."

while getopts ":r:p:m:eo" opt
do
    case $opt in
        r)
        engine_path=$OPTARG # set engine_path, depot_tools and flutter engine folder will be put into this path
        ;;
        p)
        gn_params="$gn_params --$OPTARG" # set the target platform android/ios
        ;;
        m)
        runtime_mode=$OPTARG
        gn_params="$gn_params --runtime-mode=$runtime_mode" # set runtime mode release/debug
        ;;
        e)
        bitcode="-bitcode_bundle -bitcode_verify"
        gn_params="$gn_params --bitcode" # enable-bitcode switch
        ;;
        ?)
        echo "unknown param"
        exit 1;;
    esac
done

if [ ! -d $engine_path ];
then
  mkdir $engine_path
fi

if [ "$bitcode" == "-bitcode_bundle -bitcode_verify" ];
then
  echo "true" > bitcode.conf
else
echo "false" > bitcode.conf
fi

if [ "$runtime_mode" == "release" ];
then
  optimize=""
  output_path="ios_release"
  ninja_params="-C out/$output_path flutter/third_party/txt:txt_lib"
elif [ "$runtime_mode" == "debug" ];
then
  optimize="--unoptimized"
  output_path="ios_debug_unopt"
  ninja_params=" -C out/$output_path flutter/third_party/txt:txt_lib"
elif [ "$runtime_mode" == "profile" ];
then
  echo "not support profile build yet"
  exit 1
fi

gn_params="$gn_params $optimize"

#set environment variable
function isexist()
{
    source_str=$1
    test_str=$2
    
    strings=$(echo $source_str | sed 's/:/ /g')
    for str in $strings
    do  
        if [ $test_str = $str ]; then
            return 0
        fi  
    done
    return 1
}

if [ ! $FLUTTER_ROOT_PATH ];then
  echo "export FLUTTER_ROOT_PATH=$engine_path/engine/src" >> ~/.bash_profile
else
  echo "This environment variable has been set, skip"
fi

if isexist $PATH $engine_path/depot_tools; then 
  echo "This environment variable has been set, skip"
else 
  echo "export PATH=$engine_path/depot_tools:\$PATH" >> ~/.bash_profile
fi
source ~/.bash_profile

echo "\nGetting Depot Tools..." 
if [ ! -n "$engine_path" ]; then   
  echo "Flutter engine path is not exist, please set the path by using \"-r\" param to set a engine path."  
  exit 1
fi
cd $engine_path	
if [ -d 'depot_tools' ] && [ -d "depot_tools/.git" ];
then
  echo "depot_tools already installed, skip"
else
  git clone https://chromium.googlesource.com/chromium/tools/depot_tools.git
  gclient
fi

echo "\nGetting flutter engine..."

if [ -d 'engine' ];
then
  echo "engine folder already exist, skip"
else
  mkdir engine
fi
cd engine
#git@github.com:guanghuispark/engine.git is a temp repo, replace it later
echo "solutions = [
  {
    \"managed\": False,
    \"name\": \"src/flutter\",
    \"url\": \"git@github.com:guanghuispark/engine.git\", 
    \"custom_deps\": {},
    \"deps_file\": \"DEPS\",
    \"safesync_url\": \"\",
  },
]" > .gclient

gclient sync

cd $FLUTTER_ROOT_PATH/flutter
git checkout flutter-1.17-candidate.5
gclient sync -D

echo "\nSCompiling engine..."
#apply patch to Build.gn
cd $FLUTTER_ROOT_PATH/flutter/third_party/txt
cp -f $work_path/patches/BUILD.gn.patch BUILD.gn.patch
patch < BUILD.gn.patch -N

cd $FLUTTER_ROOT_PATH/build/mac
cp -f $work_path/patches/find_sdk.patch find_sdk.patch
patch < find_sdk.patch -N

cd $FLUTTER_ROOT_PATH
./flutter/tools/gn $gn_params


echo "icu_use_data_file=false" >> out/$output_path/args.gn
ninja $ninja_params

echo "\nStarting build engine..."
cd $work_path/../

if [ -d '../com.unity.uiwidgets/Runtime/Plugins/ios' ];
then
  echo "ios folder already exist, skip create folder"
else
  mkdir ../com.unity.uiwidgets/Runtime/Plugins/ios
fi

if [ "$runtime_mode" == "release" ];
then
  rm -rf build_release/*
  mono bee.exe ios_release
  cp -r build_release/. ../com.unity.uiwidgets/Runtime/Plugins/ios
  echo "\nStarting prlink library..."
  cd Scripts/../
  tundra_file="$work_path/../artifacts/tundra.dag.json"
  python3 Scripts/prelink.py $tundra_file $runtime_mode $output_path $work_path $bitcode
elif [ "$runtime_mode" == "debug" ];
then
  rm -rf build_debug/*
  mono bee.exe ios_debug
  cp -r build_debug/. ../com.unity.uiwidgets/Runtime/Plugins/ios
fi

if [ "$runtime_mode" == "debug" ];
then
  echo "\nRevert patches..."
  cd $FLUTTER_ROOT_PATH/flutter/third_party/txt
  patch -R < BUILD.gn.patch

  cd $FLUTTER_ROOT_PATH/build/mac
  patch -R < find_sdk.patch
fi
