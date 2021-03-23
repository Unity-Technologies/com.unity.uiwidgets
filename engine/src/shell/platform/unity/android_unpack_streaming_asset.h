#pragma once
#include "runtime/mono_api.h"

namespace uiwidgets {

typedef const char* (*UnpackFileCallback)(const char* file);


class AndroidUnpackStreamingAsset{
 public:
  static UnpackFileCallback _unpack;

  static const char* Unpack(const char* file);
};

} // namespace uiwidgets
