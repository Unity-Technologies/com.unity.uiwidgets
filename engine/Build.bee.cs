using System;
using System.Linq;
using Bee.Core;
using Bee.DotNet;
using Bee.ProjectGeneration.VisualStudio;
using Bee.VisualStudioSolution;
using static Bee.NativeProgramSupport.NativeProgramConfiguration;
using Bee.NativeProgramSupport;
using Bee.Tools;
using NiceIO;
using System.Collections.Generic;
using RuntimeInformation = System.Runtime.InteropServices.RuntimeInformation;
using OSPlatform = System.Runtime.InteropServices.OSPlatform;
using Bee.ProjectGeneration.XCode;
using Bee.Toolchain.Xcode;
using Bee.Toolchain.GNU;
using Bee.Toolchain.IOS;
using System.Diagnostics;
using System.IO;
using Bee.Toolchain.Android;

enum UIWidgetsBuildTargetPlatform
{
    windows,
    mac,
    ios,
    android
}

static class BuildUtils
{
    public static bool IsHostWindows()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    public static bool IsHostMac()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }
}

//ios build helpers
class UserIOSSdkLocator : IOSSdkLocator
{
    public UserIOSSdkLocator() : base(Architecture.Arm64) {}

    public IOSSdk UserIOSSdk(NPath path)
    {
        return DefaultSdkFromXcodeApp(path);
    }
}

class IOSAppToolchain : IOSToolchain
{
    //there is a bug in XCodeProjectFile.cs and the class IosPlatform, where the acceptale Platform Name is not matched
    //we workaround this bug by overriding LegacyPlatformIdentifier to the correct one
    public override string LegacyPlatformIdentifier => $"iphone_{Architecture.Name}";

    //copied from com.unity.platforms.ios/Editor/Unity.Platform.Editor/bee~/IOSAppToolchain.cs
    private static NPath _XcodePath = null;

        private static NPath XcodePath
        {
            get
            {
                if (_XcodePath == null)
                {
                    string error = "";

                    try
                    {
                        if (HostPlatform.IsOSX)
                        {
                            var start = new ProcessStartInfo("xcode-select", "-p")
                            {
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                RedirectStandardInput = true,
                                UseShellExecute = false,
                            };
                            string output = "";
                            using (var process = Process.Start(start))
                            {
                                process.OutputDataReceived += (sender, e) => { output += e.Data; };
                                process.ErrorDataReceived += (sender, e) => { error += e.Data; };
                                process.BeginOutputReadLine();
                                process.BeginErrorReadLine();
                                process.WaitForExit(); //? doesn't work correctly if time interval is set
                            }

                            _XcodePath = error == "" ? output : "";
                            if (_XcodePath != "" && _XcodePath.DirectoryExists())
                            {
                                _XcodePath = XcodePath.Parent.Parent;
                            }
                            else
                            {
                                throw new InvalidOperationException("Failed to find Xcode, xcode-select did not return a valid path");
                            }
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine(
                            $"xcode-select did not return a valid path. Error message, if any, was: {error}. " +
                            $"Often this can be fixed by making sure you have Xcode command line tools" +
                            $" installed correctly, and then running `sudo xcode-select -r`");
                        throw e;
                    }
                }

                return _XcodePath;
            }
        }

    //private static string XcodePath = "/Applications/Xcode.app/";
    public IOSAppToolchain() : base((new UserIOSSdkLocator()).UserIOSSdk(XcodePath))
    {
    }
}

/**
*   How to add new target platform (taking iOS as the example)
*   (1) add a new static void DeployIOS() {} in which we need call SetupLibUIWidgets() with UIWidgetsBuildTargetPlatform.ios as its
*       first parameter. We should also generate the corresponding project file (i.e., XcodeProjectFile). Finally, we should call 
*       Backend.Current.AddAliasDependency("ios", dep) to add the alias "ios" to all the final output files for ios platform. By 
*       doing so, we can call "mono bee.exe ios" in command line to tell bee to process the specific subgraph for ios build only.
*       (refer to the following session for the details on this: https://unity.slack.com/archives/C1RM0NBLY/p1615797377297800)
*
*   (2) add the DeployIOS() function inside the available target platforms of MacOS in Main() since the dependencies on ios-platform 
*       , i.e., skia, flutter, etc. can only be built on Mac.
*
*   (3) pick the corresponding toolchains for UIWidgetsBuildTargetPlatform.ios at the end of the function SetupLibUIWidgets() and setup 
*       the build targets.
*
*   (4) change all the build settings (e.g., Defines, Includes, Source files) accordingly with platform-filters like IsMac, IsWindows,
*       IsIosOrTvos inside bee for plaform-dependent settings.
*
*   (5) finally, try call "mono bee.exe" with our predefined platform-dependent alias name "ios" in step (1), i.e., "mono bee.exe ios" 
*       to start the build
**/
class Build
{
    //bee.exe win
    static void DeployWindows()
    {
        var libUIWidgets = SetupLibUIWidgets(UIWidgetsBuildTargetPlatform.windows, out var dependencies_debug, out var dependencies_release);

        var builder = new VisualStudioNativeProjectFileBuilder(libUIWidgets);
        builder = libUIWidgets.SetupConfigurations.Aggregate(
            builder,
            (current, c) => current.AddProjectConfiguration(c));

        var sln = new VisualStudioSolution();
        sln.Path = "libUIWidgets.gen.sln";
        var deployed = builder.DeployTo("libUIWidgets.gen.vcxproj");
        sln.Projects.Add(deployed);
        Backend.Current.AddAliasDependency("ProjectFiles", sln.Setup());

        Backend.Current.AddAliasDependency("win_debug", deployed.Path);
        foreach (var dep in dependencies_debug)
        {
            Backend.Current.AddAliasDependency("win_debug", dep);
        }
        foreach (var dep in dependencies_release)
        {
            Backend.Current.AddAliasDependency("win_release", dep);
        }
    }

    //bee.exe mac
    static void DeployAndroid(bool isArm64 = false)
    {
        buildArm64 = isArm64;
        var libUIWidgets = SetupLibUIWidgets(UIWidgetsBuildTargetPlatform.android, out var dependencies_debug, out var dependencies_release);
        var androidProject = AndroidNativeProgramExtensions.DynamicLinkerSettingsForAndroid(libUIWidgets);
        foreach (var dep in dependencies_debug)
        {
            Backend.Current.AddAliasDependency(isArm64 ? "android_debug_arm64" : "android_debug", dep);
        }
        foreach (var dep in dependencies_release)
        {
            Backend.Current.AddAliasDependency(isArm64 ? "android_release_arm64" : "android_release", dep);
        }
    }

    static void DeployMac()
    {
        var libUIWidgets = SetupLibUIWidgets(UIWidgetsBuildTargetPlatform.mac, out var dependencies_debug, out var dependencies_release);

        var nativePrograms = new List<NativeProgram>();
        nativePrograms.Add(libUIWidgets);
        var xcodeProject = new XCodeProjectFile(nativePrograms, new NPath("libUIWidgetsMac.xcodeproj/project.pbxproj"));

        Backend.Current.AddAliasDependency("mac_debug", new NPath("libUIWidgetsMac.xcodeproj/project.pbxproj"));
        foreach (var dep in dependencies_debug)
        {
            Backend.Current.AddAliasDependency("mac_debug", dep);
        }
        foreach (var dep in dependencies_release)
        {
            Backend.Current.AddAliasDependency("mac_release", dep);
        }
    }

