# coding=utf-8
import os
import sys
import json

def get_xcode_path():
    res = os.popen('xcode-select -p')
    return res.read()

def get_target_files(tundra_file, runtime_mode):
    if not os.path.exists(tundra_file):
        print('tundra.dag.json file not found')
        return null
    with open(tundra_file, 'r') as f:
        temp = json.loads(f.read())
        json_list = temp['Nodes']
        target_files=''
        for item in json_list:
            if item['Annotation'].startswith('Lib_iOS_arm64') and item['Annotation'].find(runtime_mode) != -1:
                action = item['Action']
                o_file_list = action.split("\"")
                for o in o_file_list:
                    if o.endswith('.o'):
                        target_files += ' '+o
        return target_files
    
def prelinkfiles(tundra_file, runtime_mode, output_path, work_path, bitcode):
    target_files = get_target_files(tundra_file, runtime_mode)
    if not target_files:
        print("get prelink xxx.o files failed")
    else:
        flutter_root_path = os.environ['FLUTTER_ROOT_PATH']
        os.system('nm -j ' + flutter_root_path + '/out/' + output_path + '/obj/flutter/third_party/txt/libtxt_lib.a > third.symbol')
        xcode_path = get_xcode_path().strip()
        os.system('\"' + xcode_path + '/Toolchains/XcodeDefault.xctoolchain/usr/bin/ld" -r -arch arm64 ' + bitcode + ' -syslibroot ' + xcode_path + '/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS.sdk -unexported_symbols_list third.symbol ' + target_files + ' ' + flutter_root_path + '/out/' + output_path + '/obj/flutter/third_party/txt/libtxt_lib.a -o "libUIWidgets.o"')
        os.system('\"' + xcode_path + '/Toolchains/XcodeDefault.xctoolchain/usr/bin/libtool" -arch_only arm64 -static "libUIWidgets.o" -o "libUIWidgets.a"')
        os.system('\"' + xcode_path + '/Toolchains/XcodeDefault.xctoolchain/usr/bin/strip" -x "libUIWidgets.a"')
        os.system('cp -r libUIWidgets.a ' + '../com.unity.uiwidgets/Runtime/Plugins/ios/libUIWidgets.a')

if __name__ == "__main__":
    if len(sys.argv) > 5:
        prelinkfiles(sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5])
    else:
        prelinkfiles(sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4], "")
