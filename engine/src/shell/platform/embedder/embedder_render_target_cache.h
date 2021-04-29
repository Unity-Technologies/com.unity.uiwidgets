#pragma once

#include <set>
#include <stack>
#include <tuple>
#include <unordered_map>

#include "flutter/fml/macros.h"
#include "shell/platform/embedder/embedder_external_view.h"

namespace uiwidgets {

class EmbedderRenderTargetCache {
 public:
  EmbedderRenderTargetCache();

  ~EmbedderRenderTargetCache();

  using RenderTargets =
      std::unordered_map<EmbedderExternalView::ViewIdentifier,
                         std::unique_ptr<EmbedderRenderTarget>,
                         EmbedderExternalView::ViewIdentifier::Hash,
                         EmbedderExternalView::ViewIdentifier::Equal>;

  std::pair<RenderTargets, EmbedderExternalView::ViewIdentifierSet>
  GetExistingTargetsInCache(
      const EmbedderExternalView::PendingViews& pending_views);

  std::set<std::unique_ptr<EmbedderRenderTarget>>
  ClearAllRenderTargetsInCache();

  void CacheRenderTarget(EmbedderExternalView::ViewIdentifier view_identifier,
                         std::unique_ptr<EmbedderRenderTarget> target);

  size_t GetCachedTargetsCount() const;

 private:
  using CachedRenderTargets =
      std::unordered_map<EmbedderExternalView::RenderTargetDescriptor,
                         std::stack<std::unique_ptr<EmbedderRenderTarget>>,
                         EmbedderExternalView::RenderTargetDescriptor::Hash,
                         EmbedderExternalView::RenderTargetDescriptor::Equal>;

  CachedRenderTargets cached_render_targets_;

  FML_DISALLOW_COPY_AND_ASSIGN(EmbedderRenderTargetCache);
};

}  // namespace uiwidgets
