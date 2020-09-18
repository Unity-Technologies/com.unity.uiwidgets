using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = UnityEngine.Rect;

namespace Unity.UIWidgets.editor {
    public delegate bool SubmitCallback(SurfaceFrame surfaceFrame, Canvas canvas);

    public class SurfaceFrame : IDisposable {
        bool _submitted;

        readonly GrSurface _surface;

        readonly SubmitCallback _submitCallback;

        public SurfaceFrame(GrSurface surface, SubmitCallback submitCallback) {
            _surface = surface;
            _submitCallback = submitCallback;
        }

        public void Dispose() {
            if (_submitCallback != null && !_submitted) {
                _submitCallback(this, null);
            }
        }

        public Canvas getCanvas() {
            return _surface != null ? _surface.getCanvas() : null;
        }

        public bool submit() {
            if (_submitted) {
                return false;
            }

            _submitted = _performSubmit();

            return _submitted;
        }

        bool _performSubmit() {
            if (_submitCallback == null) {
                return false;
            }

            if (_submitCallback(this, getCanvas())) {
                return true;
            }

            return false;
        }
    }

    public interface Surface : IDisposable {
        SurfaceFrame acquireFrame(Size size, float devicePixelRatio, int antiAliasing);

        MeshPool getMeshPool();
    }

    public class WindowSurfaceImpl : Surface {
        static Material _guiTextureMat;
        static Material _uiDefaultMat;

        public delegate void DrawToTargetFunc(Rect screenRect, Texture texture, Material mat);

        internal static Material _getGUITextureMat() {
            if (_guiTextureMat) {
                return _guiTextureMat;
            }

            var guiTextureShader = Shader.Find("UIWidgets/GUITexture");
            if (guiTextureShader == null) {
                throw new Exception("UIWidgets/GUITexture not found");
            }

            _guiTextureMat = new Material(guiTextureShader);
            _guiTextureMat.hideFlags = HideFlags.HideAndDontSave;
            return _guiTextureMat;
        }

        internal static Material _getUIDefaultMat() {
            if (_uiDefaultMat) {
                return _uiDefaultMat;
            }

            var uiDefaultShader = Shader.Find("UIWidgets/UIDefault");
            if (uiDefaultShader == null) {
                throw new Exception("UIWidgets/UIDefault not found");
            }

            _uiDefaultMat = new Material(uiDefaultShader);
            _uiDefaultMat.hideFlags = HideFlags.HideAndDontSave;
            return _uiDefaultMat;
        }


        GrSurface _surface;
        readonly DrawToTargetFunc _drawToTargetFunc;
        MeshPool _meshPool = new MeshPool();

        public WindowSurfaceImpl(DrawToTargetFunc drawToTargetFunc = null) {
            _drawToTargetFunc = drawToTargetFunc;
        }

        public SurfaceFrame acquireFrame(Size size, float devicePixelRatio, int antiAliasing) {
            _createOrUpdateRenderTexture(size, devicePixelRatio, antiAliasing);

            return new SurfaceFrame(_surface,
                (frame, canvas) => _presentSurface(canvas));
        }

        public MeshPool getMeshPool() {
            return _meshPool;
        }

        public void Dispose() {
            if (_surface != null) {
                _surface.Dispose();
                _surface = null;
            }

            if (_meshPool != null) {
                _meshPool.Dispose();
                _meshPool = null;
            }
        }

        protected bool _presentSurface(Canvas canvas) {
            if (canvas == null) {
                return false;
            }

            _surface.getCanvas().flush();
            _surface.getCanvas().reset();

            var screenRect = new Rect(0, 0,
                _surface.size.width / _surface.devicePixelRatio,
                _surface.size.height / _surface.devicePixelRatio);

            if (_drawToTargetFunc == null) {
                Graphics.DrawTexture(screenRect, _surface.getRenderTexture(),
                    _getGUITextureMat());
            }
            else {
                _drawToTargetFunc(screenRect, _surface.getRenderTexture(),
                    _getUIDefaultMat());
            }

            return true;
        }

        void _createOrUpdateRenderTexture(Size size, float devicePixelRatio, int antiAliasing) {
            if (_surface != null
                && _surface.size == size
                && _surface.devicePixelRatio == devicePixelRatio
                && _surface.antiAliasing == antiAliasing
                && _surface.getRenderTexture() != null) {                
                return;
            }

            if (_surface != null) {
                _surface.Dispose();
                _surface = null;
            }

            _surface = new GrSurface(size, devicePixelRatio, antiAliasing, _meshPool);
        }
    }

    public class GrSurface : IDisposable {
        public readonly Size size;

        public readonly float devicePixelRatio;
        
        public readonly int antiAliasing;

        readonly MeshPool _meshPool;

        RenderTexture _renderTexture;

        CommandBufferCanvas _canvas;

        public RenderTexture getRenderTexture() {
            return _renderTexture;
        }

        public Canvas getCanvas() {
            if (_canvas == null) {
                _canvas = new CommandBufferCanvas(
                    _renderTexture, devicePixelRatio, _meshPool);
            }

            return _canvas;
        }

        public GrSurface(Size size, float devicePixelRatio, int antiAliasing, MeshPool meshPool) {
            this.size = size;
            this.devicePixelRatio = devicePixelRatio;
            this.antiAliasing = antiAliasing;

            var desc = new RenderTextureDescriptor(
                (int) this.size.width, (int) this.size.height,
                RenderTextureFormat.Default, 24) {
                useMipMap = false,
                autoGenerateMips = false,
            };
            
            if (antiAliasing != 0) {
                desc.msaaSamples = antiAliasing;
            }

            _renderTexture = new RenderTexture(desc);
            _renderTexture.hideFlags = HideFlags.HideAndDontSave;

            _meshPool = meshPool;
        }

        public void Dispose() {
            if (_renderTexture) {
                _renderTexture = ObjectUtils.SafeDestroy(_renderTexture);
            }

            if (_canvas != null) {
                _canvas.reset();
                _canvas.dispose();
                _canvas = null;
            }
        }
    }
}