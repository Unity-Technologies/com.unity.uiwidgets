#include <include/gpu/GrContext.h>
#include <src/gpu/gl/GrGLDefines.h>

#include "cassert"
#include "include/core/SkCanvas.h"
#include "include/core/SkSurface.h"
#include "include/core/SkTextBlob.h"
#include "include/effects/SkPerlinNoiseShader.h"
#include "include/gpu/GrBackendSurface.h"
#include "include/gpu/GrContext.h"
#include "include/gpu/gl/GrGLTypes.h"
#include "modules/skottie/include/Skottie.h"
#include "platform_base.h"
#include "render_api.h"
#include "src/gpu/gl/GrGLDefines.h"
#include "windows.h"
#include <wingdi.h>

#if SUPPORT_OPENGL_UNIFIED

#include <GLES2/gl2.h>

class RenderAPI_OpenGLCoreES : public RenderAPI {
 public:
  RenderAPI_OpenGLCoreES(UnityGfxRenderer apiType);
  virtual ~RenderAPI_OpenGLCoreES() {}

  void ProcessDeviceEvent(UnityGfxDeviceEventType type,
                          IUnityInterfaces* interfaces) override;

  void* CreateTexture(int width, int height) override { return nullptr; }

  void CreateTexture1(void* ptr1, int width, int height) override;
  void SetImageTexture(void* ptr) override;

  void Draw() override;
  void PreDraw() override {}
  void PostDraw() override {}

 private:
  void CreateResources();
  void draw(SkCanvas* canvas);

 private:
  UnityGfxRenderer m_APIType;
  GLuint m_VertexShader;
  GLuint m_FragmentShader;
  GLuint m_Program;
  GLuint m_VertexArray;
  GLuint m_VertexBuffer;
  int m_UniformWorldMatrix;
  int m_UniformProjMatrix;

  sk_sp<GrContext> gr_context_;
  GrBackendTexture m_backendTex;
  sk_sp<SkSurface> m_SkSurface;
  void* surface_texture1_ptr_;
  sk_sp<SkImage> image_;

  void* texture1_ptr_;
  int width_, height_;
  void* image1_ptr_;
  HGLRC skia_ctx_ = NULL;

  sk_sp<skottie::Animation> animation_;
};

RenderAPI* CreateRenderAPI_OpenGLCoreES(UnityGfxRenderer apiType) {
  return new RenderAPI_OpenGLCoreES(apiType);
}

void RenderAPI_OpenGLCoreES::CreateResources() {

	HGLRC current_ctx = wglGetCurrentContext();
  HDC hdc = wglGetCurrentDC();

	//skia_ctx_ = wglCreateContext(hdc);
 // wglShareLists(current_ctx, skia_ctx_);

  gr_context_ = GrContext::MakeGL();
  animation_ =
      skottie::Animation::MakeFromFile(R"(C:\Users\kgdev\skottie.txt)");

	skia_ctx_ = NULL;
  m_SkSurface = nullptr;
        surface_texture1_ptr_ = nullptr;
        image_ = nullptr;
}

RenderAPI_OpenGLCoreES::RenderAPI_OpenGLCoreES(UnityGfxRenderer apiType)
    : m_APIType(apiType) {}

void RenderAPI_OpenGLCoreES::ProcessDeviceEvent(UnityGfxDeviceEventType type,
                                                IUnityInterfaces* interfaces) {
  if (type == kUnityGfxDeviceEventInitialize) {
    CreateResources();
  } else if (type == kUnityGfxDeviceEventShutdown) {
    //@TODO: release resources
  }
}

void RenderAPI_OpenGLCoreES::CreateTexture1(void* ptr1, int width, int height) {
  HGLRC current_ctx = wglGetCurrentContext();
  HDC hdc = wglGetCurrentDC();
	
  texture1_ptr_ = ptr1;
  width_ = width;
  height_ = height;

}

void RenderAPI_OpenGLCoreES::SetImageTexture(void* ptr) { image1_ptr_ = ptr; }

