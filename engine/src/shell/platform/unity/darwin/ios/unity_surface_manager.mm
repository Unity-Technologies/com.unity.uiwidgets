#include "unity_surface_manager.h"
#include <flutter/fml/logging.h>

#include "Unity/IUnityGraphics.h"
#include "Unity/IUnityGraphicsMetal.h"

#define GL_UNSIGNED_INT_8_8_8_8_REV 0x8367

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

  gl_context_ = [[EAGLContext alloc] initWithAPI:kEAGLRenderingAPIOpenGLES2];
  gl_resource_context_ = [[EAGLContext alloc] initWithAPI:[gl_context_ API] sharegroup: [gl_context_ sharegroup]];
  FML_DCHECK(gl_context_ != nullptr && gl_resource_context_ != nullptr);
}

UnitySurfaceManager::~UnitySurfaceManager() { ReleaseNativeRenderContext(); }

void* UnitySurfaceManager::CreateRenderTexture(size_t width, size_t height)
{
  //Constants
  const int ConstCVPixelFormat = kCVPixelFormatType_32BGRA;             
  const MTLPixelFormat ConstMetalViewPixelFormat = MTLPixelFormatBGRA8Unorm_sRGB;
  const GLuint ConstGLInternalFormat = GL_RGBA;        
  const GLuint ConstGLFormat = GL_BGRA_EXT;
  const GLuint ConstGLType = GL_UNSIGNED_INT_8_8_8_8_REV;

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

bool UnitySurfaceManager::ClearCurrentContext()
{
  [EAGLContext setCurrentContext:nil];
  return true;
}

bool UnitySurfaceManager::MakeCurrentContext()
{
  [EAGLContext setCurrentContext:gl_context_];
  return true;
}

bool UnitySurfaceManager::MakeCurrentResourceContext()
{
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
  gl_resource_context_ = nullptr;

  FML_DCHECK(gl_context_);
  gl_context_ = nullptr;

  [EAGLContext setCurrentContext:nil];

  FML_DCHECK(metal_device_ != nullptr);
  metal_device_ = nullptr;
}

bool UnitySurfaceManager::ReleaseNativeRenderTexture()
{
  //release gl resources
  CFRelease(gl_tex_ref_);
  CFRelease(gl_tex_cache_ref_);

  //release metal resources
  CFRelease(metal_tex_ref_);
  CFRelease(metal_tex_cache_ref_);

  //release cv pixelbuffer
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