#pragma once

#include "frame_info.h"
#include "include/codec/SkCodec.h"
#include "include/core/SkBitmap.h"
#include "include/core/SkImage.h"
#include "runtime/mono_state.h"

namespace uiwidgets {

class Codec : public fml::RefCountedThreadSafe<Codec> {
 public:
  virtual ~Codec() {}

  virtual int frameCount() const = 0;

  virtual int repetitionCount() const = 0;

  typedef void (*InstantiateImageCodecCallback)(Mono_Handle callback_handle,
                                                Codec* codec);

  typedef void (*GetNextFrameCallback)(Mono_Handle callback_handle,
                                       FrameInfo* frame_info);

  virtual const char* getNextFrame(GetNextFrameCallback callback,
                                   Mono_Handle callback_handle) = 0;

  virtual size_t GetAllocationSize() { return 0; }

  void dispose();

  struct _ImageInfo {
    int width;
    int height;
    int format;
    int rowBytes;
  };

  struct PendingCallback {
    std::weak_ptr<MonoState> mono_state;
    GetNextFrameCallback callback;
    Mono_Handle callback_handle;
  };
};

}  // namespace uiwidgets
