#pragma once

#include "runtime/mono_api.h"
#include "runtime/mono_state.h"

namespace uiwidgets {

class CanvasImage;

typedef void (*RawEncodeImageCallback)(Mono_Handle callback_handle,
                                       const uint8_t* data, size_t length);

struct EncodeImageCallback {
  std::weak_ptr<MonoState> mono_state;
  RawEncodeImageCallback callback;
  Mono_Handle callback_handle;
};

const char* EncodeImage(CanvasImage* canvas_image, int format,
                        RawEncodeImageCallback callback, Mono_Handle callback_handle);

}  // namespace uiwidgets
