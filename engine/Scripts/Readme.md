## build windows lib using prebuild libs
1. copy windows prebuilds under `engine/libs`
1. copy and unzip `headerout.zip` under `engine/libs`
2. build command

    build release
    ```
    .\local_build_win.ps1
    ```
    build debug
    ```
    .\local_build_win.ps1 -mode debug
    ```