    //bee.exe ios
    static void DeployIOS()
    {
        var libUIWidgets = SetupLibUIWidgets(UIWidgetsBuildTargetPlatform.ios, out var dependencies_debug, out var dependencies_release);
        var nativePrograms = new List<NativeProgram>();
        nativePrograms.Add(libUIWidgets);
        var xcodeProject = new XCodeProjectFile(nativePrograms, new NPath("libUIWidgetsIOS.xcodeproj/project.pbxproj"));

        Backend.Current.AddAliasDependency("ios_debug",  new NPath("libUIWidgetsIOS.xcodeproj/project.pbxproj"));
        foreach(var dep in dependencies_debug) {
            Backend.Current.AddAliasDependency("ios_debug", dep);
        }
        foreach(var dep in dependencies_release) {
            Backend.Current.AddAliasDependency("ios_release", dep);
        }
    }

    static void Main()
    {
        flutterRoot = Environment.GetEnvironmentVariable("FLUTTER_ROOT_PATH");
        if (string.IsNullOrEmpty(flutterRoot))
        {
            flutterRoot = Environment.GetEnvironmentVariable("USERPROFILE") + "/engine/src";
        }
        skiaRoot = flutterRoot + "/third_party/skia";

         try
        {
            if(File.Exists("Scripts/bitcode.conf"))
            {
                using (StreamReader sr = new StreamReader("Scripts/bitcode.conf"))
                {
                    string line;
                    if ((line = sr.ReadLine()) != null)
                    {
                        if(line.Trim() == "true")
                        {
                            ios_bitcode_enabled = true;
                        }
                        else
                        {
                            ios_bitcode_enabled = false;
                        }
                    }
                }
            }
            else
            {
                ios_bitcode_enabled = false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }



        //available target platforms of Windows
        if (BuildUtils.IsHostWindows())
        {
            DeployWindows();
        }
        //available target platforms of MacOS
        else if (BuildUtils.IsHostMac())
        {
            DeployMac();
            DeployAndroid();
            DeployAndroid(true);
            DeployIOS();
        }
    }

    private static string skiaRoot;
    private static string flutterRoot;
    private static bool buildArm64;

    //this setting is disabled by default, don't change it unless you know what you are doing
    //it must be set the same as the settings we choose to build the flutter txt library
    //refer to the readme file for the details
    private static bool ios_bitcode_enabled = false;

    static NativeProgram SetupLibUIWidgets(UIWidgetsBuildTargetPlatform platform, out List<NPath> dependencies_debug, out List<NPath> dependencies_release)
    {
        var np = new NativeProgram("libUIWidgets")
        {
            Sources =
            {
                "src/assets/asset_manager.cc",
                "src/assets/asset_manager.h",
                "src/assets/asset_resolver.h",
                "src/assets/directory_asset_bundle.cc",
                "src/assets/directory_asset_bundle.h",

                "src/common/settings.cc",
                "src/common/settings.h",
                "src/common/task_runners.cc",
                "src/common/task_runners.h",

                "src/flow/layers/backdrop_filter_layer.cc",
                "src/flow/layers/backdrop_filter_layer.h",
                "src/flow/layers/clip_path_layer.cc",
                "src/flow/layers/clip_path_layer.h",
                "src/flow/layers/clip_rect_layer.cc",
                "src/flow/layers/clip_rect_layer.h",
                "src/flow/layers/clip_rrect_layer.cc",
                "src/flow/layers/clip_rrect_layer.h",
                "src/flow/layers/color_filter_layer.cc",
                "src/flow/layers/color_filter_layer.h",
                "src/flow/layers/container_layer.cc",
                "src/flow/layers/container_layer.h",
                "src/flow/layers/image_filter_layer.cc",
                "src/flow/layers/image_filter_layer.h",
                "src/flow/layers/layer.cc",
                "src/flow/layers/layer.h",
                "src/flow/layers/layer_tree.cc",
                "src/flow/layers/layer_tree.h",
                "src/flow/layers/opacity_layer.cc",
                "src/flow/layers/opacity_layer.h",
                "src/flow/layers/performance_overlay_layer.cc",
                "src/flow/layers/performance_overlay_layer.h",
                "src/flow/layers/physical_shape_layer.cc",
                "src/flow/layers/physical_shape_layer.h",
                "src/flow/layers/picture_layer.cc",
                "src/flow/layers/picture_layer.h",
                "src/flow/layers/platform_view_layer.cc",
                "src/flow/layers/platform_view_layer.h",
                "src/flow/layers/shader_mask_layer.cc",
                "src/flow/layers/shader_mask_layer.h",
                "src/flow/layers/texture_layer.cc",
                "src/flow/layers/texture_layer.h",
                "src/flow/layers/transform_layer.cc",
                "src/flow/layers/transform_layer.h",
                "src/flow/compositor_context.cc",
                "src/flow/compositor_context.h",
                "src/flow/embedded_views.cc",
                "src/flow/embedded_views.h",
                "src/flow/instrumentation.cc",
                "src/flow/instrumentation.h",
                "src/flow/matrix_decomposition.cc",
                "src/flow/matrix_decomposition.h",
                "src/flow/paint_utils.cc",
                "src/flow/paint_utils.h",
                "src/flow/raster_cache.cc",
                "src/flow/raster_cache.h",
                "src/flow/raster_cache_key.cc",
                "src/flow/raster_cache_key.h",
                "src/flow/rtree.cc",
                "src/flow/rtree.h",
                "src/flow/skia_gpu_object.cc",
                "src/flow/skia_gpu_object.h",
                "src/flow/texture.cc",
                "src/flow/texture.h",

                "src/lib/ui/compositing/scene.cc",
                "src/lib/ui/compositing/scene.h",
                "src/lib/ui/compositing/scene_builder.cc",
                "src/lib/ui/compositing/scene_builder.h",


                "src/lib/ui/text/icu_util.h",
                "src/lib/ui/text/icu_util.cc",
                "src/lib/ui/text/asset_manager_font_provider.cc",
                "src/lib/ui/text/asset_manager_font_provider.h",
                "src/lib/ui/text/paragraph_builder.cc",
                "src/lib/ui/text/paragraph_builder.h",
                "src/lib/ui/text/font_collection.cc",
                "src/lib/ui/text/font_collection.h",
                "src/lib/ui/text/paragraph.cc",
                "src/lib/ui/text/paragraph.h",

                "src/lib/ui/painting/canvas.cc",
                "src/lib/ui/painting/canvas.h",
                "src/lib/ui/painting/codec.cc",
                "src/lib/ui/painting/codec.h",
                "src/lib/ui/painting/color_filter.cc",
                "src/lib/ui/painting/color_filter.h",
                "src/lib/ui/painting/engine_layer.cc",
                "src/lib/ui/painting/engine_layer.h",
                "src/lib/ui/painting/frame_info.cc",
                "src/lib/ui/painting/frame_info.h",
                "src/lib/ui/painting/gradient.cc",
                "src/lib/ui/painting/gradient.h",
                "src/lib/ui/painting/image.cc",
                "src/lib/ui/painting/image.h",
                "src/lib/ui/painting/image_decoder.cc",
                "src/lib/ui/painting/image_decoder.h",
                "src/lib/ui/painting/image_encoding.cc",
                "src/lib/ui/painting/image_encoding.h",
                "src/lib/ui/painting/image_filter.cc",
                "src/lib/ui/painting/image_filter.h",
                "src/lib/ui/painting/image_shader.cc",
                "src/lib/ui/painting/image_shader.h",
                "src/lib/ui/painting/matrix.cc",
                "src/lib/ui/painting/matrix.h",
                "src/lib/ui/painting/multi_frame_codec.cc",
                "src/lib/ui/painting/multi_frame_codec.h",
                "src/lib/ui/painting/path.cc",
                "src/lib/ui/painting/path.h",
                "src/lib/ui/painting/path_measure.cc",
                "src/lib/ui/painting/path_measure.h",
                "src/lib/ui/painting/paint.cc",
                "src/lib/ui/painting/paint.h",
                "src/lib/ui/painting/picture.cc",
                "src/lib/ui/painting/picture.h",
                "src/lib/ui/painting/picture_recorder.cc",
                "src/lib/ui/painting/picture_recorder.h",
                "src/lib/ui/painting/rrect.cc",
                "src/lib/ui/painting/rrect.h",
                "src/lib/ui/painting/shader.cc",
                "src/lib/ui/painting/shader.h",
                "src/lib/ui/painting/single_frame_codec.cc",
                "src/lib/ui/painting/single_frame_codec.h",
                "src/lib/ui/painting/skottie.cc",
                "src/lib/ui/painting/skottie.h",
                "src/lib/ui/painting/vertices.cc",
                "src/lib/ui/painting/vertices.h",

                "src/lib/ui/window/platform_message_response_mono.cc",
                "src/lib/ui/window/platform_message_response_mono.h",
                "src/lib/ui/window/platform_message_response.cc",
                "src/lib/ui/window/platform_message_response.h",
                "src/lib/ui/window/platform_message.cc",
                "src/lib/ui/window/platform_message.h",
                "src/lib/ui/window/pointer_data.cc",
                "src/lib/ui/window/pointer_data.h",
                "src/lib/ui/window/pointer_data_packet.cc",
                "src/lib/ui/window/pointer_data_packet.h",
                "src/lib/ui/window/pointer_data_packet_converter.cc",
                "src/lib/ui/window/pointer_data_packet_converter.h",
                "src/lib/ui/window/viewport_metrics.cc",
                "src/lib/ui/window/viewport_metrics.h",
                "src/lib/ui/window/window.cc",
                "src/lib/ui/window/window.h",

                "src/lib/ui/io_manager.h",
                "src/lib/ui/snapshot_delegate.h",
                "src/lib/ui/ui_mono_state.cc",
                "src/lib/ui/ui_mono_state.h",

                "src/runtime/mono_api.cc",
                "src/runtime/mono_api.h",
                "src/runtime/mono_isolate.cc",
                "src/runtime/mono_isolate.h",
                "src/runtime/mono_isolate_scope.cc",
                "src/runtime/mono_isolate_scope.h",
                "src/runtime/mono_microtask_queue.cc",
                "src/runtime/mono_microtask_queue.h",
                "src/runtime/mono_state.cc",
                "src/runtime/mono_state.h",
                "src/runtime/runtime_controller.cc",
                "src/runtime/runtime_controller.h",
                "src/runtime/runtime_delegate.cc",
                "src/runtime/runtime_delegate.h",
                "src/runtime/start_up.cc",
                "src/runtime/start_up.h",
                "src/runtime/window_data.cc",
                "src/runtime/window_data.h",

                "src/shell/common/animator.cc",
                "src/shell/common/animator.h",
                "src/shell/common/canvas_spy.cc",
                "src/shell/common/canvas_spy.h",
                "src/shell/common/engine.cc",
                "src/shell/common/engine.h",
                "src/shell/common/lists.h",
                "src/shell/common/lists.cc",
                "src/shell/common/persistent_cache.cc",
                "src/shell/common/persistent_cache.h",
                "src/shell/common/pipeline.cc",
                "src/shell/common/pipeline.h",
                "src/shell/common/platform_view.cc",
                "src/shell/common/platform_view.h",
                "src/shell/common/pointer_data_dispatcher.cc",
                "src/shell/common/pointer_data_dispatcher.h",
                "src/shell/common/rasterizer.cc",
                "src/shell/common/rasterizer.h",
                "src/shell/common/run_configuration.cc",
                "src/shell/common/run_configuration.h",
                "src/shell/common/shell.cc",
                "src/shell/common/shell.h",
                "src/shell/common/shell_io_manager.cc",
                "src/shell/common/shell_io_manager.h",
                "src/shell/common/surface.cc",
                "src/shell/common/surface.h",
                "src/shell/common/switches.cc",
                "src/shell/common/switches.h",
                "src/shell/common/thread_host.cc",
                "src/shell/common/thread_host.h",
                "src/shell/common/vsync_waiter.cc",
                "src/shell/common/vsync_waiter.h",
                "src/shell/common/vsync_waiter_fallback.cc",
                "src/shell/common/vsync_waiter_fallback.h",

                "src/shell/gpu/gpu_surface_delegate.h",
                "src/shell/gpu/gpu_surface_gl.cc",
                "src/shell/gpu/gpu_surface_gl.h",
                "src/shell/gpu/gpu_surface_gl_delegate.cc",
                "src/shell/gpu/gpu_surface_gl_delegate.h",
                "src/shell/gpu/gpu_surface_software.cc",
                "src/shell/gpu/gpu_surface_software.h",
                "src/shell/gpu/gpu_surface_software_delegate.cc",
                "src/shell/gpu/gpu_surface_software_delegate.h",

                "src/shell/platform/embedder/embedder.cc",
                "src/shell/platform/embedder/embedder.h",
                "src/shell/platform/embedder/embedder_engine.cc",
                "src/shell/platform/embedder/embedder_engine.h",
                "src/shell/platform/embedder/embedder_external_texture_gl.cc",
                "src/shell/platform/embedder/embedder_external_texture_gl.h",
                "src/shell/platform/embedder/embedder_external_view.cc",
                "src/shell/platform/embedder/embedder_external_view.h",
                "src/shell/platform/embedder/embedder_external_view_embedder.cc",
                "src/shell/platform/embedder/embedder_external_view_embedder.h",
                "src/shell/platform/embedder/embedder_layers.cc",
                "src/shell/platform/embedder/embedder_layers.h",
                "src/shell/platform/embedder/embedder_platform_message_response.cc",
                "src/shell/platform/embedder/embedder_platform_message_response.h",
                "src/shell/platform/embedder/embedder_render_target.cc",
                "src/shell/platform/embedder/embedder_render_target.h",
                "src/shell/platform/embedder/embedder_render_target_cache.cc",
                "src/shell/platform/embedder/embedder_render_target_cache.h",
                "src/shell/platform/embedder/embedder_surface.cc",
                "src/shell/platform/embedder/embedder_surface.h",
                "src/shell/platform/embedder/embedder_surface_gl.cc",
                "src/shell/platform/embedder/embedder_surface_gl.h",
                "src/shell/platform/embedder/embedder_surface_software.cc",
                "src/shell/platform/embedder/embedder_surface_software.h",
                "src/shell/platform/embedder/embedder_task_runner.cc",
                "src/shell/platform/embedder/embedder_task_runner.h",
                "src/shell/platform/embedder/embedder_thread_host.cc",
                "src/shell/platform/embedder/embedder_thread_host.h",
                "src/shell/platform/embedder/platform_view_embedder.cc",
                "src/shell/platform/embedder/platform_view_embedder.h",
                "src/shell/platform/embedder/vsync_waiter_embedder.cc",
                "src/shell/platform/embedder/vsync_waiter_embedder.h",

                "src/shell/platform/unity/gfx_worker_task_runner.cc",
                "src/shell/platform/unity/gfx_worker_task_runner.h",
                "src/shell/platform/unity/uiwidgets_system.h",

              
                "src/shell/platform/unity/unity_console.cc",
                "src/shell/platform/unity/unity_console.h",

                "src/shell/version/version.cc",
                "src/shell/version/version.h",

                "src/engine.cc",
                "src/platform_base.h",
            },
            OutputName = { c => "libUIWidgets" },
        };

        // include these files for test only
        var testSources = new NPath[] {
                "src/tests/render_engine.cc",
                "src/tests/render_api.cc",
                "src/tests/render_api.h",
                "src/tests/render_api_d3d11.cc",      // test d3d rendering
                "src/tests/render_api_vulkan.cc",     // test vulkan rendering 
                "src/tests/render_api_opengles.cc",   // test opengles rendering
                "src/tests/TestLoadICU.cpp",          // test ICU
        };

        var winSources = new NPath[] {
                "src/shell/platform/unity/windows/uiwidgets_panel.cc",
                "src/shell/platform/unity/windows/uiwidgets_panel.h",
                "src/shell/platform/unity/windows/uiwidgets_system.cc",
                "src/shell/platform/unity/windows/uiwidgets_system.h",
                "src/shell/platform/unity/windows/unity_external_texture_gl.cc",
                "src/shell/platform/unity/windows/unity_external_texture_gl.h",
                "src/shell/platform/unity/windows/unity_surface_manager.cc",
                "src/shell/platform/unity/windows/unity_surface_manager.h",
                "src/shell/platform/unity/windows/win32_task_runner.cc",
                "src/shell/platform/unity/windows/win32_task_runner.h",
        };

        var macSources = new NPath[] {
                "src/shell/platform/unity/darwin/macos/uiwidgets_panel.mm",
                "src/shell/platform/unity/darwin/macos/uiwidgets_panel.h",
                "src/shell/platform/unity/darwin/macos/uiwidgets_system.mm",
                "src/shell/platform/unity/darwin/macos/uiwidgets_system.h",
                "src/shell/platform/unity/darwin/macos/cocoa_task_runner.cc",
                "src/shell/platform/unity/darwin/macos/cocoa_task_runner.h",
                "src/shell/platform/unity/darwin/macos/unity_surface_manager.mm",
                "src/shell/platform/unity/darwin/macos/unity_surface_manager.h",
                "src/shell/platform/unity/darwin/macos/unity_external_texture_gl.mm",
                "src/shell/platform/unity/darwin/macos/unity_external_texture_gl.h"
        };

        var androidSource = new NPath[]
        {
            "src/shell/platform/unity/android_unpack_streaming_asset.cc",
            "src/shell/platform/unity/android_unpack_streaming_asset.h",
            "src/shell/platform/unity/android/unity_surface_manager.cc",
            "src/shell/platform/unity/android/uiwidgets_system.cc",
            "src/shell/platform/unity/android/android_task_runner.cc",
            "src/shell/platform/unity/android/uiwidgets_panel.cc",
            "src/shell/platform/unity/android/unity_external_texture_gl.cc"
        };

        var iosSources = new NPath[] {
                "src/shell/platform/unity/darwin/ios/uiwidgets_panel.mm",
                "src/shell/platform/unity/darwin/ios/uiwidgets_panel.h",
                "src/shell/platform/unity/darwin/ios/uiwidgets_system.mm",
                "src/shell/platform/unity/darwin/ios/uiwidgets_system.h",
                "src/shell/platform/unity/darwin/ios/cocoa_task_runner.cc",
                "src/shell/platform/unity/darwin/ios/cocoa_task_runner.h",
                "src/shell/platform/unity/darwin/ios/unity_surface_manager.mm",
                "src/shell/platform/unity/darwin/ios/unity_surface_manager.h",
                "src/shell/platform/unity/darwin/ios/unity_external_texture_gl.mm",
                "src/shell/platform/unity/darwin/ios/unity_external_texture_gl.h"
        };

        np.Sources.Add(c => IsWindows(c), winSources);
        np.Sources.Add(c => IsMac(c), macSources);
        np.Sources.Add(c => IsIosOrTvos(c), iosSources);
        np.Sources.Add(c => IsAndroid(c), androidSource);

        np.Libraries.Add(c => IsWindows(c), new BagOfObjectFilesLibrary(
            new NPath[]{
                flutterRoot + "/third_party/icu/flutter/icudtl.o"
        }));
        np.CompilerSettings().Add(c => c.WithCppLanguageVersion(CppLanguageVersion.Cpp17));
        np.CompilerSettings().Add(c => IsMac(c) || IsIosOrTvos(c), c => c.WithCustomFlags(new []{"-Wno-c++11-narrowing"}));

        if (ios_bitcode_enabled) {
            np.CompilerSettingsForIosOrTvos().Add(c => c.WithEmbedBitcode(true));
        }

        np.Defines.Add(c => IsAndroid(c), new[] {
            "USE_OPENSSL=1",
            "USE_OPENSSL_CERTS=1",
            "ANDROID",
            "HAVE_SYS_UIO_H",
            "__STDC_CONSTANT_MACROS",
            "__STDC_FORMAT_MACROS",
            // "_FORTIFY_SOURCE=2",
            "__compiler_offsetof=__builtin_offsetof",
            "nan=__builtin_nan",
            "__GNU_SOURCE=1",
            "_LIBCPP_DISABLE_VISIBILITY_ANNOTATIONS",
            "_LIBCPP_ENABLE_THREAD_SAFETY_ANNOTATIONS",
            "_DEBUG",
            "U_USING_ICU_NAMESPACE=0",
            "U_ENABLE_DYLOAD=0",
            "USE_CHROMIUM_ICU=1",
            "U_STATIC_IMPLEMENTATION",
            "ICU_UTIL_DATA_IMPL=ICU_UTIL_DATA_FILE",
            "UCHAR_TYPE=uint16_t",
            "FLUTTER_RUNTIME_MODE_DEBUG=1",
            "FLUTTER_RUNTIME_MODE_PROFILE=2",
            "FLUTTER_RUNTIME_MODE_RELEASE=3",
            "FLUTTER_RUNTIME_MODE_JIT_RELEASE=4",
            "FLUTTER_RUNTIME_MODE=1",
            "FLUTTER_JIT_RUNTIME=1",

            // confige for rapidjson
            "UIWIDGETS_FORCE_ALIGNAS_8=\"1\"",
            "RAPIDJSON_HAS_STDSTRING",
            "RAPIDJSON_HAS_CXX11_RANGE_FOR",
            "RAPIDJSON_HAS_CXX11_RVALUE_REFS",
            "RAPIDJSON_HAS_CXX11_TYPETRAITS",
            "RAPIDJSON_HAS_CXX11_NOEXCEPT",
            "SK_ENABLE_SPIRV_VALIDATION",
            "SK_GAMMA_APPLY_TO_A8",
            "SK_GAMMA_EXPONENT=1.4",
            "SK_GAMMA_CONTRAST=0.0",
            "SK_ALLOW_STATIC_GLOBAL_INITIALIZERS=1",
            "GR_TEST_UTILS=1",
            "SKIA_IMPLEMENTATION=1",
            "SK_GL",
            "SK_ENABLE_DUMP_GPU",
            "SK_SUPPORT_PDF",
            "SK_CODEC_DECODES_JPEG",
            "SK_ENCODE_JPEG",
            "SK_ENABLE_ANDROID_UTILS",
            "SK_USE_LIBGIFCODEC",
            "SK_HAS_HEIF_LIBRARY",
            "SK_CODEC_DECODES_PNG",
            "SK_ENCODE_PNG",
            "SK_CODEC_DECODES_RAW",
            "SK_ENABLE_SKSL_INTERPRETER",
            "SK_CODEC_DECODES_WEBP",
            "SK_ENCODE_WEBP",
            "SK_XML",


            //"UIWIDGETS_ENGINE_VERSION=\"0.0\"",
            //"SKIA_VERSION=\"0.0\"",
            "XML_STATIC",
        });
        np.CompilerSettings().Add(c => IsAndroid(c), c =>
            {
                var flags = new List<string>{
                    "-MD",
                    "-MF",

                    "-I.",
                    "-Ithird_party",
                    "-Isrc",
                    "-I" + flutterRoot,
                    "-I" + flutterRoot + "/third_party/rapidjson/include",
                    "-I" + skiaRoot,
                    "-I" + skiaRoot + "/include/third_party/vulkan",
                    "-I" + flutterRoot + "/flutter/third_party/txt/src",
                    "-I" + flutterRoot + "/third_party/harfbuzz/src",
                    "-I" + skiaRoot + "/third_party/externals/icu/source/common",

                    // "-Igen",
                    "-I" + flutterRoot + "/third_party/libcxx/include",
                    "-I" + flutterRoot + "/third_party/libcxxabi/include",
                    "-I" + flutterRoot + "/third_party/icu/source/common",
                    "-I" + flutterRoot + "/third_party/icu/source/i18n",

                    // ignore deprecated code
                    "-Wno-deprecated-declarations",

                    "-fno-strict-aliasing",
                    "-mfloat-abi=softfp",
                    "-mthumb",
                    "-fPIC",
                    "-pipe",
                    "-fcolor-diagnostics",
                    "-ffunction-sections",
                    "-funwind-tables",
                    "-fno-short-enums",
                    "-nostdinc++",
                    "-mfpu=neon",
                    "-Wall",
                    "-Wextra",
                    "-Wendif-labels",
                    "-Werror",
                    "-Wno-missing-field-initializers",
                    "-Wno-unused-parameter",
                    "-Wno-unused-variable",
                    "-Wno-unused-command-line-argument",
                    "-Wno-unused-function",
                    // "-Wno-non-c-typedef-for-linkage",
                    "-isystem" + flutterRoot + "/third_party/android_tools/ndk/sources/android/support/include",
                    // "-isystem" + flutterRoot +
                    //"-D__ANDROID_API__=16",
                    // "-fvisibility=hidden",
                    "--sysroot=" + flutterRoot + "/third_party/android_tools/ndk/sysroot",
                    "-Wstring-conversion",
                    // supress new line error
                    // "-Wnewline-eof",
                    "-O0",
                    "-g2",
                    "-fvisibility-inlines-hidden",
                    "-std=c++17",
                    "-fno-rtti",
                    "-fno-exceptions",
                    "-nostdlib"
                };
                if (buildArm64)
                {
                    flags.Add(new []
                    {
                        "--target=aarch64-linux-android",
                        "-isystem"+ flutterRoot +
                        "/third_party/android_tools/ndk/sysroot/usr/include/aarch64-linux-android",

                    });
                }
                else
                {
                    flags.Add(new []
                    {
                         "-march=armv7-a",
                        "-mtune=generic-armv7-a",
                         "--target=arm-linux-androideabi",
                         "-isystem"+ flutterRoot +
                        "/third_party/android_tools/ndk/sysroot/usr/include/arm-linux-androideabi",
                    });
                }
                return  c.WithCustomFlags(flags.ToArray());
            }
        );

        np.IncludeDirectories.Add("third_party");
        np.IncludeDirectories.Add("src");

        np.Defines.Add("UIWIDGETS_ENGINE_VERSION=\\\"0.0\\\"", "SKIA_VERSION=\\\"0.0\\\"");
        np.Defines.Add(c => IsMac(c) || IsIosOrTvos(c), "UIWIDGETS_FORCE_ALIGNAS_8=\\\"1\\\"");

        //since we support both metal and opengl core backends on Mac, it will cause error in Mac Editor when enabling 
        //the default persistent cache in the native UIWidgets engine. 
        //
        //specifically, if the user first set the graphics backend as metal and open an UIWidgets project, the underline
        //native UIWidgets engine will cache the shader files generated for metal from some glsl codes with the cache key
        //as md5 of the codes. 
        //then if the user reopen the project using the same Editor with opengl core bankend, using the same md5 key, 
        //the UIWidgets engine will mistakenly reuse the cached metal shader files for opengl core.
        //
        //since this scenario (i.e., switching between different graphics backends for the same App) rarely happens for
        //flutter, we have to find out a solution for UIWidgets engine. For now, our solution is as follows:
        //
        //Disable the persistent cache (via force its IsValid() method to always return false) in UIWidget engine for Mac
        //note that there might be some performance issue (mainly first frame delay) after this fix on Mac
        np.Defines.Add(c => IsMac(c), "UIWIDGETS_FORCE_DISABLE_PERSISTENTCACHE=\\\"1\\\"");

        np.Defines.Add(c => c.CodeGen == CodeGen.Debug,
            new[] { "_ITERATOR_DEBUG_LEVEL=2", "_HAS_ITERATOR_DEBUGGING=1", "_SECURE_SCL=1" });

        np.Defines.Add(c => c.CodeGen == CodeGen.Release,
            new[] { "UIWidgets_RELEASE=1" });

        np.LinkerSettings().Add(c => {
            return IsWindows(c) && c.CodeGen == CodeGen.Debug;
        }, l => l.WithCustomFlags_workaround(new[] { "/DEBUG:FULL" }));

        np.LinkerSettings().Add(c => IsAndroid(c), l =>
        {
            var flags = new List<string>()
            {
                "-Wl,--fatal-warnings",
                "-fPIC",
                "-Wl,-z,noexecstack",
                "-Wl,-z,now",
                "-Wl,-z,relro",
                "-Wl,-z,defs",
                "-Wl,--no-undefined",
                "-Wl,--exclude-libs,ALL",
                "-fuse-ld=lld",
                "-Wl,--icf=all",
                "-nostdlib++",
                "-Wl,--warn-shared-textrel",
                "-nostdlib",
                "-Wl,--build-id=sha1",
                "-g",
                "-Wl,-soname=libUIWidgets.so",
                "-Wl,--whole-archive",
            };
            
            if (buildArm64)
            {
                flags.Add(new []
                {
                    "--gcc-toolchain="+ flutterRoot +
                    "/third_party/android_tools/ndk/toolchains/aarch64-linux-android-4.9/prebuilt/darwin-x86_64",
                    "--sysroot="+ flutterRoot+"/third_party/android_tools/ndk/platforms/android-22/arch-arm64",
                    "-L"+ flutterRoot + "/third_party/android_tools/ndk/sources/cxx-stl/llvm-libc++/libs/arm64-v8a",
                    "-L/Users/siyao/Documents/GitHub/com.unity.uiwidgets/engine/Scripts/engine/src/third_party/android_tools/ndk/toolchains/aarch64-linux-android-4.9/prebuilt/darwin-x86_64/lib/gcc/aarch64-linux-android/4.9.x",
                });
            }
            else
            {
                flags.Add(new []
                {
                    "--gcc-toolchain="+ flutterRoot +
                    "/third_party/android_tools/ndk/toolchains/arm-linux-androideabi-4.9/prebuilt/darwin-x86_64",
                    "--target=arm-linux-androideabi",
                    "--sysroot="+ flutterRoot+"/third_party/android_tools/ndk/platforms/android-16/arch-arm",
                    "-L"+ flutterRoot + "/third_party/android_tools/ndk/sources/cxx-stl/llvm-libc++/libs/armeabi-v7a",
                });
            }
            return l.WithCustomFlags_workaround(flags.ToArray());
        });
        
        SetupDependency(np);
        //SetupFml(np);
        //SetupSkia(np);
        //SetupTxt(np);
        var codegens = new[] { CodeGen.Debug, CodeGen.Release };
        dependencies_debug = new List<NPath>();
        dependencies_release = new List<NPath>();
        if (platform == UIWidgetsBuildTargetPlatform.windows)
        {
            var toolchain = ToolChain.Store.Windows().VS2019().Sdk_17134().x64();

            foreach (var codegen in codegens)
            {
                var config = new NativeProgramConfiguration(codegen, toolchain, lump: true);

            
                if(codegen == CodeGen.Debug)
                {
                    var builtNP = np.SetupSpecificConfiguration(config, toolchain.DynamicLibraryFormat)
                    .DeployTo("build_debug");
                    dependencies_debug.Add(builtNP.Path);
                }
                else if(codegen == CodeGen.Release)
                {
                    var builtNP = np.SetupSpecificConfiguration(config, toolchain.DynamicLibraryFormat)
                    .DeployTo("build_release");
                    dependencies_release.Add(builtNP.Path);
                }
                
            }
        }
        else if (platform == UIWidgetsBuildTargetPlatform.android)
        {
            AndroidNdkToolchain androidToolchain = null;
            if (buildArm64)
            {
                androidToolchain = ToolChain.Store.Android().r19().Arm64();
            }
            else
            { 
                androidToolchain = ToolChain.Store.Android().r19().Armv7();
            }

            var validConfigurations = new List<NativeProgramConfiguration>();

            foreach (var codegen in codegens)
            {
                var config = new NativeProgramConfiguration(codegen, androidToolchain, lump: true);
                validConfigurations.Add(config);

                
                if(codegen == CodeGen.Debug)
                {
                    var buildNP = np.SetupSpecificConfiguration(config, androidToolchain.DynamicLibraryFormat)
                        .DeployTo(buildArm64 ? "build_debug_arm64" : "build_debug");
                    dependencies_debug.Add(buildNP.Path);
                }
                else if(codegen == CodeGen.Release)
                {
                    var buildNP = np.SetupSpecificConfiguration(config, androidToolchain.DynamicLibraryFormat)
                        .DeployTo(buildArm64 ? "build_release_64" : "build_release");
                    dependencies_release.Add(buildNP.Path);
                }
                
            }
            np.ValidConfigurations = validConfigurations;
        }
        else if (platform == UIWidgetsBuildTargetPlatform.mac)
        {
            var toolchain = ToolChain.Store.Host();
            var validConfigurations = new List<NativeProgramConfiguration>();
            foreach (var codegen in codegens)
            {
                var config = new NativeProgramConfiguration(codegen, toolchain, lump: true);
                validConfigurations.Add(config);

                var buildProgram = np.SetupSpecificConfiguration(config, toolchain.DynamicLibraryFormat);
                
                if(codegen == CodeGen.Debug)
                {
                    var buildNp = buildProgram.DeployTo("build_debug");
                    dependencies_debug.Add(buildNp.Path);
                }
                else if(codegen == CodeGen.Release)
                {
                    var buildNp = buildProgram.DeployTo("build_release");
                    dependencies_release.Add(buildNp.Path);
                }
            }
            np.ValidConfigurations = validConfigurations;

        }
        else if (platform == UIWidgetsBuildTargetPlatform.ios)
        {
            var toolchain = new IOSAppToolchain(); 
            var validConfigurations = new List<NativeProgramConfiguration>();
            foreach (var codegen in codegens)
            {
                var config = new NativeProgramConfiguration(codegen, toolchain, lump: true);
                validConfigurations.Add(config);
                var buildProgram = np.SetupSpecificConfiguration(config, toolchain.StaticLibraryFormat);
                
                if(codegen == CodeGen.Debug)
                {
                    var builtNP = buildProgram.DeployTo("build_debug");
                    dependencies_debug.Add(builtNP.Path);
                }
                else if(codegen == CodeGen.Release)
                {
                    var builtNP = buildProgram.DeployTo("build_release");
                    dependencies_release.Add(builtNP.Path);
                }
            }

            np.ValidConfigurations = validConfigurations;
        }

        return np;
    }

    static void SetupDependency(NativeProgram np)
    {
        SetupRadidJson(np);

        np.Defines.Add(c => IsIosOrTvos(c), new []
        {
            //lib flutter
            "__STDC_CONSTANT_MACROS",
            "__STDC_FORMAT_MACROS",
            "_FORTIFY_SOURCE=2",
            "_LIBCPP_DISABLE_VISIBILITY_ANNOTATIONS",
            "_LIBCPP_ENABLE_THREAD_SAFETY_ANNOTATIONS",
            "_DEBUG",
            "FLUTTER_RUNTIME_MODE_DEBUG=1",
            "FLUTTER_RUNTIME_MODE_PROFILE=2",
            "FLUTTER_RUNTIME_MODE_RELEASE=3",
            "FLUTTER_RUNTIME_MODE_JIT_RELEASE=4",
            "FLUTTER_RUNTIME_MODE=1",
            "FLUTTER_JIT_RUNTIME=1",

            //lib skia
            "SK_ENABLE_SPIRV_VALIDATION",
            "SK_ASSUME_GL_ES=1","SK_ENABLE_API_AVAILABLE",
            "SK_GAMMA_APPLY_TO_A8",
            "SK_ALLOW_STATIC_GLOBAL_INITIALIZERS=1",
            "GR_TEST_UTILS=1",
            "SKIA_IMPLEMENTATION=1",
            "SK_GL",
            "SK_ENABLE_DUMP_GPU",
            "SK_SUPPORT_PDF",
            "SK_CODEC_DECODES_JPEG",
            "SK_ENCODE_JPEG",
            "SK_ENABLE_ANDROID_UTILS",
            "SK_USE_LIBGIFCODEC",
            "SK_HAS_HEIF_LIBRARY",
            "SK_CODEC_DECODES_PNG",
            "SK_ENCODE_PNG",
            "SK_CODEC_DECODES_RAW",
            "SK_ENABLE_SKSL_INTERPRETER",
            "SK_CODEC_DECODES_WEBP",
            "SK_ENCODE_WEBP",
            "SK_XML",
        });

        np.Defines.Add(c => IsIosOrTvos(c), new[] { 
            "__STDC_CONSTANT_MACROS",
            "__STDC_FORMAT_MACROS",
            "_FORTIFY_SOURCE=2",
            "_LIBCPP_DISABLE_VISIBILITY_ANNOTATIONS",
            "_LIBCPP_ENABLE_THREAD_SAFETY_ANNOTATIONS",
            "_DEBUG",
            "SK_GL",
            "SK_METAL",
            "SK_ENABLE_DUMP_GPU",
            "SK_CODEC_DECODES_JPEG",
            "SK_ENCODE_JPEG",
            "SK_CODEC_DECODES_PNG",
            "SK_ENCODE_PNG",
            "SK_CODEC_DECODES_WEBP",
            "SK_ENCODE_WEBP",
            "SK_HAS_WUFFS_LIBRARY",
            "FLUTTER_RUNTIME_MODE_DEBUG=1",
            "FLUTTER_RUNTIME_MODE_PROFILE=2",
            "FLUTTER_RUNTIME_MODE_RELEASE=3",
            "FLUTTER_RUNTIME_MODE_JIT_RELEASE=4",
            "FLUTTER_RUNTIME_MODE=1",
            "FLUTTER_JIT_RUNTIME=1",
            "U_USING_ICU_NAMESPACE=0",
            "U_ENABLE_DYLOAD=0",
            "USE_CHROMIUM_ICU=1",
            "U_STATIC_IMPLEMENTATION",
            "ICU_UTIL_DATA_IMPL=ICU_UTIL_DATA_FILE",
            "UCHAR_TYPE=uint16_t",
            "SK_DISABLE_REDUCE_OPLIST_SPLITTING",
            "SK_ENABLE_DUMP_GPU",
            "SK_DISABLE_AAA",
            "SK_DISABLE_READBUFFER",
            "SK_DISABLE_EFFECT_DESERIALIZATION",
            "SK_DISABLE_LEGACY_SHADERCONTEXT",
            "SK_DISABLE_LOWP_RASTER_PIPELINE",
            "SK_FORCE_RASTER_PIPELINE_BLITTER",
            "SK_GL",
            "SK_ASSUME_GL_ES=1",
            "SK_ENABLE_API_AVAILABLE"
        });


        np.CompilerSettings().Add(c => IsIosOrTvos(c), c => c.WithCustomFlags(new[] {
            "-MD",
            "-MF",

            "-I.",
            "-Ithird_party",
            "-Isrc",
            "-I"+ flutterRoot,
            "-I"+ flutterRoot+"/third_party/rapidjson/include",
            "-I"+ skiaRoot,
            "-I"+ flutterRoot+"/flutter/third_party/txt/src",
            "-I" + flutterRoot + "/third_party/harfbuzz/src",
            "-I" + skiaRoot + "/third_party/externals/icu/source/common",

            // "-Igen",
            "-I"+ flutterRoot+"/third_party/icu/source/common",
            "-I"+ flutterRoot+"/third_party/icu/source/i18n",

            "-fvisibility-inlines-hidden",
        }));

        np.Libraries.Add(IsIosOrTvos, c => {
            return new PrecompiledLibrary[]
            {
                new SystemFramework("CoreFoundation"),
                new SystemFramework("ImageIO"),
                new SystemFramework("MobileCoreServices"),
                new SystemFramework("CoreGraphics"),
                new SystemFramework("CoreText"),
                new SystemFramework("UIKit"),
            };
        });

        // TODO: fix warning, there are some type mismatches
        var ignoreWarnigs = new string[] { "4244", "4267", "5030", "4101", "4996", "4359", "4018", "4091", "4722", "4312", "4838", "4172", "4005", "4311", "4477" };
        np.CompilerSettings().Add(c => IsWindows(c), s => s.WithWarningPolicies(ignoreWarnigs.Select((code) => new WarningAndPolicy(code, WarningPolicy.Silent)).ToArray()));

        np.Defines.Add(c => IsMac(c), new[]
        {
            //lib flutter
            "USE_OPENSSL=1",
            "__STDC_CONSTANT_MACROS",
            "__STDC_FORMAT_MACROS",
            "_FORTIFY_SOURCE=2",
            "_LIBCPP_DISABLE_AVAILABILITY=1",
            "_LIBCPP_DISABLE_VISIBILITY_ANNOTATIONS",
            "_LIBCPP_ENABLE_THREAD_SAFETY_ANNOTATIONS",
            "_DEBUG",
            "FLUTTER_RUNTIME_MODE_DEBUG=1",
            "FLUTTER_RUNTIME_MODE_PROFILE=2",
            "FLUTTER_RUNTIME_MODE_RELEASE=3",
            "FLUTTER_RUNTIME_MODE_JIT_RELEASE=4",
            "FLUTTER_RUNTIME_MODE=1",
            "FLUTTER_JIT_RUNTIME=1",

            //lib skia
            "SK_ENABLE_SPIRV_VALIDATION",
            "SK_ASSUME_GL=1",
            "SK_ENABLE_API_AVAILABLE",
            "SK_GAMMA_APPLY_TO_A8",
            "GR_OP_ALLOCATE_USE_NEW",
            "SK_ALLOW_STATIC_GLOBAL_INITIALIZERS=1",
            "GR_TEST_UTILS=1",
            "SKIA_IMPLEMENTATION=1",
            "SK_GL",
            "SK_ENABLE_DUMP_GPU",
            "SK_SUPPORT_PDF",
            "SK_CODEC_DECODES_JPEG",
            "SK_ENCODE_JPEG",
            "SK_ENABLE_ANDROID_UTILS",
            "SK_USE_LIBGIFCODEC",
            "SK_HAS_HEIF_LIBRARY",
            "SK_CODEC_DECODES_PNG",
            "SK_ENCODE_PNG",
            "SK_CODEC_DECODES_RAW",
            "SK_ENABLE_SKSL_INTERPRETER",
            "SKVM_JIT_WHEN_POSSIBLE",
            "SK_CODEC_DECODES_WEBP",
            "SK_ENCODE_WEBP",
            "SK_XML",
        });

        //lib txt
        np.Defines.Add(c => IsMac(c), new[] {
            "SK_USING_THIRD_PARTY_ICU", "U_USING_ICU_NAMESPACE=0",
            "U_ENABLE_DYLOAD=0", "USE_CHROMIUM_ICU=1", "U_STATIC_IMPLEMENTATION",
            "ICU_UTIL_DATA_IMPL=ICU_UTIL_DATA_STATIC"
        });

        np.IncludeDirectories.Add(c => IsWindows(c), new NPath[] {
             ".",
            "third_party",
            "src",
            flutterRoot,
            flutterRoot + "/third_party/rapidjson/include",
            flutterRoot +"/third_party/angle/include",
            skiaRoot,
            flutterRoot + "/flutter/third_party/txt/src",
            flutterRoot + "/third_party/harfbuzz/src",
            flutterRoot + "/third_party/icu/source/common",

            flutterRoot + "/third_party/icu/source/common",
            flutterRoot + "/third_party/icu/source/i18n",
        });
        np.CompilerSettings().Add(c => {
            return IsWindows(c) && c.CodeGen == CodeGen.Debug;
        }, c => c.WithCustomFlags(new[] {
            "-D_DEBUG",
            "-DFLUTTER_RUNTIME_MODE_DEBUG=1",
            "-DFLUTTER_RUNTIME_MODE_DEBUG=1",
            "/MTd",
            "/Od",
            "/Ob0",
            "/RTC1",
        }));

        np.CompilerSettings().Add(c => IsWindows(c), c => c.WithCustomFlags(new[] {
            "-D_SILENCE_CXX17_CODECVT_HEADER_DEPRECATION_WARNING",
            "-DUSE_OPENSSL=1",
            "-D__STD_C",
            "-D_CRT_RAND_S",
            "-D_CRT_SECURE_NO_DEPRECATE",
            "-D_HAS_EXCEPTIONS=0",
            "-D_SCL_SECURE_NO_DEPRECATE",
            "-DWIN32_LEAN_AND_MEAN",
            "-DNOMINMAX",
            "-D_ATL_NO_OPENGL",
            "-D_WINDOWS",
            "-DCERT_CHAIN_PARA_HAS_EXTRA_FIELDS",
            "-DNTDDI_VERSION=0x06030000",
            "-DPSAPI_VERSION=1",
            "-DWIN32",
            "-D_SECURE_ATL",
            "-D_USING_V110_SDK71_",
            "-D_UNICODE",
            "-DUNICODE",
            "-D_WIN32_WINNT=0x0603",
            "-DWINVER=0x0603",
            "-DU_USING_ICU_NAMESPACE=0",
            "-DU_ENABLE_DYLOAD=0",
            "-DUSE_CHROMIUM_ICU=1",
            "-DU_STATIC_IMPLEMENTATION",
            "-DICU_UTIL_DATA_IMPL=ICU_UTIL_DATA_FILE",
            "-DUCHAR_TYPE=wchar_t",
            "-DFLUTTER_RUNTIME_MODE_PROFILE=2",
            "-DFLUTTER_RUNTIME_MODE_RELEASE=3",
            "-DFLUTTER_RUNTIME_MODE_JIT_RELEASE=4",
            "-DFLUTTER_RUNTIME_MODE=1",
            "-DFLUTTER_JIT_RUNTIME=1",

            "-DSK_ENABLE_SPIRV_VALIDATION",
            "-D_CRT_SECURE_NO_WARNINGS",
            "-D_HAS_EXCEPTIONS=0",
            "-DWIN32_LEAN_AND_MEAN",
            "-DNOMINMAX",
            "-DSK_GAMMA_APPLY_TO_A8",
            "-DSK_ALLOW_STATIC_GLOBAL_INITIALIZERS=1",
            // TODO: fix this by update txt_lib build setting, reference: https://github.com/microsoft/vcpkg/issues/12123
            // "-DGR_TEST_UTILS=1",
            "-DSKIA_IMPLEMENTATION=1",
            "-DSK_GL",
            "-DSK_ENABLE_DUMP_GPU",
            "-DSK_SUPPORT_PDF",
            "-DSK_CODEC_DECODES_JPEG",
            "-DSK_ENCODE_JPEG",
            "-DSK_SUPPORT_XPS",
            "-DSK_ENABLE_ANDROID_UTILS",
            "-DSK_USE_LIBGIFCODEC",
            "-DSK_HAS_HEIF_LIBRARY",
            "-DSK_CODEC_DECODES_PNG",
            "-DSK_ENCODE_PNG",
            "-DSK_ENABLE_SKSL_INTERPRETER",
            "-DSK_CODEC_DECODES_WEBP",
            "-DSK_ENCODE_WEBP",
            "-DSK_XML",

             "-DLIBEGL_IMPLEMENTATION",
            "-D_CRT_SECURE_NO_WARNINGS",
            "-D_HAS_EXCEPTIONS=0",
            "-DWIN32_LEAN_AND_MEAN",
            "-DNOMINMAX",
            "-DANGLE_ENABLE_ESSL",
            "-DANGLE_ENABLE_GLSL",
            "-DANGLE_ENABLE_HLSL",
            "-DANGLE_ENABLE_OPENGL",
            "-DEGL_EGLEXT_PROTOTYPES",
            "-DGL_GLEXT_PROTOTYPES",
            "-DANGLE_ENABLE_D3D11",
            "-DANGLE_ENABLE_D3D9",
            "-DGL_APICALL=",
            "-DGL_API=",
            "-DEGLAPI=",
            "/FS",
            "/Zi",
            "/WX",
            "/std:c++17",
            "/GR-",

        }));

        np.CompilerSettings().Add(c => IsMac(c), c => c.WithCustomFlags(new[] {
            "-MD",
            "-MF",

            "-I.",
            "-Ithird_party",
            "-Isrc",
            "-I"+ flutterRoot,
            "-I"+ flutterRoot+"/third_party/rapidjson/include",
            "-I"+ skiaRoot,
            "-I"+ flutterRoot+"/flutter/third_party/txt/src",
            "-I" + flutterRoot + "/third_party/harfbuzz/src",
            "-I" + skiaRoot + "/third_party/externals/icu/source/common",

            // "-Igen",
            "-I"+ flutterRoot+"/third_party/icu/source/common",
            "-I"+ flutterRoot+"/third_party/icu/source/i18n",

            "-fvisibility-inlines-hidden",
        }));

        var windowsSkiaBuild = skiaRoot + "/out/Debug";

        np.Libraries.Add(c => {
            return IsWindows(c) && c.CodeGen == CodeGen.Debug;
        }, c =>
        {
            return new PrecompiledLibrary[]
            {
                new StaticLibrary(flutterRoot+"/out/host_debug_unopt/obj/flutter/third_party/txt/txt_lib.lib"),
                new StaticLibrary(flutterRoot+"/out/host_debug_unopt/obj/third_party/angle/angle_lib.lib"),
                new StaticLibrary(flutterRoot+"/out/host_debug_unopt/obj/third_party/angle/libEGL_static.lib"),
                new StaticLibrary(flutterRoot+"/out/host_debug_unopt/obj/third_party/angle/libGLESv2_static.lib"),
                new SystemLibrary("dxguid.lib"),
            };
        });

        np.Libraries.Add(c => {
            return IsWindows(c) && c.CodeGen == CodeGen.Release;
        }, c =>
        {
            return new PrecompiledLibrary[]
            {
                new StaticLibrary(flutterRoot+"/out/host_release/obj/flutter/third_party/txt/txt_lib.lib"),
                new StaticLibrary(flutterRoot+"/out/host_release/obj/third_party/angle/angle_lib.lib"),
                new StaticLibrary(flutterRoot+"/out/host_release/obj/third_party/angle/libEGL_static.lib"),
                new StaticLibrary(flutterRoot+"/out/host_release/obj/third_party/angle/libGLESv2_static.lib"),
            };
        });
        np.Libraries.Add(IsWindows, c =>
        {
            return new PrecompiledLibrary[]
            {
                new SystemLibrary("User32.lib"),
                new SystemLibrary("Rpcrt4.lib"),
                new SystemLibrary("d3d9.lib"),
                new SystemLibrary("gdi32.lib"),
            };
        });

        np.Libraries.Add(IsMac, c =>
        {
            if(c.CodeGen == CodeGen.Debug){
                return new PrecompiledLibrary[]{
                    new StaticLibrary(flutterRoot+"/out/host_debug_unopt/obj/flutter/third_party/txt/libtxt_lib.a"),
                };
            } else {
                return new PrecompiledLibrary[]{
                    new StaticLibrary(flutterRoot+"/out/host_release/obj/flutter/third_party/txt/libtxt_lib.a"),
                };
            }
        });
        np.Libraries.Add(IsMac, c =>
        {
            return new PrecompiledLibrary[]
            {
                new SystemFramework("Foundation"),
                new SystemFramework("ApplicationServices"),
                new SystemFramework("OpenGL"),
                new SystemFramework("AppKit"),
                new SystemFramework("CoreVideo"),
            };
        });

        np.Libraries.Add(IsAndroid, c =>
        {
            if (c.CodeGen == CodeGen.Debug)
            {
                if (buildArm64)
                {
                    return new PrecompiledLibrary[]
                    {
                        new StaticLibrary(flutterRoot +
                                          "/out/android_debug_unopt_arm64/obj/flutter/third_party/txt/libtxt_lib.a"),
                    };
                }
                else
                {
                    return new PrecompiledLibrary[]
                    {
                        new StaticLibrary(flutterRoot +
                                          "/out/android_debug_unopt/obj/flutter/third_party/txt/libtxt_lib.a"),
                    };
                }
            }
            else
            {
                return new PrecompiledLibrary[]
                {
                    new StaticLibrary(buildArm64 ?
                        flutterRoot + "/out/android_release_arm64/obj/flutter/third_party/txt/libtxt_lib.a"
                        :flutterRoot + "/out/android_release/obj/flutter/third_party/txt/libtxt_lib.a"
                        ),
                };
            }
        });

        np.Libraries.Add(IsAndroid, c =>
        {
            var basePath = skiaRoot + "/out/arm";
            var libraries = new List<PrecompiledLibrary>
            {
                // icudtl
                new StaticLibrary("icudtl.o"),

                new SystemLibrary("gcc"),
                new SystemLibrary("c"),
                new SystemLibrary("dl"),
                new SystemLibrary("m"),
                new SystemLibrary("android"),
                new SystemLibrary("EGL"),
                new SystemLibrary("GLESv2"),
                new SystemLibrary("log"),
            };
            if (buildArm64)
            {
                libraries.Add(new PrecompiledLibrary[]
                {
                    new StaticLibrary(flutterRoot +
                                      "/third_party/android_tools/ndk/platforms/android-22/arch-arm64/usr/lib/crtbegin_so.o"),
                    new StaticLibrary(flutterRoot +
                                      "/third_party/android_tools/ndk/platforms/android-22/arch-arm64/usr/lib/crtbegin_so.o"),
                });
            }
            else
            {
                libraries.Add(new PrecompiledLibrary[]
                {
                    new StaticLibrary(flutterRoot +
                                      "/third_party/android_tools/ndk/platforms/android-16/arch-arm/usr/lib/crtbegin_so.o"),
                    new StaticLibrary(flutterRoot +
                                      "/third_party/android_tools/ndk/platforms/android-16/arch-arm/usr/lib/crtend_so.o"),
                    new SystemLibrary("android_support"),
                    new SystemLibrary("unwind"),
                });
            }

            return libraries;
        });
    }


    static void SetupRadidJson(NativeProgram np)
    {
        // gn desc .\out\host_debug_unopt\ //third_party/rapidjson:rapidjson
        np.Defines.Add(new[]
        {
            "RAPIDJSON_HAS_STDSTRING",
            "RAPIDJSON_HAS_CXX11_RANGE_FOR",
            "RAPIDJSON_HAS_CXX11_RVALUE_REFS",
            "RAPIDJSON_HAS_CXX11_TYPETRAITS",
            "RAPIDJSON_HAS_CXX11_NOEXCEPT"
        });

        np.Defines.Add(c => IsMac(c), new[]
        {
            "USE_OPENSSL=1",
            "__STDC_CONSTANT_MACROS",
            "__STDC_FORMAT_MACROS",
            "_FORTIFY_SOURCE=2",
            "_LIBCPP_DISABLE_AVAILABILITY=1",
            "_LIBCPP_DISABLE_VISIBILITY_ANNOTATIONS",
            "_LIBCPP_ENABLE_THREAD_SAFETY_ANNOTATIONS",
            "_DEBUG"
        });

        np.IncludeDirectories.Add(flutterRoot + "/third_party/rapidjson/include");
    }
}