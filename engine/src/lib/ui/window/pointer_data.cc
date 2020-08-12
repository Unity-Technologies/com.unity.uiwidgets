#include "pointer_data.h"

#include <string.h>

namespace uiwidgets {

static_assert(sizeof(PointerData) == kBytesPerField * kPointerDataFieldCount,
              "PointerData has the wrong size");

void PointerData::Clear() { memset(this, 0, sizeof(PointerData)); }

}  // namespace uiwidgets
