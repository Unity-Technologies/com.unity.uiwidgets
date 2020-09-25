#pragma once
#include "runtime/mono_api.h"

namespace uiwidgets {
#define DATALIST(T, N)  \
  extern "C" struct N { \
    T* data;            \
    int length;         \
  };
DATALIST(float, Float32List)
DATALIST(int, Int32List)
DATALIST(char, CharList)
}  // namespace uiwidgets