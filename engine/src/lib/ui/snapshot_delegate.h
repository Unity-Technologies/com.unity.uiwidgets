#pragma once

#include "include/core/SkImage.h"
#include "include/core/SkPicture.h"

namespace uiwidgets {

class SnapshotDelegate {
 public:
  virtual sk_sp<SkImage> MakeRasterSnapshot(sk_sp<SkPicture> picture,
                                            SkISize picture_size) = 0;

  virtual sk_sp<SkImage> ConvertToRasterImage(sk_sp<SkImage> image) = 0;
};

}  // namespace uiwidgets
