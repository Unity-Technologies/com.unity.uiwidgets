#pragma once

#include "image.h"

namespace uiwidgets {

// A single animation frame.
class FrameInfo final : public fml::RefCountedThreadSafe<FrameInfo> {
 public:
  int durationMillis() { return durationMillis_; }
  fml::RefPtr<CanvasImage> image() { return image_; }

 private:
  FrameInfo(fml::RefPtr<CanvasImage> image, int durationMillis);

  ~FrameInfo();

  const fml::RefPtr<CanvasImage> image_;
  const int durationMillis_;

  FML_FRIEND_MAKE_REF_COUNTED(FrameInfo);
  FML_FRIEND_REF_COUNTED_THREAD_SAFE(FrameInfo);
};

}  // namespace uiwidgets
