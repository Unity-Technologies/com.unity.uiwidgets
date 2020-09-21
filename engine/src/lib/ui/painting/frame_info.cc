#include "frame_info.h"

namespace uiwidgets {

FrameInfo::FrameInfo(fml::RefPtr<CanvasImage> image, int durationMillis)
    : image_(std::move(image)), durationMillis_(durationMillis) {}

FrameInfo::~FrameInfo() {}

UIWIDGETS_API(void) FrameInfo_dispose(FrameInfo* ptr) { ptr->Release(); }

UIWIDGETS_API(int) FrameInfo_durationMillis(FrameInfo* ptr) {
  return ptr->durationMillis();
}

UIWIDGETS_API(CanvasImage*) FrameInfo_image(FrameInfo* ptr) {
  auto image = ptr->image();
  image->AddRef();
  return image.get();
}

}  // namespace uiwidgets
