#pragma once

#include "flow/skia_gpu_object.h"
#include "image_encoding.h"
#include "include/core/SkImage.h"

namespace uiwidgets {

class CanvasImage final : public fml::RefCountedThreadSafe<CanvasImage> {
  FML_FRIEND_MAKE_REF_COUNTED(CanvasImage);

 public:
  ~CanvasImage();
  static fml::RefPtr<CanvasImage> Create() {
    return fml::MakeRefCounted<CanvasImage>();
  }

  int width() { return image_.get()->width(); }

  int height() { return image_.get()->height(); }

  const char* toByteData(int format, RawEncodeImageCallback callback,
                         Mono_Handle callback_handle);

  void dispose();

  sk_sp<SkImage> image() const { return image_.get(); }
  void set_image(SkiaGPUObject<SkImage> image) { image_ = std::move(image); }

  size_t GetAllocationSize();

 private:
  CanvasImage();

  SkiaGPUObject<SkImage> image_;
};

}  // namespace uiwidgets
