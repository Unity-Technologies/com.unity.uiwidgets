#include "unity_surface_manager.h"
#include <flutter/fml/logging.h>

#include "Unity/IUnityGraphics.h"
#include "Unity/IUnityGraphicsMetal.h"

#define GL_UNSIGNED_INT_8_8_8_8_REV 0x8367

namespace uiwidgets {

std::vector<GLContextPair> UnitySurfaceManager::gl_context_pool_;
EAGLContext* UnitySurfaceManager::unity_gl_context_;

GLContextPair UnitySurfaceManager::GetFreeOpenGLContext(bool useOpenGL)
{
  if (gl_context_pool_.size() == 0)
  {
    EAGLContext* gl;
    if (useOpenGL)
    {
      gl = [[EAGLContext alloc] initWithAPI:[unity_gl_context_ API] sharegroup: [unity_gl_context_ sharegroup]];
    }
    else
    {
      gl = [[EAGLContext alloc] initWithAPI:kEAGLRenderingAPIOpenGLES2];
    }
    EAGLContext* gl_resource = [[EAGLContext alloc] initWithAPI:[gl API] sharegroup: [gl sharegroup]];
    return GLContextPair(gl, gl_resource);
  }
  auto context_pair = gl_context_pool_.back();
  gl_context_pool_.pop_back();

  return context_pair;
}

void UnitySurfaceManager::RecycleOpenGLContext(EAGLContext* gl, EAGLContext* gl_resource)
{
  MakeCurrentContext();
  ClearCurrentContext();
  MakeCurrentResourceContext();
  ClearCurrentContext();

  gl_context_pool_.push_back(GLContextPair(gl, gl_resource));
}

void UnitySurfaceManager::ReleaseResource()
{
  while(gl_context_pool_.size() > 0)
  {
    gl_context_pool_.pop_back();
  }
}

UnitySurfaceManager::UnitySurfaceManager(IUnityInterfaces* unity_interfaces)
{
  FML_DCHECK(metal_device_ == nullptr);

  //get main gfx device
  auto* graphics = unity_interfaces->Get<IUnityGraphics>();
  UnityGfxRenderer renderer = graphics->GetRenderer();

  if (renderer == kUnityGfxRendererMetal)
  {
    auto* metalGraphics = unity_interfaces->Get<IUnityGraphicsMetalV1>();

    metal_device_ = metalGraphics->MetalDevice();

    useOpenGL = false;
  }
  else if (renderer == kUnityGfxRendererOpenGLES20 || renderer == kUnityGfxRendererOpenGLES30)
  {
    FML_DCHECK(unity_gl_context_ != nil);

    useOpenGL = true;
  }

  //create opengl context
  if (gl_context_ == nullptr && gl_resource_context_ == nullptr) {
    auto new_context = GetFreeOpenGLContext(useOpenGL);
    gl_context_ = new_context.gl_context_;
    gl_resource_context_ = new_context.gl_resource_context_;
  }

  FML_DCHECK(gl_context_ != nullptr && gl_resource_context_ != nullptr);
}

UnitySurfaceManager::~UnitySurfaceManager() { ReleaseNativeRenderContext(); }

void* UnitySurfaceManager::CreateRenderTexture(void *native_texture_ptr, size_t width, size_t height)
{
  //Constants
  const int ConstCVPixelFormat = kCVPixelFormatType_32BGRA;             
  const MTLPixelFormat ConstMetalViewPixelFormat = MTLPixelFormatBGRA8Unorm;
  const GLuint ConstGLInternalFormat = GL_RGBA;        
  const GLuint ConstGLFormat = GL_BGRA_EXT;
  const GLuint ConstGLType = GL_UNSIGNED_INT_8_8_8_8_REV;
  if (useOpenGL)
  {
    gl_tex_ = static_cast<int>(reinterpret_cast<intptr_t>(native_texture_ptr));

    EAGLContext* _pre_context = [EAGLContext currentContext];
    GLint old_texture_binding_2d;
    glGetIntegerv(GL_TEXTURE_BINDING_2D, &old_texture_binding_2d);
    
    [EAGLContext setCurrentContext:gl_context_];
    glBindTexture(GL_TEXTURE_2D, gl_tex_);
    glGenFramebuffers(1, &default_fbo_);

    GLint old_framebuffer_binding;
    glGetIntegerv(GL_FRAMEBUFFER_BINDING, &old_framebuffer_binding);

    glBindFramebuffer(GL_FRAMEBUFFER, default_fbo_);
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, gl_tex_, 0);

    FML_CHECK(glCheckFramebufferStatus(GL_FRAMEBUFFER) ==
                GL_FRAMEBUFFER_COMPLETE);

    //reset gl states
    glBindFramebuffer(GL_FRAMEBUFFER, old_framebuffer_binding);
    glBindTexture(GL_TEXTURE_2D, old_texture_binding_2d);
    if (_pre_context != nil)
    {
      [EAGLContext setCurrentContext:_pre_context];
    }
    return (void*) gl_tex_;
  }
  else
  {
    //render context must be available
    FML_DCHECK(metal_device_ != nullptr && gl_context_ != nullptr);

    //render textures must be released already
    FML_DCHECK(pixelbuffer_ref == nullptr && default_fbo_ == 0 && gl_tex_ == 0 && gl_tex_cache_ref_ == nullptr && gl_tex_ref_ == nullptr && metal_tex_ == nullptr && metal_tex_ref_ == nullptr && metal_tex_cache_ref_ == nullptr);

    CVReturn cvret;
    //create pixel buffer
    NSDictionary* cvBufferProperties = @{
              (__bridge NSString*)kCVPixelBufferOpenGLCompatibilityKey : @YES,
              (__bridge NSString*)kCVPixelBufferMetalCompatibilityKey : @YES,
          };
          cvret = CVPixelBufferCreate(kCFAllocatorDefault,
                                width, height,
                                  ConstCVPixelFormat,
                                  (__bridge CFDictionaryRef)cvBufferProperties,
                                  &pixelbuffer_ref);

          FML_DCHECK(cvret == kCVReturnSuccess);

    //create metal context
    // 1. Create a Metal Core Video texture cache from the pixel buffer.
    cvret = CVMetalTextureCacheCreate(
                      kCFAllocatorDefault,
                      nil,
                      metal_device_,
                      nil,
                      &metal_tex_cache_ref_);

    FML_DCHECK(cvret == kCVReturnSuccess);

    // 2. Create a CoreVideo pixel buffer backed Metal texture image from the texture cache.
    cvret = CVMetalTextureCacheCreateTextureFromImage(
                      kCFAllocatorDefault,
                      metal_tex_cache_ref_,
                      pixelbuffer_ref, nil,
                      ConstMetalViewPixelFormat,
                      width, height,
                      0,
                      &metal_tex_ref_);

    FML_DCHECK(cvret == kCVReturnSuccess);

    // 3. Get a Metal texture using the CoreVideo Metal texture reference.
    metal_tex_ = CVMetalTextureGetTexture(metal_tex_ref_);

    FML_DCHECK(metal_tex_ != nullptr);

    //create GL Texture
    // 1. Create an OpenGL ES CoreVideo texture cache from the pixel buffer.
    cvret = CVOpenGLESTextureCacheCreate(kCFAllocatorDefault,
                      nil,
                      gl_context_,
                      nil,
                      &gl_tex_cache_ref_);

    FML_DCHECK(cvret == kCVReturnSuccess);

    // 2. Create a CVPixelBuffer-backed OpenGL ES texture image from the texture cache.
    cvret = CVOpenGLESTextureCacheCreateTextureFromImage(kCFAllocatorDefault,
                      gl_tex_cache_ref_,
                      pixelbuffer_ref,
                      nil,
                      GL_TEXTURE_2D,
                      ConstGLInternalFormat,
                      width, height,
                      ConstGLFormat,
                      ConstGLType,
                      0,
                      &gl_tex_ref_);

    FML_DCHECK(cvret == kCVReturnSuccess);

    // 3. Get an OpenGL ES texture name from the CVPixelBuffer-backed OpenGL ES texture image.
    gl_tex_ = CVOpenGLESTextureGetName(gl_tex_ref_);

    //initialize gl renderer
    [EAGLContext setCurrentContext:gl_context_];
    glGenFramebuffers(1, &default_fbo_);
    glBindFramebuffer(GL_FRAMEBUFFER, default_fbo_);

    const GLenum texType = GL_TEXTURE_2D;
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, texType, gl_tex_, 0);

