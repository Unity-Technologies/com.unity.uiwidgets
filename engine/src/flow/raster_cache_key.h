#pragma once

#include <unordered_map>

#include "flow/matrix_decomposition.h"
#include "flutter/fml/logging.h"

namespace uiwidgets {

template <typename ID>
class RasterCacheKey {
 public:
  RasterCacheKey(ID id, const SkMatrix& ctm) : id_(id), matrix_(ctm) {
    matrix_[SkMatrix::kMTransX] = SkScalarFraction(ctm.getTranslateX());
    matrix_[SkMatrix::kMTransY] = SkScalarFraction(ctm.getTranslateY());
#ifndef SUPPORT_FRACTIONAL_TRANSLATION
    FML_DCHECK(matrix_.getTranslateX() == 0 && matrix_.getTranslateY() == 0);
#endif
  }

  ID id() const { return id_; }
  const SkMatrix& matrix() const { return matrix_; }

  struct Hash {
    uint32_t operator()(RasterCacheKey const& key) const {
      return std::hash<ID>()(key.id_);
    }
  };

  struct Equal {
    constexpr bool operator()(const RasterCacheKey& lhs,
                              const RasterCacheKey& rhs) const {
      return lhs.id_ == rhs.id_ && lhs.matrix_ == rhs.matrix_;
    }
  };

  template <class Value>
  using Map = std::unordered_map<RasterCacheKey, Value, Hash, Equal>;

 private:
  ID id_;

  // ctm where only fractional (0-1) translations are preserved:
  //   matrix_ = ctm;
  //   matrix_[SkMatrix::kMTransX] = SkScalarFraction(ctm.getTranslateX());
  //   matrix_[SkMatrix::kMTransY] = SkScalarFraction(ctm.getTranslateY());
  SkMatrix matrix_;
};

// The ID is the uint32_t picture uniqueID
using PictureRasterCacheKey = RasterCacheKey<uint32_t>;

class Layer;

// The ID is the uint64_t layer unique_id
using LayerRasterCacheKey = RasterCacheKey<uint64_t>;

}  // namespace uiwidgets
