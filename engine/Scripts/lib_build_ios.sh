engine_path=$(pwd)
runtime_mode=release

while getopts ":r:m:" opt
do
    case $opt in
        r)
        engine_path=$OPTARG # set engine_path, depot_tools and flutter engine folder will be put into this path
        ;;
        m)
        runtime_mode=$OPTARG
        ;;
        ?)
        echo "unknown param"
        exit 1;;
    esac
done

python3 lib_build.py -r $engine_path -p ios -m $runtime_mode

