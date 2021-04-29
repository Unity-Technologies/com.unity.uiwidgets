#pragma once

#include <memory>
#include <set>
#include <string>

#include "common/task_runners.h"
#include "flutter/fml/compiler_specific.h"
#include "flutter/fml/macros.h"
#include "flutter/fml/mapping.h"
#include "lib/ui/io_manager.h"
#include "lib/ui/snapshot_delegate.h"
#include "lib/ui/ui_mono_state.h"

namespace uiwidgets {

class MonoIsolate : public UIMonoState {
 public:
  static std::weak_ptr<MonoIsolate> CreateRootIsolate(
      const Settings& settings, TaskRunners task_runners,
      std::unique_ptr<Window> window,
      fml::WeakPtr<SnapshotDelegate> snapshot_delegate,
      fml::WeakPtr<IOManager> io_manager,
      fml::RefPtr<SkiaUnrefQueue> skia_unref_queue,
      fml::WeakPtr<ImageDecoder> image_decoder);

  ~MonoIsolate() override;

  bool Shutdown();

  void AddIsolateShutdownCallback(const fml::closure& closure);

  std::weak_ptr<MonoIsolate> GetWeakIsolatePtr();

  fml::RefPtr<fml::TaskRunner> GetMessageHandlingTaskRunner() const;

 private:
  class AutoFireClosure {
   public:
    AutoFireClosure(const fml::closure& closure);

    ~AutoFireClosure();

   private:
    fml::closure closure_;
    FML_DISALLOW_COPY_AND_ASSIGN(AutoFireClosure);
  };

  std::vector<std::unique_ptr<AutoFireClosure>> shutdown_callbacks_;
  fml::RefPtr<fml::TaskRunner> message_handling_task_runner_;

  MonoIsolate(const Settings& settings, TaskRunners task_runners,
              fml::WeakPtr<SnapshotDelegate> snapshot_delegate,
              fml::WeakPtr<IOManager> io_manager,
              fml::RefPtr<SkiaUnrefQueue> unref_queue,
              fml::WeakPtr<ImageDecoder> image_decoder);

  void SetMessageHandlingTaskRunner(fml::RefPtr<fml::TaskRunner> runner);

  FML_DISALLOW_COPY_AND_ASSIGN(MonoIsolate);
};

}  // namespace uiwidgets