void RenderAPI_OpenGLCoreES::draw(SkCanvas* canvas) {
  canvas->drawColor(SK_ColorWHITE);

  SkPaint paint;
  paint.setStyle(SkPaint::kStroke_Style);
  paint.setStrokeWidth(4);
  paint.setColor(SK_ColorRED);

  SkRect rect = SkRect::MakeXYWH(50, 50, 40, 60);
  canvas->drawRect(rect, paint);

  SkRRect oval;
  oval.setOval(rect);
  oval.offset(40, 60);
  paint.setColor(SK_ColorBLUE);
  canvas->drawRRect(oval, paint);

  paint.setColor(SK_ColorCYAN);
  canvas->drawCircle(180, 50, 25, paint);

  rect.offset(80, 0);
  paint.setColor(SK_ColorYELLOW);
  canvas->drawRoundRect(rect, 10, 10, paint);

  SkPath path;
  path.cubicTo(768, 0, -512, 256, 256, 256);
  paint.setColor(SK_ColorGREEN);
  canvas->drawPath(path, paint);

  canvas->drawImage(image_, 128, 128, &paint);

  SkRect rect2 = SkRect::MakeXYWH(0, 0, 40, 60);
  canvas->drawImageRect(image_, rect2, &paint);

  SkPaint paint2;
  auto text = SkTextBlob::MakeFromString("Hello, Skia!", SkFont(nullptr, 18));
  canvas->drawTextBlob(text.get(), 50, 25, paint2);
}

double t2 = 0;

void RenderAPI_OpenGLCoreES::Draw() {
  // mutex_skia->AcquireSync(0, 5000);

		HGLRC current_ctx = wglGetCurrentContext();
  HDC hdc = wglGetCurrentDC();

	if (!skia_ctx_) {
      skia_ctx_ = wglCreateContext(hdc);
          wglShareLists(current_ctx, skia_ctx_);
	}

   bool result = wglMakeCurrent(hdc, skia_ctx_);

  if (!m_SkSurface || surface_texture1_ptr_ != texture1_ptr_) {
    surface_texture1_ptr_ = texture1_ptr_;
    GrGLTextureInfo textureInfo;
    textureInfo.fTarget = GR_GL_TEXTURE_2D;
    textureInfo.fID = GrGLuint((long)texture1_ptr_);
    textureInfo.fFormat = GR_GL_RGBA8;

    m_backendTex =
        GrBackendTexture(width_, height_, GrMipMapped::kNo, textureInfo);

    m_SkSurface = SkSurface::MakeFromBackendTexture(
        gr_context_.get(), m_backendTex, kBottomLeft_GrSurfaceOrigin, 1,
        kRGBA_8888_SkColorType, nullptr, nullptr);
  }

  if (!image_) {
    GrGLTextureInfo textureInfo;
    textureInfo.fTarget = GR_GL_TEXTURE_2D;
    textureInfo.fID = GrGLuint((long)image1_ptr_);
    textureInfo.fFormat = GR_GL_RGBA8;

    GrBackendTexture backendTex =
        GrBackendTexture(1000, 1000, GrMipMapped::kNo, textureInfo);

    image_ = SkImage::MakeFromTexture(
        gr_context_.get(), backendTex, kBottomLeft_GrSurfaceOrigin,
        kRGBA_8888_SkColorType, kOpaque_SkAlphaType, nullptr);
  }

  SkCanvas* canvas = m_SkSurface->getCanvas();
  draw(canvas);

   canvas->clear(SK_ColorWHITE);

   t2 += 1.0 / 60;
   if (t2 >= animation_->duration()) {
     t2 = 0.0;
   }

   animation_->seekFrameTime(t2);
   animation_->render(canvas);

  // SkRect rect = SkRect::MakeLTRB(100, 100, 200, 200);
  // canvas->drawImageRect(image_, rect, nullptr);

  // mutex_skia->ReleaseSync(0);
  canvas->flush();
  canvas->getGrContext()->submit(true);

  // if (rdoc_api) rdoc_api->EndFrameCapture(m_DeviceSkia, NULL);

	wglMakeCurrent(wglGetCurrentDC(), current_ctx);
}

#endif  // #if SUPPORT_OPENGL_UNIFIED
