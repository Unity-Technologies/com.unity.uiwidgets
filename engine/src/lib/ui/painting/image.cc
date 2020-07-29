#include "image.h"

#include "image_encoding.h"

namespace uiwidgets {

typedef CanvasImage Image;

CanvasImage::CanvasImage() = default;

CanvasImage::~CanvasImage() = default;

const char* CanvasImage::toByteData(int format, EncodeImageCallback callback,
                                    Mono_Handle callback_handle) {
  return EncodeImage(this, format, callback, callback_handle);
}

void CanvasImage::dispose() {}

size_t CanvasImage::GetAllocationSize() {
  if (auto image = image_.get()) {
    const auto& info = image->imageInfo();
    const auto kMipmapOverhead = 4.0 / 3.0;
    const size_t image_byte_size = info.computeMinByteSize() * kMipmapOverhead;
    return image_byte_size + sizeof(this);
  } else {
    return sizeof(CanvasImage);
  }
}

}  // namespace uiwidgets
