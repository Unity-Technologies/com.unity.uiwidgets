#include "multi_frame_codec.h"

#include "flutter/fml/make_copyable.h"
#include "include/core/SkPixelRef.h"
#include "lib/ui/ui_mono_state.h"

namespace uiwidgets {

MultiFrameCodec::MultiFrameCodec(std::unique_ptr<SkCodec> codec)
    : state_(new State(std::move(codec))) {}

MultiFrameCodec::~MultiFrameCodec() = default;

MultiFrameCodec::State::State(std::unique_ptr<SkCodec> codec)
    : codec_(std::move(codec)),
      frameCount_(codec_->getFrameCount()),
      repetitionCount_(codec_->getRepetitionCount()),
      nextFrameIndex_(0) {}

static void InvokeNextFrameCallback(
    fml::RefPtr<FrameInfo> frameInfo,
    std::unique_ptr<Codec::PendingCallback> callback, size_t trace_id) {
  std::shared_ptr<MonoState> mono_state = callback->mono_state.lock();
  if (!mono_state) {
    FML_DLOG(ERROR) << "Could not acquire Mono state while attempting to fire "
                       "next frame callback.";
    callback->callback(callback->callback_handle, nullptr);
    return;
  }
  MonoState::Scope scope(mono_state);
  if (!frameInfo) {
    callback->callback(callback->callback_handle, nullptr);
  } else {
    frameInfo->AddRef();
    callback->callback(callback->callback_handle, frameInfo.get());
  }
}

// Copied the source bitmap to the destination. If this cannot occur due to
// running out of memory or the image info not being compatible, returns false.
static bool CopyToBitmap(SkBitmap* dst, SkColorType dstColorType,
                         const SkBitmap& src) {
  SkPixmap srcPM;
  if (!src.peekPixels(&srcPM)) {
    return false;
  }

  SkBitmap tmpDst;
  SkImageInfo dstInfo = srcPM.info().makeColorType(dstColorType);
  if (!tmpDst.setInfo(dstInfo)) {
    return false;
  }

  if (!tmpDst.tryAllocPixels()) {
    return false;
  }

  SkPixmap dstPM;
  if (!tmpDst.peekPixels(&dstPM)) {
    return false;
  }

  if (!srcPM.readPixels(dstPM)) {
    return false;
  }

  dst->swap(tmpDst);
  return true;
}

sk_sp<SkImage> MultiFrameCodec::State::GetNextFrameImage(
    fml::WeakPtr<GrContext> resourceContext) {
  SkBitmap bitmap = SkBitmap();
  SkImageInfo info = codec_->getInfo().makeColorType(kN32_SkColorType);
  if (info.alphaType() == kUnpremul_SkAlphaType) {
    info = info.makeAlphaType(kPremul_SkAlphaType);
  }
  bitmap.allocPixels(info);

  SkCodec::Options options;
  options.fFrameIndex = nextFrameIndex_;
  SkCodec::FrameInfo frameInfo;
  codec_->getFrameInfo(nextFrameIndex_, &frameInfo);
  const int requiredFrameIndex = frameInfo.fRequiredFrame;
  if (requiredFrameIndex != SkCodec::kNoFrame) {
    if (lastRequiredFrame_ == nullptr) {
      FML_LOG(ERROR) << "Frame " << nextFrameIndex_ << " depends on frame "
                     << requiredFrameIndex
                     << " and no required frames are cached.";
      return nullptr;
    } else if (lastRequiredFrameIndex_ != requiredFrameIndex) {
      FML_DLOG(INFO) << "Required frame " << requiredFrameIndex
                     << " is not cached. Using " << lastRequiredFrameIndex_
                     << " instead";
    }

    if (lastRequiredFrame_->getPixels() &&
        CopyToBitmap(&bitmap, lastRequiredFrame_->colorType(),
                     *lastRequiredFrame_)) {
      options.fPriorFrame = requiredFrameIndex;
    }
  }

  if (SkCodec::kSuccess != codec_->getPixels(info, bitmap.getPixels(),
                                             bitmap.rowBytes(), &options)) {
    FML_LOG(ERROR) << "Could not getPixels for frame " << nextFrameIndex_;
    return nullptr;
  }

  // Hold onto this if we need it to decode future frames.
  if (frameInfo.fDisposalMethod == SkCodecAnimation::DisposalMethod::kKeep) {
    lastRequiredFrame_ = std::make_unique<SkBitmap>(bitmap);
    lastRequiredFrameIndex_ = nextFrameIndex_;
  }

  if (resourceContext) {
    SkPixmap pixmap(bitmap.info(), bitmap.pixelRef()->pixels(),
                    bitmap.pixelRef()->rowBytes());
    return SkImage::MakeCrossContextFromPixmap(resourceContext.get(), pixmap,
                                               true);
  } else {
    // Defer decoding until time of draw later on the raster thread. Can happen
    // when GL operations are currently forbidden such as in the background
    // on iOS.
    return SkImage::MakeFromBitmap(bitmap);
  }
}

void MultiFrameCodec::State::GetNextFrameAndInvokeCallback(
    std::unique_ptr<PendingCallback> callback,
    fml::RefPtr<fml::TaskRunner> ui_task_runner,
    fml::WeakPtr<GrContext> resourceContext,
    fml::RefPtr<SkiaUnrefQueue> unref_queue, size_t trace_id) {
  fml::RefPtr<FrameInfo> frameInfo = NULL;
  sk_sp<SkImage> skImage = GetNextFrameImage(resourceContext);
  if (skImage) {
    fml::RefPtr<CanvasImage> image = CanvasImage::Create();
    image->set_image({skImage, std::move(unref_queue)});
    SkCodec::FrameInfo skFrameInfo;
    codec_->getFrameInfo(nextFrameIndex_, &skFrameInfo);
    frameInfo =
        fml::MakeRefCounted<FrameInfo>(std::move(image), skFrameInfo.fDuration);
  }
  nextFrameIndex_ = (nextFrameIndex_ + 1) % frameCount_;

  ui_task_runner->PostTask(fml::MakeCopyable(
      [callback = std::move(callback), frameInfo, trace_id]() mutable {
        InvokeNextFrameCallback(frameInfo, std::move(callback), trace_id);
      }));
}

const char* MultiFrameCodec::getNextFrame(GetNextFrameCallback callback,
                                          Mono_Handle callback_handle) {
  static size_t trace_counter = 1;
  const size_t trace_id = trace_counter++;

  if (!callback || !callback_handle) {
    return "Callback must be a function";
  }

  auto* mono_state = UIMonoState::Current();

  const auto& task_runners = mono_state->GetTaskRunners();

  task_runners.GetIOTaskRunner()->PostTask(fml::MakeCopyable(
      [callback = std::make_unique<PendingCallback>(PendingCallback{
           MonoState::Current()->GetWeakPtr(), callback, callback_handle}),
       weak_state = std::weak_ptr<MultiFrameCodec::State>(state_), trace_id,
       ui_task_runner = task_runners.GetUITaskRunner(),
       io_manager = mono_state->GetIOManager()]() mutable {
        auto state = weak_state.lock();
        if (!state) {
          ui_task_runner->PostTask(
              fml::MakeCopyable([callback = std::move(callback)]() {
                callback->callback(callback->callback_handle, nullptr);
              }));
          return;
        }
        state->GetNextFrameAndInvokeCallback(
            std::move(callback), std::move(ui_task_runner),
            io_manager->GetResourceContext(), io_manager->GetSkiaUnrefQueue(),
            trace_id);
      }));

  return nullptr;
}

int MultiFrameCodec::frameCount() const { return state_->frameCount_; }

int MultiFrameCodec::repetitionCount() const {
  return state_->repetitionCount_;
}

}  // namespace uiwidgets
