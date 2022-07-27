## build windows lib using prebuild libs
1. copy and unzip `headerout.zip` under `engine/libs`
2. copy windows prebuilds under `engine/libs/headerout`

|File| Directory|
|---|---|
| icudtl.o|  engine\libs\headerout\third_party\icu\flutter |
| angle_lib.lib|  engine\libs\headerout\out\[host_debug_unopt/host_release]\obj\third_party\angle |
| libEGL_static.lib|  engine\libs\headerout\out\[host_debug_unopt/host_release]\obj\third_party\angle |
| libGLESv2_static.lib|  engine\libs\headerout\out\[host_debug_unopt/host_release]\obj\third_party\angle |
| txt_lib.lib|  engine\libs\headerout\out\[host_debug_unopt/host_release]\obj\flutter\third_party\txt |

3. build command

    build release
    ```
    .\local_build_win.ps1
    ```
    build debug
    ```
    .\local_build_win.ps1 -mode debug
    ```