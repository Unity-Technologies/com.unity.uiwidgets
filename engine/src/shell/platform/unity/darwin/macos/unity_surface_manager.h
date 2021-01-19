#pragma once

#include <OpenGL/gl3.h>
#include <AppKit/AppKit.h>
#include <Metal/Metal.h>
#include <CoreVideo/CoreVideo.h>

#include "Unity/IUnityInterface.h"
#include "flutter/fml/macros.h"

namespace uiwidgets {
class UnitySurfaceManager {
 public:
  UnitySurfaceManager(IUnityInterfaces* unity_interfaces);
  ~UnitySurfaceManager();

  void* CreateRenderTexture(size_t width, size_t height);

  void ReleaseNativeRenderContext();

  bool ReleaseNativeRenderTexture();

  bool ClearCurrentContext();

  bool MakeCurrentContext();

  bool MakeCurrentResourceContext();
  
  uint32_t GetFbo();

 private:
  //pixel buffer handles
  CVPixelBufferRef pixelbuffer_ref = nullptr;
  //openGL handlers
  NSOpenGLContext *gl_context_ = NULL;
  NSOpenGLContext *gl_resource_context_ = NULL;
  GLuint default_fbo_ = 0;
  GLuint gl_tex_ = 0;
  CVOpenGLTextureCacheRef gl_tex_cache_ref_ = nullptr;
  CVOpenGLTextureRef gl_tex_ref_ = nullptr;

  //metal handlers
  id<MTLDevice> metal_device_;
  id<MTLTexture> metal_tex_;
  CVMetalTextureRef metal_tex_ref_ = nullptr;
  CVMetalTextureCacheRef metal_tex_cache_ref_ = nullptr;
};

} // namespace uiwidgets