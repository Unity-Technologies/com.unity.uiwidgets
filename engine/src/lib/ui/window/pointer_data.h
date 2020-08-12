#pragma once

#include <stdint.h>

namespace uiwidgets {

static constexpr int kPointerDataFieldCount = 28;
static constexpr int kBytesPerField = sizeof(int32_t);

enum PointerButtonMouse : int32_t {
  kPointerButtonMousePrimary = 1 << 0,
  kPointerButtonMouseSecondary = 1 << 1,
  kPointerButtonMouseMiddle = 1 << 2,
  kPointerButtonMouseBack = 1 << 3,
  kPointerButtonMouseForward = 1 << 4,
};

enum PointerButtonTouch : int32_t {
  kPointerButtonTouchContact = 1 << 0,
};

enum PointerButtonStylus : int32_t {
  kPointerButtonStylusContact = 1 << 0,
  kPointerButtonStylusPrimary = 1 << 1,
  kPointerButtonStylusSecondary = 1 << 2,
};

struct alignas(8) PointerData {
  enum class Change : int32_t {
    kCancel,
    kAdd,
    kRemove,
    kHover,
    kDown,
    kMove,
    kUp,
  };

  enum class DeviceKind : int32_t {
    kTouch,
    kMouse,
    kStylus,
    kInvertedStylus,
  };

  enum class SignalKind : int32_t {
    kNone,
    kScroll,
  };

  int32_t time_stamp;
  Change change;
  DeviceKind kind;
  SignalKind signal_kind;
  int32_t device;
  int32_t pointer_identifier;
  float physical_x;
  float physical_y;
  float physical_delta_x;
  float physical_delta_y;
  int32_t buttons;
  int32_t obscured;
  int32_t synthesized;
  float pressure;
  float pressure_min;
  float pressure_max;
  float distance;
  float distance_max;
  float size;
  float radius_major;
  float radius_minor;
  float radius_min;
  float radius_max;
  float orientation;
  float tilt;
  int32_t platformData;
  float scroll_delta_x;
  float scroll_delta_y;

  void Clear();
};

}  // namespace uiwidgets
