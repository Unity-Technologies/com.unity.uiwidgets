#include "image_decoder.h"

#include <algorithm>

#include "flutter/fml/make_copyable.h"
#include "include/codec/SkCodec.h"
#include "src/codec/SkCodecImageGenerator.h"

namespace uiwidgets {
ImageDecoder::ImageDecoder(
    TaskRunners runners,
    std::shared_ptr<fml::ConcurrentTaskRunner> concurrent_task_runner,
    fml::WeakPtr<IOManager> io_manager) {}

fml::WeakPtr<ImageDecoder> ImageDecoder::GetWeakPtr() const {
  return fml::WeakPtr<ImageDecoder>();
}
}  // namespace uiwidgets
