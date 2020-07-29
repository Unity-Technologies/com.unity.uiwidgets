#pragma once

#include <flutter/fml/macros.h>

#include "mono_api.h"

namespace uiwidgets {

class MonoIsolateScope {
 public:
  explicit MonoIsolateScope(Mono_Isolate isolate);
  ~MonoIsolateScope();

 private:
  Mono_Isolate isolate_;
  Mono_Isolate previous_;

  FML_DISALLOW_COPY_AND_ASSIGN(MonoIsolateScope);
};

}  // namespace uiwidgets
