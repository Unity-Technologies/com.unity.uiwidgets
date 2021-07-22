#pragma once

#include <OpenGL/gl3.h>
#include <AppKit/AppKit.h>
#include <Metal/Metal.h>
#include <CoreVideo/CoreVideo.h>
#include <vector>

#include "Unity/IUnityInterface.h"
#include "flutter/fml/macros.h"

namespace uiwidgets {

struct GLContextPair
{
  NSOpenGLContext *gl_context_;
  NSOpenGLContext *gl_resource_context_;

  GLContextPair(NSOpenGLContext* gl, NSOpenGLContext* gl_resource)
  {
    gl_context_ = gl;
    gl_resource_context_ = gl_resource;
  }
};

class UnitySurfaceManager {
 public:
  UnitySurfaceManager(IUnityInterfaces* unity_interfaces);
  ~UnitySurfaceManager();

  //openGL contexts pool
  static std::vector<GLContextPair> gl_context_pool_;
  static void ReleaseResource();

  void* CreateRenderTexture(size_t width, size_t height);

  void ReleaseNativeRenderContext();

  bool ReleaseNativeRenderTexture();

  bool ClearCurrentContext();

  bool MakeCurrentContext();

  bool MakeCurrentResourceContext();
  
  uint32_t GetFbo();

 private:
  static GLContextPair GetFreeOpenGLContext();
  void RecycleOpenGLContext(NSOpenGLContext* gl, NSOpenGLContext* gl_resource);
  //pixel buffer handles
  CVPixelBufferRef pixelbuffer_ref = nullptr;
  //openGL handlers
  NSOpenGLContext *gl_context_;
  NSOpenGLContext *gl_resource_context_;
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