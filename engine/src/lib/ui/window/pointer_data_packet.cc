#include "pointer_data_packet.h"

#include <string.h>

namespace uiwidgets {

PointerDataPacket::PointerDataPacket(size_t count)
    : data_(count * sizeof(PointerData)) {}

PointerDataPacket::PointerDataPacket(uint8_t* data, size_t num_bytes)
    : data_(data, data + num_bytes) {}

PointerDataPacket::~PointerDataPacket() = default;

void PointerDataPacket::SetPointerData(size_t i, const PointerData& data) {
  memcpy(&data_[i * sizeof(PointerData)], &data, sizeof(PointerData));
}

}  // namespace uiwidgets
