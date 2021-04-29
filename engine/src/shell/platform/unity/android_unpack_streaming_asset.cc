#include "android_unpack_streaming_asset.h"
#include <stdarg.h>

namespace uiwidgets {

const char* AndroidUnpackStreamingAsset::Unpack(const char* file) {
  return _unpack(file);
}

UnpackFileCallback AndroidUnpackStreamingAsset::_unpack;

UIWIDGETS_API(void) 
InitUnpackFile(UnpackFileCallback _unpack) { 
  AndroidUnpackStreamingAsset::_unpack = _unpack; 
}

}
