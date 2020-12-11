#include "skottie.h"

#include "lib/ui/ui_mono_state.h"

namespace uiwidgets {
fml::RefPtr<Skottie> Skottie::Create(char* path) {
  sk_sp<skottie::Animation> animation_ = skottie::Animation::MakeFromFile(path);
  return fml::MakeRefCounted<Skottie>(animation_);
}

Skottie::Skottie(sk_sp<skottie::Animation> animation) {
  animation_ = animation;
}

void Skottie::paint(Canvas* canvas, float x, float y, float width, float height,
                    float frame) {
  animation_->seekFrameTime(frame);
  SkRect rect = SkRect::MakeXYWH(x, y, width, height);
  animation_->render(canvas->canvas(), &rect);
}

float Skottie::duration() { return animation_->duration(); }
UIWIDGETS_API(Skottie*)
Skottie_Construct(char* path) {
  fml::RefPtr<Skottie> skottie = Skottie::Create(path);
  skottie->AddRef();
  return skottie.get();
}

UIWIDGETS_API(void)
Skottie_Dispose(Skottie* ptr) { ptr->Release(); }

UIWIDGETS_API(void)
Skottie_Paint(Skottie* ptr, Canvas* canvas, float x, float y, float width,
              float height, float frame) {
  ptr->paint(canvas, x, y, width, height, frame);
}

UIWIDGETS_API(float)
Skottie_Duration(Skottie* ptr) { return ptr->duration(); }
}  // namespace uiwidgets