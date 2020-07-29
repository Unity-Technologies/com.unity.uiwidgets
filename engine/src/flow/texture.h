#pragma once

#include <map>

#include "flutter/fml/macros.h"
#include "flutter/fml/synchronization/waitable_event.h"
#include "include/core/SkCanvas.h"

namespace uiwidgets {

class Texture {
 public:
  Texture(int64_t id);  // Called from UI or raster thread.
  virtual ~Texture();   // Called from raster thread.

  // Called from raster thread.
  virtual void Paint(SkCanvas& canvas, const SkRect& bounds, bool freeze,
                     GrContext* context) = 0;

  // Called from raster thread.
  virtual void OnGrContextCreated() = 0;

  // Called from raster thread.
  virtual void OnGrContextDestroyed() = 0;

  // Called on raster thread.
  virtual void MarkNewFrameAvailable() = 0;

  // Called on raster thread.
  virtual void OnTextureUnregistered() = 0;

  int64_t Id() { return id_; }

 private:
  int64_t id_;

  FML_DISALLOW_COPY_AND_ASSIGN(Texture);
};

class TextureRegistry {
 public:
  TextureRegistry();

  // Called from raster thread.
  void RegisterTexture(std::shared_ptr<Texture> texture);

  // Called from raster thread.
  void UnregisterTexture(int64_t id);

  // Called from raster thread.
  std::shared_ptr<Texture> GetTexture(int64_t id);

  // Called from raster thread.
  void OnGrContextCreated();

  // Called from raster thread.
  void OnGrContextDestroyed();

 private:
  std::map<int64_t, std::shared_ptr<Texture>> mapping_;

  FML_DISALLOW_COPY_AND_ASSIGN(TextureRegistry);
};

}  // namespace uiwidgets
