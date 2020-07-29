#pragma once

#include <vector>

#include "flutter/fml/mapping.h"
#include "flutter/fml/memory/ref_counted.h"
#include "flutter/fml/memory/ref_ptr.h"

namespace uiwidgets {

class PlatformMessageResponse
    : public fml::RefCountedThreadSafe<PlatformMessageResponse> {
  FML_FRIEND_REF_COUNTED_THREAD_SAFE(PlatformMessageResponse);

 public:
  // Callable on any thread.
  virtual void Complete(std::unique_ptr<fml::Mapping> data) = 0;
  virtual void CompleteEmpty() = 0;

  bool is_complete() const { return is_complete_; }

 protected:
  PlatformMessageResponse();
  virtual ~PlatformMessageResponse();

  bool is_complete_ = false;
};

}  // namespace uiwidgets
