#pragma once

#include "flutter/fml/memory/ref_counted.h"
#include "txt/paragraph.h"
#include "shell/common/lists.h"
#include "lib/ui/painting/canvas.h"
#include "lib/ui/ui_mono_state.h"

namespace uiwidgets {

class Paragraph : public fml::RefCountedThreadSafe<Paragraph> {
  FML_FRIEND_MAKE_REF_COUNTED(Paragraph);

 public:
  static fml::RefPtr<Paragraph> Create();

  static fml::RefPtr<Paragraph> Create(
      std::unique_ptr<txt::Paragraph> txt_paragraph) {
    return fml::MakeRefCounted<Paragraph>(std::move(txt_paragraph));
  }

  ~Paragraph();

  float width();
  float height();
  float longestLine();
  float minIntrinsicWidth();
  float maxIntrinsicWidth();
  float alphabeticBaseline();
  float ideographicBaseline();
  bool didExceedMaxLines();

  void layout(float width);
  void paint(Canvas* canvas, float x, float y);
  Float32List getRectsForRange(unsigned start, unsigned end,
                        unsigned boxHeightStyle, unsigned boxWidthStyle);
  Float32List getRectsForPlaceholders();
  void getPositionForOffset(float dx, float dy, int* offset);
  void getWordBoundary(unsigned offset, int* boundaryPtr);
  void getLineBoundary(unsigned offset, int* boundaryPtr);
  Float32List computeLineMetrics();

  size_t GetAllocationSize();
  std::unique_ptr<txt::Paragraph> m_paragraph;

 private:
  explicit Paragraph(std::unique_ptr<txt::Paragraph> paragraph);
};

}  // namespace uiwidgets