    return (__bridge void*)metal_tex_;
  }
}

bool UnitySurfaceManager::ClearCurrentContext()
{
  if (unity_previous_gl_context_ != nil)
  {
    [EAGLContext setCurrentContext:unity_previous_gl_context_];
    unity_previous_gl_context_ = nil;
  }
  else
  {
    [EAGLContext setCurrentContext:nil];
  }
  return true;
}

bool UnitySurfaceManager::MakeCurrentContext()
{
  EAGLContext* _pre_context = [EAGLContext currentContext];
  if (_pre_context != nil && _pre_context != gl_context_ &&
      _pre_context != gl_resource_context_)
  {
    unity_previous_gl_context_ = _pre_context;
  }

  [EAGLContext setCurrentContext:gl_context_];
  return true;
}

bool UnitySurfaceManager::MakeCurrentResourceContext()
{
  EAGLContext* _pre_context = [EAGLContext currentContext];
  if (_pre_context != nil && _pre_context != gl_context_ &&
      _pre_context != gl_resource_context_)
  {
    unity_previous_gl_context_ = _pre_context;
  }

  [EAGLContext setCurrentContext:gl_resource_context_];
  return true;
}

uint32_t UnitySurfaceManager::GetFbo()
{
  return default_fbo_;
}

void UnitySurfaceManager::ReleaseNativeRenderContext()
{
  FML_DCHECK(gl_resource_context_);
  FML_DCHECK(gl_context_);
  RecycleOpenGLContext(gl_context_, gl_resource_context_);
  gl_context_ = nullptr;
  gl_resource_context_ = nullptr;

  [EAGLContext setCurrentContext:nil];

  if (!useOpenGL) 
  {
    FML_DCHECK(metal_device_ != nullptr);
    metal_device_ = nullptr;
  }
}

