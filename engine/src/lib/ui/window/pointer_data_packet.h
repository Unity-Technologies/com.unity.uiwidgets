#pragma once

#include <string.h>

#include <vector>

#include "flutter/fml/macros.h"
#include "pointer_data.h"

namespace uiwidgets {

class PointerDataPacket {
 public:
  explicit PointerDataPacket(size_t count);
  PointerDataPacket(uint8_t* data, size_t num_bytes);
  ~PointerDataPacket();

  void SetPointerData(size_t i, const PointerData& data);
  const std::vector<uint8_t>& data() const { return data_; }

 private:
  std::vector<uint8_t> data_;

  FML_DISALLOW_COPY_AND_ASSIGN(PointerDataPacket);
};

}  // namespace uiwidgets
