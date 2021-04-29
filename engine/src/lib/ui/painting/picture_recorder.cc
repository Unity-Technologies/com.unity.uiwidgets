#include "picture_recorder.h"

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
  return canvas_ && canvas_->IsRecording();
}

SkCanvas* PictureRecorder::BeginRecording(SkRect bounds) {
  return picture_recorder_.beginRecording(bounds, &rtree_factory_);
}

fml::RefPtr<Picture> PictureRecorder::endRecording() {
  if (!isRecording()) return nullptr;

  fml::RefPtr<Picture> picture = Picture::Create(UIMonoState::CreateGPUObject(
      picture_recorder_.finishRecordingAsPicture()));

  canvas_->Clear();
  canvas_ = nullptr;

  return picture;
}

UIWIDGETS_API(PictureRecorder*) PictureRecorder_constructor() {
  const auto recorder = PictureRecorder::Create();
  recorder->AddRef();
  return recorder.get();
}

UIWIDGETS_API(void) PictureRecorder_dispose(PictureRecorder* ptr) {
  ptr->Release();
}

UIWIDGETS_API(bool) PictureRecorder_isRecording(PictureRecorder* ptr) {
  return ptr->isRecording();
}

UIWIDGETS_API(Picture*) PictureRecorder_endRecording(PictureRecorder* ptr) {
  const auto picture = ptr->endRecording();
  picture->AddRef();
  return picture.get();
}
}  // namespace uiwidgets
