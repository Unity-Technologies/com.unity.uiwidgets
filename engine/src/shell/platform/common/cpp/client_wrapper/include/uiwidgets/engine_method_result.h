

#ifndef UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_ENGINE_METHOD_RESULT_H_
#define UIWIDGETS_SHELL_PLATFORM_COMMON_CPP_CLIENT_WRAPPER_INCLUDE_UIWIDGETS_ENGINE_METHOD_RESULT_H_

#include <memory>
#include <string>
#include <vector>

#include "binary_messenger.h"
#include "method_codec.h"
#include "method_result.h"

namespace uiwidgets {

namespace internal {

class ReplyManager {
 public:
  ReplyManager(BinaryReply reply_handler_);
  ~ReplyManager();

  ReplyManager(ReplyManager const&) = delete;
  ReplyManager& operator=(ReplyManager const&) = delete;

  void SendResponseData(const std::vector<uint8_t>* data);

 private:
  BinaryReply reply_handler_;
};
}  // namespace internal

template <typename T>
class EngineMethodResult : public MethodResult<T> {
 public:
  EngineMethodResult(BinaryReply reply_handler, const MethodCodec<T>* codec)
      : reply_manager_(
            std::make_unique<internal::ReplyManager>(std::move(reply_handler))),
        codec_(codec) {}

  ~EngineMethodResult() = default;

 protected:
  void SuccessInternal(const T* result) override {
    std::unique_ptr<std::vector<uint8_t>> data =
        codec_->EncodeSuccessEnvelope(result);
    reply_manager_->SendResponseData(data.get());
  }

  void ErrorInternal(const std::string& error_code,
                     const std::string& error_message,
                     const T* error_details) override {
    std::unique_ptr<std::vector<uint8_t>> data =
        codec_->EncodeErrorEnvelope(error_code, error_message, error_details);
    reply_manager_->SendResponseData(data.get());
  }

  void NotImplementedInternal() override {
    reply_manager_->SendResponseData(nullptr);
  }

 private:
  std::unique_ptr<internal::ReplyManager> reply_manager_;

  const MethodCodec<T>* codec_;
};

}  // namespace uiwidgets

#endif
