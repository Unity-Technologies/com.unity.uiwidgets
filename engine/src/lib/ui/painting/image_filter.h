#pragma once

#include "image.h"
#include "include/core/SkImageFilter.h"
#include "picture.h"

namespace uiwidgets {

class ImageFilter : public fml::RefCountedThreadSafe<ImageFilter> {
  FML_FRIEND_MAKE_REF_COUNTED(ImageFilter);

 public:
  ~ImageFilter();
  static fml::RefPtr<ImageFilter> Create();

  void initImage(CanvasImage* image);
  void initPicture(Picture*);
  void initBlur(double sigma_x, double sigma_y);
  void initMatrix(const float* matrix4, int filter_quality);

  const sk_sp<SkImageFilter>& filter() const { return filter_; }

 private:
  ImageFilter();

  sk_sp<SkImageFilter> filter_;
};

}  // namespace uiwidgets
