// // Copyright 2013 The UIWidgets Authors. All rights reserved.
// // Use of this source code is governed by a BSD-style license that can be
// // found in the LICENSE file.

// #include <memory>
// #include <vector>

// #include "shell/platform/common/cpp/client_wrapper/include/uiwidgets/plugin_registrar.h"
// #include "shell/platform/common/cpp/client_wrapper/testing/stub_uiwidgets_api.h"
// #include "gtest/gtest.h"

// namespace uiwidgets {

// namespace {

// // Stub implementation to validate calls to the API.
// class TestApi : public testing::StubUIWidgetsApi {
//  public:
//   // |uiwidgets::testing::StubUIWidgetsApi|
//   bool MessengerSend(const char* channel,
//                      const uint8_t* message,
//                      const size_t message_size) override {
//     last_data_sent_ = message;
//     return message_engine_result;
//   }

//   bool MessengerSendWithReply(const char* channel,
//                               const uint8_t* message,
//                               const size_t message_size,
//                               const UIWidgetsDesktopBinaryReply reply,
//                               void* user_data) override {
//     last_data_sent_ = message;
//     return message_engine_result;
//   }

//   // Called for UIWidgetsDesktopMessengerSetCallback.
//   void MessengerSetCallback(const char* channel,
//                             UIWidgetsDesktopMessageCallback callback,
//                             void* user_data) override {
//     last_callback_set_ = callback;
//   }

//   const uint8_t* last_data_sent() { return last_data_sent_; }
//   UIWidgetsDesktopMessageCallback last_callback_set() {
//     return last_callback_set_;
//   }

//  private:
//   const uint8_t* last_data_sent_ = nullptr;
//   UIWidgetsDesktopMessageCallback last_callback_set_ = nullptr;
// };

// }  // namespace

// // Tests that the registrar returns a messenger that passes Send through to the
// // C API.
// TEST(MethodCallTest, MessengerSend) {
//   testing::ScopedStubUIWidgetsApi scoped_api_stub(std::make_unique<TestApi>());
//   auto test_api = static_cast<TestApi*>(scoped_api_stub.stub());

//   auto dummy_registrar_handle =
//       reinterpret_cast<UIWidgetsDesktopPluginRegistrarRef>(1);
//   PluginRegistrar registrar(dummy_registrar_handle);
//   BinaryMessenger* messenger = registrar.messenger();

//   std::vector<uint8_t> message = {1, 2, 3, 4};
//   messenger->Send("some_channel", &message[0], message.size());
//   EXPECT_EQ(test_api->last_data_sent(), &message[0]);
// }

// // Tests that the registrar returns a messenger that passes callback
// // registration and unregistration through to the C API.
// TEST(MethodCallTest, MessengerSetMessageHandler) {
//   testing::ScopedStubUIWidgetsApi scoped_api_stub(std::make_unique<TestApi>());
//   auto test_api = static_cast<TestApi*>(scoped_api_stub.stub());

//   auto dummy_registrar_handle =
//       reinterpret_cast<UIWidgetsDesktopPluginRegistrarRef>(1);
//   PluginRegistrar registrar(dummy_registrar_handle);
//   BinaryMessenger* messenger = registrar.messenger();
//   const std::string channel_name("foo");

//   // Register.
//   BinaryMessageHandler binary_handler = [](const uint8_t* message,
//                                            const size_t message_size,
//                                            BinaryReply reply) {};
//   messenger->SetMessageHandler(channel_name, std::move(binary_handler));
//   EXPECT_NE(test_api->last_callback_set(), nullptr);

//   // Unregister.
//   messenger->SetMessageHandler(channel_name, nullptr);
//   EXPECT_EQ(test_api->last_callback_set(), nullptr);
// }

// }  // namespace uiwidgets
