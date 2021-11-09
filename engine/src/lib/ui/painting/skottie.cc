#include "skottie.h"

#include "lib/ui/ui_mono_state.h"
#if __ANDROID__
#include "shell/platform/unity/android_unpack_streaming_asset.h"
#endif
namespace uiwidgets {
fml::RefPtr<Skottie> Skottie::Create(char* path) {
#if __ANDROID__
  std::string pthstr = std::string(path);
  int id = pthstr.find("assets/") + 7;
  std::string file = pthstr.substr(id);
  const char* fileOut = AndroidUnpackStreamingAsset::Unpack(file.c_str());
  path = (char*)fileOut;
#endif
  sk_sp<skottie::Animation> animation_ = skottie::Animation::MakeFromFile(path);
  if(animation_ == nullptr){
    return nullptr;
  }
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
  if(skottie.get() == nullptr){
    return nullptr;
  }
  skottie->AddRef();
  return skottie.get();
}

UIWIDGETS_API(void)
Skottie_Dispose(Skottie* ptr) {
  if(ptr == nullptr){
      return;
  }
  ptr->Release();
}

UIWIDGETS_API(void)
Skottie_Paint(Skottie* ptr, Canvas* canvas, float x, float y, float width,
              float height, float frame) {
  if(ptr == nullptr){
      return;
  }
  ptr->paint(canvas, x, y, width, height, frame);
}

UIWIDGETS_API(float)
Skottie_Duration(Skottie* ptr) {
  if(ptr == nullptr){
      return 0;
  }
  return ptr->duration(); }
}  // namespace uiwidgets