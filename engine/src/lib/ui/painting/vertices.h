#pragma once

#include <flutter/fml/memory/ref_counted.h>

#include "include/core/SkVertices.h"

namespace uiwidgets {

class Vertices : public fml::RefCountedThreadSafe<Vertices> {
  FML_FRIEND_MAKE_REF_COUNTED(Vertices);

 public:
  ~Vertices();

  static fml::RefPtr<Vertices> Create();

  bool init(SkVertices::VertexMode vertex_mode, float* positions,
            int positions_length, float* texture_coordinates,
            int texture_coordinates_length, int32_t* colors, int colors_length,
            uint16_t* indices, int indices_length);

  const sk_sp<SkVertices>& vertices() const { return vertices_; }

 private:
  Vertices();

  sk_sp<SkVertices> vertices_;
};

}  // namespace uiwidgets
