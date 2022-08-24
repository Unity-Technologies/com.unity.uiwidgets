#define RAPIDJSON_HAS_STDSTRING 1

#include "embedder.h"

#include <iostream>

#include "assets/directory_asset_bundle.h"
#include "common/task_runners.h"
#include "flutter/fml/build_config.h"
#include "flutter/fml/closure.h"
#include "flutter/fml/command_line.h"
#include "flutter/fml/file.h"
#include "flutter/fml/make_copyable.h"
#include "flutter/fml/message_loop.h"
#include "flutter/fml/native_library.h"
#include "flutter/fml/paths.h"
#include "flutter/fml/trace_event.h"
#include "rapidjson/rapidjson.h"
#include "rapidjson/writer.h"
#include "shell/common/persistent_cache.h"
#include "shell/common/rasterizer.h"
#include "shell/platform/embedder/embedder_engine.h"
#include "shell/platform/embedder/embedder_platform_message_response.h"
#include "shell/platform/embedder/embedder_render_target.h"
#include "shell/platform/embedder/embedder_task_runner.h"
#include "shell/platform/embedder/embedder_thread_host.h"
#include "shell/platform/embedder/platform_view_embedder.h"

using namespace uiwidgets;

static UIWidgetsEngineResult LogEmbedderError(UIWidgetsEngineResult code,
                                              const char* reason,
                                              const char* code_name,
                                              const char* function,
                                              const char* file, int line) {
#if OS_WIN
  constexpr char kSeparator = '\\';
#else
  constexpr char kSeparator = '/';
#endif
  const auto file_base =
      (::strrchr(file, kSeparator) ? strrchr(file, kSeparator) + 1 : file);
  char error[256] = {};
  snprintf(error, (sizeof(error) / sizeof(char)),
           "%s (%d): '%s' returned '%s'. %s", file_base, line, function,
           code_name, reason);
  std::cerr << error << std::endl;
  return code;
}

