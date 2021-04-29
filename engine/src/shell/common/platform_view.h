#pragma once

#include <memory>

#include "common/task_runners.h"
#include "flow/texture.h"
#include "flutter/fml/macros.h"
#include "flutter/fml/memory/weak_ptr.h"
#include "include/core/SkSize.h"
#include "include/gpu/GrContext.h"
#include "lib/ui/window/platform_message.h"
#include "lib/ui/window/pointer_data_packet.h"
#include "lib/ui/window/pointer_data_packet_converter.h"
#include "lib/ui/window/viewport_metrics.h"
#include "shell/common/pointer_data_dispatcher.h"
#include "shell/common/surface.h"
#include "shell/common/vsync_waiter.h"

namespace uiwidgets {

class PlatformView {
 public:
  class Delegate {
   public:
    virtual void OnPlatformViewCreated(std::unique_ptr<Surface> surface) = 0;
    virtual void OnPlatformViewDestroyed() = 0;
    virtual void OnPlatformViewSetNextFrameCallback(
        const fml::closure& closure) = 0;
    virtual void OnPlatformViewSetViewportMetrics(
        const ViewportMetrics& metrics) = 0;
    virtual void OnPlatformViewDispatchPlatformMessage(
        fml::RefPtr<PlatformMessage> message) = 0;
    virtual void OnPlatformViewDispatchPointerDataPacket(
        std::unique_ptr<PointerDataPacket> packet) = 0;
    virtual void OnPlatformViewSetAccessibilityFeatures(int32_t flags) = 0;
    virtual void OnPlatformViewRegisterTexture(
        std::shared_ptr<Texture> texture) = 0;
    virtual void OnPlatformViewUnregisterTexture(int64_t texture_id) = 0;
    virtual void OnPlatformViewMarkTextureFrameAvailable(
        int64_t texture_id) = 0;
  };

  explicit PlatformView(Delegate& delegate, TaskRunners task_runners);
  virtual ~PlatformView();
  virtual std::unique_ptr<VsyncWaiter> CreateVSyncWaiter();
  void DispatchPlatformMessage(fml::RefPtr<PlatformMessage> message);
  virtual void HandlePlatformMessage(fml::RefPtr<PlatformMessage> message);
  virtual void SetAccessibilityFeatures(int32_t flags);
  void SetViewportMetrics(const ViewportMetrics& metrics);
  void NotifyCreated();
  virtual void NotifyDestroyed();
  virtual sk_sp<GrContext> CreateResourceContext() const;
  virtual void ReleaseResourceContext() const;
  virtual PointerDataDispatcherMaker GetDispatcherMaker();
  fml::WeakPtr<PlatformView> GetWeakPtr() const;

  virtual void OnPreEngineRestart() const;
  void SetNextFrameCallback(const fml::closure& closure);
  void DispatchPointerDataPacket(std::unique_ptr<PointerDataPacket> packet);
  void RegisterTexture(std::shared_ptr<Texture> texture);
  void UnregisterTexture(int64_t texture_id);
  void MarkTextureFrameAvailable(int64_t texture_id);

 protected:
  PlatformView::Delegate& delegate_;
  const TaskRunners task_runners_;

  PointerDataPacketConverter pointer_data_packet_converter_;
  SkISize size_;
  fml::WeakPtrFactory<PlatformView> weak_factory_;

  // Unlike all other methods on the platform view, this is called on the
  // GPU task runner.
  virtual std::unique_ptr<Surface> CreateRenderingSurface();

 private:
  FML_DISALLOW_COPY_AND_ASSIGN(PlatformView);
};

}  // namespace uiwidgets
