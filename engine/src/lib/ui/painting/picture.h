#pragma once

#include "flow/skia_gpu_object.h"
#include "image.h"
#include "include/core/SkPicture.h"

namespace uiwidgets {
class Canvas;

class Picture : public fml::RefCountedThreadSafe<Picture> {
  FML_FRIEND_MAKE_REF_COUNTED(Picture);

 public:
  ~Picture();
  static fml::RefPtr<Picture> Create(SkiaGPUObject<SkPicture> picture);

  sk_sp<SkPicture> picture() const { return picture_.get(); }

  typedef void (*RawImageCallback)(Mono_Handle callback_handle,
                                   CanvasImage* image);

  const char* toImage(uint32_t width, uint32_t height,
                      RawImageCallback raw_image_callback,
                      Mono_Handle callback_handle);

  void dispose();

  size_t GetAllocationSize();

  static const char* RasterizeToImage(sk_sp<SkPicture> picture, uint32_t width,
                                      uint32_t height,
                                      RawImageCallback raw_image_callback,
                                      Mono_Handle callback_handle);

 private:
  explicit Picture(SkiaGPUObject<SkPicture> picture);

  SkiaGPUObject<SkPicture> picture_;
};

}  // namespace uiwidgets
