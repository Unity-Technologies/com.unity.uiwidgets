#pragma once
#include <string>

#include "flutter/fml/macros.h"
#include "flutter/fml/mapping.h"

namespace fml {
	namespace icu2 {

		void InitializeICU(const std::string& icu_data_path = "");

		void InitializeICUFromMapping(std::unique_ptr<Mapping> mapping);

	} 
}
