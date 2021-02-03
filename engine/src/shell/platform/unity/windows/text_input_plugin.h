

#ifndef UIWidgets_SHELL_PLATFORM_WINDOWS_TEXT_INPUT_PLUGIN_H_
#define UIWidgets_SHELL_PLATFORM_WINDOWS_TEXT_INPUT_PLUGIN_H_

#include <map>
#include <memory>

#include "rapidjson/document.h"
#include "rapidjson/rapidjson.h"
#include "shell/platform/common/cpp/client_wrapper/include/uiwidgets/binary_messenger.h"
#include "shell/platform/common/cpp/client_wrapper/include/uiwidgets/method_channel.h"
#include "shell/platform/common/cpp/text_input_model.h"
//#include "shell/platform/windows/keyboard_hook_handler.h"
#include "shell/platform/unity/windows/public/uiwidgets_windows.h"

namespace uiwidgets {

// class Win32UIWidgetsWindow;

// class TextInputPlugin : public KeyboardHookHandler {
class TextInputPlugin {
 public:
  explicit TextInputPlugin(uiwidgets::BinaryMessenger* messenger);

  virtual ~TextInputPlugin();

  //void KeyboardHook(Win32UIWidgetsWindow* window, int key, int scancode,
  //                  int action, int mods) override;

  //void CharHook(Win32UIWidgetsWindow* window, char32_t code_point) override;

 private:
  //void SendStateUpdate(const TextInputModel& model);

  //void EnterPressed(TextInputModel* model);
  void TextInputPlugin::SendStateUpdate(const TextInputModel& model);

  void HandleMethodCall(
      const uiwidgets::MethodCall<rapidjson::Document>& method_call,
      std::unique_ptr<uiwidgets::MethodResult<rapidjson::Document>> result);

  std::unique_ptr<uiwidgets::MethodChannel<rapidjson::Document>> channel_;

  std::unique_ptr<TextInputModel> active_model_;
};

}  // namespace uiwidgets

#endif
