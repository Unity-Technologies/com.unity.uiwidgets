#pragma once

#include <memory>
#include <unordered_map>

#include "flutter/fml/macros.h"
#include "shell/common/shell.h"
#include "shell/common/thread_host.h"
#include "shell/platform/embedder/embedder.h"
#include "shell/platform/embedder/embedder_engine.h"
#include "shell/platform/embedder/embedder_external_texture_gl.h"
#include "shell/platform/embedder/embedder_thread_host.h"

namespace uiwidgets {

struct ShellArgs;

class EmbedderEngine {
 public:
  EmbedderEngine(std::unique_ptr<EmbedderThreadHost> thread_host,
                 const TaskRunners& task_runners, const WindowData& window_data, const Settings& settings,
                 RunConfiguration run_configuration,
                 Shell::CreateCallback<PlatformView> on_create_platform_view,
                 Shell::CreateCallback<Rasterizer> on_create_rasterizer,
                 EmbedderExternalTextureGL::ExternalTextureCallback
                     external_texture_callback);

  ~EmbedderEngine();

  bool LaunchShell();

  bool CollectShell();

  const TaskRunners& GetTaskRunners() const;

  bool NotifyCreated();

  bool NotifyDestroyed();

  bool RunRootIsolate();

  bool IsValid() const;

  bool SetViewportMetrics(ViewportMetrics metrics);

  bool DispatchPointerDataPacket(std::unique_ptr<PointerDataPacket> packet);

  bool SendPlatformMessage(fml::RefPtr<PlatformMessage> message);

  bool RegisterTexture(int64_t texture);

  bool UnregisterTexture(int64_t texture);

  bool MarkTextureFrameAvailable(int64_t texture);

  bool SetAccessibilityFeatures(int32_t flags);

  bool OnVsyncEvent(intptr_t baton, fml::TimePoint frame_start_time,
                    fml::TimePoint frame_target_time);

  bool ReloadSystemFonts();

  bool PostRenderThreadTask(const fml::closure& task);

  bool RunTask(const UIWidgetsTask* task);

  bool PostTaskOnEngineManagedNativeThreads(
      std::function<void(UIWidgetsNativeThreadType)> closure) const;

  Shell& GetShell();

 private:
  const std::unique_ptr<EmbedderThreadHost> thread_host_;
  TaskRunners task_runners_;
  RunConfiguration run_configuration_;
  std::unique_ptr<ShellArgs> shell_args_;
  std::unique_ptr<Shell> shell_;
  const EmbedderExternalTextureGL::ExternalTextureCallback
      external_texture_callback_;

  FML_DISALLOW_COPY_AND_ASSIGN(EmbedderEngine);
};

}  // namespace uiwidgets
