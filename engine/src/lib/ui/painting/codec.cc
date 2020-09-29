#include "codec.h"

#include <variant>

#include "common/task_runners.h"
#include "flutter/fml/logging.h"
#include "flutter/fml/make_copyable.h"
#include "flutter/fml/trace_event.h"
#include "frame_info.h"
#include "include/codec/SkCodec.h"
#include "include/core/SkPixelRef.h"
#include "multi_frame_codec.h"
#include "single_frame_codec.h"

#if OS_ANDROID
#include <sys/mman.h>
#endif

namespace uiwidgets {

namespace {

// This must be kept in sync with the enum in painting.dart
enum PixelFormat {
  kRGBA8888,
  kBGRA8888,
};

#if OS_ANDROID

// Compressed image buffers are allocated on the UI thread but are deleted on a
// decoder worker thread.  Android's implementation of malloc appears to
// continue growing the native heap size when the allocating thread is
// different from the freeing thread.  To work around this, create an SkData
// backed by an anonymous mapping.
sk_sp<SkData> MakeSkDataWithCopy(const void* data, size_t length) {
  if (length == 0) {
    return SkData::MakeEmpty();
  }

  size_t mapping_length = length + sizeof(size_t);
  void* mapping = ::mmap(nullptr, mapping_length, PROT_READ | PROT_WRITE,
                         MAP_ANONYMOUS | MAP_PRIVATE, -1, 0);

  if (mapping == MAP_FAILED) {
    return SkData::MakeEmpty();
  }

  *reinterpret_cast<size_t*>(mapping) = mapping_length;
  void* mapping_data = reinterpret_cast<char*>(mapping) + sizeof(size_t);
  ::memcpy(mapping_data, data, length);

  SkData::ReleaseProc proc = [](const void* ptr, void* context) {
    size_t* size_ptr = reinterpret_cast<size_t*>(context);
    FML_DCHECK(ptr == size_ptr + 1);
    if (::munmap(const_cast<void*>(context), *size_ptr) == -1) {
      FML_LOG(ERROR) << "munmap of codec SkData failed";
    }
  };

  return SkData::MakeWithProc(mapping_data, length, proc, mapping);
}

#else

sk_sp<SkData> MakeSkDataWithCopy(const void* data, size_t length) {
  return SkData::MakeWithCopy(data, length);
}

#endif  // OS_ANDROID

}  // anonymous namespace

static std::variant<ImageDecoder::ImageInfo, std::string> ConvertImageInfo(
    Codec::_ImageInfo _image_info) {
  PixelFormat pixel_format = static_cast<PixelFormat>(_image_info.format);
  SkColorType color_type = kUnknown_SkColorType;
  switch (pixel_format) {
    case kRGBA8888:
      color_type = kRGBA_8888_SkColorType;
      break;
    case kBGRA8888:
      color_type = kBGRA_8888_SkColorType;
      break;
  }
  if (color_type == kUnknown_SkColorType) {
    return "Invalid pixel format";
  }

  int width = _image_info.width;
  if (width <= 0) {
    return "width must be greater than zero";
  }
  int height = _image_info.height;
  if (height <= 0) {
    return "height must be greater than zero";
  }

  ImageDecoder::ImageInfo image_info;
  image_info.sk_info =
      SkImageInfo::Make(width, height, color_type, kPremul_SkAlphaType);
  image_info.row_bytes = _image_info.rowBytes;

  if (image_info.row_bytes < image_info.sk_info.minRowBytes()) {
    return "rowBytes does not match the width of the image";
  }

  return image_info;
}

UIWIDGETS_API(const char*)
Codec_instantiateImageCodec(uint8_t* data, int data_length,
                            Codec::InstantiateImageCodecCallback callback,
                            Mono_Handle callback_handle,
                            Codec::_ImageInfo _image_info, bool has_image_info,
                            int target_width, int target_height) {
  if (!callback || !callback_handle) {
    return "Callback must be a function";
  }

  std::optional<ImageDecoder::ImageInfo> image_info;

  if (has_image_info) {
    auto image_info_results = ConvertImageInfo(_image_info);
    if (auto value =
            std::get_if<ImageDecoder::ImageInfo>(&image_info_results)) {
      image_info = *value;
    } else if (auto error = std::get_if<std::string>(&image_info_results)) {
      return error->c_str();
    }
  }

  sk_sp<SkData> buffer = MakeSkDataWithCopy(data, data_length);

  if (image_info) {
    const auto expected_size =
        image_info->row_bytes * image_info->sk_info.height();
    if (buffer->size() < expected_size) {
      return "Pixel buffer size does not match image size";
    }
  }

  const int targetWidth = target_width;
  const int targetHeight = target_height;

  std::unique_ptr<SkCodec> codec;
  bool single_frame;
  if (image_info) {
    single_frame = true;
  } else {
    codec = SkCodec::MakeFromData(buffer);
    if (!codec) {
      return "Could not instantiate image codec.";
    }
    single_frame = codec->getFrameCount() == 1;
  }

  fml::RefPtr<Codec> ui_codec;

  if (single_frame) {
    ImageDecoder::ImageDescriptor descriptor;
    descriptor.decompressed_image_info = image_info;

    if (targetWidth > 0) {
      descriptor.target_width = targetWidth;
    }
    if (targetHeight > 0) {
      descriptor.target_height = targetHeight;
    }
    descriptor.data = std::move(buffer);

    ui_codec = fml::MakeRefCounted<SingleFrameCodec>(std::move(descriptor));
  } else {
    ui_codec = fml::MakeRefCounted<MultiFrameCodec>(std::move(codec));
  }

  ui_codec->AddRef();
  callback(callback_handle, ui_codec.get());
  return nullptr;
}

void Codec::dispose() {}

UIWIDGETS_API(void) Codec_dispose(Codec* ptr) { ptr->Release(); }

UIWIDGETS_API(int) Codec_frameCount(Codec* ptr) { return ptr->frameCount(); }

UIWIDGETS_API(int) Codec_repetitionCount(Codec* ptr) {
  return ptr->repetitionCount();
}

UIWIDGETS_API(const char*)
Codec_getNextFrame(Codec* ptr, Codec::GetNextFrameCallback callback,
                   Mono_Handle callback_handle) {
  return ptr->getNextFrame(callback, callback_handle);
}

}  // namespace uiwidgets
