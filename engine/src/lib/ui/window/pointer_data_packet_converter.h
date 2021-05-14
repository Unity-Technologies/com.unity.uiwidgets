#pragma once

#include <string.h>

#include <map>
#include <memory>
#include <vector>

#include "flutter/fml/macros.h"
#include "pointer_data_packet.h"

namespace uiwidgets {

struct PointerState {
  int32_t pointer_identifier;
  bool isDown;
  int64_t previous_buttons;
  float physical_x;
  float physical_y;
};

class PointerDataPacketConverter {
 public:
  PointerDataPacketConverter();
  ~PointerDataPacketConverter();

  std::unique_ptr<PointerDataPacket> Convert(
      std::unique_ptr<PointerDataPacket> packet);

 private:
  std::map<int32_t, PointerState> states_;

  int32_t pointer_;

  void ConvertPointerData(PointerData pointer_data,
                          std::vector<PointerData>& converted_pointers);

  PointerState EnsurePointerState(PointerData pointer_data);

  void UpdateDeltaAndState(PointerData& pointer_data, PointerState& state);

  void UpdatePointerIdentifier(PointerData& pointer_data, PointerState& state,
                               bool start_new_pointer);

  bool LocationNeedsUpdate(const PointerData& pointer_data,
                           const PointerState& state);

  FML_DISALLOW_COPY_AND_ASSIGN(PointerDataPacketConverter);
};

}  // namespace uiwidgets
