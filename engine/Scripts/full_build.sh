engine_path=
platform=
runtime_mode=
gn_params=""
ninja_params=""
bitcode=""

while getopts ":r:p:m:eo" opt
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
    "windows")  echo 'You select windows'
    ;;
    *)  echo "unknown platform, only support \"android\",\"ios\",\"mac\",\"windows\""
    ;;
esac

