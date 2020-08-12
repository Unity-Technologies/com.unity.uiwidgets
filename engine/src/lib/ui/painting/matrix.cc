#include "matrix.h"

#include "flutter/fml/logging.h"

namespace uiwidgets {

// Mappings from SkMatrix-index to input-index.
static const int kSkMatrixIndexToMatrix4Index[] = {
    // clang-format off
    0, 4, 12,
    1, 5, 13,
    3, 7, 15,
    // clang-format on
};

SkMatrix ToSkMatrix(const float* matrix4) {
  FML_DCHECK(matrix4);

  SkMatrix sk_matrix;
  for (int i = 0; i < 9; ++i) {
    const int matrix4_index = kSkMatrixIndexToMatrix4Index[i];
    sk_matrix[i] = matrix4[matrix4_index];
  }
  return sk_matrix;
}

}  // namespace uiwidgets
