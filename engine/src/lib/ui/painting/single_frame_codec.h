#pragma once

#include "codec.h"
#include "flutter/fml/macros.h"
#include "frame_info.h"
#include "image_decoder.h"

namespace uiwidgets {

class SingleFrameCodec : public Codec {
 public:
  SingleFrameCodec(ImageDecoder::ImageDescriptor descriptor);

  ~SingleFrameCodec() override;

  // |Codec|
  int frameCount() const override;

  // |Codec|
  int repetitionCount() const override;

  // |Codec|
  const char* getNextFrame(GetNextFrameCallback callback,
                           Mono_Handle callback_handle) override;

  size_t GetAllocationSize() override;

 private:
  enum class Status { kNew, kInProgress, kComplete };
  Status status_;
  ImageDecoder::ImageDescriptor descriptor_;
  fml::RefPtr<FrameInfo> cached_frame_;

  std::vector<PendingCallback> pending_callbacks_;

  FML_FRIEND_MAKE_REF_COUNTED(SingleFrameCodec);
  FML_FRIEND_REF_COUNTED_THREAD_SAFE(SingleFrameCodec);
};

}  // namespace uiwidgets
