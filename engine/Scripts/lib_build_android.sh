engine_path=$(pwd)
runtime_mode=release
architecture=false
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
        architecture=true
        ;;
        ?)
        echo "unknown param"
        exit 1;;
    esac
done

if [ $architecture ]; then
    python3 lib_build.py -r $engine_path -p android -m $runtime_mode --arm64
else
    python3 lib_build.py -r $engine_path -p android -m $runtime_mode
fi