#define LOG_EMBEDDER_ERROR(code, reason) \
  LogEmbedderError(code, reason, #code, __FUNCTION__, __FILE__, __LINE__)

static bool IsOpenGLRendererConfigValid(const UIWidgetsRendererConfig* config) {
  if (config->type != kOpenGL) {
    return false;
  }

  const UIWidgetsOpenGLRendererConfig* open_gl_config = &config->open_gl;

  if (open_gl_config->make_current == nullptr ||
      open_gl_config->clear_current == nullptr ||
      open_gl_config->present == nullptr ||
      open_gl_config->fbo_callback == nullptr) {
    return false;
  }

  return true;
}

static bool IsSoftwareRendererConfigValid(
    const UIWidgetsRendererConfig* config) {
  if (config->type != kSoftware) {
    return false;
  }

  const UIWidgetsSoftwareRendererConfig* software_config = &config->software;

  if (software_config->surface_present_callback == nullptr) {
    return false;
  }

  return true;
}

static bool IsRendererValid(const UIWidgetsRendererConfig* config) {
  if (config == nullptr) {
    return false;
  }

  switch (config->type) {
    case kOpenGL:
      return IsOpenGLRendererConfigValid(config);
    case kSoftware:
      return IsSoftwareRendererConfigValid(config);
    default:
      return false;
  }

  return false;
}

#if OS_LINUX || OS_WIN
static void* DefaultGLProcResolver(const char* name) {
  static fml::RefPtr<fml::NativeLibrary> proc_library =
#if OS_LINUX
      fml::NativeLibrary::CreateForCurrentProcess();
#elif OS_WIN  // OS_LINUX
      fml::NativeLibrary::Create("opengl32.dll");
#endif        // OS_WIN
  return static_cast<void*>(
      const_cast<uint8_t*>(proc_library->ResolveSymbol(name)));
}
#endif  // OS_LINUX || OS_WIN

static Shell::CreateCallback<PlatformView>
InferOpenGLPlatformViewCreationCallback(
    const UIWidgetsRendererConfig* config, void* user_data,
    PlatformViewEmbedder::PlatformDispatchTable platform_dispatch_table,
    std::unique_ptr<EmbedderExternalViewEmbedder> external_view_embedder) {
  if (config->type != kOpenGL) {
    return nullptr;
  }

  auto gl_make_current = [ptr = config->open_gl.make_current,
                          user_data]() -> bool { return ptr(user_data); };

  auto gl_clear_current = [ptr = config->open_gl.clear_current,
                           user_data]() -> bool { return ptr(user_data); };

  auto gl_present = [ptr = config->open_gl.present, user_data]() -> bool {
    return ptr(user_data);
  };

  auto gl_fbo_callback = [ptr = config->open_gl.fbo_callback,
                          user_data]() -> intptr_t { return ptr(user_data); };

  const UIWidgetsOpenGLRendererConfig* open_gl_config = &config->open_gl;
  std::function<bool()> gl_make_resource_current_callback = nullptr;
  if (open_gl_config->make_resource_current != nullptr) {
    gl_make_resource_current_callback =
        [ptr = config->open_gl.make_resource_current, user_data]() {
          return ptr(user_data);
        };
  }

  std::function<SkMatrix(void)> gl_surface_transformation_callback = nullptr;
  if (open_gl_config->surface_transformation != nullptr) {
    gl_surface_transformation_callback =
        [ptr = config->open_gl.surface_transformation, user_data]() {
          UIWidgetsTransformation transformation = ptr(user_data);
          return SkMatrix::MakeAll(transformation.scaleX,  //
                                   transformation.skewX,   //
                                   transformation.transX,  //
                                   transformation.skewY,   //
                                   transformation.scaleY,  //
                                   transformation.transY,  //
                                   transformation.pers0,   //
                                   transformation.pers1,   //
                                   transformation.pers2    //
          );
        };

    // If there is an external view embedder, ask it to apply the surface
    // transformation to its surfaces as well.
    if (external_view_embedder) {
      external_view_embedder->SetSurfaceTransformationCallback(
          gl_surface_transformation_callback);
    }
  }

  GPUSurfaceGLDelegate::GLProcResolver gl_proc_resolver = nullptr;
  if (open_gl_config->gl_proc_resolver != nullptr) {
    gl_proc_resolver = [ptr = config->open_gl.gl_proc_resolver,
                        user_data](const char* gl_proc_name) {
      return ptr(user_data, gl_proc_name);
    };
  } else {
#if OS_LINUX || OS_WIN
    gl_proc_resolver = DefaultGLProcResolver;
#endif
  }

  bool fbo_reset_after_present = open_gl_config->fbo_reset_after_present;

  EmbedderSurfaceGL::GLDispatchTable gl_dispatch_table = {
      gl_make_current,                     // gl_make_current_callback
      gl_clear_current,                    // gl_clear_current_callback
      gl_present,                          // gl_present_callback
      gl_fbo_callback,                     // gl_fbo_callback
      gl_make_resource_current_callback,   // gl_make_resource_current_callback
      gl_surface_transformation_callback,  // gl_surface_transformation_callback
      gl_proc_resolver,                    // gl_proc_resolver
  };

  return fml::MakeCopyable(
      [gl_dispatch_table, fbo_reset_after_present, platform_dispatch_table,
       external_view_embedder =
           std::move(external_view_embedder)](Shell& shell) mutable {
        return std::make_unique<PlatformViewEmbedder>(
            shell,                    // delegate
            shell.GetTaskRunners(),   // task runners
            gl_dispatch_table,        // embedder GL dispatch table
            fbo_reset_after_present,  // fbo reset after present
            platform_dispatch_table,  // embedder platform dispatch table
            std::move(external_view_embedder)  // external view embedder
        );
      });
}

static Shell::CreateCallback<PlatformView>
InferSoftwarePlatformViewCreationCallback(
    const UIWidgetsRendererConfig* config, void* user_data,
    PlatformViewEmbedder::PlatformDispatchTable platform_dispatch_table,
    std::unique_ptr<EmbedderExternalViewEmbedder> external_view_embedder) {
  if (config->type != kSoftware) {
    return nullptr;
  }

  auto software_present_backing_store =
      [ptr = config->software.surface_present_callback, user_data](
          const void* allocation, size_t row_bytes, size_t height) -> bool {
    return ptr(user_data, allocation, row_bytes, height);
  };

  EmbedderSurfaceSoftware::SoftwareDispatchTable software_dispatch_table = {
      software_present_backing_store,  // required
  };

  return fml::MakeCopyable([software_dispatch_table, platform_dispatch_table,
                            external_view_embedder = std::move(
                                external_view_embedder)](Shell& shell) mutable {
    return std::make_unique<PlatformViewEmbedder>(
        shell,                             // delegate
        shell.GetTaskRunners(),            // task runners
        software_dispatch_table,           // software dispatch table
        platform_dispatch_table,           // platform dispatch table
        std::move(external_view_embedder)  // external view embedder
    );
  });
}

static Shell::CreateCallback<PlatformView> InferPlatformViewCreationCallback(
    const UIWidgetsRendererConfig* config, void* user_data,
    PlatformViewEmbedder::PlatformDispatchTable platform_dispatch_table,
    std::unique_ptr<EmbedderExternalViewEmbedder> external_view_embedder) {
  if (config == nullptr) {
    return nullptr;
  }

  switch (config->type) {
    case kOpenGL:
      return InferOpenGLPlatformViewCreationCallback(
          config, user_data, platform_dispatch_table,
          std::move(external_view_embedder));
    case kSoftware:
      return InferSoftwarePlatformViewCreationCallback(
          config, user_data, platform_dispatch_table,
          std::move(external_view_embedder));
    default:
      return nullptr;
  }
  return nullptr;
}

static sk_sp<SkSurface> MakeSkSurfaceFromBackingStore(
    GrContext* context, const UIWidgetsBackingStoreConfig& config,
    const UIWidgetsOpenGLTexture* texture) {
  GrGLTextureInfo texture_info;
  texture_info.fTarget = texture->target;
  texture_info.fID = texture->name;
  texture_info.fFormat = texture->format;

  GrBackendTexture backend_texture(config.size.width,   //
                                   config.size.height,  //
                                   GrMipMapped::kNo,    //
                                   texture_info         //
  );

  SkSurfaceProps surface_properties(
      SkSurfaceProps::InitType::kLegacyFontHost_InitType);

  auto surface = SkSurface::MakeFromBackendTexture(
      context,                      // context
      backend_texture,              // back-end texture
      kBottomLeft_GrSurfaceOrigin,  // surface origin
      1,                            // sample count
      kN32_SkColorType,             // color type
      SkColorSpace::MakeSRGB(),     // color space
      &surface_properties,          // surface properties
      static_cast<SkSurface::TextureReleaseProc>(
          texture->destruction_callback),  // release proc
      texture->user_data                   // release context
  );

  if (!surface) {
    FML_LOG(ERROR) << "Could not wrap embedder supplied render texture.";
    texture->destruction_callback(texture->user_data);
    return nullptr;
  }

  return surface;
}

static sk_sp<SkSurface> MakeSkSurfaceFromBackingStore(
    GrContext* context, const UIWidgetsBackingStoreConfig& config,
    const UIWidgetsOpenGLFramebuffer* framebuffer) {
  GrGLFramebufferInfo framebuffer_info = {};
  framebuffer_info.fFormat = framebuffer->target;
  framebuffer_info.fFBOID = framebuffer->name;

  GrBackendRenderTarget backend_render_target(
      config.size.width,   // width
      config.size.height,  // height
      1,                   // sample count
      0,                   // stencil bits
      framebuffer_info     // framebuffer info
  );

  SkSurfaceProps surface_properties(
      SkSurfaceProps::InitType::kLegacyFontHost_InitType);

  auto surface = SkSurface::MakeFromBackendRenderTarget(
      context,                      //  context
      backend_render_target,        // backend render target
      kBottomLeft_GrSurfaceOrigin,  // surface origin
      kN32_SkColorType,             // color type
      SkColorSpace::MakeSRGB(),     // color space
      &surface_properties,          // surface properties
      static_cast<SkSurface::RenderTargetReleaseProc>(
          framebuffer->destruction_callback),  // release proc
      framebuffer->user_data                   // release context
  );

  if (!surface) {
    FML_LOG(ERROR) << "Could not wrap embedder supplied frame-buffer.";
    framebuffer->destruction_callback(framebuffer->user_data);
    return nullptr;
  }
  return surface;
}

static sk_sp<SkSurface> MakeSkSurfaceFromBackingStore(
    GrContext* context, const UIWidgetsBackingStoreConfig& config,
    const UIWidgetsSoftwareBackingStore* software) {
  const auto image_info =
      SkImageInfo::MakeN32Premul(config.size.width, config.size.height);

  struct Captures {
    VoidCallback destruction_callback;
    void* user_data;
  };
  auto captures = std::make_unique<Captures>();
  captures->destruction_callback = software->destruction_callback;
  captures->user_data = software->user_data;
  auto release_proc = [](void* pixels, void* context) {
    auto captures = reinterpret_cast<Captures*>(context);
    captures->destruction_callback(captures->user_data);
  };

  auto surface = SkSurface::MakeRasterDirectReleaseProc(
      image_info,                               // image info
      const_cast<void*>(software->allocation),  // pixels
      software->row_bytes,                      // row bytes
      release_proc,                             // release proc
      captures.release()                        // release context
  );

  if (!surface) {
    FML_LOG(ERROR)
        << "Could not wrap embedder supplied software render buffer.";
    software->destruction_callback(software->user_data);
    return nullptr;
  }
  return surface;
}

static std::unique_ptr<EmbedderRenderTarget> CreateEmbedderRenderTarget(
    const UIWidgetsCompositor* compositor,
    const UIWidgetsBackingStoreConfig& config, GrContext* context) {
  UIWidgetsBackingStore backing_store = {};
  backing_store.struct_size = sizeof(backing_store);

  // Safe access checks on the compositor struct have been performed in
  // InferExternalViewEmbedderFromArgs and are not necessary here.
  auto c_create_callback = compositor->create_backing_store_callback;
  auto c_collect_callback = compositor->collect_backing_store_callback;

  {
    TRACE_EVENT0("uiwidgets", "UIWidgetsCompositorCreateBackingStore");
    if (!c_create_callback(&config, &backing_store, compositor->user_data)) {
      FML_LOG(ERROR) << "Could not create the embedder backing store.";
      return nullptr;
    }
  }

  if (backing_store.struct_size != sizeof(backing_store)) {
    FML_LOG(ERROR) << "Embedder modified the backing store struct size.";
    return nullptr;
  }

  // In case we return early without creating an embedder render target, the
  // embedder has still given us ownership of its baton which we must return
  // back to it. If this method is successful, the closure is released when the
  // render target is eventually released.
  fml::ScopedCleanupClosure collect_callback(
      [c_collect_callback, backing_store, user_data = compositor->user_data]() {
        TRACE_EVENT0("uiwidgets", "UIWidgetsCompositorCollectBackingStore");
        c_collect_callback(&backing_store, user_data);
      });

  // No safe access checks on the renderer are necessary since we allocated
  // the struct.

  sk_sp<SkSurface> render_surface;

  switch (backing_store.type) {
    case kUIWidgetsBackingStoreTypeOpenGL:
      switch (backing_store.open_gl.type) {
        case kUIWidgetsOpenGLTargetTypeTexture:
          render_surface = MakeSkSurfaceFromBackingStore(
              context, config, &backing_store.open_gl.texture);
          break;
        case kUIWidgetsOpenGLTargetTypeFramebuffer:
          render_surface = MakeSkSurfaceFromBackingStore(
              context, config, &backing_store.open_gl.framebuffer);
          break;
      }
      break;
    case kUIWidgetsBackingStoreTypeSoftware:
      render_surface = MakeSkSurfaceFromBackingStore(context, config,
                                                     &backing_store.software);
      break;
  };

  if (!render_surface) {
    FML_LOG(ERROR) << "Could not create a surface from an embedder provided "
                      "render target.";
    return nullptr;
  }

  return std::make_unique<EmbedderRenderTarget>(
      backing_store, std::move(render_surface), collect_callback.Release());
}

static std::pair<std::unique_ptr<EmbedderExternalViewEmbedder>,
                 bool /* halt engine launch if true */>
InferExternalViewEmbedderFromArgs(const UIWidgetsCompositor* compositor) {
  if (compositor == nullptr) {
    return {nullptr, false};
  }

  auto c_create_callback = compositor->create_backing_store_callback;
  auto c_collect_callback = compositor->collect_backing_store_callback;
  auto c_present_callback = compositor->present_layers_callback;

  // Make sure the required callbacks are present
  if (!c_create_callback || !c_collect_callback || !c_present_callback) {
    FML_LOG(ERROR) << "Required compositor callbacks absent.";
    return {nullptr, true};
  }

  UIWidgetsCompositor captured_compositor = *compositor;

  EmbedderExternalViewEmbedder::CreateRenderTargetCallback
      create_render_target_callback =
          [captured_compositor](GrContext* context, const auto& config) {
            return CreateEmbedderRenderTarget(&captured_compositor, config,
                                              context);
          };

  EmbedderExternalViewEmbedder::PresentCallback present_callback =
      [c_present_callback,
       user_data = compositor->user_data](const auto& layers) {
        TRACE_EVENT0("uiwidgets", "UIWidgetsCompositorPresentLayers");
        return c_present_callback(
            const_cast<const UIWidgetsLayer**>(layers.data()), layers.size(),
            user_data);
      };

  return {std::make_unique<EmbedderExternalViewEmbedder>(
              create_render_target_callback, present_callback),
          false};
}

struct _UIWidgetsPlatformMessageResponseHandle {
  fml::RefPtr<PlatformMessage> message;
};

UIWidgetsEngineResult UIWidgetsEngineRun(const UIWidgetsRendererConfig* config,
                                         const UIWidgetsProjectArgs* args,
                                         void* user_data,
                                         UIWidgetsEngine* engine_out) {
  auto result = UIWidgetsEngineInitialize(config, args, user_data, engine_out);

  if (result != kSuccess) {
    return result;
  }

  return UIWidgetsEngineRunInitialized(*engine_out);
}

UIWidgetsEngineResult UIWidgetsEngineInitialize(
    const UIWidgetsRendererConfig* config, const UIWidgetsProjectArgs* args,
    void* user_data, UIWidgetsEngine* engine_out) {
  if (engine_out == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments,
                              "The engine out parameter was missing.");
  }

  if (args == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments,
                              "The UIWidgets project arguments were missing.");
  }

  if (args->assets_path == nullptr) {
    return LOG_EMBEDDER_ERROR(
        kInvalidArguments,
        "The assets path in the UIWidgets project arguments was missing.");
  }

  if (args->task_observer_add == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments,
                              "The task_observer_add in the UIWidgets project "
                              "arguments was missing.");
  }

  if (args->task_observer_remove == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments,
                              "The task_observer_remove in the UIWidgets "
                              "project arguments was missing.");
  }

  if (!IsRendererValid(config)) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments,
                              "The renderer configuration was invalid.");
  }

  std::string icu_data_path;
  if (args->icu_data_path != nullptr) {
    icu_data_path = args->icu_data_path;
  }

  if (args->persistent_cache_path != nullptr) {
    std::string persistent_cache_path = args->persistent_cache_path;
    PersistentCache::SetCacheDirectoryPath(persistent_cache_path);
  }

  if (args->is_persistent_cache_read_only) {
    PersistentCache::gIsReadOnly = true;
  }

  fml::CommandLine command_line;
  if (args->command_line_argc != 0 && args->command_line_argv != nullptr) {
    command_line = fml::CommandLineFromArgcArgv(args->command_line_argc,
                                                args->command_line_argv);
  }

  Settings settings;  // = SettingsFromCommandLine(command_line);

  settings.icu_data_path = icu_data_path;
  settings.icu_mapper = args->icu_mapper;
  settings.assets_path = args->assets_path;
  settings.font_data = args->font_asset;

  settings.task_observer_add = [task_observer_add = args->task_observer_add,
                                user_data](intptr_t key,
                                           fml::closure callback) {
    task_observer_add(key, &callback, user_data);
  };
  settings.task_observer_remove =
      [task_observer_remove = args->task_observer_remove,
       user_data](intptr_t key) { task_observer_remove(key, user_data); };

  settings.mono_entrypoint_callback =
      [custom_mono_entrypoint = args->custom_mono_entrypoint, user_data]() {
        custom_mono_entrypoint(user_data);
      };

  if (args->root_isolate_create_callback != nullptr) {
    VoidCallback callback = args->root_isolate_create_callback;
    settings.root_isolate_create_callback = [callback, user_data]() {
      callback(user_data);
    };
  }

  PlatformViewEmbedder::PlatformMessageResponseCallback
      platform_message_response_callback = nullptr;
  if (args->platform_message_callback != nullptr) {
    platform_message_response_callback =
        [ptr = args->platform_message_callback,
         user_data](fml::RefPtr<PlatformMessage> message) {
          auto handle = new UIWidgetsPlatformMessageResponseHandle();
          const UIWidgetsPlatformMessage incoming_message = {
              sizeof(UIWidgetsPlatformMessage),  // struct_size
              message->channel().c_str(),        // channel
              message->data().data(),            // message
              message->data().size(),            // message_size
              handle,                            // response_handle
          };
          handle->message = std::move(message);
          return ptr(&incoming_message, user_data);
        };
  }

  VsyncWaiterEmbedder::VsyncCallback vsync_callback = nullptr;
  if (args->vsync_callback != nullptr) {
    vsync_callback = [ptr = args->vsync_callback, user_data](intptr_t baton) {
      return ptr(user_data, baton);
    };
  }

  auto external_view_embedder_result =
      InferExternalViewEmbedderFromArgs(args->compositor);
  if (external_view_embedder_result.second) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments,
                              "Compositor arguments were invalid.");
  }

  PlatformViewEmbedder::PlatformDispatchTable platform_dispatch_table = {
      platform_message_response_callback,  //
      vsync_callback,                      //
  };

  auto on_create_platform_view = InferPlatformViewCreationCallback(
      config, user_data, platform_dispatch_table,
      std::move(external_view_embedder_result.first));

  if (!on_create_platform_view) {
    return LOG_EMBEDDER_ERROR(
        kInternalInconsistency,
        "Could not infer platform view creation callback.");
  }

  Shell::CreateCallback<Rasterizer> on_create_rasterizer = [](Shell& shell) {
    return std::make_unique<Rasterizer>(shell, shell.GetTaskRunners());
  };

  // TODO(chinmaygarde): This is the wrong spot for this. It belongs in the
  // platform view jump table.
  EmbedderExternalTextureGL::ExternalTextureCallback external_texture_callback;
  if (config->type == kOpenGL) {
    const UIWidgetsOpenGLRendererConfig* open_gl_config = &config->open_gl;
    if (open_gl_config->gl_external_texture_frame_callback != nullptr) {
      external_texture_callback =
          [ptr = open_gl_config->gl_external_texture_frame_callback, user_data](
              int64_t texture_identifier, GrContext* context,
              const SkISize& size) -> sk_sp<SkImage> {
        UIWidgetsOpenGLTexture texture = {};

        if (!ptr(user_data, texture_identifier, size.width(), size.height(),
                 &texture)) {
          return nullptr;
        }

        GrGLTextureInfo gr_texture_info = {texture.target, texture.name,
                                           texture.format};

        size_t width = size.width();
        size_t height = size.height();

        if (texture.width != 0 && texture.height != 0) {
          width = texture.width;
          height = texture.height;
        }

        GrBackendTexture gr_backend_texture(width, height, GrMipMapped::kNo,
                                            gr_texture_info);
        SkImage::TextureReleaseProc release_proc = texture.destruction_callback;
        auto image = SkImage::MakeFromTexture(
            context,                   // context
            gr_backend_texture,        // texture handle
            kTopLeft_GrSurfaceOrigin,  // origin
            kRGBA_8888_SkColorType,    // color type
            kPremul_SkAlphaType,       // alpha type
            nullptr,                   // colorspace
            release_proc,              // texture release proc
            texture.user_data          // texture release context
        );

        if (!image) {
          // In case Skia rejects the image, call the release proc so that
          // embedders can perform collection of intermediates.
          if (release_proc) {
            release_proc(texture.user_data);
          }
          FML_LOG(ERROR) << "Could not create external texture.";
          return nullptr;
        }

        return image;
      };
    }
  }

  auto thread_host = EmbedderThreadHost::CreateEmbedderManagedThreadHost(
      args->custom_task_runners);

  if (!thread_host || !thread_host->IsValid()) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments,
                              "Could not setup or infer thread configuration "
                              "to run the UIWidgets engine on.");
  }

  auto task_runners = thread_host->GetTaskRunners();

  if (!task_runners.IsValid()) {
    return LOG_EMBEDDER_ERROR(kInternalInconsistency,
                              "Task runner configuration was invalid.");
  }

  auto run_configuration = RunConfiguration::InferFromSettings(settings);

  if (!run_configuration.IsValid()) {
    return LOG_EMBEDDER_ERROR(
        kInvalidArguments,
        "Could not infer the UIWidgets project to run from given arguments.");
  }

  WindowData window_data;
  window_data.viewport_metrics.physical_width =
      args->initial_window_metrics.width;
  window_data.viewport_metrics.physical_height =
      args->initial_window_metrics.height;
  window_data.viewport_metrics.device_pixel_ratio =
      args->initial_window_metrics.pixel_ratio;

  // Create the engine but don't launch the shell or run the root isolate.
  auto embedder_engine =
      std::make_unique<EmbedderEngine>(std::move(thread_host),        //
                                       task_runners,                  //
                                       window_data,                   //
                                       settings,                      //
                                       std::move(run_configuration),  //
                                       on_create_platform_view,       //
                                       on_create_rasterizer,          //
                                       external_texture_callback      //
      );

  // Release the ownership of the embedder engine to the caller.
  *engine_out = reinterpret_cast<UIWidgetsEngine>(embedder_engine.release());
  return kSuccess;
}

