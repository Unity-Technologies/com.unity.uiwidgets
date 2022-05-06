#include "unity_surface_manager.h"
#include <flutter/fml/logging.h>

#include "Unity/IUnityGraphics.h"
#include "Unity/IUnityGraphicsMetal.h"

namespace uiwidgets {

std::vector<GLContextPair> UnitySurfaceManager::gl_context_pool_;
NSOpenGLContext* UnitySurfaceManager::unity_gl_context_;

GLContextPair UnitySurfaceManager::GetFreeOpenGLContext(bool useOpenGLCore)
{
  if (gl_context_pool_.size() == 0)
  {
    NSOpenGLPixelFormatAttribute attrs[] =
    {
      NSOpenGLPFAAccelerated,
      0
    };
    
    NSOpenGLPixelFormat *pixelFormat = [[NSOpenGLPixelFormat alloc] initWithAttributes:attrs];
    if (useOpenGLCore)
    {
        pixelFormat = [unity_gl_context_ pixelFormat];
    }

    NSOpenGLContext* gl_resource;
    NSOpenGLContext* gl;

    NSOpenGLContext* base_gl_context = nil;
    if (useOpenGLCore)
    {
      base_gl_context = unity_gl_context_;
    }
    while(gl_resource == nil) {
      gl = [[NSOpenGLContext alloc] initWithFormat:pixelFormat shareContext:base_gl_context];
      gl_resource = [[NSOpenGLContext alloc] initWithFormat:pixelFormat shareContext:gl];  
    
      if (gl_resource == nil) {
        CGLReleaseContext([gl CGLContextObj]);
        gl = nullptr;
      }
    }
    return GLContextPair(gl, gl_resource);
  }

  auto context_pair = gl_context_pool_.back();
  gl_context_pool_.pop_back();

  return context_pair;
}

void UnitySurfaceManager::RecycleOpenGLContext(NSOpenGLContext* gl, NSOpenGLContext* gl_resource)
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
    auto context_pair = gl_context_pool_.back();
    CGLReleaseContext([context_pair.gl_context_ CGLContextObj]);
    CGLReleaseContext([context_pair.gl_resource_context_ CGLContextObj]);
    gl_context_pool_.pop_back();
  }
}

UnitySurfaceManager::UnitySurfaceManager(IUnityInterfaces* unity_interfaces)
{
  //get main gfx device
  auto* graphics = unity_interfaces->Get<IUnityGraphics>();
  UnityGfxRenderer renderer = graphics->GetRenderer();
  
  if (renderer == kUnityGfxRendererMetal)
  {
    FML_DCHECK(metal_device_ == nullptr);
    
    auto* metalGraphics = unity_interfaces->Get<IUnityGraphicsMetalV1>();

    metal_device_ = metalGraphics->MetalDevice();
    
    useOpenGLCore = false;
  }
  else if (renderer == kUnityGfxRendererOpenGLCore)
  {
    FML_DCHECK(unity_gl_context_ != nil);
    
    useOpenGLCore = true;
  }
  
  //create opengl context
  if (gl_context_ == nullptr && gl_resource_context_ == nullptr) {
    auto new_context = GetFreeOpenGLContext(useOpenGLCore);
    gl_context_ = new_context.gl_context_;
    gl_resource_context_ = new_context.gl_resource_context_;
  }
  FML_DCHECK(gl_context_ != nullptr && gl_resource_context_ != nullptr);
}

UnitySurfaceManager::~UnitySurfaceManager() { ReleaseNativeRenderContext(); }

