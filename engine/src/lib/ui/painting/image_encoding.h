#pragma once

#include "runtime/mono_api.h"

namespace uiwidgets {

class CanvasImage;

typedef void (*EncodeImageCallback)(Mono_Handle callback_handle,
                                    const uint8_t* data, size_t length);

const char* EncodeImage(CanvasImage* canvas_image, int format,
                        EncodeImageCallback callback,
                        Mono_Handle callback_handle);

}  // namespace uiwidgets