UIWidgetsEngineResult UIWidgetsEngineRunInitialized(UIWidgetsEngine engine) {
  if (!engine) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Engine handle was invalid.");
  }

  auto embedder_engine = reinterpret_cast<EmbedderEngine*>(engine);

  // The engine must not already be running. Initialize may only be called once
  // on an engine instance.
  if (embedder_engine->IsValid()) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Engine handle was invalid.");
  }

  // Step 1: Launch the shell.
  if (!embedder_engine->LaunchShell()) {
    return LOG_EMBEDDER_ERROR(
        kInvalidArguments,
        "Could not launch the engine using supplied initialization arguments.");
  }

  // Step 2: Tell the platform view to initialize itself.
  if (!embedder_engine->NotifyCreated()) {
    return LOG_EMBEDDER_ERROR(kInternalInconsistency,
                              "Could not create platform view components.");
  }

  // Step 3: Launch the root isolate.
  if (!embedder_engine->RunRootIsolate()) {
    return LOG_EMBEDDER_ERROR(
        kInvalidArguments,
        "Could not run the root isolate of the UIWidgets application using the "
        "project arguments specified.");
  }

  return kSuccess;
}

UIWidgetsEngineResult UIWidgetsEngineDeinitialize(UIWidgetsEngine engine) {
  if (engine == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Engine handle was invalid.");
  }

  auto embedder_engine = reinterpret_cast<EmbedderEngine*>(engine);
  embedder_engine->NotifyDestroyed();
  embedder_engine->CollectShell();
  return kSuccess;
}

