#include "shader.h"

namespace uiwidgets {

Shader::Shader(SkiaGPUObject<SkShader> shader) : shader_(std::move(shader)) {}

Shader::~Shader() = default;

}  // namespace uiwidgets
