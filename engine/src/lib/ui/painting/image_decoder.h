#pragma once

#include <memory>
#include <optional>

#include "common/task_runners.h"
#include "flow/skia_gpu_object.h"
#include "flutter/fml/concurrent_message_loop.h"
#include "flutter/fml/macros.h"
#include "flutter/fml/mapping.h"
#include "flutter/fml/trace_event.h"
#include "include/core/SkData.h"
#include "include/core/SkImage.h"
#include "include/core/SkImageInfo.h"
#include "include/core/SkRefCnt.h"
#include "include/core/SkSize.h"
#include "lib/ui/io_manager.h"

namespace uiwidgets {

// An object that coordinates image decompression and texture upload across
// multiple threads/components in the shell. This object must be created,
// accessed and collected on the UI thread (typically the engine or its runtime
// controller). None of the expensive operations performed by this component
// occur in a frame pipeline.
class ImageDecoder {
 public:
  ImageDecoder(
      TaskRunners runners,
      std::shared_ptr<fml::ConcurrentTaskRunner> concurrent_task_runner,
      fml::WeakPtr<IOManager> io_manager);

  ~ImageDecoder();

  struct ImageInfo {
    SkImageInfo sk_info = {};
    size_t row_bytes = 0;
  };

  struct ImageDescriptor {
    sk_sp<SkData> data;
    std::optional<ImageInfo> decompressed_image_info;
    std::optional<uint32_t> target_width;
    std::optional<uint32_t> target_height;
  };

  using ImageResult = std::function<void(SkiaGPUObject<SkImage>)>;

  // Takes an image descriptor and returns a handle to a texture resident on the
  // GPU. All image decompression and resizes are done on a worker thread
  // concurrently. Texture upload is done on the IO thread and the result
  // returned back on the UI thread. On error, the texture is null but the
  // callback is guaranteed to return on the UI thread.
  void Decode(ImageDescriptor descriptor, const ImageResult& result);

  fml::WeakPtr<ImageDecoder> GetWeakPtr() const;

 private:
  TaskRunners runners_;
  std::shared_ptr<fml::ConcurrentTaskRunner> concurrent_task_runner_;
  fml::WeakPtr<IOManager> io_manager_;
  fml::WeakPtrFactory<ImageDecoder> weak_factory_;

  FML_DISALLOW_COPY_AND_ASSIGN(ImageDecoder);
};

sk_sp<SkImage> ImageFromCompressedData(sk_sp<SkData> data,
                                       std::optional<uint32_t> target_width,
                                       std::optional<uint32_t> target_height,
                                       const fml::tracing::TraceFlow& flow);

}  // namespace uiwidgets
