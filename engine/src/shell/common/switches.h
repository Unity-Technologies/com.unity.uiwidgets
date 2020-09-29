#pragma once
#include <flutter/fml/memory/ref_counted.h>

#include "flutter/fml/mapping.h"

namespace uiwidgets {
std::unique_ptr<fml::Mapping> GetICUStaticMapping();
std::unique_ptr<fml::Mapping> GetSymbolMapping(std::string symbol_prefix,
                                               std::string native_lib_path);
}  // namespace uiwidgets