bool UnitySurfaceManager::ReleaseNativeRenderTexture()
{
  if (!useOpenGL)
  {
    //release gl resources
    CFRelease(gl_tex_ref_);
    CFRelease(gl_tex_cache_ref_);

    //release metal resources
    CFRelease(metal_tex_ref_);
    CFRelease(metal_tex_cache_ref_);

    //release cv pixelbuffer
    CVPixelBufferRelease(pixelbuffer_ref);
  }
  else
  {
    EAGLContext* _pre_context = [EAGLContext currentContext];
    [EAGLContext setCurrentContext:gl_context_];
    glDeleteTextures(1, &gl_tex_);
    if (_pre_context != nil)
    {
      [EAGLContext setCurrentContext:_pre_context];
    }
  }
  

  FML_DCHECK(default_fbo_ != 0);
  glDeleteFramebuffers(1, &default_fbo_);
  default_fbo_ = 0;

  //gl_tex_ is released in CVOpenGLTextureRelease
  gl_tex_ = 0;
  gl_tex_cache_ref_ = nullptr;
  gl_tex_ref_ = nullptr;

  metal_tex_ref_ = nullptr;
  metal_tex_cache_ref_ = nullptr;
  //since ARC is enabled by default, no need to release the texture
  metal_tex_ = nullptr;
  
  pixelbuffer_ref = nullptr;
  return true;
}

void UnitySurfaceManager::GetUnityContext()
{
  if (unity_gl_context_ != nil)
  {
    return;
  }
  unity_gl_context_ = [EAGLContext currentContext];
}
}  // namespace uiwidgets
