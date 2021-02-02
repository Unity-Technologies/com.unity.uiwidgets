

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_PLUGIN_REGISTRY_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_PLUGIN_REGISTRY_H_

#include "shell/platform/common/cpp/public/uiwidgets_plugin_registrar.h"

#include <string>

namespace uiwidgets {

class PluginRegistry {
 public:
  PluginRegistry() = default;
  virtual ~PluginRegistry() = default;

  PluginRegistry(PluginRegistry const&) = delete;
  PluginRegistry& operator=(PluginRegistry const&) = delete;

  virtual UIWidgetsDesktopPluginRegistrarRef GetRegistrarForPlugin(
      const std::string& plugin_name) = 0;
};

}  // namespace uiwidgets

#endif
