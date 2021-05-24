engine_path=
platform=
runtime_mode=
gn_params=""
ninja_params=""
bitcode=""

while getopts ":r:p:m:v:e" opt
do
    case $opt in
        r)
        engine_path=$OPTARG # set engine_path, depot_tools and flutter engine folder will be put into this path
        ;;
        p)
        platform=$OPTARG
        ;;
        m)
        runtime_mode=$OPTARG
        ;;
        e)
        bitcode="-e" # enable-bitcode switch
        ;;
        v)
        # do nothing here
        ;;
        ?)
        echo "unknown param"
        exit 1;;
    esac
done

case $platform in
    "android")  ./lib_build_android.sh -r $engine_path -p android -m $runtime_mode
    ;;
    "ios")  ./lib_build_ios.sh -r $engine_path -p ios -m $runtime_mode $bitcode
    ;;
    "mac")  ./lib_build_mac.sh -m $runtime_mode -r $engine_path
    ;;
    "windows")  echo "Please run lib_build_win.py with params directly"
    ;;
    *)  echo "unknown platform, only support \"android\",\"ios\",\"mac\",\"windows\""
    ;;
esac

