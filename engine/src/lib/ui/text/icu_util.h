#pragma once
#include <string>

#include "flutter/fml/macros.h"
#include "flutter/fml/mapping.h"

namespace uiwidgets {
namespace icu {

void InitializeICU(const std::string& icu_data_path = "");

void InitializeICUFromMapping(std::unique_ptr<fml::Mapping> mapping);

}  // namespace icu
}  // namespace uiwidgets
