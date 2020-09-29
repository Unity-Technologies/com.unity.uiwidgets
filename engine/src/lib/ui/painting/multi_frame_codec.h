#pragma once

#include "codec.h"
#include "flutter/fml/macros.h"
#include "runtime/mono_state.h"

namespace uiwidgets {

class MultiFrameCodec : public Codec {
 public:
  MultiFrameCodec(std::unique_ptr<SkCodec> codec);

  ~MultiFrameCodec() override;

  // |Codec|
  int frameCount() const override;

  // |Codec|
  int repetitionCount() const override;

  // |Codec|
  const char* getNextFrame(GetNextFrameCallback callback,
                           Mono_Handle callback_handle) override;

 private:
	
  // Captures the state shared between the IO and UI task runners.
  //
  // The state is initialized on the UI task runner when the Dart object is
  // created. Decoding occurs on the IO task runner. Since it is possible for
  // the UI object to be collected independently of the IO task runner work,
  // it is not safe for this state to live directly on the MultiFrameCodec.
  // Instead, the MultiFrameCodec creates this object when it is constructed,
  // shares it with the IO task runner's decoding work, and sets the live_
  // member to false when it is destructed.
  struct State {
    State(std::unique_ptr<SkCodec> codec);

    const std::unique_ptr<SkCodec> codec_;
    const int frameCount_;
    const int repetitionCount_;

    // The non-const members and functions below here are only read or written
    // to on the IO thread. They are not safe to access or write on the UI
    // thread.
    int nextFrameIndex_;
    // The last decoded frame that's required to decode any subsequent frames.
    std::unique_ptr<SkBitmap> lastRequiredFrame_;

    // The index of the last decoded required frame.
    int lastRequiredFrameIndex_ = -1;

    sk_sp<SkImage> GetNextFrameImage(fml::WeakPtr<GrContext> resourceContext);

    void GetNextFrameAndInvokeCallback(
        std::unique_ptr<PendingCallback> callback,
        fml::RefPtr<fml::TaskRunner> ui_task_runner,
        fml::WeakPtr<GrContext> resourceContext,
        fml::RefPtr<SkiaUnrefQueue> unref_queue, size_t trace_id);
  };

  // Shared across the UI and IO task runners.
  std::shared_ptr<State> state_;

  FML_FRIEND_MAKE_REF_COUNTED(MultiFrameCodec);
  FML_FRIEND_REF_COUNTED_THREAD_SAFE(MultiFrameCodec);
};

}  // namespace uiwidgets
