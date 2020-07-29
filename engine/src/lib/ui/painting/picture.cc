#include "picture.h"

#include "flutter/fml/make_copyable.h"
#include "image.h"
#include "include/core/SkImage.h"
#include "lib/ui/painting/canvas.h"
#include "lib/ui/ui_mono_state.h"

namespace uiwidgets {

fml::RefPtr<Picture> Picture::Create(SkiaGPUObject<SkPicture> picture) {
  return fml::MakeRefCounted<Picture>(std::move(picture));
}

Picture::Picture(SkiaGPUObject<SkPicture> picture)
    : picture_(std::move(picture)) {}

Picture::~Picture() = default;

const char* Picture::toImage(uint32_t width, uint32_t height,
                             RawImageCallback raw_image_callback,
                             Mono_Handle callback_handle) {
  if (!picture_.get()) {
    return "Picture is null";
  }

  return RasterizeToImage(picture_.get(), width, height, raw_image_callback,
                          callback_handle);
}

void Picture::dispose() {}

const char* Picture::RasterizeToImage(sk_sp<SkPicture> picture, uint32_t width,
                                      uint32_t height,
                                      RawImageCallback raw_image_callback,
                                      Mono_Handle callback_handle) {
  if (!raw_image_callback || !callback_handle) {
    return "Image callback was invalid";
  }

  if (width == 0 || height == 0) {
    return "Image dimensions for scene were invalid.";
  }

  auto* dart_state = UIMonoState::Current();
  auto dart_state_weak = dart_state->GetWeakPtr();

  auto unref_queue = dart_state->GetSkiaUnrefQueue();
  auto ui_task_runner = dart_state->GetTaskRunners().GetUITaskRunner();
  auto raster_task_runner = dart_state->GetTaskRunners().GetRasterTaskRunner();
  auto snapshot_delegate = dart_state->GetSnapshotDelegate();

  // We can't create an image on this task runner because we don't have a
  // graphics context. Even if we did, it would be slow anyway. Also, this
  // thread owns the sole reference to the layer tree. So we flatten the layer
  // tree into a picture and use that as the thread transport mechanism.

  auto picture_bounds = SkISize::Make(width, height);

  auto ui_task = fml::MakeCopyable([dart_state_weak, raw_image_callback,
                                    callback_handle, unref_queue](
                                       sk_sp<SkImage> raster_image) mutable {
    auto dart_state = dart_state_weak.lock();
    if (!dart_state) {
      // The root isolate could have died in the meantime.
      return;
    }
    MonoState::Scope scope(dart_state);

    if (!raster_image) {
      raw_image_callback(callback_handle, nullptr);
      return;
    }

    auto dart_image = CanvasImage::Create();
    dart_image->set_image({std::move(raster_image), std::move(unref_queue)});
    auto* raw_dart_image = dart_image.get();

    // All done!
    raw_image_callback(callback_handle, raw_dart_image);
  });

  // Kick things off on the raster rask runner.
  fml::TaskRunner::RunNowOrPostTask(
      raster_task_runner,
      [ui_task_runner, snapshot_delegate, picture, picture_bounds, ui_task] {
        sk_sp<SkImage> raster_image =
            snapshot_delegate->MakeRasterSnapshot(picture, picture_bounds);

        fml::TaskRunner::RunNowOrPostTask(
            ui_task_runner,
            [ui_task, raster_image]() { ui_task(raster_image); });
      });

  return nullptr;
}

}  // namespace uiwidgets
