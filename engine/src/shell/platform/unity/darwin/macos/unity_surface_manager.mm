#include "unity_surface_manager.h"
#include <flutter/fml/logging.h>

#include "Unity/IUnityGraphics.h"
#include "Unity/IUnityGraphicsMetal.h"

namespace uiwidgets {
UnitySurfaceManager::UnitySurfaceManager(IUnityInterfaces* unity_interfaces)
{
  FML_DCHECK(metal_device_ == nullptr);

  //get main gfx device (metal)
  auto* graphics = unity_interfaces->Get<IUnityGraphics>();
    
  FML_DCHECK(graphics->GetRenderer() == kUnityGfxRendererMetal);

  auto* metalGraphics = unity_interfaces->Get<IUnityGraphicsMetalV1>();

  metal_device_ = metalGraphics->MetalDevice();

  //create opengl context
  FML_DCHECK(!gl_context_);
  FML_DCHECK(!gl_resource_context_);

  NSOpenGLPixelFormatAttribute attrs[] =
    {
      NSOpenGLPFAAccelerated,
      0
    };

  NSOpenGLPixelFormat *pixelFormat = [[NSOpenGLPixelFormat alloc] initWithAttributes:attrs];
    
  //gl_context_ may be created unproperly, we can check this using the relevant gl_resource_context_
  //loop until the created gl_context_ is ok, otherwise the program will crash anyway
  while(gl_resource_context_ == nil) {
    gl_context_ = [[NSOpenGLContext alloc] initWithFormat:pixelFormat shareContext:nil];
    gl_resource_context_ = [[NSOpenGLContext alloc] initWithFormat:pixelFormat shareContext:gl_context_];  
    
    if (gl_resource_context_ == nil) {
        CGLReleaseContext(gl_context_.CGLContextObj);
        gl_context_ = nullptr;
    }
  }

  FML_DCHECK(gl_context_ != nullptr && gl_resource_context_ != nullptr);
}

UnitySurfaceManager::~UnitySurfaceManager() { ReleaseNativeRenderContext(); }

void* UnitySurfaceManager::CreateRenderTexture(size_t width, size_t height)
{
  //Constants
  const MTLPixelFormat ConstMetalViewPixelFormat = MTLPixelFormatBGRA8Unorm_sRGB;
  const int ConstCVPixelFormat = kCVPixelFormatType_32BGRA;
  const GLuint ConstGLInternalFormat = GL_SRGB8_ALPHA8;
  const GLuint ConstGLFormat = GL_BGRA;
  const GLuint ConstGLType = GL_UNSIGNED_INT_8_8_8_8_REV;

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

bool UnitySurfaceManager::ClearCurrentContext()
{
  [NSOpenGLContext clearCurrentContext];
  return true;
}

bool UnitySurfaceManager::MakeCurrentContext()
{
  [gl_context_ makeCurrentContext];
  return true;
}

bool UnitySurfaceManager::MakeCurrentResourceContext()
{
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
  CGLReleaseContext(gl_resource_context_.CGLContextObj);
  gl_resource_context_ = nullptr;

  FML_DCHECK(gl_context_);
  CGLReleaseContext(gl_context_.CGLContextObj);
  gl_context_ = nullptr;

  FML_DCHECK(metal_device_ != nullptr);
  metal_device_ = nullptr;
}

bool UnitySurfaceManager::ReleaseNativeRenderTexture()
{
  //release gl resources
  CVOpenGLTextureRelease(gl_tex_ref_);
  CVOpenGLTextureCacheRelease(gl_tex_cache_ref_);

  //release metal resources
  CFRelease(metal_tex_ref_);
  CFRelease(metal_tex_cache_ref_);

  //release pixel buffer
  CVPixelBufferRelease(pixelbuffer_ref);

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
}  // namespace uiwidgets
