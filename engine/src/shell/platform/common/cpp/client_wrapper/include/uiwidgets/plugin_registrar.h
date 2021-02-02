

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_PLUGIN_REGISTRAR_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_PLUGIN_REGISTRAR_H_

#include "shell/platform/common/cpp/public/uiwidgets_plugin_registrar.h"

#include <memory>
#include <set>
#include <string>

#include "binary_messenger.h"

namespace uiwidgets {

class Plugin;

class PluginRegistrar {
 public:
  explicit PluginRegistrar(UIWidgetsDesktopPluginRegistrarRef core_registrar);

  virtual ~PluginRegistrar();

  PluginRegistrar(PluginRegistrar const&) = delete;
  PluginRegistrar& operator=(PluginRegistrar const&) = delete;

  BinaryMessenger* messenger() { return messenger_.get(); }

  void AddPlugin(std::unique_ptr<Plugin> plugin);

  void EnableInputBlockingForChannel(const std::string& channel);

 private:
  UIWidgetsDesktopPluginRegistrarRef registrar_;

  std::unique_ptr<BinaryMessenger> messenger_;

  std::set<std::unique_ptr<Plugin>> plugins_;
};

class Plugin {
 public:
  virtual ~Plugin() = default;
};

}  // namespace uiwidgets

#endif
