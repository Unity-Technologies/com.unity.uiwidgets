#pragma once

#include "flutter/fml/memory/ref_counted.h"
#include "lib/ui/painting/canvas.h"
#include "modules/skottie/include/Skottie.h"

namespace uiwidgets {

class Skottie : public fml::RefCountedThreadSafe<Skottie> {
  FML_FRIEND_MAKE_REF_COUNTED(Skottie);

 public:
  static fml::RefPtr<Skottie> Create(char* path);

  void paint(Canvas* canvas, float x, float y, float width, float height,
             float frame);

  float duration();

 private:
  explicit Skottie(sk_sp<skottie::Animation> animation);

  sk_sp<skottie::Animation> animation_;
  bool is_null;
};
}  // namespace uiwidgets
