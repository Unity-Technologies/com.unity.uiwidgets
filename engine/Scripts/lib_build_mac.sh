work_path=$(pwd)
engine_path=
platform=
gn_params=""
optimize="--unoptimized"
ninja_params=""

while getopts ":r:p:m:eo" opt
do
    case $opt in
        r)
        engine_path=$OPTARG # set engine_path, depot_tools and flutter engine folder will be put into this path
        ;;
        p)
        gn_params="$gn_params --$OPTARG" # set the target platform android/ios/linux
        ;;
        m)
        runtime_node=$OPTARG
        gn_params="$gn_params --runtime-mode=$runtime_node" # set runtime mode release/debug/profile
        if [ "$runtime_node" == "release" ];then
          ninja_params=" -C out/host_release flutter/third_party/txt:txt_lib"
        elif [ "$runtime_node" == "debug" ];then
          ninja_params=" -C out/host_debug_unopt/ flutter/third_party/txt:txt_lib"
        elif [ "$runtime_node" == "profile" ];then
          echo "not support profile build yet"
          exit 1
        fi
        ;;
        e)
        gn_params="$gn_params --bitcode" # enable-bitcode switch
        ;;
        o)
        optimize="" # optimize code switch
        ;;
        ?)
        echo "unknown param"
        exit 1;;
    esac
done

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
  echo "This environment variable has been set, no need to set it again..."
fi

if isexist $PATH $engine_path/depot_tools; then 
  echo "This environment variable has been set, no need to set it again..."
else 
  echo "export PATH=$engine_path/depot_tools:\$PATH" >> ~/.bash_profile
fi
source ~/.bash_profile

echo "\nGetting Depot Tools..." 
if [ ! -n "$engine_path" ]; then   
  echo "Flutter engine path is null, please set the path by using \"-r\" param to set a engine path."  
  exit 1
else
  echo "$engine_path"
fi    
cd $engine_path	
if [ -d 'depot_tools' ] && [ -d "depot_tools/.git" ];
then
  echo "depot_tools already installed, skip download"
else
  git clone https://chromium.googlesource.com/chromium/tools/depot_tools.git
  gclient
fi

echo "\nGetting flutter engine..."
if [ ! -d 'engine' ];then
  mkdir engine
  else
  echo "engine folder already exist, skip create"
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

cd src/flutter
git checkout flutter-1.17-candidate.5
gclient sync -D

#apply patch to Build.gn
cd third_party/txt
cp -f $work_path/patches/BUILD.gn.patch BUILD.gn.patch
patch < BUILD.gn.patch -N

echo "\nStarting compile engine..."
cd $engine_path/engine/src/build/mac
cp -f $work_path/patches/find_sdk.patch find_sdk.patch
patch < find_sdk.patch -N
cd ../..
./flutter/tools/gn $gn_params

echo "icu_use_data_file=false" >> out/host_debug_unopt/args.gn
ninja $ninja_params

echo "\nStarting build engine..."
#run mono
cd $work_path 
cd ..
echo "flutter root : $FLUTTER_ROOT"
mono bee.exe mac