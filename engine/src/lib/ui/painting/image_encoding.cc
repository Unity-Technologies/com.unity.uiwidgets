#include "image_encoding.h"

#include <memory>
#include <utility>

#include "flutter/fml/build_config.h"
#include "flutter/fml/make_copyable.h"
#include "flutter/fml/trace_event.h"
#include "include/core/SkCanvas.h"
#include "include/core/SkEncodedImageFormat.h"
#include "include/core/SkImage.h"
#include "include/core/SkSurface.h"
#include "lib/ui/painting/image.h"
#include "lib/ui/ui_mono_state.h"

namespace uiwidgets {
namespace {

void InvokeDataCallback(std::unique_ptr<EncodeImageCallback> callback,
                        sk_sp<SkData> buffer) {
  std::shared_ptr<MonoState> mono_state = callback->mono_state.lock();
  if (!mono_state) {
    callback->callback(callback->callback_handle, nullptr, 0);
    return;
  }
  MonoState::Scope scope(mono_state);
  if (!buffer) {
    callback->callback(callback->callback_handle, nullptr, 0);
  } else {
    callback->callback(callback->callback_handle, buffer->bytes(),
                       buffer->size());
  }
}

// This must be kept in sync with the enum in painting.dart
enum ImageByteFormat {
  kRawRGBA,
  kRawUnmodified,
  kPNG,
};

sk_sp<SkImage> ConvertToRasterUsingResourceContext(
    sk_sp<SkImage> image, GrContext* resource_context) {
  sk_sp<SkSurface> surface;
  SkImageInfo surface_info = SkImageInfo::MakeN32Premul(image->dimensions());
  if (resource_context) {
    surface = SkSurface::MakeRenderTarget(resource_context, SkBudgeted::kNo,
                                          surface_info);
  } else {
    surface = SkSurface::MakeRaster(surface_info);
  }

  if (surface == nullptr || surface->getCanvas() == nullptr) {
    FML_LOG(ERROR) << "Could not create a surface to copy the texture into.";
    return nullptr;
  }

  surface->getCanvas()->drawImage(image, 0, 0);
  surface->getCanvas()->flush();

  auto snapshot = surface->makeImageSnapshot();

  if (snapshot == nullptr) {
    FML_LOG(ERROR) << "Could not snapshot image to encode.";
    return nullptr;
  }

  return snapshot->makeRasterImage();
}

void ConvertImageToRaster(sk_sp<SkImage> image,
                          std::function<void(sk_sp<SkImage>)> encode_task,
                          fml::RefPtr<fml::TaskRunner> raster_task_runner,
                          fml::RefPtr<fml::TaskRunner> io_task_runner,
                          GrContext* resource_context,
                          fml::WeakPtr<SnapshotDelegate> snapshot_delegate) {
  // Check validity of the image.
  if (image == nullptr) {
    FML_LOG(ERROR) << "Image was null.";
    encode_task(nullptr);
    return;
  }

  auto dimensions = image->dimensions();

  if (dimensions.isEmpty()) {
    FML_LOG(ERROR) << "Image dimensions were empty.";
    encode_task(nullptr);
    return;
  }

  SkPixmap pixmap;
  if (image->peekPixels(&pixmap)) {
    // This is already a raster image.
    encode_task(image);
    return;
  }

  if (sk_sp<SkImage> raster_image = image->makeRasterImage()) {
    // The image can be converted to a raster image.
    encode_task(raster_image);
    return;
  }

  // Cross-context images do not support makeRasterImage. Convert these images
  // by drawing them into a surface.  This must be done on the raster thread
  // to prevent concurrent usage of the image on both the IO and raster threads.
  raster_task_runner->PostTask([image, encode_task = std::move(encode_task),
                                resource_context, snapshot_delegate,
                                io_task_runner]() {
    sk_sp<SkImage> raster_image =
        snapshot_delegate->ConvertToRasterImage(image);

    io_task_runner->PostTask([image, encode_task = std::move(encode_task),
                              raster_image = std::move(raster_image),
                              resource_context]() mutable {
      if (!raster_image) {
        // The rasterizer was unable to render the cross-context image
        // (presumably because it does not have a GrContext).  In that case,
        // convert the image on the IO thread using the resource context.
        raster_image =
            ConvertToRasterUsingResourceContext(image, resource_context);
      }
      encode_task(raster_image);
    });
  });
}

sk_sp<SkData> CopyImageByteData(sk_sp<SkImage> raster_image,
                                SkColorType color_type) {
  FML_DCHECK(raster_image);

  SkPixmap pixmap;

  if (!raster_image->peekPixels(&pixmap)) {
    FML_LOG(ERROR) << "Could not copy pixels from the raster image.";
    return nullptr;
  }

  // The color types already match. No need to swizzle. Return early.
  if (pixmap.colorType() == color_type) {
    return SkData::MakeWithCopy(pixmap.addr(), pixmap.computeByteSize());
  }

  // Perform swizzle if the type doesnt match the specification.
  auto surface = SkSurface::MakeRaster(
      SkImageInfo::Make(raster_image->width(), raster_image->height(),
                        color_type, kPremul_SkAlphaType, nullptr));

  if (!surface) {
    FML_LOG(ERROR) << "Could not setup the surface for swizzle.";
    return nullptr;
  }

  surface->writePixels(pixmap, 0, 0);

  if (!surface->peekPixels(&pixmap)) {
    FML_LOG(ERROR) << "Pixel address is not available.";
    return nullptr;
  }

  return SkData::MakeWithCopy(pixmap.addr(), pixmap.computeByteSize());
}

sk_sp<SkData> EncodeImage(sk_sp<SkImage> raster_image, ImageByteFormat format) {
  TRACE_EVENT0("uiwidgets", __FUNCTION__);

  if (!raster_image) {
    return nullptr;
  }

  switch (format) {
    case kPNG: {
      auto png_image =
          raster_image->encodeToData(SkEncodedImageFormat::kPNG, 0);

      if (png_image == nullptr) {
        FML_LOG(ERROR) << "Could not convert raster image to PNG.";
        return nullptr;
      }
      return png_image;
    } break;
    case kRawRGBA: {
      return CopyImageByteData(raster_image, kRGBA_8888_SkColorType);
    } break;
    case kRawUnmodified: {
      return CopyImageByteData(raster_image, raster_image->colorType());
    } break;
  }

  FML_LOG(ERROR) << "Unknown error encoding image.";
  return nullptr;
}

void EncodeImageAndInvokeDataCallback(
    sk_sp<SkImage> image, std::unique_ptr<EncodeImageCallback> callback,
    ImageByteFormat format, fml::RefPtr<fml::TaskRunner> ui_task_runner,
    fml::RefPtr<fml::TaskRunner> raster_task_runner,
    fml::RefPtr<fml::TaskRunner> io_task_runner, GrContext* resource_context,
    fml::WeakPtr<SnapshotDelegate> snapshot_delegate) {
  auto callback_task = fml::MakeCopyable(
      [callback = std::move(callback)](sk_sp<SkData> encoded) mutable {
        InvokeDataCallback(std::move(callback), std::move(encoded));
      });

  auto encode_task = [callback_task = std::move(callback_task), format,
                      ui_task_runner](sk_sp<SkImage> raster_image) mutable {
    sk_sp<SkData> encoded = EncodeImage(std::move(raster_image), format);
    ui_task_runner->PostTask(
        [callback_task = std::move(callback_task),
         encoded = std::move(encoded)] { callback_task(encoded); });
  };

  ConvertImageToRaster(std::move(image), encode_task, raster_task_runner,
                       io_task_runner, resource_context, snapshot_delegate);
}

}  // namespace

const char* EncodeImage(CanvasImage* canvas_image, int format,
                        RawEncodeImageCallback raw_callback,
                        Mono_Handle callback_handle) {
  if (!canvas_image) return "encode called with non-genuine Image.";

  if (!raw_callback || !callback_handle) return "Callback must be a function.";

  ImageByteFormat image_format = static_cast<ImageByteFormat>(format);

  auto callback = std::make_unique<EncodeImageCallback>(EncodeImageCallback{
      MonoState::Current()->GetWeakPtr(), raw_callback, callback_handle});

  const auto& task_runners = UIMonoState::Current()->GetTaskRunners();

  task_runners.GetIOTaskRunner()->PostTask(fml::MakeCopyable(
      [callback = std::move(callback), image = canvas_image->image(),
       image_format, ui_task_runner = task_runners.GetUITaskRunner(),
       raster_task_runner = task_runners.GetRasterTaskRunner(),
       io_task_runner = task_runners.GetIOTaskRunner(),
       io_manager = UIMonoState::Current()->GetIOManager(),
       snapshot_delegate =
           UIMonoState::Current()->GetSnapshotDelegate()]() mutable {
        EncodeImageAndInvokeDataCallback(
            std::move(image), std::move(callback), image_format,
            std::move(ui_task_runner), std::move(raster_task_runner),
            std::move(io_task_runner), io_manager->GetResourceContext().get(),
            std::move(snapshot_delegate));
      }));

  return nullptr;
}

}  // namespace uiwidgets