UIWidgetsEngineResult UIWidgetsEngineShutdown(UIWidgetsEngine engine) {
  auto result = UIWidgetsEngineDeinitialize(engine);
  if (result != kSuccess) {
    return result;
  }
  auto embedder_engine = reinterpret_cast<EmbedderEngine*>(engine);
  delete embedder_engine;
  return kSuccess;
}

UIWidgetsEngineResult UIWidgetsEngineSendWindowMetricsEvent(
    UIWidgetsEngine engine,
    const UIWidgetsWindowMetricsEvent* uiwidgets_metrics) {
  if (engine == nullptr || uiwidgets_metrics == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Engine handle was invalid.");
  }

  ViewportMetrics metrics;

  metrics.physical_width = uiwidgets_metrics->width;
  metrics.physical_height = uiwidgets_metrics->height;
  metrics.device_pixel_ratio = uiwidgets_metrics->pixel_ratio;

  if (metrics.device_pixel_ratio <= 0.0) {
    return LOG_EMBEDDER_ERROR(
        kInvalidArguments,
        "Device pixel ratio was invalid. It must be greater than zero.");
  }

  return reinterpret_cast<EmbedderEngine*>(engine)->SetViewportMetrics(metrics)
             ? kSuccess
             : LOG_EMBEDDER_ERROR(kInvalidArguments,
                                  "Viewport metrics were invalid.");
}

