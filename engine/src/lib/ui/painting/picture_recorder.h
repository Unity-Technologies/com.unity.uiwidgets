#pragma once

#include <flutter/fml/memory/ref_counted.h>
#include "include/core/SkPictureRecorder.h"

namespace uiwidgets {
class Canvas;
class Picture;

class PictureRecorder : public fml::RefCountedThreadSafe<PictureRecorder> {
  FML_FRIEND_MAKE_REF_COUNTED(PictureRecorder);

 public:
  static fml::RefPtr<PictureRecorder> Create();

  ~PictureRecorder();

  SkCanvas* BeginRecording(SkRect bounds);
  fml::RefPtr<Picture> endRecording();
  bool isRecording();

  void set_canvas(fml::RefPtr<Canvas> canvas) { canvas_ = std::move(canvas); }

 private:
  PictureRecorder();

  SkRTreeFactory rtree_factory_;
  SkPictureRecorder picture_recorder_;
  fml::RefPtr<Canvas> canvas_;
};

}  // namespace uiwidgets
