#include "single_frame_codec.h"

#include "frame_info.h"
#include "lib/ui/ui_mono_state.h"

namespace uiwidgets {

SingleFrameCodec::SingleFrameCodec(ImageDecoder::ImageDescriptor descriptor)
    : status_(Status::kNew), descriptor_(std::move(descriptor)) {}

SingleFrameCodec::~SingleFrameCodec() = default;

int SingleFrameCodec::frameCount() const { return 1; }

int SingleFrameCodec::repetitionCount() const { return 0; }

const char* SingleFrameCodec::getNextFrame(GetNextFrameCallback callback,
                                           Mono_Handle callback_handle) {
  if (!callback || !callback_handle) {
    return "Callback must be a function";
  }

  if (status_ == Status::kComplete) {
    cached_frame_->AddRef();
    callback(callback_handle, cached_frame_.get());
    return nullptr;
  }

  // This has to be valid because this method is called from Dart.
  auto* mono_state = UIMonoState::Current();

  pending_callbacks_.emplace_back(PendingCallback{mono_state->GetWeakPtr(), callback, callback_handle});

  if (status_ == Status::kInProgress) {
    // Another call to getNextFrame is in progress and will invoke the
    // pending callbacks when decoding completes.
    return nullptr;
  }

  auto decoder = mono_state->GetImageDecoder();

  if (!decoder) {
    return "Image decoder not available.";
  }

  // The SingleFrameCodec must be deleted on the UI thread.  Allocate a RefPtr
  // on the heap to ensure that the SingleFrameCodec remains alive until the
  // decoder callback is invoked on the UI thread.  The callback can then
  // drop the reference.
  fml::RefPtr<SingleFrameCodec>* raw_codec_ref =
      new fml::RefPtr<SingleFrameCodec>(this);

  decoder->Decode(descriptor_, [raw_codec_ref](auto image) {
    std::unique_ptr<fml::RefPtr<SingleFrameCodec>> codec_ref(raw_codec_ref);
    fml::RefPtr<SingleFrameCodec> codec(std::move(*codec_ref));

    auto state = codec->pending_callbacks_.front().mono_state.lock();

    if (!state) {
      // This is probably because the isolate has been terminated before the
      // image could be decoded.

      for (const auto& entry : codec->pending_callbacks_) {
        entry.callback(entry.callback_handle, nullptr);
      }
      codec->pending_callbacks_.clear();
      return;
    }

    MonoState::Scope scope(state.get());

    if (image.get()) {
      auto canvas_image = fml::MakeRefCounted<CanvasImage>();
      canvas_image->set_image(std::move(image));

      codec->cached_frame_ = fml::MakeRefCounted<FrameInfo>(
          std::move(canvas_image), 0 /* duration */);
    }

    // The cached frame is now available and should be returned to any future
    // callers.
    codec->status_ = Status::kComplete;

    // Invoke any callbacks that were provided before the frame was decoded.
    for (const auto& entry : codec->pending_callbacks_) {
      codec->cached_frame_->AddRef();
      entry.callback(entry.callback_handle, codec->cached_frame_.get());
    }
    codec->pending_callbacks_.clear();
  });

  // The encoded data is no longer needed now that it has been handed off
  // to the decoder.
  descriptor_.data.reset();

  status_ = Status::kInProgress;

  return nullptr;
}

size_t SingleFrameCodec::GetAllocationSize() {
  const auto& data = descriptor_.data;
  const auto data_byte_size = data ? data->size() : 0;
  const auto frame_byte_size = (cached_frame_ && cached_frame_->image())
                                   ? cached_frame_->image()->GetAllocationSize()
                                   : 0;
  return data_byte_size + frame_byte_size + sizeof(this);
}

}  // namespace uiwidgets
