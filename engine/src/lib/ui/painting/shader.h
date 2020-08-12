#pragma once

#include "flow/skia_gpu_object.h"
#include "include/core/SkShader.h"
#include "lib/ui/ui_mono_state.h"

namespace uiwidgets {

class Shader : public fml::RefCountedThreadSafe<Shader> {
  FML_FRIEND_MAKE_REF_COUNTED(Shader);

 public:
  virtual ~Shader();

  sk_sp<SkShader> shader() { return shader_.get(); }

  void set_shader(SkiaGPUObject<SkShader> shader) {
    shader_ = std::move(shader);
  }

 protected:
  Shader(SkiaGPUObject<SkShader> shader = {});

 private:
  SkiaGPUObject<SkShader> shader_;
};

}  // namespace uiwidgets
