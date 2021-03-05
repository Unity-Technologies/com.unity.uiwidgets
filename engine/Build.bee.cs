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
using System.Runtime.InteropServices;
using Bee.ProjectGeneration.XCode;
using Bee.Toolchain.Xcode;

class BuildUtils
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

class Build
{
    static void Main()
    {
        skiaRoot = Environment.GetEnvironmentVariable("SKIA_ROOT");
        if (string.IsNullOrEmpty(skiaRoot))
        {
            skiaRoot = Environment.GetEnvironmentVariable("USERPROFILE") + "/skia_repo/skia";
        }

        flutterRoot = Environment.GetEnvironmentVariable("FLUTTER_ROOT");
        if (string.IsNullOrEmpty(flutterRoot))
        {
            flutterRoot = Environment.GetEnvironmentVariable("USERPROFILE") + "/engine/src";
        }

        var buildAndroidEnv = Environment.GetEnvironmentVariable("BUILD_ANDROID");
        buildAndroid = true;// buildAndroidEnv == "true";


        var libUIWidgets = SetupLibUIWidgets();

        //create ide projects
        if (BuildUtils.IsHostWindows())
        {
            //create vs project
            var builder = new VisualStudioNativeProjectFileBuilder(libUIWidgets);
            builder = libUIWidgets.SetupConfigurations.Aggregate(
                builder,
                (current, c) => current.AddProjectConfiguration(c));

            var sln = new VisualStudioSolution();
            sln.Path = "libUIWidgets.gen.sln";
            sln.Projects.Add(builder.DeployTo("libUIWidgets.gen.vcxproj"));
            Backend.Current.AddAliasDependency("ProjectFiles", sln.Setup());
        }
        else if (BuildUtils.IsHostMac())
        {
            if(buildAndroid){
                var androidProject = AndroidNativeProgramExtensions.DynamicLinkerSettingsForAndroid(libUIWidgets);
            }else {
                //create xcode project
                var nativePrograms = new List<NativeProgram>();
                nativePrograms.Add(libUIWidgets);
                var xcodeProject = new XCodeProjectFile(nativePrograms, new NPath("libUIWidgets.xcodeproj/project.pbxproj"));
            }
        }
    }

    private static string skiaRoot;
    private static string flutterRoot;
    private static bool buildAndroid;

