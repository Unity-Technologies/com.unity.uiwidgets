#pragma once

#include <memory>
#include <vector>

#include "flow/layers/layer_tree.h"
#include "lib/ui/text/font_collection.h"

#include "lib/ui/window/platform_message.h"

namespace uiwidgets {

class RuntimeDelegate {
 public:
  virtual std::string DefaultRouteName() = 0;

  virtual void ScheduleFrame(bool regenerate_layer_tree = true) = 0;

  virtual void Render(std::unique_ptr<LayerTree> layer_tree) = 0;

  virtual void HandlePlatformMessage(fml::RefPtr<PlatformMessage> message) = 0;
  
  virtual FontCollection& GetFontCollection() = 0;
  
  virtual void SetNeedsReportTimings(bool value) = 0;

 protected:
  virtual ~RuntimeDelegate();
};

}  // namespace uiwidgets
