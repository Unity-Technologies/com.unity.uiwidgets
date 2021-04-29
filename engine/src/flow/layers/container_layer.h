#pragma once

#include <vector>

#include "flow/layers/layer.h"

namespace uiwidgets {

class ContainerLayer : public Layer {
 public:
  ContainerLayer();

  virtual void Add(std::shared_ptr<Layer> layer);

  void Preroll(PrerollContext* context, const SkMatrix& matrix) override;
  void Paint(PaintContext& context) const override;

  const std::vector<std::shared_ptr<Layer>>& layers() const { return layers_; }

 protected:
  void PrerollChildren(PrerollContext* context, const SkMatrix& child_matrix,
                       SkRect* child_paint_bounds);
  void PaintChildren(PaintContext& context) const;

  // For OpacityLayer to restructure to have a single child.
  void ClearChildren() { layers_.clear(); }

 private:
  std::vector<std::shared_ptr<Layer>> layers_;

  FML_DISALLOW_COPY_AND_ASSIGN(ContainerLayer);
};

}  // namespace uiwidgets
