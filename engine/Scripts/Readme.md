## build windows lib using prebuild libs
1. download `engine_dependencies` under `engine/libs`
2. copy and unzip `headerout.zip` under `engine\libs\engine_dependencies\src`
3. build command

    build release
    ```
    .\local_build_win.ps1
    ```
    build debug
    ```
    .\local_build_win.ps1 -mode debug
    ```