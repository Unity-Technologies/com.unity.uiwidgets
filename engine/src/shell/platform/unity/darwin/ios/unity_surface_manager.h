#pragma once

#include <OpenGLES/EAGL.h>
#include <OpenGLES/ES2/gl.h>
#include <OpenGLES/ES2/glext.h>
#include <OpenGLES/ES3/gl.h>

#include <UIKit/UIKit.h>
#include <Metal/Metal.h>
#include <CoreVideo/CoreVideo.h>

#include "Unity/IUnityInterface.h"
#include "flutter/fml/macros.h"

namespace uiwidgets {
class UnitySurfaceManager {
 public:
  UnitySurfaceManager(IUnityInterfaces* unity_interfaces);
  ~UnitySurfaceManager();

  //openGLES contexts
  static EAGLContext *gl_context_;
  static EAGLContext *gl_resource_context_;

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

  //gl handlers
  GLuint default_fbo_ = 0;
  GLuint gl_tex_ = 0;
  CVOpenGLESTextureCacheRef gl_tex_cache_ref_ = nullptr;
  CVOpenGLESTextureRef gl_tex_ref_ = nullptr;

  //metal handlers
  id<MTLDevice> metal_device_;
  id<MTLTexture> metal_tex_;
  CVMetalTextureRef metal_tex_ref_ = nullptr;
  CVMetalTextureCacheRef metal_tex_cache_ref_ = nullptr;
};

} // namespace uiwidgets 