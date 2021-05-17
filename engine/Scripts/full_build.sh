engine_path=
platform=
runtime_mode=
gn_params=""
optimize=""
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
        o)
        optimize="-o" # optimize code switch
        ;;
        ?)
        echo "unknown param"
        exit 1;;
    esac
done

case $platform in
    "andorid")  echo 'You select android'
    ;;
    "ios")  ./lib_build_ios.sh -r $engine_path $optimize -p ios -m $runtime_mode $bitcode
    ;;
    "mac")  ./lib_build_mac.sh -m $runtime_mode -r $engine_path $optimize
    ;;
    "windows")  echo 'You select windows'
    ;;
    "all")  echo 'You select all'
    ;;
    *)  echo "unknown platform, only support \"android\",\"ios\",\"mac\",\"windows\""
    ;;
esac

