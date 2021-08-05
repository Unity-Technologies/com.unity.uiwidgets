engine_path=$(pwd)
runtime_mode=release
architecture=""
while getopts ":r:m:a:" opt
do
    case $opt in
        r)
        engine_path=$OPTARG # set engine_path, depot_tools and flutter engine folder will be put into this path
        ;;
        m)
        runtime_mode=$OPTARG
        ;;
        a)
        architecture=$OPTARG
        ;;
        ?)
        echo "unknown param"
        exit 1;;
    esac
done

if [ $architecture = "arm64" ]; then
    python3 lib_build.py -r $engine_path -p android -m $runtime_mode --arm64
elif [ ! $architecture  ] || [ $architecture = "arm32" ]; then
    python3 lib_build.py -r $engine_path -p android -m $runtime_mode
else
    echo "If you want to build android arm64, please enter '-a arm64'. \Without entering this, it will build android arm32 by default"
fi