void* UnitySurfaceManager::CreateRenderTexture(void *native_texture_ptr, size_t width, size_t height)
{
  //Constants
  const MTLPixelFormat ConstMetalViewPixelFormat = MTLPixelFormatBGRA8Unorm;
  const int ConstCVPixelFormat = kCVPixelFormatType_32BGRA;
  const GLuint ConstGLInternalFormat = GL_SRGB8_ALPHA8;
  const GLuint ConstGLFormat = GL_BGRA;
  const GLuint ConstGLType = GL_UNSIGNED_INT_8_8_8_8_REV;
  if (useOpenGLCore)
  {
    gl_tex_ = static_cast<int>(reinterpret_cast<intptr_t>(native_texture_ptr));
    
    NSOpenGLContext* _pre_context = [NSOpenGLContext currentContext];
    GLint old_texture_binding_2d;
    glGetIntegerv(GL_TEXTURE_BINDING_2D, &old_texture_binding_2d);
      
    [gl_context_ makeCurrentContext];
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
      [_pre_context makeCurrentContext];
    }
    return (void*) gl_tex_;
  }
  else
  {
    //render context must be available
    FML_DCHECK(metal_device_ != nullptr && gl_context_ != nullptr);
    
    //render textures must be released already
    FML_DCHECK(pixelbuffer_ref == nullptr && default_fbo_ == 0 && gl_tex_ == 0 && gl_tex_cache_ref_ == nullptr && gl_tex_ref_ == nullptr && metal_tex_ == nullptr && metal_tex_ref_ == nullptr && metal_tex_cache_ref_ == nullptr);
    //create pixel buffer
    auto gl_pixelformat_ = gl_context_.pixelFormat.CGLPixelFormatObj;

    NSDictionary* cvBufferProperties = @{
      (__bridge NSString*)kCVPixelBufferOpenGLCompatibilityKey : @YES,
      (__bridge NSString*)kCVPixelBufferMetalCompatibilityKey : @YES,
    };

    CVReturn cvret = CVPixelBufferCreate(kCFAllocatorDefault,
            width, height,
            ConstCVPixelFormat,
            (__bridge CFDictionaryRef)cvBufferProperties,
            &pixelbuffer_ref);
    FML_DCHECK(cvret == kCVReturnSuccess);

    //create metal texture
    cvret = CVMetalTextureCacheCreate(
            kCFAllocatorDefault,
            nil,
            metal_device_,
            nil,
            &metal_tex_cache_ref_);
    FML_DCHECK(cvret == kCVReturnSuccess);

    cvret = CVMetalTextureCacheCreateTextureFromImage(
            kCFAllocatorDefault,
            metal_tex_cache_ref_,
            pixelbuffer_ref, nil,
            ConstMetalViewPixelFormat,
            width, height,
            0,
            &metal_tex_ref_);
    FML_DCHECK(cvret == kCVReturnSuccess);

    metal_tex_ = CVMetalTextureGetTexture(metal_tex_ref_);
    FML_DCHECK(metal_tex_ != nullptr);

    //create opengl texture
    cvret  = CVOpenGLTextureCacheCreate(
            kCFAllocatorDefault,
            nil,
            gl_context_.CGLContextObj,
            gl_pixelformat_,
            nil,
            &gl_tex_cache_ref_);
    FML_DCHECK(cvret == kCVReturnSuccess);

    cvret = CVOpenGLTextureCacheCreateTextureFromImage(
            kCFAllocatorDefault,
            gl_tex_cache_ref_,
            pixelbuffer_ref,
            nil,
            &gl_tex_ref_);
    FML_DCHECK(cvret == kCVReturnSuccess);

    gl_tex_ = CVOpenGLTextureGetName(gl_tex_ref_);

    //initialize gl renderer
    [gl_context_ makeCurrentContext];
    glGenFramebuffers(1, &default_fbo_);
    glBindFramebuffer(GL_FRAMEBUFFER, default_fbo_);

    const GLenum texType = GL_TEXTURE_RECTANGLE;
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, texType, gl_tex_, 0);

    return (__bridge void*)metal_tex_;
  }
}

bool UnitySurfaceManager::ClearCurrentContext()
{
  [NSOpenGLContext clearCurrentContext];
  if (unity_previous_gl_context_ != nil)
  {
    [unity_previous_gl_context_ makeCurrentContext];
    unity_previous_gl_context_ = nil;
  }
  return true;
}

bool UnitySurfaceManager::MakeCurrentContext()
{
  NSOpenGLContext* _pre_context = [NSOpenGLContext currentContext];
  if (_pre_context != nil && _pre_context != gl_context_ &&
      _pre_context != gl_resource_context_)
  {
    unity_previous_gl_context_ = _pre_context;
  }

  [gl_context_ makeCurrentContext];
  return true;
}

bool UnitySurfaceManager::MakeCurrentResourceContext()
{
  NSOpenGLContext* _pre_context = [NSOpenGLContext currentContext];
  if (_pre_context != nil && _pre_context != gl_context_ &&
      _pre_context != gl_resource_context_)
  {
    unity_previous_gl_context_ = _pre_context;
  }

  [gl_resource_context_ makeCurrentContext];
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
  
  if (!useOpenGLCore)
  {
    FML_DCHECK(metal_device_ != nullptr);
    metal_device_ = nullptr;
  }
}

bool UnitySurfaceManager::ReleaseNativeRenderTexture()
{
  if (!useOpenGLCore) {
    //release gl resources
    CVOpenGLTextureRelease(gl_tex_ref_);
    CVOpenGLTextureCacheRelease(gl_tex_cache_ref_);

    //release metal resources
    CFRelease(metal_tex_ref_);
    CFRelease(metal_tex_cache_ref_);

    //release pixel buffer
    CVPixelBufferRelease(pixelbuffer_ref);
  }
  else{
    NSOpenGLContext* _pre_context = [NSOpenGLContext currentContext];
    [gl_context_ makeCurrentContext];
    glDeleteTextures(1, &gl_tex_);
    if (_pre_context != nil)
    {
      [_pre_context makeCurrentContext];
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
  //get unity opengl core context
  //if we can get the current active NSOpenGLContext then use it directly
  NSOpenGLContext* _cur_unity_gl_context = [NSOpenGLContext currentContext];
  if (_cur_unity_gl_context != nullptr)
  {
      unity_gl_context_ = _cur_unity_gl_context;
      return;
  }
  
  //otherwise, we choose to fetch the CGLContextObj and create a new NSOpenGLContext for it
  //HOWEVER, according to this doc: https://developer.apple.com/documentation/appkit/nsopenglcontext/1436180-initwithcglcontextobj?language=objc
  //only one NSOpenGLContext object can wrap a specific context, which means that the lines below
  //might cause problem if the fetched CGLContextObj is already bound to a NSOpenGLContext, which in fact happens on Mac StandalonePlayer (OpenGLCore backend).
  //For now, this code path will be activated for Mac Editor (OpenGLCore backend) only, on which the
  //_cur_unity_gl_context turns out to be nullptr.
  //TODO: investigate a better solution for Mac Editor (OpenGLCore backend)
  CGLContextObj glContext = CGLGetCurrentContext();
  unity_gl_context_ = [[NSOpenGLContext alloc] initWithCGLContextObj:glContext];
}
}  // namespace uiwidgets
