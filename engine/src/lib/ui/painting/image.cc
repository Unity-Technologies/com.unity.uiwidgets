#include "image.h"

#include "image_encoding.h"

namespace uiwidgets {

typedef CanvasImage Image;

CanvasImage::CanvasImage() = default;

CanvasImage::~CanvasImage() = default;

const char* CanvasImage::toByteData(int format, RawEncodeImageCallback callback,
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

UIWIDGETS_API(void) Image_dispose(CanvasImage* ptr) { ptr->Release(); }

UIWIDGETS_API(int) Image_width(CanvasImage* ptr) { return ptr->width(); }

UIWIDGETS_API(int) Image_height(CanvasImage* ptr) { return ptr->height(); }

UIWIDGETS_API(const char*)
Image_toByteData(CanvasImage* ptr, int format,
                 RawEncodeImageCallback encode_image_callback,
                 Mono_Handle callback_handle) {
  return ptr->toByteData(format, encode_image_callback, callback_handle);
}

}  // namespace uiwidgets
