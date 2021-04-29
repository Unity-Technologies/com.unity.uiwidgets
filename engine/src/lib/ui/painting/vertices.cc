#include "vertices.h"

#include <algorithm>

#include "runtime/mono_api.h"

namespace uiwidgets {

namespace {

void DecodePoints(float* coords, int coords_length, SkPoint* points) {
  for (int i = 0; i < coords_length; i += 2)
    points[i / 2] = SkPoint::Make(coords[i], coords[i + 1]);
}

template <typename T>
void DecodeInts(int32_t* ints, int ints_length, T* out) {
  for (int i = 0; i < ints_length; i++) out[i] = ints[i];
}

}  // namespace

Vertices::Vertices() = default;

Vertices::~Vertices() = default;

fml::RefPtr<Vertices> Vertices::Create() {
  return fml::MakeRefCounted<Vertices>();
}

bool Vertices::init(SkVertices::VertexMode vertex_mode, float* positions,
                    int positions_length, float* texture_coordinates,
                    int texture_coordinates_length, int32_t* colors,
                    int colors_length, uint16_t* indices, int indices_length) {
  uint32_t builderFlags = 0;
  if (texture_coordinates)
    builderFlags |= SkVertices::kHasTexCoords_BuilderFlag;
  if (colors) builderFlags |= SkVertices::kHasColors_BuilderFlag;

  SkVertices::Builder builder(vertex_mode, positions_length / 2, indices_length,
                              builderFlags);

  if (!builder.isValid()) return false;

  // positions are required for SkVertices::Builder
  FML_DCHECK(positions);
  if (positions) DecodePoints(positions, positions_length, builder.positions());

  if (texture_coordinates) {
    // SkVertices::Builder assumes equal numbers of elements
    FML_DCHECK(positions_length == texture_coordinates_length);
    DecodePoints(texture_coordinates, texture_coordinates_length,
                 builder.texCoords());
  }
  if (colors) {
    // SkVertices::Builder assumes equal numbers of elements
    FML_DCHECK(positions_length / 2 == colors_length);
    DecodeInts<SkColor>(colors, colors_length, builder.colors());
  }

  if (indices) {
    std::copy(indices, indices + indices_length, builder.indices());
  }

  vertices_ = builder.detach();

  return true;
}

UIWIDGETS_API(Vertices*) Vertices_constructor() {
  const auto path = Vertices::Create();
  path->AddRef();
  return path.get();
}

UIWIDGETS_API(void) Vertices_dispose(Vertices* ptr) { ptr->Release(); }

UIWIDGETS_API(bool)
Vertices_init(Vertices* ptr, SkVertices::VertexMode vertex_mode,
              float* positions, int positions_length,
              float* texture_coordinates, int texture_coordinates_length,
              int32_t* colors, int colors_length, uint16_t* indices,
              int indices_length) {
  return ptr->init(vertex_mode, positions, positions_length,
                   texture_coordinates, texture_coordinates_length, colors,
                   colors_length, indices, indices_length);
}

}  // namespace uiwidgets
