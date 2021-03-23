#pragma once
#include "runtime/mono_api.h"

namespace uiwidgets {

typedef bool (*UnpackFileCallback)(const char* file);


class AndroidUnpackStreamingAsset{
 public:
  static UnpackFileCallback _unpack;

  static bool Unpack(const char* file);
};

} // namespace uiwidgets
