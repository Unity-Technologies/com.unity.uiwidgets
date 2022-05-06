using System;
using Unity.UIWidgets.async;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public class TextureBox : RenderBox {

        public TextureBox(int? textureId, Texture texture) {
            _textureId = textureId;
            _texture = texture;
        }

        Texture _texture;

        public int? textureId {
            get { return _textureId; }
            set {
                D.assert(value != null);
                if (value != _textureId) {
                    _textureId = value;
                    markNeedsPaint();
                }                
            }
        }
        
        int? _textureId;
        Timer _timer;
        bool _frameCallbackScheduled;
        

        protected override bool sizedByParent {
            get { return true; }
        }

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        public override bool  isRepaintBoundary {
            get { return true; }
        }

        protected override void performResize() {
            size = constraints.biggest;
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        void _scheduleAppFrame() {
            if (_frameCallbackScheduled || textureId == null) {
                return;
            }
            
            _frameCallbackScheduled = true;
            SchedulerBinding.instance.scheduleFrameCallback(_handleAppFrame);
        }

        void _handleAppFrame(TimeSpan timestamp) {
            _frameCallbackScheduled = false;
            if (textureId == null) {
                return;
            }

            TimeSpan delay = TimeSpan.Zero;
            delay = new TimeSpan((long) (delay.Ticks * scheduler_.timeDilation));
            _timer = Timer.create(delay, () => _scheduleAppFrame());
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (_textureId == null && _texture == null) {
                return;
            }
            if (_textureId == null) {
                markNeedsLayout();
                SchedulerBinding.instance.scheduleFrameCallback(_handleAppFrame);
                if (_texture.GetNativeTexturePtr() != IntPtr.Zero) {
                    _textureId = UIWidgetsPanelWrapper.current.RegisterExternalTexture(_texture.GetNativeTexturePtr());
                }
                return;
            }

            //texture id == -1 means that the texture is not valid, we won't paint it at all
            if (_textureId.Value == -1) {
                return;
            }
            _scheduleAppFrame();
            context.addLayer(new TextureLayer(
                rect: Rect.fromLTWH(offset.dx, offset.dy, size.width, size.height),
                textureId: _textureId.Value
            ));
        }
    }
}
