

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_METHOD_RESULT_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_METHOD_RESULT_H_

#include <string>

namespace uiwidgets {

template <typename T>
class MethodResult {
 public:
  MethodResult() = default;

  virtual ~MethodResult() = default;

  MethodResult(MethodResult const&) = delete;
  MethodResult& operator=(MethodResult const&) = delete;

  void Success(const T* result = nullptr) { SuccessInternal(result); }

  void Error(const std::string& error_code,
             const std::string& error_message = "",
             const T* error_details = nullptr) {
    ErrorInternal(error_code, error_message, error_details);
  }

  void NotImplemented() { NotImplementedInternal(); }

 protected:
  virtual void SuccessInternal(const T* result) = 0;

  virtual void ErrorInternal(const std::string& error_code,
                             const std::string& error_message,
                             const T* error_details) = 0;

  virtual void NotImplementedInternal() = 0;
};

}  // namespace uiwidgets

#endif
