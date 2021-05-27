@echo off
set current_dir=%~dp0
set runtime_mode=release
set engine_path=%~dp0 
set vs_path="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community"

:GETOPTS
 if /I "%1" == "-m" set runtime_mode=%2 & shift
 if /I "%1" == "-r" set engine_path=%2 & shift
 if /I "%1" == "-v" set vs_path=%2 & shift
 shift
if not "%1" == "" goto GETOPTS

cmd /k python3 lib_build.py -m %runtime_mode% -r %engine_path% -p windows -v %vs_path%