// Returns the PointerData::Change for the given UIWidgetsPointerPhase.
inline PointerData::Change ToPointerDataChange(UIWidgetsPointerPhase phase) {
  switch (phase) {
    case kCancel:
      return PointerData::Change::kCancel;
    case kUp:
      return PointerData::Change::kUp;
    case kDown:
      return PointerData::Change::kDown;
    case kMove:
      return PointerData::Change::kMove;
    case kAdd:
      return PointerData::Change::kAdd;
    case kRemove:
      return PointerData::Change::kRemove;
    case kHover:
      return PointerData::Change::kHover;
    case kMouseDown:
      return PointerData::Change::kMouseDown;
    case kMouseUp:
      return PointerData::Change::kMouseUp;
  }
  return PointerData::Change::kCancel;
}

// Returns the PointerData::DeviceKind for the given
// UIWidgetsPointerDeviceKind.
inline PointerData::DeviceKind ToPointerDataKind(
    UIWidgetsPointerDeviceKind device_kind) {
  switch (device_kind) {
    case kUIWidgetsPointerDeviceKindMouse:
      return PointerData::DeviceKind::kMouse;
    case kUIWidgetsPointerDeviceKindTouch:
      return PointerData::DeviceKind::kTouch;
    case kUIWidgetsPointerDeviceKindKeyboard:
      return PointerData::DeviceKind::kKeyboard;
  }
  return PointerData::DeviceKind::kMouse;
}