    static NativeProgram SetupLibUIWidgets()
    {
        var np = new NativeProgram("libUIWidgets")
        {
            Sources =
            {
                //"src/assets/test.cc",
                //"src/assets/test.h",
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
            OutputName = { c => $"libUIWidgets{(c.CodeGen == CodeGen.Debug ? "_d" : "")}" },
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
        };


        np.Sources.Add(c => IsWindows(c), winSources);
        np.Sources.Add(c => {
            var a = IsMac(c);
            return a;
        }, macSources);

        np.Libraries.Add(c => IsWindows(c), new BagOfObjectFilesLibrary(
            new NPath[]{
                skiaRoot + "/third_party/externals/icu/flutter/icudtl.o"
        }));
        np.CompilerSettings().Add(c => c.WithCppLanguageVersion(CppLanguageVersion.Cpp17));
        np.CompilerSettings().Add(c => IsMac(c), c => c.WithCustomFlags(new []{"-Wno-c++11-narrowing"}));

        np.CompilerSettings().Add(c => IsAndroid(c), c => c.WithCustomFlags(new[] { "-stdlib=libc++" }));
        np.CompilerSettings().Add(c => IsAndroid(c), c => c.WithCustomFlags(new[] { "-nostdlib" }));
        np.CompilerSettings().Add(c => IsAndroid(c), c => c.WithCustomFlags(new[] { "-Wno-c++11-narrowing" }));

        np.IncludeDirectories.Add("third_party");
        np.IncludeDirectories.Add("src");

        np.Defines.Add("UIWIDGETS_ENGINE_VERSION=\\\"0.0\\\"", "SKIA_VERSION=\\\"0.0\\\"");
        np.Defines.Add(c => IsMac(c), "UIWIDGETS_FORCE_ALIGNAS_8=\\\"1\\\"");

        np.Defines.Add(c => IsAndroid(c), "UIWIDGETS_FORCE_ALIGNAS_8=\\\"1\\\"");

        np.Defines.Add(c => c.CodeGen == CodeGen.Debug,
            new[] { "_ITERATOR_DEBUG_LEVEL=2", "_HAS_ITERATOR_DEBUGGING=1", "_SECURE_SCL=1" });

        np.Defines.Add(c => c.CodeGen == CodeGen.Release,
            new[] { "UIWidgets_RELEASE=1" });

        np.LinkerSettings().Add(c => IsWindows(c), l => l.WithCustomFlags_workaround(new[] { "/DEBUG:FULL" }));

        np.LinkerSettings().Add(c => IsAndroid(c), l => l.WithCustomFlags_workaround(new[] {
            "-Wl,--fatal-warnings",
            "-fPIC",
            "-Wl,-z,noexecstack",
            "-Wl,-z,now",
            "-Wl,-z,relro",
            "-Wl,-z,defs",
            "--gcc-toolchain=/Users/siyao/temp/flutter/engine/src/third_party/android_tools/ndk/toolchains/x86_64-4.9/prebuilt/darwin-x86_64",
            "-Wl,--no-undefined",
            "-Wl,--exclude-libs,ALL",
            "-fuse-ld=lld",
            "-Wl,--icf=all",
            "--target=arm-linux-androideabi",
            "-nostdlib++",
            "-Wl,--warn-shared-textrel",
            "-nostdlib",
            "--sysroot=/Users/siyao/temp/flutter/engine/src/third_party/android_tools/ndk/platforms/android-16/arch-arm"
        }));

        SetupFml(np);
        SetupRadidJson(np);
        SetupSkia(np);
        SetupTxt(np);

        var codegens = new[] { CodeGen.Debug };

        if (BuildUtils.IsHostWindows())
        {
            var toolchain = ToolChain.Store.Windows().VS2019().Sdk_17134().x64();

            foreach (var codegen in codegens)
            {
                var config = new NativeProgramConfiguration(codegen, toolchain, lump: true);

                var builtNP = np.SetupSpecificConfiguration(config, toolchain.DynamicLibraryFormat)
                    .DeployTo("build");

                builtNP.DeployTo("../Samples/UIWidgetsSamples_2019_4/Assets/Plugins/x86_64");
            }
        }
        else if (BuildUtils.IsHostMac())
        {
            if (buildAndroid)
            {
                //var codegens = new[] { CodeGen.Debug };

                var androidToolchain = ToolChain.Store.Android().r19().Armv7();

                var validConfigurations = new List<NativeProgramConfiguration>();

                foreach (var codegen in codegens)
                {
                    var config = new NativeProgramConfiguration(codegen, androidToolchain, lump: true);
                    validConfigurations.Add(config);

                    var builtNP = np.SetupSpecificConfiguration(config, androidToolchain.DynamicLibraryFormat).DeployTo("build");

                    builtNP.DeployTo("../Samples/UIWidgetsSamples_2019_4/Assets/Plugins/Android");
                }
                np.ValidConfigurations = validConfigurations;
            }
            else
            {
                var toolchain = ToolChain.Store.Host();
                var validConfigurations = new List<NativeProgramConfiguration>();
                foreach (var codegen in codegens)
                {
                    var config = new NativeProgramConfiguration(codegen, toolchain, lump: true);
                    validConfigurations.Add(config);

                    var builtNP = np.SetupSpecificConfiguration(config, toolchain.DynamicLibraryFormat).DeployTo("build");

                    builtNP.DeployTo("../Samples/UIWidgetsSamples_2019_4/Assets/Plugins/osx");
                }

                np.ValidConfigurations = validConfigurations;
            }
        }

        return np;
    }

    static void SetupFml(NativeProgram np)
    {

        np.Defines.Add(c => IsWindows(c), new[]
        {
            // gn desc out\host_debug_unopt\ //flutter/fml:fml_lib defines
            "USE_OPENSSL=1",
            "__STD_C",
            "_CRT_RAND_S",
            "_CRT_SECURE_NO_DEPRECATE",
            "_HAS_EXCEPTIONS=0",
            "_SCL_SECURE_NO_DEPRECATE",
            "WIN32_LEAN_AND_MEAN",
            "NOMINMAX",
            "_ATL_NO_OPENGL",
            "_WINDOWS",
            "CERT_CHAIN_PARA_HAS_EXTRA_FIELDS",
            "NTDDI_VERSION=0x06030000",
            "PSAPI_VERSION=1",
            "WIN32",
            "_SECURE_ATL",
            "_USING_V110_SDK71_",
            "_UNICODE",
            "UNICODE",
            "_WIN32_WINNT=0x0603",
            "WINVER=0x0603",
            "_LIBCPP_ENABLE_THREAD_SAFETY_ANNOTATIONS",
            "_DEBUG",
            "FLUTTER_RUNTIME_MODE_DEBUG=1",
            "FLUTTER_RUNTIME_MODE_PROFILE=2",
            "FLUTTER_RUNTIME_MODE_RELEASE=3",
            "FLUTTER_RUNTIME_MODE_JIT_RELEASE=4",
            "FLUTTER_RUNTIME_MODE=1",
            "FLUTTER_JIT_RUNTIME=1",
        });

        np.Defines.Add(c => IsMac(c), new []
        {
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
            "FLUTTER_JIT_RUNTIME=1"
        });

        np.Defines.Add(c => IsAndroid(c), new[]
        {
            // bin/gn desc out/arm //:skia defines
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
            "XML_STATIC"
        });

        np.IncludeDirectories.Add(flutterRoot);

        var fmlLibPath = flutterRoot + "/out/host_debug_unopt";
        np.Libraries.Add(c => IsWindows(c), c => {
            return new PrecompiledLibrary[]
            {
                new StaticLibrary(fmlLibPath + "/obj/flutter/fml/fml_lib.lib"),
                new SystemLibrary("Rpcrt4.lib"),
            };
        });

        np.Libraries.Add(c => IsMac(c), c => {
            return new PrecompiledLibrary[]
            {
                new StaticLibrary(fmlLibPath + "/obj/flutter/fml/libfml_lib.a"),
                new SystemFramework("Foundation"),
            };
        });


        np.Libraries.Add(c => IsAndroid(c), c =>
        {
            var basePath = flutterRoot + "/out/android_debug_unopt";

            return new PrecompiledLibrary[]
            {
                new StaticLibrary(basePath + "/obj/flutter/fml/libfml_lib.a"),
            };
        });
        //np.CompilerSettings().Add(c => IsAndroid(c), c => c.WithCustomFlags(new[] { "-fvisibility-inlines-hidden -std=c++17 -fno-rtti -fno-exceptions" }));
        np.CompilerSettings().Add(c => IsAndroid(c), c => c.WithCustomFlags(new[] { "-std=c++14 -fno-exceptions -fno-unwind-tables -fno-asynchronous-unwind-tables" }));
    }

    static void SetupSkia(NativeProgram np)
    {
        np.Defines.Add(c => IsWindows(c), new[]
        {
            // bin\gn desc out\Debug\ //:skia defines
            "SK_ENABLE_SPIRV_VALIDATION",
            "_CRT_SECURE_NO_WARNINGS",
            "_HAS_EXCEPTIONS=0",
            "WIN32_LEAN_AND_MEAN",
            "NOMINMAX",
            "SK_GAMMA_APPLY_TO_A8",
            "SK_ALLOW_STATIC_GLOBAL_INITIALIZERS=1",
            "GR_TEST_UTILS=1",
            "SKIA_IMPLEMENTATION=1",
            "SK_GL",
            "SK_ENABLE_DUMP_GPU",
            "SK_SUPPORT_PDF",
            "SK_CODEC_DECODES_JPEG",
            "SK_ENCODE_JPEG",
            "SK_SUPPORT_XPS",
            "SK_ENABLE_ANDROID_UTILS",
            "SK_USE_LIBGIFCODEC",
            "SK_HAS_HEIF_LIBRARY",
            "SK_CODEC_DECODES_PNG",
            "SK_ENCODE_PNG",
            "SK_ENABLE_SKSL_INTERPRETER",
            "SK_CODEC_DECODES_WEBP",
            "SK_ENCODE_WEBP",
            "SK_XML",

            // bin\gn desc out\Debug\ //third_party/angle2:libEGL defines
            "LIBEGL_IMPLEMENTATION",
            "_CRT_SECURE_NO_WARNINGS",
            "_HAS_EXCEPTIONS=0",
            "WIN32_LEAN_AND_MEAN",
            "NOMINMAX",
            "ANGLE_ENABLE_ESSL",
            "ANGLE_ENABLE_GLSL",
            "ANGLE_ENABLE_HLSL",
            "ANGLE_ENABLE_OPENGL",
            "EGL_EGLEXT_PROTOTYPES",
            "GL_GLEXT_PROTOTYPES",
            "ANGLE_ENABLE_D3D11",
            "ANGLE_ENABLE_D3D9",
            "GL_APICALL=",
            "GL_API=",
            "EGLAPI=",
        });

        np.Defines.Add(c => IsMac(c), new[]
        {
            // bin\gn desc out\Debug\ //:skia defines
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
            "SK_XML"
        });

        np.Defines.Add(c => IsAndroid(c), new[]
        {
            // bin/gn desc out/arm //:skia defines
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
            "XML_STATIC"
        });

        np.IncludeDirectories.Add(skiaRoot);
        np.IncludeDirectories.Add(c => IsWindows(c), skiaRoot + "/third_party/externals/angle2/include");
        // np.IncludeDirectories.Add(skiaRoot + "/include/third_party/vulkan");

        np.IncludeDirectories.Add(c => IsAndroid(c), skiaRoot + "/include/third_party/vulkan");
        np.CompilerSettings().Add(c => IsAndroid(c), c => c.WithCustomFlags(new[] { "-std=c++14" }));

        np.Libraries.Add(IsWindows, c =>
        {
            var basePath = skiaRoot + "/out/Debug";
            return new PrecompiledLibrary[]
            {
                new StaticLibrary(basePath + "/skia.lib"),
                new StaticLibrary(basePath + "/skottie.lib"),
                new StaticLibrary(basePath + "/sksg.lib"),
                new StaticLibrary(basePath + "/skshaper.lib"),
                new StaticLibrary(basePath + "/harfbuzz.lib"),
                new StaticLibrary(basePath + "/libEGL.dll.lib"),
                new StaticLibrary(basePath + "/libGLESv2.dll.lib"),
                // new SystemLibrary("Opengl32.lib"), 
                new SystemLibrary("User32.lib"),
                //new SystemLibrary("D3D12.lib"), 
                //new SystemLibrary("DXGI.lib"), 
                //new SystemLibrary("d3dcompiler.lib"),
                // new SystemLibrary(basePath + "/obj/tools/trace/trace.ChromeTracingTracer.obj"),
                // new SystemLibrary(basePath + "/obj/tools/trace/trace.EventTracingPriv.obj"),
                // new SystemLibrary(basePath + "/obj/tools/trace/trace.SkDebugfTracer.obj"),
                // new SystemLibrary(basePath + "/obj/tools/flags/flags.CommandLineFlags.obj"),
            };
        });

        np.Libraries.Add(IsMac, c => {
            var basePath = skiaRoot + "/out/Debug";
            return new PrecompiledLibrary[]
            {
                new StaticLibrary(basePath + "/libskia.a"),
                new StaticLibrary(basePath + "/libskottie.a"),
                new StaticLibrary(basePath + "/libsksg.a"),
                new StaticLibrary(basePath + "/libskshaper.a"),
                new SystemFramework("ApplicationServices"),
                new SystemFramework("OpenGL"),
                new SystemFramework("AppKit"),
                new SystemFramework("CoreVideo"),
            };
        });

        np.Libraries.Add(IsAndroid, c => {
            var basePath = skiaRoot + "/out/arm";
            return new PrecompiledLibrary[]
            {
                new StaticLibrary(basePath + "/libskia.a"),
                new StaticLibrary(basePath + "/libskottie.a"),
                new StaticLibrary(basePath + "/libsksg.a"),
                new StaticLibrary(basePath + "/libskshaper.a"),
                //new SystemLibrary("/Users/siyao/temp/flutter/engine/src/third_party/android_tools/ndk/sources/cxx-stl/llvm-libc++/libs/armeabi-v7a"),
                //new StaticLibrary("/Users/siyao/temp/flutter/engine/src/third_party/android_tools/ndk/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/arm-linux-androideabi/libunwind.a"),
                //new StaticLibrary("/Users/siyao/temp/flutter/engine/src/third_party/android_tools/ndk/toolchains/llvm/prebuilt/darwin-x86_64/lib/gcc/arm-linux-androideabi/4.9.x/libgcc.a"),
                //new StaticLibrary("/Users/siyao/temp/flutter/engine/src/third_party/android_tools/ndk/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/arm-linux-androideabi/libandroid_support.a"),

                new SystemLibrary("android"),
                new SystemLibrary("EGL"),
                new SystemLibrary("GLESv2"),
                new SystemLibrary("android_support"),
                new SystemLibrary("gcc"),
                new SystemLibrary("stdc++"),
                new SystemLibrary("unwind"),
                new SystemLibrary("c"),
                new SystemLibrary("dl"),
                new SystemLibrary("m"),
                new SystemLibrary("log")
            };
        });

        var basePath = skiaRoot + "/out/Debug";
        np.SupportFiles.Add(c => IsWindows(c), new [] {
                new DeployableFile(basePath + "/libEGL.dll"),
                new DeployableFile(basePath + "/libEGL.dll.pdb"),
                new DeployableFile(basePath + "/libGLESv2.dll"),
                new DeployableFile(basePath + "/libGLESv2.dll.pdb"),
            }
        );

    }

    static void SetupTxtDependency(NativeProgram np)
    {
        np.Defines.Add(IsWindows, new[] { "SK_USING_THIRD_PARTY_ICU", "U_USING_ICU_NAMESPACE=0", "U_DISABLE_RENAMING",
            "U_ENABLE_DYLOAD=0", "USE_CHROMIUM_ICU=1", "U_STATIC_IMPLEMENTATION",
            "ICU_UTIL_DATA_IMPL=ICU_UTIL_DATA_STATIC"
        });
        np.Defines.Add(IsAndroid, new string[] {
            // gn desc out/android_debug_unopt //flutter/third_party/txt:txt defines
             "USE_OPENSSL=1",
              "USE_OPENSSL_CERTS=1",
              "ANDROID",
              "HAVE_SYS_UIO_H",
              "__STDC_CONSTANT_MACROS",
              "__STDC_FORMAT_MACROS",
              "_FORTIFY_SOURCE=2",
              "__compiler_offsetof=__builtin_offsetof",
              "nan=__builtin_nan",
              "_LIBCPP_DISABLE_AVAILABILITY=1",
              "_LIBCPP_DISABLE_VISIBILITY_ANNOTATIONS",
              "_LIBCPP_ENABLE_THREAD_SAFETY_ANNOTATIONS",
              "_DEBUG",
              "SK_GL",
              "SK_ENABLE_DUMP_GPU",
              "SK_CODEC_DECODES_JPEG",
              "SK_ENCODE_JPEG",
              "SK_CODEC_DECODES_PNG",
              "SK_ENCODE_PNG",
              "SK_CODEC_DECODES_WEBP",
              "SK_ENCODE_WEBP",
              "SK_HAS_WUFFS_LIBRARY",
              "SK_XML",
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
              "SK_SUPPORT_LEGACY_DIDCONCAT44",
              "SK_DISABLE_LEGACY_SHADERCONTEXT",
              "SK_DISABLE_LOWP_RASTER_PIPELINE",
              "SK_FORCE_RASTER_PIPELINE_BLITTER",
              "SK_GL",
              "SK_ASSUME_GL_ES=1"
        });
        np.IncludeDirectories.Add(flutterRoot + "/flutter/third_party/txt/src");
        np.IncludeDirectories.Add(skiaRoot + "/third_party/externals/harfbuzz/src");
        np.IncludeDirectories.Add(skiaRoot + "/third_party/externals/icu/source/common");
    }

    static void SetupTxt(NativeProgram np)
    {
        // gn desc .\out\host_debug_unopt\ //flutter/third_party/txt:txt
        IEnumerable<NPath> sources = new List<NPath> {
            "src/log/log.cc",
            "src/log/log.h",
            "src/minikin/CmapCoverage.cpp",
            "src/minikin/CmapCoverage.h",
            "src/minikin/Emoji.cpp",
            "src/minikin/Emoji.h",
            "src/minikin/FontCollection.cpp",
            "src/minikin/FontCollection.h",
            "src/minikin/FontFamily.cpp",
            "src/minikin/FontFamily.h",
            "src/minikin/FontLanguage.cpp",
            "src/minikin/FontLanguage.h",
            "src/minikin/FontLanguageListCache.cpp",
            "src/minikin/FontLanguageListCache.h",
            "src/minikin/FontUtils.cpp",
            "src/minikin/FontUtils.h",
            "src/minikin/GraphemeBreak.cpp",
            "src/minikin/GraphemeBreak.h",
            "src/minikin/HbFontCache.cpp",
            "src/minikin/HbFontCache.h",
            "src/minikin/Hyphenator.cpp",
            "src/minikin/Hyphenator.h",
            "src/minikin/Layout.cpp",
            "src/minikin/Layout.h",
            "src/minikin/LayoutUtils.cpp",
            "src/minikin/LayoutUtils.h",
            "src/minikin/LineBreaker.cpp",
            "src/minikin/LineBreaker.h",
            "src/minikin/Measurement.cpp",
            "src/minikin/Measurement.h",
            "src/minikin/MinikinFont.cpp",
            "src/minikin/MinikinFont.h",
            "src/minikin/MinikinInternal.cpp",
            "src/minikin/MinikinInternal.h",
            "src/minikin/SparseBitSet.cpp",
            "src/minikin/SparseBitSet.h",
            "src/minikin/WordBreaker.cpp",
            "src/minikin/WordBreaker.h",
            "src/txt/asset_font_manager.cc",
            "src/txt/asset_font_manager.h",
            "src/txt/font_asset_provider.cc",
            "src/txt/font_asset_provider.h",
            "src/txt/font_collection.cc",
            "src/txt/font_collection.h",
            "src/txt/font_features.cc",
            "src/txt/font_features.h",
            "src/txt/font_skia.cc",
            "src/txt/font_skia.h",
            "src/txt/font_style.h",
            "src/txt/font_weight.h",
            "src/txt/line_metrics.h",
            "src/txt/paint_record.cc",
            "src/txt/paint_record.h",
            "src/txt/paragraph.h",
            "src/txt/paragraph_builder.cc",
            "src/txt/paragraph_builder.h",
            "src/txt/paragraph_builder_txt.cc",
            "src/txt/paragraph_builder_txt.h",
            "src/txt/paragraph_style.cc",
            "src/txt/paragraph_style.h",
            "src/txt/paragraph_txt.cc",
            "src/txt/paragraph_txt.h",
            "src/txt/placeholder_run.cc",
            "src/txt/placeholder_run.h",
            "src/txt/platform.h",
            "src/txt/run_metrics.h",
            "src/txt/styled_runs.cc",
            "src/txt/styled_runs.h",
            "src/txt/test_font_manager.cc",
            "src/txt/test_font_manager.h",
            "src/txt/text_baseline.h",
            "src/txt/text_decoration.cc",
            "src/txt/text_decoration.h",
            "src/txt/text_shadow.cc",
            "src/txt/text_shadow.h",
            "src/txt/text_style.cc",
            "src/txt/text_style.h",
            "src/txt/typeface_font_asset_provider.cc",
            "src/txt/typeface_font_asset_provider.h",
            "src/utils/JenkinsHash.cpp",
            "src/utils/JenkinsHash.h",
            "src/utils/LinuxUtils.h",
            "src/utils/LruCache.h",
            "src/utils/MacUtils.h",
            "src/utils/TypeHelpers.h",
            "src/utils/WindowsUtils.h",
        };

        var txtLib = new NativeProgram("txt_lib")
        {
            IncludeDirectories = {
                "third_party",

                flutterRoot,
                skiaRoot,
            },
        };

        SetupTxtDependency(txtLib);

        var ignoreWarnigs = new string[] { "4091", "4722", "4312", "4838", "4172", "4005", "4311", "4477" }; // todo comparing the list with engine
        txtLib.CompilerSettings().Add(s => s.WithWarningPolicies(ignoreWarnigs.Select((code) => new WarningAndPolicy(code, WarningPolicy.Silent)).ToArray()));
        txtLib.CompilerSettings().Add(c => IsMac(c), c => c.WithCppLanguageVersion(CppLanguageVersion.Cpp17));
        txtLib.CompilerSettings().Add(c => IsMac(c), c => c.WithCustomFlags(new []{"-Wno-c++11-narrowing"}));

        txtLib.CompilerSettings().Add(c => IsAndroid(c), c => c.WithCppLanguageVersion(CppLanguageVersion.Cpp17));
        txtLib.CompilerSettings().Add(c => IsAndroid(c), c => c.WithCustomFlags(new[] { "-stdlib=libc++" }));
        txtLib.CompilerSettings().Add(c => IsAndroid(c), c => c.WithCustomFlags(new[] { "-Wno-c++11-narrowing" }));

        txtLib.Defines.Add(c => c.CodeGen == CodeGen.Debug,
            new[] { "_ITERATOR_DEBUG_LEVEL=2", "_HAS_ITERATOR_DEBUGGING=1", "_SECURE_SCL=1" });
        txtLib.Defines.Add(c => IsWindows(c),
            new[] { "UCHAR_TYPE=wchar_t" });
        txtLib.Defines.Add(c => !IsWindows(c),
                    new[] { "UCHAR_TYPE=uint16_t    " });
        txtLib.Defines.Add(c => c.CodeGen == CodeGen.Release,
            new[] { "UIWidgets_RELEASE=1" });


        var txtPath = new NPath(flutterRoot + "/flutter/third_party/txt");
        sources = sources.Select(p => txtPath.Combine(p));
        txtLib.Sources.Add(sources);
        txtLib.Sources.Add(c => IsWindows(c), txtPath.Combine(new NPath("src/txt/platform_windows.cc")));
        txtLib.Sources.Add(c => IsMac(c), txtPath.Combine(new NPath("src/txt/platform_mac.mm")));
        txtLib.NonLumpableFiles.Add(sources);

        np.Libraries.Add(txtLib);
        SetupTxtDependency(np);

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

    // static void SetupSkiaAndroid(NativeProgram np)
    // {
    //     var skiaRoot = Environment.GetEnvironmentVariable("SKIA_ROOT");
    //     if (string.IsNullOrEmpty(skiaRoot))
    //     {
    //         skiaRoot = Environment.GetEnvironmentVariable("USERPROFILE") + "/skia_repo/skia";
    //     }
    //
    //     np.Defines.Add(new[]
    //     {
    //         // bin\gn desc out\arm64\ //:skia defines
    //         "SK_ENABLE_SPIRV_VALIDATION",
    //         "SK_GAMMA_APPLY_TO_A8",
    //         "SK_GAMMA_EXPONENT=1.4",
    //         "SK_GAMMA_CONTRAST=0.0",
    //         "SK_ALLOW_STATIC_GLOBAL_INITIALIZERS=1",
    //         "GR_TEST_UTILS=1",
    //         "SK_USE_VMA",
    //         "SKIA_IMPLEMENTATION=1",
    //         "SK_GL",
    //         "SK_VULKAN",
    //         "SK_ENABLE_VK_LAYERS",
    //         "SK_ENABLE_DUMP_GPU",
    //         "SK_SUPPORT_PDF",
    //         "SK_CODEC_DECODES_JPEG",
    //         "SK_ENCODE_JPEG",
    //         "SK_ENABLE_ANDROID_UTILS",
    //         "SK_USE_LIBGIFCODEC",
    //         "SK_HAS_HEIF_LIBRARY",
    //         "SK_CODEC_DECODES_PNG",
    //         "SK_ENCODE_PNG",
    //         "SK_CODEC_DECODES_RAW",
    //         "SK_ENABLE_SKSL_INTERPRETER",
    //         "SKVM_JIT",
    //         "SK_CODEC_DECODES_WEBP",
    //         "SK_ENCODE_WEBP",
    //         "SK_XML",
    //         "XML_STATIC"
    //     });
    //
    //     np.IncludeDirectories.Add(skiaRoot);
    //
    //     np.Libraries.Add(c =>
    //     {
    //         var basePath = skiaRoot + "/out/arm64";
    //         return new PrecompiledLibrary[]
    //         {
    //             new StaticLibrary(basePath + "/libskia.a"),
    //             new StaticLibrary(basePath + "/libskottie.a"),
    //             new StaticLibrary(basePath + "/libsksg.a"),
    //             new StaticLibrary(basePath + "/libskshaper.a"),
    //             new SystemLibrary("EGL"),
    //             new SystemLibrary("GLESv2"),
    //             new SystemLibrary("log"),
    //             new StaticLibrary(basePath + "/obj/src/utils/libskia.SkJSON.o"),
    //             new StaticLibrary(basePath + "/obj/src/core/libskia.SkCubicMap.o"),
    //             new StaticLibrary(basePath + "/obj/src/effects/libskia.SkColorMatrix.o"),
    //             new StaticLibrary(basePath + "/obj/src/pathops/libskia.SkOpBuilder.o"),
    //             new StaticLibrary(basePath + "/obj/src/utils/libskia.SkParse.o"),
    //         };
    //     });
    // }
}