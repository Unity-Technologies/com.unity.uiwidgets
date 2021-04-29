#pragma once
#include <flutter/fml/memory/ref_counted.h>

#include "flutter/fml/mapping.h"

namespace uiwidgets {
#if OS_ANDROID || OS_WIN
std::unique_ptr<fml::Mapping> GetICUStaticMapping();
#endif
std::unique_ptr<fml::Mapping> GetSymbolMapping(std::string symbol_prefix,
                                               std::string native_lib_path);
}  // namespace uiwidgets