// Returns the PointerData::SignalKind for the given
// UIWidgetsPointerSignaKind.
inline PointerData::SignalKind ToPointerDataSignalKind(
    UIWidgetsPointerSignalKind kind) {
  switch (kind) {
    case kUIWidgetsPointerSignalKindNone:
      return PointerData::SignalKind::kNone;
    case kUIWidgetsPointerSignalKindScroll:
      return PointerData::SignalKind::kScroll;
    case kUIWidgetsPointerSignalKindEditorDragUpdate:
      return PointerData::SignalKind::kEditorDragUpdate;
    case kUIWidgetsPointerSignalKindEditorDragRelease:
      return PointerData::SignalKind::kEditorDragRelease;
  }
  return PointerData::SignalKind::kNone;
}

// Returns the buttons to synthesize for a PointerData from a
// UIWidgetsPointerEvent with no type or buttons set.
inline int64_t PointerDataButtonsForLegacyEvent(PointerData::Change change) {
  switch (change) {
    case PointerData::Change::kDown:
    case PointerData::Change::kMove:
      // These kinds of change must have a non-zero `buttons`, otherwise gesture
      // recognizers will ignore these events.
      return kPointerButtonMousePrimary;
    case PointerData::Change::kCancel:
    case PointerData::Change::kAdd:
    case PointerData::Change::kRemove:
    case PointerData::Change::kHover:
    case PointerData::Change::kUp:
      return 0;
    default:
      return 0;
  }
  return 0;
}

UIWidgetsEngineResult UIWidgetsEngineSendPointerEvent(
    UIWidgetsEngine engine, const UIWidgetsPointerEvent* pointers,
    size_t events_count) {
  if (engine == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Engine handle was invalid.");
  }

  if (pointers == nullptr || events_count == 0) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Invalid pointer events.");
  }

  auto packet = std::make_unique<PointerDataPacket>(events_count);

  const UIWidgetsPointerEvent* current = pointers;

  for (size_t i = 0; i < events_count; ++i) {
    PointerData pointer_data;
    pointer_data.Clear();
    pointer_data.time_stamp = current->timestamp;
    pointer_data.change = ToPointerDataChange(current->phase);
    pointer_data.physical_x = current->x;
    pointer_data.physical_y = current->y;
    // Delta will be generated in pointer_data_packet_converter.cc.
    pointer_data.physical_delta_x = 0.0;
    pointer_data.physical_delta_y = 0.0;
    pointer_data.device = current->device;
    // Pointer identifier will be generated in pointer_data_packet_converter.cc.
    pointer_data.pointer_identifier = 0;
    pointer_data.signal_kind = ToPointerDataSignalKind(current->signal_kind);
    pointer_data.scroll_delta_x = current->scroll_delta_x;
    pointer_data.scroll_delta_y = current->scroll_delta_y;
    UIWidgetsPointerDeviceKind device_kind = current->device_kind;
    // For backwards compatibility with embedders written before the device kind
    // and buttons were exposed, if the device kind is not set treat it as a
    // mouse, with a synthesized primary button state based on the phase.
    pointer_data.modifier = current->modifier;
    if (device_kind == 0) {
      pointer_data.kind = PointerData::DeviceKind::kMouse;
      pointer_data.buttons =
          PointerDataButtonsForLegacyEvent(pointer_data.change);

    } else {
      pointer_data.kind = ToPointerDataKind(device_kind);
      if (pointer_data.kind == PointerData::DeviceKind::kTouch) {
        // For touch events, set the button internally rather than requiring
        // it at the API level, since it's a confusing construction to expose.
        if (pointer_data.change == PointerData::Change::kDown ||
            pointer_data.change == PointerData::Change::kMove) {
          pointer_data.buttons = kPointerButtonTouchContact;
        }
      } else {
        // Buttons use the same mask values, so pass them through directly.
        pointer_data.buttons = current->buttons;
      }
    }
    packet->SetPointerData(i, pointer_data);
    current = reinterpret_cast<const UIWidgetsPointerEvent*>(
        reinterpret_cast<const uint8_t*>(current) + current->struct_size);
  }

  return reinterpret_cast<EmbedderEngine*>(engine)->DispatchPointerDataPacket(
             std::move(packet))
             ? kSuccess
             : LOG_EMBEDDER_ERROR(kInternalInconsistency,
                                  "Could not dispatch pointer events to the "
                                  "running UIWidgets application.");
}

UIWidgetsEngineResult UIWidgetsEngineSendPlatformMessage(
    UIWidgetsEngine engine, const UIWidgetsPlatformMessage* uiwidgets_message) {
  if (engine == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Invalid engine handle.");
  }

  if (uiwidgets_message == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Invalid message argument.");
  }

  if (uiwidgets_message->channel == nullptr) {
    return LOG_EMBEDDER_ERROR(
        kInvalidArguments, "Message argument did not specify a valid channel.");
  }

  size_t message_size = uiwidgets_message->message_size;
  const uint8_t* message_data = uiwidgets_message->message;

  if (message_size != 0 && message_data == nullptr) {
    return LOG_EMBEDDER_ERROR(
        kInvalidArguments,
        "Message size was non-zero but the message data was nullptr.");
  }

  const UIWidgetsPlatformMessageResponseHandle* response_handle =
      uiwidgets_message->response_handle;

  fml::RefPtr<PlatformMessageResponse> response;
  if (response_handle && response_handle->message) {
    response = response_handle->message->response();
  }

  fml::RefPtr<PlatformMessage> message;
  if (message_size == 0) {
    message = fml::MakeRefCounted<PlatformMessage>(uiwidgets_message->channel,
                                                   response);
  } else {
    message = fml::MakeRefCounted<PlatformMessage>(
        uiwidgets_message->channel,
        std::vector<uint8_t>(message_data, message_data + message_size),
        response);
  }

  return reinterpret_cast<EmbedderEngine*>(engine)->SendPlatformMessage(
             std::move(message))
             ? kSuccess
             : LOG_EMBEDDER_ERROR(kInternalInconsistency,
                                  "Could not send a message to the running "
                                  "UIWidgets application.");
}

