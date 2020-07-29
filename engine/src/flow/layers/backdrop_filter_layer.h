#include "flow/layers/container_layer.h"
#include "include/core/SkImageFilter.h"

namespace uiwidgets {

class BackdropFilterLayer : public ContainerLayer {
 public:
  BackdropFilterLayer(sk_sp<SkImageFilter> filter);

  void Preroll(PrerollContext* context, const SkMatrix& matrix) override;

  void Paint(PaintContext& context) const override;

 private:
  sk_sp<SkImageFilter> filter_;

  FML_DISALLOW_COPY_AND_ASSIGN(BackdropFilterLayer);
};

}  // namespace uiwidgets
