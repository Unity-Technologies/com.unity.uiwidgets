#pragma once

#include <vector>
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

struct GLContextPair
{
  EAGLContext *gl_context_;
  EAGLContext *gl_resource_context_;

  GLContextPair(EAGLContext* gl, EAGLContext* gl_resource)
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

  void* CreateRenderTexture(void *native_texture_ptr, size_t width, size_t height);

  void ReleaseNativeRenderContext();

  bool ReleaseNativeRenderTexture();

  bool ClearCurrentContext();

  bool MakeCurrentContext();

  bool MakeCurrentResourceContext();

  uint32_t GetFbo();

  static void GetUnityContext();

 private:
  static GLContextPair GetFreeOpenGLContext();
  static GLContextPair GetFreeOpenGLContext(bool useOpenGL);
  void RecycleOpenGLContext(EAGLContext* gl, EAGLContext* gl_resource);
  //pixel buffer handles
  CVPixelBufferRef pixelbuffer_ref = nullptr;

  bool useOpenGL;

  //gl handlers
  static EAGLContext *unity_gl_context_;       //used for OpenGL only
  EAGLContext *unity_previous_gl_context_ = nil;

  EAGLContext *gl_context_;
  EAGLContext *gl_resource_context_;
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