UIWidgetsEngineResult UIWidgetsPlatformMessageCreateResponseHandle(
    UIWidgetsEngine engine, UIWidgetsDataCallback data_callback,
    void* user_data, UIWidgetsPlatformMessageResponseHandle** response_out) {
  if (engine == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Engine handle was invalid.");
  }

  if (data_callback == nullptr || response_out == nullptr) {
    return LOG_EMBEDDER_ERROR(
        kInvalidArguments, "Data callback or the response handle was invalid.");
  }

  EmbedderPlatformMessageResponse::Callback response_callback =
      [user_data, data_callback](const uint8_t* data, size_t size) {
        data_callback(data, size, user_data);
      };

  auto platform_task_runner = reinterpret_cast<EmbedderEngine*>(engine)
                                  ->GetTaskRunners()
                                  .GetPlatformTaskRunner();

  auto handle = new UIWidgetsPlatformMessageResponseHandle();

  handle->message = fml::MakeRefCounted<PlatformMessage>(
      "",  // The channel is empty and unused as the response handle is going to
           // referenced directly in the |UIWidgetsEngineSendPlatformMessage|
           // with the container message discarded.
      fml::MakeRefCounted<EmbedderPlatformMessageResponse>(
          std::move(platform_task_runner), response_callback));
  *response_out = handle;
  return kSuccess;
}

UIWidgetsEngineResult UIWidgetsPlatformMessageReleaseResponseHandle(
    UIWidgetsEngine engine, UIWidgetsPlatformMessageResponseHandle* response) {
  if (engine == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Invalid engine handle.");
  }

  if (response == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Invalid response handle.");
  }
  delete response;
  return kSuccess;
}

UIWidgetsEngineResult UIWidgetsEngineSendPlatformMessageResponse(
    UIWidgetsEngine engine,
    const UIWidgetsPlatformMessageResponseHandle* handle, const uint8_t* data,
    size_t data_length) {
  if (data_length != 0 && data == nullptr) {
    return LOG_EMBEDDER_ERROR(
        kInvalidArguments,
        "Data size was non zero but the pointer to the data was null.");
  }

  auto response = handle->message->response();

  if (response) {
    if (data_length == 0) {
      response->CompleteEmpty();
    } else {
      response->Complete(std::make_unique<fml::DataMapping>(
          std::vector<uint8_t>({data, data + data_length})));
    }
  }

  delete handle;

  return kSuccess;
}

UIWidgetsEngineResult __UIWidgetsEngineFlushPendingTasksNow() {
  fml::MessageLoop::GetCurrent().RunExpiredTasksNow();
  return kSuccess;
}

UIWidgetsEngineResult UIWidgetsEngineRegisterExternalTexture(
    UIWidgetsEngine engine, int64_t texture_identifier) {
  if (engine == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Engine handle was invalid.");
  }

  if (texture_identifier == 0) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments,
                              "Texture identifier was invalid.");
  }
  if (!reinterpret_cast<EmbedderEngine*>(engine)->RegisterTexture(
          texture_identifier)) {
    return LOG_EMBEDDER_ERROR(kInternalInconsistency,
                              "Could not register the specified texture.");
  }
  return kSuccess;
}

UIWidgetsEngineResult UIWidgetsEngineUnregisterExternalTexture(
    UIWidgetsEngine engine, int64_t texture_identifier) {
  if (engine == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Engine handle was invalid.");
  }

  if (texture_identifier == 0) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments,
                              "Texture identifier was invalid.");
  }

  if (!reinterpret_cast<EmbedderEngine*>(engine)->UnregisterTexture(
          texture_identifier)) {
    return LOG_EMBEDDER_ERROR(kInternalInconsistency,
                              "Could not un-register the specified texture.");
  }

  return kSuccess;
}

UIWidgetsEngineResult UIWidgetsEngineMarkExternalTextureFrameAvailable(
    UIWidgetsEngine engine, int64_t texture_identifier) {
  if (engine == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Invalid engine handle.");
  }
  if (texture_identifier == 0) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Invalid texture identifier.");
  }
  if (!reinterpret_cast<EmbedderEngine*>(engine)->MarkTextureFrameAvailable(
          texture_identifier)) {
    return LOG_EMBEDDER_ERROR(
        kInternalInconsistency,
        "Could not mark the texture frame as being available.");
  }
  return kSuccess;
}

UIWidgetsEngineResult UIWidgetsEngineUpdateAccessibilityFeatures(
    UIWidgetsEngine engine, UIWidgetsAccessibilityFeature flags) {
  if (engine == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Invalid engine handle.");
  }
  if (!reinterpret_cast<EmbedderEngine*>(engine)->SetAccessibilityFeatures(
          flags)) {
    return LOG_EMBEDDER_ERROR(kInternalInconsistency,
                              "Could not update accessibility features.");
  }
  return kSuccess;
}

UIWidgetsEngineResult UIWidgetsEngineOnVsync(UIWidgetsEngine engine,
                                             intptr_t baton,
                                             uint64_t frame_start_time_nanos,
                                             uint64_t frame_target_time_nanos) {
  if (engine == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Invalid engine handle.");
  }

  TRACE_EVENT0("uiwidgets", "UIWidgetsEngineOnVsync");

  auto start_time = fml::TimePoint::FromEpochDelta(
      fml::TimeDelta::FromNanoseconds(frame_start_time_nanos));

  auto target_time = fml::TimePoint::FromEpochDelta(
      fml::TimeDelta::FromNanoseconds(frame_target_time_nanos));

  if (!reinterpret_cast<EmbedderEngine*>(engine)->OnVsyncEvent(
          baton, start_time, target_time)) {
    return LOG_EMBEDDER_ERROR(
        kInternalInconsistency,
        "Could not notify the running engine instance of a Vsync event.");
  }

  return kSuccess;
}

UIWidgetsEngineResult UIWidgetsEngineReloadSystemFonts(UIWidgetsEngine engine) {
  if (engine == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Invalid engine handle.");
  }

  TRACE_EVENT0("uiwidgets", "UIWidgetsEngineReloadSystemFonts");

  if (!reinterpret_cast<EmbedderEngine*>(engine)->ReloadSystemFonts()) {
    return LOG_EMBEDDER_ERROR(kInternalInconsistency,
                              "Could not reload system fonts.");
  }

  return kSuccess;
}

