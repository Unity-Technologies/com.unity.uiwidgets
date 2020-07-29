#include "picture_recorder.h"

#include <Unity/IUnityInterface.h>

#include "lib/ui/painting/canvas.h"
#include "lib/ui/painting/picture.h"
#include "lib/ui/ui_mono_state.h"

namespace uiwidgets {

fml::RefPtr<PictureRecorder> PictureRecorder::Create() {
  return fml::MakeRefCounted<PictureRecorder>();
}

PictureRecorder::PictureRecorder() {}

PictureRecorder::~PictureRecorder() {}

bool PictureRecorder::isRecording() {
  //return canvas_ && canvas_->IsRecording();
  return true;
}

SkCanvas* PictureRecorder::BeginRecording(SkRect bounds) {
  return picture_recorder_.beginRecording(bounds, &rtree_factory_);
}

fml::RefPtr<Picture> PictureRecorder::endRecording() {
  if (!isRecording()) return nullptr;

  fml::RefPtr<Picture> picture = Picture::Create(UIMonoState::CreateGPUObject(
      picture_recorder_.finishRecordingAsPicture()));

  //canvas_->Clear();
  canvas_ = nullptr;

  return picture;
}

extern "C" UNITY_INTERFACE_EXPORT PictureRecorder* UNITY_INTERFACE_API
PictureRecorder_constructor() {
  const auto recorder = PictureRecorder::Create();
  recorder->AddRef();
  return recorder.get();
}

extern "C" UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API
PictureRecorder_dispose(PictureRecorder* ptr) {
  ptr->Release();
}

extern "C" UNITY_INTERFACE_EXPORT bool UNITY_INTERFACE_API
PictureRecorder_isRecording(PictureRecorder* ptr) {
  return ptr->isRecording();
}

extern "C" UNITY_INTERFACE_EXPORT void* UNITY_INTERFACE_API
PictureRecorder_endRecording(PictureRecorder* ptr) {
  const auto picture = ptr->endRecording();
  picture->AddRef();
  return picture.get();
}
}  // namespace uiwidgets
