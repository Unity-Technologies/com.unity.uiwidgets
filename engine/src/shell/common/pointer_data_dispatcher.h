#pragma once

#include <flutter/fml/closure.h>
#include <flutter/fml/memory/weak_ptr.h>

#include <memory>

#include "lib/ui/window/pointer_data_packet.h"

namespace uiwidgets {

class PointerDataDispatcher {
 public:
  class Delegate {
   public:
    virtual void DoDispatchPacket(std::unique_ptr<PointerDataPacket> packet,
                                  uint64_t trace_flow_id) = 0;

    virtual void ScheduleSecondaryVsyncCallback(
        const fml::closure& callback) = 0;
  };

  virtual void DispatchPacket(std::unique_ptr<PointerDataPacket> packet,
                              uint64_t trace_flow_id) = 0;

  virtual ~PointerDataDispatcher();
};

class DefaultPointerDataDispatcher : public PointerDataDispatcher {
 public:
  DefaultPointerDataDispatcher(Delegate& delegate) : delegate_(delegate) {}

  void DispatchPacket(std::unique_ptr<PointerDataPacket> packet,
                      uint64_t trace_flow_id) override;

  virtual ~DefaultPointerDataDispatcher();

 protected:
  Delegate& delegate_;

  FML_DISALLOW_COPY_AND_ASSIGN(DefaultPointerDataDispatcher);
};

class SmoothPointerDataDispatcher : public DefaultPointerDataDispatcher {
 public:
  SmoothPointerDataDispatcher(Delegate& delegate);

  void DispatchPacket(std::unique_ptr<PointerDataPacket> packet,
                      uint64_t trace_flow_id) override;

  virtual ~SmoothPointerDataDispatcher();

 private:
  std::unique_ptr<PointerDataPacket> pending_packet_;
  int pending_trace_flow_id_ = -1;

  bool is_pointer_data_in_progress_ = false;

  fml::WeakPtrFactory<SmoothPointerDataDispatcher> weak_factory_;

  void DispatchPendingPacket();

  void ScheduleSecondaryVsyncCallback();

  FML_DISALLOW_COPY_AND_ASSIGN(SmoothPointerDataDispatcher);
};

using PointerDataDispatcherMaker =
    std::function<std::unique_ptr<PointerDataDispatcher>(
        PointerDataDispatcher::Delegate&)>;

}  // namespace uiwidgets
