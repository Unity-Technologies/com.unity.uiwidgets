#pragma once

#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>
#include "src/common/settings.h"

#if defined(__cplusplus)
extern "C" {
#endif

typedef enum {
  kSuccess = 0,
  kInvalidLibraryVersion,
  kInvalidArguments,
  kInternalInconsistency,
} UIWidgetsEngineResult;

typedef enum {
  kOpenGL,
  kSoftware,
} UIWidgetsRendererType;

typedef enum {
  kUIWidgetsAccessibilityFeatureAccessibleNavigation = 1 << 0,
  kUIWidgetsAccessibilityFeatureInvertColors = 1 << 1,
  kUIWidgetsAccessibilityFeatureDisableAnimations = 1 << 2,
  kUIWidgetsAccessibilityFeatureBoldText = 1 << 3,
  kUIWidgetsAccessibilityFeatureReduceMotion = 1 << 4,
} UIWidgetsAccessibilityFeature;

typedef enum {
  kUIWidgetsTextDirectionUnknown = 0,
  kUIWidgetsTextDirectionRTL = 1,
  kUIWidgetsTextDirectionLTR = 2,
} UIWidgetsTextDirection;

typedef struct _UIWidgetsEngine* UIWidgetsEngine;

typedef struct {
  float scaleX;
  float skewX;
  float transX;
  float skewY;
  float scaleY;
  float transY;
  float pers0;
  float pers1;
  float pers2;
} UIWidgetsTransformation;

typedef void (*VoidCallback)(void* /* user data */);

typedef enum {
  kUIWidgetsOpenGLTargetTypeTexture,
  kUIWidgetsOpenGLTargetTypeFramebuffer,
} UIWidgetsOpenGLTargetType;

typedef struct {
  uint32_t target;
  uint32_t name;
  uint32_t format;
  void* user_data;
  VoidCallback destruction_callback;
  size_t width;
  size_t height;
} UIWidgetsOpenGLTexture;

typedef struct {
  uint32_t target;
  uint32_t name;
  void* user_data;
  VoidCallback destruction_callback;
} UIWidgetsOpenGLFramebuffer;

typedef bool (*BoolCallback)(void* /* user data */);
typedef UIWidgetsTransformation (*TransformationCallback)(
    void* /* user data */);
typedef uint32_t (*UIntCallback)(void* /* user data */);
typedef bool (*SoftwareSurfacePresentCallback)(void* /* user data */,
                                               const void* /* allocation */,
                                               size_t /* row bytes */,
                                               size_t /* height */);
typedef void* (*ProcResolver)(void* /* user data */, const char* /* name */);
typedef bool (*TextureFrameCallback)(void* /* user data */,
                                     int64_t /* texture identifier */,
                                     size_t /* width */, size_t /* height */,
                                     UIWidgetsOpenGLTexture* /* texture out */);
typedef void (*VsyncCallback)(void* /* user data */, intptr_t /* baton */);

typedef struct {
  size_t struct_size;
  BoolCallback make_current;
  BoolCallback clear_current;
  BoolCallback present;
  UIntCallback fbo_callback;
  BoolCallback make_resource_current;
  bool fbo_reset_after_present;
  TransformationCallback surface_transformation;
  ProcResolver gl_proc_resolver;
  TextureFrameCallback gl_external_texture_frame_callback;
} UIWidgetsOpenGLRendererConfig;

typedef struct {
  size_t struct_size;
  SoftwareSurfacePresentCallback surface_present_callback;
} UIWidgetsSoftwareRendererConfig;

typedef struct {
  UIWidgetsRendererType type;
  union {
    UIWidgetsOpenGLRendererConfig open_gl;
    UIWidgetsSoftwareRendererConfig software;
  };
} UIWidgetsRendererConfig;

typedef struct {
  size_t struct_size;
  size_t width;
  size_t height;
  float pixel_ratio;
} UIWidgetsWindowMetricsEvent;

typedef enum {
  kCancel,
  kUp,
  kDown,
  kMove,
  kAdd,
  kRemove,
  kHover,
  kMouseDown,
  kMouseUp,
} UIWidgetsPointerPhase;

typedef enum {
  kUIWidgetsPointerDeviceKindTouch = 0,
  kUIWidgetsPointerDeviceKindMouse = 1,
  kUIWidgetsPointerDeviceKindKeyboard = 7,
} UIWidgetsPointerDeviceKind;

typedef enum {
  kUIWidgetsPointerButtonMousePrimary = 1 << 0,
  kUIWidgetsPointerButtonMouseSecondary = 1 << 1,
  kUIWidgetsPointerButtonMouseMiddle = 1 << 2,
  kUIWidgetsPointerButtonMouseBack = 1 << 3,
  kUIWidgetsPointerButtonMouseForward = 1 << 4,
} UIWidgetsPointerMouseButtons;

typedef enum {
  kUIWidgetsPointerSignalKindNone,
  kUIWidgetsPointerSignalKindScroll,
  kUIWidgetsPointerSignalKindEditorDragUpdate,
  kUIWidgetsPointerSignalKindEditorDragRelease
} UIWidgetsPointerSignalKind;

typedef struct {
  size_t struct_size;
  UIWidgetsPointerPhase phase;
  size_t timestamp;
  float x;
  float y;
  int32_t device;
  UIWidgetsPointerSignalKind signal_kind;
  float scroll_delta_x;
  float scroll_delta_y;
  UIWidgetsPointerDeviceKind device_kind;
  int64_t buttons;
  int64_t modifier;
} UIWidgetsPointerEvent;

struct _UIWidgetsPlatformMessageResponseHandle;
typedef struct _UIWidgetsPlatformMessageResponseHandle
    UIWidgetsPlatformMessageResponseHandle;

typedef struct {
  size_t struct_size;
  const char* channel;
  const uint8_t* message;
  size_t message_size;
  const UIWidgetsPlatformMessageResponseHandle* response_handle;
} UIWidgetsPlatformMessage;

typedef void (*UIWidgetsPlatformMessageCallback)(
    const UIWidgetsPlatformMessage* /* message*/, void* /* user data */);

typedef void (*UIWidgetsDataCallback)(const uint8_t* /* data */,
                                      size_t /* size */, void* /* user data */);

typedef struct {
  float left;
  float top;
  float right;
  float bottom;
} UIWidgetsRect;

typedef struct {
  float x;
  float y;
} UIWidgetsPoint;

typedef struct {
  float width;
  float height;
} UIWidgetsSize;

typedef struct {
  UIWidgetsRect rect;
  UIWidgetsSize upper_left_corner_radius;
  UIWidgetsSize upper_right_corner_radius;
  UIWidgetsSize lower_right_corner_radius;
  UIWidgetsSize lower_left_corner_radius;
} UIWidgetsRoundedRect;

typedef int64_t UIWidgetsPlatformViewIdentifier;

typedef struct _UIWidgetsTaskRunner* UIWidgetsTaskRunner;

typedef struct {
  UIWidgetsTaskRunner runner;
  uint64_t task;
} UIWidgetsTask;

typedef void (*UIWidgetsTaskRunnerPostTaskCallback)(
    UIWidgetsTask /* task */, uint64_t /* target time nanos */,
    void* /* user data */);

typedef struct {
  size_t struct_size;
  void* user_data;
  BoolCallback runs_task_on_current_thread_callback;
  UIWidgetsTaskRunnerPostTaskCallback post_task_callback;
  size_t identifier;
} UIWidgetsTaskRunnerDescription;

typedef struct {
  size_t struct_size;
  const UIWidgetsTaskRunnerDescription* platform_task_runner;
  const UIWidgetsTaskRunnerDescription* ui_task_runner;
  const UIWidgetsTaskRunnerDescription* render_task_runner;
} UIWidgetsCustomTaskRunners;

typedef struct {
  UIWidgetsOpenGLTargetType type;
  union {
    UIWidgetsOpenGLTexture texture;
    UIWidgetsOpenGLFramebuffer framebuffer;
  };
} UIWidgetsOpenGLBackingStore;

typedef struct {
  const void* allocation;
  size_t row_bytes;
  size_t height;
  void* user_data;
  VoidCallback destruction_callback;
} UIWidgetsSoftwareBackingStore;

typedef enum {
  kUIWidgetsPlatformViewMutationTypeOpacity,
  kUIWidgetsPlatformViewMutationTypeClipRect,
  kUIWidgetsPlatformViewMutationTypeClipRoundedRect,
  kUIWidgetsPlatformViewMutationTypeTransformation,
} UIWidgetsPlatformViewMutationType;

typedef struct {
  UIWidgetsPlatformViewMutationType type;
  union {
    double opacity;
    UIWidgetsRect clip_rect;
    UIWidgetsRoundedRect clip_rounded_rect;
    UIWidgetsTransformation transformation;
  };
} UIWidgetsPlatformViewMutation;

typedef struct {
  size_t struct_size;
  UIWidgetsPlatformViewIdentifier identifier;
  size_t mutations_count;
  const UIWidgetsPlatformViewMutation** mutations;
} UIWidgetsPlatformView;

typedef enum {
  kUIWidgetsBackingStoreTypeOpenGL,
  kUIWidgetsBackingStoreTypeSoftware,
} UIWidgetsBackingStoreType;

typedef struct {
  size_t struct_size;
  void* user_data;
  UIWidgetsBackingStoreType type;
  bool did_update;
  union {
    UIWidgetsOpenGLBackingStore open_gl;
    UIWidgetsSoftwareBackingStore software;
  };
} UIWidgetsBackingStore;

typedef struct {
  size_t struct_size;
  UIWidgetsSize size;
} UIWidgetsBackingStoreConfig;

typedef enum {
  kUIWidgetsLayerContentTypeBackingStore,
  kUIWidgetsLayerContentTypePlatformView,
} UIWidgetsLayerContentType;

typedef struct {
  size_t struct_size;
  UIWidgetsLayerContentType type;
  union {
    const UIWidgetsBackingStore* backing_store;
    const UIWidgetsPlatformView* platform_view;
  };
  UIWidgetsPoint offset;
  UIWidgetsSize size;
} UIWidgetsLayer;

typedef bool (*UIWidgetsBackingStoreCreateCallback)(
    const UIWidgetsBackingStoreConfig* config,
    UIWidgetsBackingStore* backing_store_out, void* user_data);

typedef bool (*UIWidgetsBackingStoreCollectCallback)(
    const UIWidgetsBackingStore* renderer, void* user_data);

typedef bool (*UIWidgetsLayersPresentCallback)(const UIWidgetsLayer** layers,
                                               size_t layers_count,
                                               void* user_data);

typedef struct {
  size_t struct_size;
  void* user_data;
  UIWidgetsBackingStoreCreateCallback create_backing_store_callback;
  UIWidgetsBackingStoreCollectCallback collect_backing_store_callback;
  UIWidgetsLayersPresentCallback present_layers_callback;
} UIWidgetsCompositor;

typedef struct {
  size_t struct_size;
  const char* language_code;
  const char* country_code;
  const char* script_code;
  const char* variant_code;
} UIWidgetsLocale;

typedef enum {
  kUIWidgetsNativeThreadTypePlatform,
  kUIWidgetsNativeThreadTypeRender,
  kUIWidgetsNativeThreadTypeUI,
  kUIWidgetsNativeThreadTypeWorker,
} UIWidgetsNativeThreadType;

typedef void (*UIWidgetsNativeThreadCallback)(UIWidgetsNativeThreadType type,
                                              void* user_data);

typedef void (*UIWidgetsTaskObserverAdd)(intptr_t key, void* callback,
                                         void* user_data);
typedef void (*UIWidgetsTaskObserverRemove)(intptr_t key, void* user_data);

typedef void (*UIWidgetsMonoEntrypointCallback)(void* user_data);

typedef struct {
  size_t struct_size;
  const char* assets_path;
  const char* font_asset;
  const char* icu_data_path;
  uiwidgets::MappingCallback icu_mapper;
  int command_line_argc;
  const char* const* command_line_argv;
  UIWidgetsPlatformMessageCallback platform_message_callback;
  VoidCallback root_isolate_create_callback;
  const char* persistent_cache_path;
  bool is_persistent_cache_read_only;
  VsyncCallback vsync_callback;
  const UIWidgetsCustomTaskRunners* custom_task_runners;
  const UIWidgetsCompositor* compositor;
  UIWidgetsTaskObserverAdd task_observer_add;
  UIWidgetsTaskObserverRemove task_observer_remove;
  UIWidgetsMonoEntrypointCallback custom_mono_entrypoint;
  UIWidgetsWindowMetricsEvent initial_window_metrics;
} UIWidgetsProjectArgs;

UIWidgetsEngineResult UIWidgetsEngineRun(const UIWidgetsRendererConfig* config,
                                         const UIWidgetsProjectArgs* args,
                                         void* user_data,
                                         UIWidgetsEngine* engine_out);

UIWidgetsEngineResult UIWidgetsEngineShutdown(UIWidgetsEngine engine);

UIWidgetsEngineResult UIWidgetsEngineInitialize(
    const UIWidgetsRendererConfig* config, const UIWidgetsProjectArgs* args,
    void* user_data, UIWidgetsEngine* engine_out);

UIWidgetsEngineResult UIWidgetsEngineDeinitialize(UIWidgetsEngine engine);

UIWidgetsEngineResult UIWidgetsEngineRunInitialized(UIWidgetsEngine engine);

UIWidgetsEngineResult UIWidgetsEngineSendWindowMetricsEvent(
    UIWidgetsEngine engine, const UIWidgetsWindowMetricsEvent* event);

UIWidgetsEngineResult UIWidgetsEngineSendPointerEvent(
    UIWidgetsEngine engine, const UIWidgetsPointerEvent* events,
    size_t events_count);

UIWidgetsEngineResult UIWidgetsEngineSendPlatformMessage(
    UIWidgetsEngine engine, const UIWidgetsPlatformMessage* message);

UIWidgetsEngineResult UIWidgetsPlatformMessageCreateResponseHandle(
    UIWidgetsEngine engine, UIWidgetsDataCallback data_callback,
    void* user_data, UIWidgetsPlatformMessageResponseHandle** response_out);

UIWidgetsEngineResult UIWidgetsPlatformMessageReleaseResponseHandle(
    UIWidgetsEngine engine, UIWidgetsPlatformMessageResponseHandle* response);

UIWidgetsEngineResult UIWidgetsEngineSendPlatformMessageResponse(
    UIWidgetsEngine engine,
    const UIWidgetsPlatformMessageResponseHandle* handle, const uint8_t* data,
    size_t data_length);

UIWidgetsEngineResult __UIWidgetsEngineFlushPendingTasksNow();

UIWidgetsEngineResult UIWidgetsEngineRegisterExternalTexture(
    UIWidgetsEngine engine, int64_t texture_identifier);

UIWidgetsEngineResult UIWidgetsEngineUnregisterExternalTexture(
    UIWidgetsEngine engine, int64_t texture_identifier);

UIWidgetsEngineResult UIWidgetsEngineMarkExternalTextureFrameAvailable(
    UIWidgetsEngine engine, int64_t texture_identifier);

UIWidgetsEngineResult UIWidgetsEngineUpdateAccessibilityFeatures(
    UIWidgetsEngine engine, UIWidgetsAccessibilityFeature features);

UIWidgetsEngineResult UIWidgetsEngineOnVsync(UIWidgetsEngine engine,
                                             intptr_t baton,
                                             uint64_t frame_start_time_nanos,
                                             uint64_t frame_target_time_nanos);

UIWidgetsEngineResult UIWidgetsEngineReloadSystemFonts(UIWidgetsEngine engine);

void UIWidgetsEngineTraceEventDurationBegin(const char* name);

void UIWidgetsEngineTraceEventDurationEnd(const char* name);

void UIWidgetsEngineTraceEventInstant(const char* name);

UIWidgetsEngineResult UIWidgetsEnginePostRenderThreadTask(
    UIWidgetsEngine engine, VoidCallback callback, void* callback_data);

uint64_t UIWidgetsEngineGetCurrentTime();

UIWidgetsEngineResult UIWidgetsEngineRunTask(UIWidgetsEngine engine,
                                             const UIWidgetsTask* task);

UIWidgetsEngineResult UIWidgetsEngineUpdateLocales(
    UIWidgetsEngine engine, const UIWidgetsLocale** locales,
    size_t locales_count);
UIWidgetsEngineResult UIWidgetsEngineNotifyLowMemoryWarning(
    UIWidgetsEngine engine);

UIWidgetsEngineResult UIWidgetsEnginePostCallbackOnAllNativeThreads(
    UIWidgetsEngine engine, UIWidgetsNativeThreadCallback callback,
    void* user_data);

#if defined(__cplusplus)
}  // extern "C"
#endif
