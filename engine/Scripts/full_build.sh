engine_path=
platform=
runtime_mode
gn_params=""
optimize="--unoptimized"
ninja_params=""
bitcode=

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
        bitcode="--bitcode" # enable-bitcode switch
        ;;
        o)
        optimize="" # optimize code switch
        ;;
        ?)
        echo "unknown param"
        exit 1;;
    esac
done

case $platform in
    "andorid")  echo 'You select android'
    ;;
    "ios")  echo 'You select ios'
    ;;
    "mac")  ./lib_build_mac.sh -m $runtime_mode -r $engine_path
    ;;
    "windows")  echo 'You select windows'
    ;;
    *)  echo "unknown platform, only support \"android\",\"ios\",\"mac\",\"windows\""
    ;;
esac

