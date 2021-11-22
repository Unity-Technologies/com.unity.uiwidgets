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

size_t Picture::GetAllocationSize() {
  if (auto picture = picture_.get()) {
    return picture->approximateBytesUsed();
  } else {
    return sizeof(Picture);
  }
}

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

  auto* mono_state = UIMonoState::Current();
  auto mono_state_weak = mono_state->GetWeakPtr();

  auto unref_queue = mono_state->GetSkiaUnrefQueue();
  auto ui_task_runner = mono_state->GetTaskRunners().GetUITaskRunner();
  auto raster_task_runner = mono_state->GetTaskRunners().GetRasterTaskRunner();
  auto snapshot_delegate = mono_state->GetSnapshotDelegate();

  // We can't create an image on this task runner because we don't have a
  // graphics context. Even if we did, it would be slow anyway. Also, this
  // thread owns the sole reference to the layer tree. So we flatten the layer
  // tree into a picture and use that as the thread transport mechanism.

  auto picture_bounds = SkISize::Make(width, height);

  auto ui_task = fml::MakeCopyable([mono_state_weak, raw_image_callback,
                                    callback_handle, unref_queue](
                                       sk_sp<SkImage> raster_image) mutable {
    auto mono_state = mono_state_weak.lock();
    if (!mono_state) {
      // The root isolate could have died in the meantime.
      raw_image_callback(callback_handle, nullptr);
      return;
    }
  	
    MonoState::Scope scope(mono_state);

    if (!raster_image) {
      raw_image_callback(callback_handle, nullptr);
      return;
    }

    auto mono_image = CanvasImage::Create();
    mono_image->set_image({std::move(raster_image), std::move(unref_queue)});
    mono_image->AddRef();
    auto* raw_mono_image = mono_image.get();

    // All done!
    raw_image_callback(callback_handle, raw_mono_image);
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

UIWIDGETS_API(void) Picture_dispose(Picture* ptr) { ptr->Release(); }

UIWIDGETS_API(size_t) Picture_GetAllocationSize(Picture* ptr) {
  return ptr->GetAllocationSize();
}

UIWIDGETS_API(const char*)
Picture_toImage(Picture* ptr, uint32_t width, uint32_t height,
                Picture::RawImageCallback raw_image_callback,
                Mono_Handle callback_handle) {
  return ptr->toImage(width, height, raw_image_callback, callback_handle);
}

}  // namespace uiwidgets
