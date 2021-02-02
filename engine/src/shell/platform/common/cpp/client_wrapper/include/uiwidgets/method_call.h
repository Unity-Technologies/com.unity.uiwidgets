

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_TYPED_METHOD_CALL_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_TYPED_METHOD_CALL_H_

#include <memory>
#include <string>

namespace uiwidgets {

template <typename T>
class MethodCall {
 public:
  MethodCall(const std::string& method_name, std::unique_ptr<T> arguments)
      : method_name_(method_name), arguments_(std::move(arguments)) {}

  virtual ~MethodCall() = default;

  MethodCall(MethodCall<T> const&) = delete;
  MethodCall& operator=(MethodCall<T> const&) = delete;

  const std::string& method_name() const { return method_name_; }

  const T* arguments() const { return arguments_.get(); }

 private:
  std::string method_name_;
  std::unique_ptr<T> arguments_;
};

}  // namespace uiwidgets

#endif