void UIWidgetsEngineTraceEventDurationBegin(const char* name) {
  fml::tracing::TraceEvent0("uiwidgets", name);
}

void UIWidgetsEngineTraceEventDurationEnd(const char* name) {
  fml::tracing::TraceEventEnd(name);
}

void UIWidgetsEngineTraceEventInstant(const char* name) {
  fml::tracing::TraceEventInstant0("uiwidgets", name);
}

UIWidgetsEngineResult UIWidgetsEnginePostRenderThreadTask(
    UIWidgetsEngine engine, VoidCallback callback, void* baton) {
  if (engine == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Invalid engine handle.");
  }

  if (callback == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments,
                              "Render thread callback was null.");
  }

  auto task = [callback, baton]() { callback(baton); };

  return reinterpret_cast<EmbedderEngine*>(engine)->PostRenderThreadTask(task)
             ? kSuccess
             : LOG_EMBEDDER_ERROR(kInternalInconsistency,
                                  "Could not post the render thread task.");
}

uint64_t UIWidgetsEngineGetCurrentTime() {
  return fml::TimePoint::Now().ToEpochDelta().ToNanoseconds();
}

UIWidgetsEngineResult UIWidgetsEngineRunTask(UIWidgetsEngine engine,
                                             const UIWidgetsTask* task) {
  if (engine == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Invalid engine handle.");
  }

  return reinterpret_cast<EmbedderEngine*>(engine)->RunTask(task)
             ? kSuccess
             : LOG_EMBEDDER_ERROR(kInvalidArguments,
                                  "Could not run the specified task.");
}

static bool DispatchJSONPlatformMessage(UIWidgetsEngine engine,
                                        rapidjson::Document document,
                                        const std::string& channel_name) {
  if (channel_name.size() == 0) {
    return false;
  }

  rapidjson::StringBuffer buffer;
  rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);

  if (!document.Accept(writer)) {
    return false;
  }

  const char* message = buffer.GetString();

  if (message == nullptr || buffer.GetSize() == 0) {
    return false;
  }

  auto platform_message = fml::MakeRefCounted<PlatformMessage>(
      channel_name.c_str(),                                       // channel
      std::vector<uint8_t>{message, message + buffer.GetSize()},  // message
      nullptr                                                     // response
  );

  return reinterpret_cast<EmbedderEngine*>(engine)->SendPlatformMessage(
      std::move(platform_message));
}

UIWidgetsEngineResult UIWidgetsEngineUpdateLocales(
    UIWidgetsEngine engine, const UIWidgetsLocale** locales,
    size_t locales_count) {
  if (engine == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Invalid engine handle.");
  }

  if (locales_count == 0) {
    return kSuccess;
  }

  if (locales == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "No locales were specified.");
  }

  rapidjson::Document document;
  auto& allocator = document.GetAllocator();

  document.SetObject();
  document.AddMember("method", "setLocale", allocator);

  rapidjson::Value args(rapidjson::kArrayType);
  args.Reserve(locales_count * 4, allocator);
  for (size_t i = 0; i < locales_count; ++i) {
    const UIWidgetsLocale* locale = locales[i];
    const char* language_code_str = locale->language_code;
    if (language_code_str == nullptr || ::strlen(language_code_str) == 0) {
      return LOG_EMBEDDER_ERROR(
          kInvalidArguments,
          "Language code is required but not present in UIWidgetsLocale.");
    }

    const char* country_code_str = locale->country_code;
    const char* script_code_str = locale->script_code;
    const char* variant_code_str = locale->variant_code;

    rapidjson::Value language_code, country_code, script_code, variant_code;

    language_code.SetString(language_code_str, allocator);
    country_code.SetString(country_code_str ? country_code_str : "", allocator);
    script_code.SetString(script_code_str ? script_code_str : "", allocator);
    variant_code.SetString(variant_code_str ? variant_code_str : "", allocator);

    // Required.
    args.PushBack(language_code, allocator);
    args.PushBack(country_code, allocator);
    args.PushBack(script_code, allocator);
    args.PushBack(variant_code, allocator);
  }
  document.AddMember("args", args, allocator);

  return DispatchJSONPlatformMessage(engine, std::move(document),
                                     "uiwidgets/localization")
             ? kSuccess
             : LOG_EMBEDDER_ERROR(kInternalInconsistency,
                                  "Could not send message to update locale of "
                                  "a running UIWidgets application.");
}

UIWidgetsEngineResult UIWidgetsEngineNotifyLowMemoryWarning(
    UIWidgetsEngine raw_engine) {
  auto engine = reinterpret_cast<EmbedderEngine*>(raw_engine);
  if (engine == nullptr || !engine->IsValid()) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Engine was invalid.");
  }

  engine->GetShell().NotifyLowMemoryWarning();

  rapidjson::Document document;
  auto& allocator = document.GetAllocator();

  document.SetObject();
  document.AddMember("type", "memoryPressure", allocator);

  return DispatchJSONPlatformMessage(raw_engine, std::move(document),
                                     "uiwidgets/system")
             ? kSuccess
             : LOG_EMBEDDER_ERROR(
                   kInternalInconsistency,
                   "Could not dispatch the low memory notification message.");
}

UIWidgetsEngineResult UIWidgetsEnginePostCallbackOnAllNativeThreads(
    UIWidgetsEngine engine, UIWidgetsNativeThreadCallback callback,
    void* user_data) {
  if (engine == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments, "Invalid engine handle.");
  }

  if (callback == nullptr) {
    return LOG_EMBEDDER_ERROR(kInvalidArguments,
                              "Invalid native thread callback.");
  }

  return reinterpret_cast<EmbedderEngine*>(engine)
                 ->PostTaskOnEngineManagedNativeThreads(
                     [callback, user_data](UIWidgetsNativeThreadType type) {
                       callback(type, user_data);
                     })
             ? kSuccess
             : LOG_EMBEDDER_ERROR(kInvalidArguments,
                                  "Internal error while attempting to post "
                                  "tasks to all threads.");
}
